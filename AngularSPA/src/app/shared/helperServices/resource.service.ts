
import { Resource } from '../interfaces/resource.interface';
import { CommitmentType } from '../interfaces/commitmentType.interface';
import { UserPreferences } from '../interfaces/userPreferences.interface';
import { StaffingTag, AvailabilityIncludes, CommitmentType as CommitmentTypeEnum } from '../constants/enumMaster';
import { DateService } from '../dateService';
import * as moment from 'moment';
import { ResourceCommitment } from 'src/app/shared/interfaces/resourceCommitment';
import { Training } from 'src/app/shared/interfaces/training';
import { Vacation } from 'src/app/shared/interfaces/vacation';
import { ResourceLoA } from 'src/app/shared/interfaces/resourceLoA';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ResourceStaffing } from 'src/app/shared/interfaces/resourceStaffing.interface';
import { SupplyFilterCriteria } from '../interfaces/supplyFilterCriteria.interface';
import { alertMessage } from '../interfaces/alertMessage.interface';

export class ResourceService {
  private static commitmentTypeLookups: CommitmentType[];
  private static userPreferences: UserPreferences;

  public static createResourcesDataForStaffing(data: ResourceCommitment, searchStartDate: string, searchEndDate: string,
    supplyFilterCriteriaObj: SupplyFilterCriteria, commitmentTypes: CommitmentType[], userPreferences: UserPreferences, isTriggeredFromSearch = false) {

    this.commitmentTypeLookups = commitmentTypes;
    this.userPreferences = userPreferences;

    let staffingTagSelected: any
    if(supplyFilterCriteriaObj?.staffingTags)
      staffingTagSelected = supplyFilterCriteriaObj?.staffingTags.split(',')
    else if(userPreferences.supplyViewStaffingTags)
      staffingTagSelected = userPreferences.supplyViewStaffingTags.split(',')
    else
      staffingTagSelected = []

    let availabilityIncludes: any 
    if(supplyFilterCriteriaObj?.availabilityIncludes)
      availabilityIncludes = supplyFilterCriteriaObj?.availabilityIncludes.split(',')
    else if(userPreferences.availabilityIncludes)
      availabilityIncludes = userPreferences.availabilityIncludes.split(',')
    else
      availabilityIncludes = []

    let groupBy: any;
    if(supplyFilterCriteriaObj?.groupBy)
      groupBy = supplyFilterCriteriaObj?.groupBy


    const resources: Resource[] = data.resources || [];

    const allCommitments = this.getCommitmentsForResources(data);

    resources.forEach(resource => {

      const resourceCommitments = {
        allocations: allCommitments.allocations.filter(x => x.employeeCode === resource.employeeCode),
        loas: allCommitments.loas.filter(x => x.employeeCode === resource.employeeCode),
        vacations: allCommitments.vacations.filter(x => x.employeeCode === resource.employeeCode),
        trainings: allCommitments.trainings.filter(x => x.employeeCode === resource.employeeCode),
        notAvailability: allCommitments.notAvailablities.filter(x => x.employeeCode === resource.employeeCode),
        downDay: allCommitments.downDays.filter(x => x.employeeCode === resource.employeeCode),
        shortTermAvailability: allCommitments.shortTermAvailabilities.filter(x => x.employeeCode === resource.employeeCode),
        limitedAvailability: allCommitments.limitedAvailabilities.filter(x => x.employeeCode === resource.employeeCode),
        ringFenceAllocations: allCommitments.ringFenceAllocations.filter(x => x.employeeCode === resource.employeeCode),
        transition: allCommitments.transitions.find(x => x.employeeCode === resource.employeeCode),
        transfer: allCommitments.transfers.find(x => x.employeeCode === resource.employeeCode),
        termination: allCommitments.terminations.find(x => x.employeeCode === resource.employeeCode),
        placeholderAllocations: allCommitments.placeholderAllocations.filter(x => x.employeeCode === resource.employeeCode),
        staffableAsRole: allCommitments.staffableAsRoles.find(x => x.employeeCode === resource.employeeCode)
      };
      
      resource = this.updateResourceModel(resource, searchStartDate);
      resource = this.setStaffableAsRole(resource, resourceCommitments.staffableAsRole);


      resource = this.updateAvailabilityDataForResources(resource, searchStartDate, searchEndDate, resourceCommitments,
        staffingTagSelected, availabilityIncludes, groupBy);

      resource = this.updateAvailabilityDateForTransfer(resource, resourceCommitments.transfer);
      resource = this.updateAvailabilityDateForTransition(resource, resourceCommitments.transition, isTriggeredFromSearch);
      resource = this.updateAvailabilityDateForTermination(resource, resourceCommitments.termination, isTriggeredFromSearch);

      resource = this.updateAvailabilityStatus(resource, resourceCommitments);
      resource.upcomingCommitmentsForAlerts = this.addAlertsForActiveAndFutureCommitments(resource, resourceCommitments, availabilityIncludes);

      resource.isSelected = false;
    });

    const availableResources = resources.filter(x => ((x.percentAvailable|| x.prospectivePercentAvailable) > 0 && (x.dateFirstAvailable || x.prospectiveDateFirstAvailable)) || isTriggeredFromSearch);

    return availableResources;
  }

  public static createResourcesDataForResourcesTab(data: ResourceStaffing[], searchStartDate, searchEndDate,
    supplyFilterCriteriaObj, commitmentTypes, userPreferences, isTriggeredFromSearch = false) {

    this.commitmentTypeLookups = commitmentTypes;
    this.userPreferences = userPreferences;

    const staffingTagSelected = supplyFilterCriteriaObj.staffingTags
      ? supplyFilterCriteriaObj.staffingTags.split(',')
      : [];

    let availabilityIncludes: any 
    if(supplyFilterCriteriaObj?.availabilityIncludes)
      availabilityIncludes = supplyFilterCriteriaObj?.availabilityIncludes.split(',')
    else if(userPreferences.availabilityIncludes)
      availabilityIncludes = userPreferences.availabilityIncludes.split(',')
    else
      availabilityIncludes = []

    data.forEach(resourceData => {

      const allCommitments = this.getCommitmentsForResources(resourceData);

      const resourceCommitments = {
        allocations: allCommitments.allocations,
        loas: allCommitments.loas,
        vacations: allCommitments.vacations,
        trainings: allCommitments.trainings,
        notAvailability: allCommitments.notAvailablities,
        downDay: allCommitments.downDays,
        shortTermAvailability: allCommitments.shortTermAvailabilities,
        limitedAvailability: allCommitments.limitedAvailabilities,
        ringFenceAllocations: allCommitments.ringFenceAllocations,
        transition: allCommitments.transitions ? allCommitments.transitions[0] : null, //TODO: see if this can be updated to use arrays instead of object
        transfer: allCommitments.transfers ? allCommitments.transfers[0] : null,
        termination: allCommitments.terminations ? allCommitments.terminations[0] : null,
        placeholderAllocations: allCommitments.placeholderAllocations,
        staffableAsRole: allCommitments.staffableAsRoles[0]
      };

      resourceData.resource = this.updateResourceModel(resourceData.resource, searchStartDate);
      resourceData.resource = this.setStaffableAsRole(resourceData.resource, resourceCommitments.staffableAsRole);

      //resource.upcomingCommitmentsForAlerts = this.addAlertsForActiveAndFutureCommitments(resourceCommitments, availabilityIncludes);

      resourceData.resource = this.updateAvailabilityDataForResources(resourceData.resource, searchStartDate, searchEndDate, resourceCommitments,
        staffingTagSelected, availabilityIncludes, null);

      resourceData.resource = this.updateAvailabilityDateForTransfer(resourceData.resource, resourceCommitments.transfer);
      resourceData.resource = this.updateAvailabilityDateForTransition(resourceData.resource, resourceCommitments.transition, isTriggeredFromSearch);
      resourceData.resource = this.updateAvailabilityDateForTermination(resourceData.resource, resourceCommitments.termination, isTriggeredFromSearch);

      resourceData.resource = this.updateAvailabilityStatus(resourceData.resource, resourceCommitments);

      //resource.isSelected = false;
    });

    //const availableResources = resources.filter(x => (x.percentAvailable > 0 && x.dateFirstAvailable) || x.isTerminated);
    return data;
  }

  private static getCommitmentsForResources(data: ResourceCommitment | ResourceStaffing) {
    const employeeAllocations = data.allocations || [];
    const employeeplaceholderAllocations = data.placeholderAllocations || [];
    let employeeLoAs = data.loAs || [];
    let employeeVacations = data.vacations || [];
    let employeeTrainings = data.trainings || [];
    const employeeTransitions = data.transitions || [];
    const employeeTransfers = data.transfers || [];
    const employeeTerminations = data.terminations || [];
    const staffableAsRoles = data.staffableAsRoles || [];
    const resourceViewNotes = data.resourceViewNotes || [];
    const employeeRingFenceAllocations = data.commitments?.filter(item =>
      item.commitmentTypeCode === StaffingTag.PEG
      || item.commitmentTypeCode === StaffingTag.PEG_Surge
      || item.commitmentTypeCode === StaffingTag.AAG
      || item.commitmentTypeCode === StaffingTag.ADAPT
      || item.commitmentTypeCode === StaffingTag.FRWD);
    const employeeShortTermAvailability = data.commitments?.filter(item =>
      item.commitmentTypeCode === CommitmentTypeEnum.SHORT_TERM_AVAILABLE);
    const employeeNotAvailable = data.commitments?.filter(item =>
      item.commitmentTypeCode === CommitmentTypeEnum.NOT_AVAILABLE);
    const downDay = data.commitments?.filter(item =>
      item.commitmentTypeCode === CommitmentTypeEnum.DOWN_DAY);
    const employeeLimitedAvailability = data.commitments?.filter(item =>
      item.commitmentTypeCode === CommitmentTypeEnum.LIMITED_AVAILABILITY);

    // append trainings, vacations & LOA created in BOSS to data from the source systems
    const employeeTrainingsSavedInStaffing = data.commitments?.filter(item =>
      item.commitmentTypeCode === CommitmentTypeEnum.TRAINING).map(x => {
        const trainingsSavedInStaffing: Training = {
          employeeCode: x.employeeCode,
          startDate: x.startDate,
          endDate: x.endDate,
          type: 'Training'
        };
        return trainingsSavedInStaffing;
      });

    const employeeVacationsSavedInStaffing = data.commitments?.filter(item =>
      item.commitmentTypeCode === CommitmentTypeEnum.VACATION).map(x => {
        const vacationsSavedInStaffing: Vacation = {
          employeeCode: x.employeeCode,
          startDate: x.startDate,
          endDate: x.endDate,
          description: x.description,
          type: 'Vacation'
        };
        return vacationsSavedInStaffing;
      });

    const employeeLoAsSavedInStaffing = data.commitments?.filter(item =>
      item.commitmentTypeCode === CommitmentTypeEnum.LOA).map(x => {
        const loasSavedInStaffing: ResourceLoA = {
          employeeCode: x.employeeCode,
          startDate: x.startDate,
          endDate: x.endDate,
          description: x.description,
          type: 'LOA'
        };
        return loasSavedInStaffing;
      });


    // TimeOffs saved in workday
    const employeeTimeOffs = data.timeOffs?.map(item => {
      const timeOffsSavedInWorkday: Vacation = {
        employeeCode: item.employeeCode,
        startDate: item.startDate,
        endDate: item.endDate,
        description: '',
        status: item.status,
        type: 'Vacation'
      };
      return timeOffsSavedInWorkday;
    });

    employeeTrainings = employeeTrainings.concat(employeeTrainingsSavedInStaffing);
    employeeVacations = employeeVacations.concat(employeeVacationsSavedInStaffing);
    employeeVacations = employeeVacations.concat(employeeTimeOffs);
    employeeLoAs = employeeLoAs.concat(employeeLoAsSavedInStaffing);

    if (employeeTrainings && employeeTrainings.length) {
      employeeTrainings = employeeTrainings.sort((previousElement, nextElement) => {
        return <any>new Date(previousElement.startDate) - <any>new Date(nextElement.startDate);
      });
    }

    if (employeeVacations && employeeVacations.length) {
      employeeVacations = employeeVacations.sort((previousElement, nextElement) => {
        return <any>new Date(previousElement.startDate) - <any>new Date(nextElement.startDate);
      });
    }

    if (employeeLoAs && employeeLoAs.length) {
      employeeLoAs = employeeLoAs.sort((previousElement, nextElement) => {
        return <any>new Date(previousElement.startDate) - <any>new Date(nextElement.startDate);
      });
    }

    return {
      allocations: employeeAllocations,
      placeholderAllocations: employeeplaceholderAllocations,
      loas: employeeLoAs,
      vacations: employeeVacations,
      trainings: employeeTrainings,
      transitions: employeeTransitions,
      transfers: employeeTransfers,
      terminations: employeeTerminations,
      ringFenceAllocations: employeeRingFenceAllocations,
      shortTermAvailabilities: employeeShortTermAvailability,
      notAvailablities: employeeNotAvailable,
      downDays: downDay,
      limitedAvailabilities: employeeLimitedAvailability,
      staffableAsRoles: staffableAsRoles,
      resourceViewNotes: resourceViewNotes
    };

  }

  private static getFirstAvailableDateAndAllocation(date, endDate, commitments, availPercent, staffingTagSelected, availabilityIncludes, calculateProspectiveAvailability = false) {

    // If weekends are not counted as available day then get the first weekday of next week
    if (!availabilityIncludes.includes(AvailabilityIncludes.Weekends) && DateService.isWeekend(date)) {
      date = DateService.getDayInFuture(date, 1);
    }

    const dateWithoutTime = moment(date).startOf('day');

    if (endDate != null && dateWithoutTime.isAfter(endDate)) {
      return { dateFirstAvailable: null, percentAvailable: null };
    }

    const resourceFte = availPercent;

    availPercent = this.updateAvailabilityForTransition(commitments.transition, dateWithoutTime, availPercent, availabilityIncludes);

    if (availPercent > 0) {
      availPercent = this.updateAvailablityForLoAs(commitments.loas, dateWithoutTime, availPercent);
    }

    if (availPercent > 0) {
      availPercent = this.updateAvailablityForNotAvailability(commitments.notAvailability, dateWithoutTime, availPercent);
    }

    if (availPercent > 0) {
      availPercent = this.updateAvailablityForDownDay(commitments.downDay, dateWithoutTime, availPercent);
    }

    if (availPercent > 0 && staffingTagSelected.length > 0) {
      availPercent = this.updateAvailabilityForRingFence(commitments.ringFenceAllocations, dateWithoutTime,
        availPercent, staffingTagSelected);
    }

    if (availPercent > 0) {
      availPercent = this.updateAvailablityForAssignment(commitments.allocations, dateWithoutTime, availPercent, availabilityIncludes);
    }

    
    if (availPercent > 0) {
      availPercent = this.updateAvailablityForVacations(commitments.vacations, dateWithoutTime, availPercent);
    }

    if (availPercent > 0) {
      availPercent = this.updateAvailablityForTrainings(commitments.trainings, dateWithoutTime, availPercent);
    }


    if (availPercent > 0){
      availPercent = this.updateAvailablityForConfirmedAllocationOnIncludeInCapacityPlanningCard(commitments.placeholderAllocations, dateWithoutTime, availPercent, calculateProspectiveAvailability);  

    }


    if (availPercent > 0) {
      return { dateFirstAvailable: dateWithoutTime.format('YYYY-MM-DD'), percentAvailable: availPercent };
    }

    const nextDate = dateWithoutTime.add(1, 'days');

    return this.getFirstAvailableDateAndAllocation(nextDate, endDate, commitments, resourceFte, staffingTagSelected, availabilityIncludes);

  }

  private static updateAvailabilityStatus(resource, commitments) {
    // Update availabilityStatus property. Shows in STA if transition has not started. If transition started then show in transition bucket
    
    if (commitments.transition && moment(resource.dateFirstAvailable).isSameOrAfter(moment(commitments.transition.startDate), 'day')) {
        resource.availabilityStatus = ConstantsMaster.availabilityBuckets.Transition;
    } else if (commitments.limitedAvailability.length > 0) {
        resource.availabilityStatus = ConstantsMaster.availabilityBuckets.LimitedAvailable;
    } else if (commitments.shortTermAvailability.length > 0) {
        resource.availabilityStatus = ConstantsMaster.availabilityBuckets.ShortTermAvailable;
    } else if (moment(resource.startDate).isSameOrAfter(moment(), 'day')) {
        resource.availabilityStatus = ConstantsMaster.availabilityBuckets.NotYetStarted;
        resource.activeStatus = ConstantsMaster.ResourceActiveStatus.NotYetStarted;
    } else if (this.isResourceOnIncludeInCapacity(resource.prospectiveDateFirstAvailable, commitments.placeholderAllocations)) {
      resource.availabilityStatus = [ConstantsMaster.availabilityBuckets.IncludeInCapacity];
        if (resource.dateFirstAvailable) {
          resource.availabilityStatus.push(ConstantsMaster.availabilityBuckets.Available);
        }
    } else if (this.isResourceOnPlanningCardOrPlaceholder(resource.dateFirstAvailable, commitments.placeholderAllocations)) {
        resource.availabilityStatus = ConstantsMaster.availabilityBuckets.PlaceholderAndPlanningCard;
    } else {
        resource.availabilityStatus = ConstantsMaster.availabilityBuckets.Available;
    }
    return resource;
  }

  private static updateResourceModel(resource, searchStartDate) {
    resource.dateFirstAvailable = searchStartDate == null ? DateService.getFormattedDate(new Date()) : searchStartDate;
    resource.percentAvailable = resource.fte * 100;


    return resource;
  }

  private static setStaffableAsRole(resource, staffableAsRole) {
    if (staffableAsRole) {
      resource.staffableAsTypeName = staffableAsRole.staffableAsTypeName;
    }
    return resource;
  }

  private static isResourceOnPlanningCardOrPlaceholder(dateFirstAvailable, placeholderAllocations) {
    if (placeholderAllocations.length > 0 ) {
      return placeholderAllocations.some(allocation => {
        return (!allocation.startDate || moment(allocation.startDate).isSameOrBefore(moment(dateFirstAvailable)))
          && (!allocation.endDate || moment(allocation.endDate).isSameOrAfter(moment(dateFirstAvailable))
          &&(!allocation.includeInCapacityReporting))
      });
    }
    return false;
  }

  private static isResourceOnIncludeInCapacity(dateFirstAvailable, placeholderAllocations) {

    if (placeholderAllocations.length > 0 && dateFirstAvailable) {
      return placeholderAllocations.some(allocation => {
        return (!allocation.startDate || moment(allocation.startDate).isSameOrBefore(moment(dateFirstAvailable)))
        && (!allocation.endDate || moment(allocation.endDate).isSameOrAfter(moment(dateFirstAvailable))
        && (allocation.includeInCapacityReporting && !allocation.isPlaceholderAllocation))
      });
    }
    return false;
  }

  private static updateAvailabilityDataForResources(resource, searchStartDate, searchEndDate, resourceCommitments,
    staffingTagSelected, availabilityIncludes, groupBy) {
    searchStartDate = moment(resource.startDate).isAfter(searchStartDate ?? moment(), 'day')
      ? resource.startDate : searchStartDate;
    let startDate = searchStartDate == null ? moment() : moment(searchStartDate);
    const endDate = searchEndDate == null ? searchEndDate : moment(searchEndDate);

    // For transfer in incoming office, min avail date would be transfer effective date
    if (resourceCommitments.transfer
      && resource.schedulingOffice.officeCode === resourceCommitments.transfer.operatingOfficeProposed.officeCode
      && moment(resourceCommitments.transfer.startDate).startOf('day').isSameOrAfter(resource.dateFirstAvailable)) {
      startDate = moment(resourceCommitments.transfer.startDate);
    }

    if (resource.isTerminated) {
      return resource;
    }

    const availabilityData = this.getFirstAvailableDateAndAllocation(startDate, endDate, resourceCommitments,
      resource.percentAvailable, staffingTagSelected, availabilityIncludes,false);

    
    if(groupBy?.includes("availability") && resourceCommitments.placeholderAllocations.length>0)
    {
      let includeInCapacityPlaceholderAllocations = resourceCommitments.placeholderAllocations.filter(x=>x.includeInCapacityReporting==true);
    
      if(includeInCapacityPlaceholderAllocations.length>0)
      {
        const [prospectiveStartDate, prospectiveEndDate] = this.getDatesForProspectiveAllocation(resourceCommitments);

        const prospectiveAvailabilityData = this.getFirstAvailableDateAndAllocation(prospectiveStartDate, prospectiveEndDate, resourceCommitments,
        resource.percentAvailable, staffingTagSelected, availabilityIncludes,true);

        // is include in capacity PC (prospectiveAvailabilityData.dateFirstAvailable) then keep both otherwise make both % and date null
        resource.prospectiveDateFirstAvailable = this.isResourceOnIncludeInCapacity(prospectiveAvailabilityData.dateFirstAvailable, resourceCommitments.placeholderAllocations) ? prospectiveAvailabilityData.dateFirstAvailable : null;
        
        if(prospectiveAvailabilityData.percentAvailable > 0)

          resource.prospectivePercentAvailable = prospectiveAvailabilityData.percentAvailable;
        
      }  

    }

    resource.dateFirstAvailable = availabilityData.dateFirstAvailable;
    resource.percentAvailable = availabilityData.percentAvailable;

    return resource;
  }

  private static getDatesForProspectiveAllocation(resource) {
    
    let today = new Date();
    const todayFormatted = DateService.getFormattedDate(today);
    
    // take only those placeholderAllocations with includeInCapacityReporting as true and end date >= today
    const filteredPlanningCardAllocations = resource.placeholderAllocations.filter(allocation => 
        allocation.includeInCapacityReporting === true && 
        (!allocation.endDate || moment(allocation.endDate).isAfter(moment(todayFormatted)))
    );

    // Find the first date of the filtered allocations
    filteredPlanningCardAllocations.sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime());
    let planningCardStartDate = filteredPlanningCardAllocations.length > 0 ? filteredPlanningCardAllocations[0].startDate : null;

    if (planningCardStartDate && moment(todayFormatted).isSameOrAfter(moment(planningCardStartDate))) {
        planningCardStartDate = todayFormatted;
    }

    // Find the last date of the filtered allocations
    filteredPlanningCardAllocations.sort((a, b) => new Date(b.endDate).getTime() - new Date(a.endDate).getTime());
    const planningCardEndDate = filteredPlanningCardAllocations.length > 0 ? filteredPlanningCardAllocations[0].endDate : null;

    return [planningCardStartDate, planningCardEndDate];

  }

  private static updateAvailabilityDateForTransfer(resource, transfer) {
    // 2 rows are created for transferred resources on supply panel.
    // Update availability based on office in which resource would be on the date of "date first available"

    // row with scheduling Office = office in which transferred resource currently is.Will not show in this office after transfer date
    if (transfer && resource.schedulingOffice.officeCode === transfer.operatingOfficeCurrent.officeCode
      && moment(resource.dateFirstAvailable).isSameOrAfter(moment(transfer.startDate).startOf('day'))) {

      resource.dateFirstAvailable = null;
      resource.percentAvailable = null;
    }

    return resource;

  }

  public static updateAvailabilityDateForTransition(resource, transition, isTriggeredFromSearch) {
    /*if resource is on transition and their availability date is after their transition end date then,
      1) do no show in supply panel
      2) only show them when searching for them with N/A as availability date and percent
    */
    if (transition && moment(resource.dateFirstAvailable).isAfter(moment(transition.endDate))) {

      if (isTriggeredFromSearch) {
        resource.onTransitionOrTerminationAndNotAvailable = true;
      } else {
        resource.dateFirstAvailable = null;
        resource.percentAvailable = null;
      }

    }

    return resource;
  }

  public static updateAvailabilityDateForTermination(resource, termination, isTriggeredFromSearch) {
    /*if resource's availability date is on or after their termination date then,
      1) do not show in supply panel
      2) only show them when searching for them with N/A as availability date and percent
    */
    if (termination && moment(resource.dateFirstAvailable).isSameOrAfter(moment(termination.endDate))) {

      if (isTriggeredFromSearch) {
        resource.onTransitionOrTerminationAndNotAvailable = true;
      }

      resource.dateFirstAvailable = null;
      resource.percentAvailable = null;
    }
    else if(resource.isTerminated){
      resource.dateFirstAvailable = null;
      resource.percentAvailable = null;
    }

    return resource;
  }


  private static addAlertsForActiveAndFutureCommitments(resource, commitments, availabilityIncludes) {
    const upcomingCommitmentsForAlerts: alertMessage = {
      commitments: [],
      allocations: []
    };
    if (commitments.transfer && moment(commitments.transfer.startDate).startOf('day').isAfter(moment().format('LL'))) {
      this.addAlertForTransfer(commitments.transfer, upcomingCommitmentsForAlerts);
    }
    if (availabilityIncludes.includes(AvailabilityIncludes.CD) && commitments.allocations) {
      const cdAllocations = commitments.allocations.filter(x => x.caseTypeCode === parseInt(AvailabilityIncludes.CD));
      if (cdAllocations.length > 0) {
        this.addAlertForCD(cdAllocations, upcomingCommitmentsForAlerts);
      }
    }
    if(commitments.allocations.length > 0){
      this.addAlertForAllocations(resource,commitments.allocations, upcomingCommitmentsForAlerts);
    }
    if(commitments.placeholderAllocations.length > 0){
      this.addAlertforPlanningCardAllocations(resource,commitments.placeholderAllocations, upcomingCommitmentsForAlerts);
    }
    if (commitments.loas.length > 0) {
      this.addAlertForLoA(commitments.loas, upcomingCommitmentsForAlerts);
    }
    if (commitments.vacations.length > 0) {
      this.addAlertForVacations(commitments.vacations, upcomingCommitmentsForAlerts);
    }
    if (commitments.trainings.length > 0) {
      this.addAlertForTrainings(commitments.trainings, upcomingCommitmentsForAlerts);
    }
    if (commitments.ringFenceAllocations.length > 0) {
      this.addAlertForRingFenceAllocations(commitments.ringFenceAllocations, upcomingCommitmentsForAlerts);
    }
    if (commitments.notAvailability.length > 0) {
      this.addAlertForNotAvailability(commitments.notAvailability, upcomingCommitmentsForAlerts);
    }
    if (commitments.downDay.length > 0) {
      this.addAlertForDownDay(commitments.downDay, upcomingCommitmentsForAlerts);
    }
    if (commitments.shortTermAvailability.length > 0) {
      this.addAlertForShortTermAvailability(commitments.shortTermAvailability, upcomingCommitmentsForAlerts);
    }
    if (commitments.limitedAvailability.length > 0) {
      this.addAlertForLimitedAvailability(commitments.limitedAvailability, upcomingCommitmentsForAlerts);
    }
    if (commitments.transition && moment(commitments.transition.endDate).startOf('day').isAfter(moment().format('LL'))) {
      this.addAlertForTransition(commitments.transition, upcomingCommitmentsForAlerts);
    } else if (commitments.termination && moment(commitments.termination.endDate).startOf('day').isSameOrAfter(moment().format('LL'))) {
      this.addAlertForTermination(commitments.termination, upcomingCommitmentsForAlerts);
    }
    if (commitments.staffableAsRole) {
      this.addAlertForStaffableAsRole(commitments.staffableAsRole, upcomingCommitmentsForAlerts);
    }   
    this.sortArrayByStartDate(upcomingCommitmentsForAlerts.allocations);
    this.sortArrayByStartDate(upcomingCommitmentsForAlerts.commitments);

    return upcomingCommitmentsForAlerts;
  }

  private static sortArrayByStartDate(array:any[]) {
    const dateRegex = /\bfrom\s+(\d{1,2}-[A-Za-z]{3}-\d{4})\b/;
    array.sort((prevElem, nextElem) => {
      const prevElementDates = prevElem.match(dateRegex);
      const nextElementDates = nextElem.match(dateRegex);

      if (prevElementDates && nextElementDates) {
        const prevStartDate = new Date(prevElementDates[1]);
        const nextStartDate = new Date(nextElementDates[1]);
        return prevStartDate.getTime() - nextStartDate.getTime();
      } else {
        return 0;
      }
    });
  }

  private static addAlertForStaffableAsRole(staffableAsRole, upcomingCommitmentsForAlerts) {
    upcomingCommitmentsForAlerts.commitments.push(`Staffable as: ${staffableAsRole.staffableAsTypeName}`);
  }

  private static addAlertForTransfer(transfer, upcomingCommitmentsForAlerts) {

    if (this.isFutureCommitment(transfer.startDate)) {
      const dateDiff = moment(transfer.startDate).diff(moment(), 'days') + 1;

      upcomingCommitmentsForAlerts.commitments.push(
        this.getAlertMessageForFutureCommitment(`Transfer from ${transfer.operatingOfficeCurrent.officeName} to
            ${transfer.operatingOfficeProposed.officeName} effective from`, transfer.startDate, null, dateDiff)
      );
    }

  }

  private static addAlertForCD(cdAllocations, upcomingCommitmentsForAlerts) {
    cdAllocations.every(allocation => {

      if (this.isFutureCommitment(allocation.startDate)) {
        const dateDiff = moment(allocation.startDate).diff(moment(), 'days') + 1;

        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForFutureCommitment('CD case from', allocation.startDate, allocation.endDate, dateDiff)
        );
      } else if(this.isActiveCommitment(allocation.startDate, allocation.endDate)) {
        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForActiveCommitment('CD case from', allocation.startDate, allocation.endDate)
        );
      }

      return true;

    });
  }

  private static addAlertforPlanningCardAllocations(resource,allocations, upcomingCommitmentsForAlerts) {
     allocations.every(allocation => {
      if(!allocation.isPlaceholderAllocation && allocation.isPlanningCardShared){
          if (this.isFutureAllocation(allocation.startDate,resource.dateFirstAvailable)) {
            upcomingCommitmentsForAlerts.allocations.push(
              this.getAlertsForFutureAllocations(allocation)
            );
          }
      }
      return true;
    });
  }

  private static addAlertForAllocations(resource,allocations, upcomingCommitmentsForAlerts) {
    allocations.every(allocation => {

      if (this.isFutureAllocation(allocation.startDate,resource.dateFirstAvailable)) {
        upcomingCommitmentsForAlerts.allocations.push(
          this.getAlertsForFutureAllocations(allocation)
        );
      }
      return true;
    });
  }
  
  private static addAlertForLoA(loas, upcomingCommitmentsForAlerts) {
    loas.every(loa => {

      // people on ACTIVE LOA are not shown on supply so no need for message for Active LOA
      if (this.isFutureCommitment(loa.startDate)) {
        const dateDiff = moment(loa.startDate).diff(moment(), 'days') + 1;

        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForFutureCommitment('LOA from', loa.startDate, loa.endDate, dateDiff)
        );
      }

      return true;
    });
  }

  private static addAlertForVacations(vacations, upcomingCommitmentsForAlerts) {
    vacations.every(vacation => {
         
      let vacationStatus = vacation.status && vacation.status == 'Submitted' ? '(Pending Approval)' : '';

      if (this.isFutureCommitment(vacation.startDate)) {
        const dateDiff = moment(vacation.startDate).diff(moment(), 'days') + 1;


        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForFutureCommitment(`Vacation ${vacationStatus} from`, vacation.startDate, vacation.endDate, dateDiff)
        );
      }
       else if(this.isActiveCommitment(vacation.startDate, vacation.endDate)) {
        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForActiveCommitment(`Vacation ${vacationStatus} from`, vacation.startDate, vacation.endDate)
        );
      }

      return true;

    });
  }

  private static addAlertForTrainings(trainings, upcomingCommitmentsForAlerts) {
    trainings.every(training => {

      if (this.isFutureCommitment(training.startDate)) {
        const dateDiff = moment(training.startDate).diff(moment(), 'days') + 1;

        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForFutureCommitment('Training from', training.startDate, training.endDate, dateDiff)
        );
      } else if(this.isActiveCommitment(training.startDate, training.endDate)) {
        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForActiveCommitment('Training from', training.startDate, training.endDate)
        );
      }

      return true;
    });
  }

  private static addAlertForRingFenceAllocations(ringFenceAllocations, upcomingCommitmentsForAlerts) {
    ringFenceAllocations.every(allocation => {
      const commitmentName = this.commitmentTypeLookups.find(x => x.commitmentTypeCode === allocation.commitmentTypeCode)?.commitmentTypeName ?? "";

      if (this.isFutureCommitment(allocation.startDate)) {
        const dateDiff = moment(allocation.startDate).diff(moment(), 'days') + 1;
        let messagePrefix = `Commitment (${commitmentName}) from`;
        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForFutureCommitment(messagePrefix, allocation.startDate, allocation.endDate, dateDiff)
        );
        return true;
      } else if (!this.isActivePEGCommitment(commitmentName, allocation.startDate, allocation.endDate)) {
        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForActiveCommitment(`Commitment (${commitmentName}) from`, allocation.startDate, allocation.endDate)
        );
        return true;
      }

      return true;

    });
  }

  private static addAlertForNotAvailability(notAvailabilities, upcomingCommitmentsForAlerts) {
    notAvailabilities.every(nonAvailability => {

      // people who are NOT AVAILABLE today are not shown on supply so no need for message for Active check
      if (this.isFutureCommitment(nonAvailability.startDate)) {
        const dateDiff = moment(nonAvailability.startDate).diff(moment(), 'days') + 1;

        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForFutureCommitment('Not Available from', nonAvailability.startDate, nonAvailability.endDate, dateDiff)
        );
      }

      return true;
    });

  }

  private static addAlertForDownDay(downDays, upcomingCommitmentsForAlerts) {
    downDays.every(downDay => {

      // people who are on PEG Down Day today are not shown on supply so no need for message for Active check
      if (this.isFutureCommitment(downDay.startDate)) {
        const dateDiff = moment(downDay.startDate).diff(moment(), 'days') + 1;

        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForFutureCommitment('Down Day from', downDay.startDate, downDay.endDate, dateDiff)
        );
      }

      return true;
    });

  }

  private static addAlertForShortTermAvailability(shortTermAvailabilities, upcomingCommitmentsForAlerts) {

    shortTermAvailabilities.every(shortTermAvailability => {

      if (this.isFutureCommitment(shortTermAvailability.startDate)) {
        const dateDiff = moment(shortTermAvailability.startDate).diff(moment(), 'days') + 1;

        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForFutureCommitment('Short Term Available from', shortTermAvailability.startDate, shortTermAvailability.endDate, dateDiff)
        );
      } else if(this.isActiveCommitment(shortTermAvailability.startDate, shortTermAvailability.endDate)){
        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForActiveCommitment('Short Term Available from', shortTermAvailability.startDate, shortTermAvailability.endDate)
        );
      }

      return true;

    });
  }

  private static addAlertForLimitedAvailability(limitedAvailabilities, upcomingCommitmentsForAlerts) {
    limitedAvailabilities.every(limitedAvailability => {

      if (this.isFutureCommitment(limitedAvailability.startDate)) {
        const dateDiff = moment(limitedAvailability.startDate).diff(moment(), 'days') + 1;

        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForFutureCommitment('Limited Availability from', limitedAvailability.startDate, limitedAvailability.endDate, dateDiff)
        );
      } else if(this.isActiveCommitment(limitedAvailability.startDate, limitedAvailability.endDate)) {
        upcomingCommitmentsForAlerts.commitments.push(
          this.getAlertMessageForActiveCommitment('Limited Availability from', limitedAvailability.startDate, limitedAvailability.endDate)
        );
      }

      return true;

    });
  }

  private static addAlertForTransition(transition, upcomingCommitmentsForAlerts) {

    if (this.isFutureCommitment(transition.startDate)) {
      const dateDiff = moment(transition.startDate).diff(moment(), 'days') + 1;

      upcomingCommitmentsForAlerts.commitments.push(
        this.getAlertMessageForFutureCommitment('Transition from', transition.startDate, transition.endDate, dateDiff)
      );
    } else if(this.isActiveCommitment(transition.startDate,transition.endDate)) {
      upcomingCommitmentsForAlerts.commitments.push(
        this.getAlertMessageForActiveCommitment('Transition from', transition.startDate, transition.endDate)
      );
    }

    return true;
  }

  private static addAlertForTermination(termination, upcomingCommitmentsForAlerts) {
    // Termination don't have start date, so checking on end date
    if (this.isFutureCommitment(termination.endDate)) {
      const dateDiff = moment(termination.endDate).diff(moment(), 'days') + 1;

      upcomingCommitmentsForAlerts.commitments.push(
        this.getAlertMessageForFutureCommitment('Termination effective from', termination.endDate, null, dateDiff)
      );
    }

  }

  private static updateAvailabilityForTransition(transition, date, availPercent, availabilityIncludes) {
    if (transition && date.isSameOrAfter(moment(transition.startDate), 'day') &&
      date.isSameOrBefore(moment(transition.endDate), 'day') && !availabilityIncludes.includes(AvailabilityIncludes.Transition)) {
      availPercent = 0;
    }

    return availPercent;

  }

  private static updateAvailablityForLoAs(loas, date, availPercent) {
    loas.forEach(loa => {
      if (date.isSameOrAfter(moment(loa.startDate), 'day') && date.isSameOrBefore(moment(loa.endDate), 'day')) {
        availPercent = 0;
      }
    });

    return availPercent;
  }

  private static updateAvailablityForVacations(vacations, date, availPercent) {
    vacations.forEach(vacation => {
      const startDate = moment(vacation.startDate).startOf('day');
      const endDate = moment(vacation.endDate).startOf('day');
      if (date.isSameOrAfter(startDate) &&
        date.isSameOrBefore(endDate) && endDate.diff(startDate, 'days') >= this.userPreferences.vacationThreshold) {
        availPercent = 0;
      }
    });

    return availPercent;
  }

  private static updateAvailablityForTrainings(trainings, date, availPercent) {
    trainings.forEach(training => {
      const startDate = moment(training.startDate).startOf('day');
      const endDate = moment(training.endDate).startOf('day');
      if (date.isSameOrAfter(startDate) && date.isSameOrBefore(endDate)
        && endDate.diff(startDate, 'days') >= this.userPreferences.trainingThreshold) {
        availPercent = 0;
      }
    });

    return availPercent;
  }

  private static updateAvailablityForNotAvailability(notAvailabilities, date, availPercent) {
    notAvailabilities.forEach(notAvailability => {
      if (date.isSameOrAfter(moment(notAvailability.startDate), 'day') &&
        date.isSameOrBefore(moment(notAvailability.endDate), 'day')) {
        availPercent = 0;
      }
    });

    return availPercent;
  }

  private static updateAvailablityForDownDay(downDays, date, availPercent) {
    downDays.forEach(downDay => {
      if (date.isSameOrAfter(moment(downDay.startDate), 'day') &&
        date.isSameOrBefore(moment(downDay.endDate), 'day')) {
        availPercent = 0;
      }
    });

    return availPercent;
  }

  private static updateAvailablityForAssignment(assignments, date, availPercent, availabilityIncludes) {
    assignments.forEach(assignment => {

      if ((assignment.oldCaseCode && availabilityIncludes.includes(assignment.caseTypeCode?.toString()))
        || (!assignment.oldCaseCode && availabilityIncludes.includes(AvailabilityIncludes.Opportunity))
      ) {
        availPercent = availPercent;
      } else if (date.isSameOrAfter(moment(assignment.startDate).startOf('day')) && date.isSameOrBefore(moment(assignment.endDate).startOf('day'))) {
        availPercent = availPercent - assignment.allocation;
      }

    });
    return availPercent;
  }

  private static updateAvailablityForConfirmedAllocationOnIncludeInCapacityPlanningCard(assignments, date, availPercent, calculateProspectiveAvailability) {
    let sumOfPlaceholderAllocationsPercent = 0;
    assignments.forEach(assignment => {

      if (this.isAvailabilityOnPlanningCardEffected(assignment,date)) {
   
        sumOfPlaceholderAllocationsPercent = sumOfPlaceholderAllocationsPercent + assignment.allocation;
   
        if(calculateProspectiveAvailability)
        availPercent = Math.min(sumOfPlaceholderAllocationsPercent, availPercent);
        else
        availPercent = availPercent - assignment.allocation;
      }

    });
    return availPercent;
  }

  private static isAvailabilityOnPlanningCardEffected(assignment,date)
  {
    if(this.isConfirmedandPlanningCardAllocation(assignment) && this.isTodayLiesInAllocationDateRange(assignment, date) && this.isIncludeInCapacityReporting(assignment))
      return true;
    else
      return false;
  }

  private static isConfirmedandPlanningCardAllocation(assignment)
  {
    if(assignment.planningCardId && assignment.isPlaceholderAllocation === false)
      return true;
    
    else
      return false;
  }

  private static isTodayLiesInAllocationDateRange(assignment, date)
  {
    if(date.isSameOrAfter(moment(assignment.startDate).startOf('day')) && date.isSameOrBefore(moment(assignment.endDate).startOf('day')))
      return true

    else
      return false;
  }

  private static isIncludeInCapacityReporting(assignment)
  {
    if(assignment.includeInCapacityReporting === true)
    {
      return true;
    }
    else
      return false;
  }

  private static updateAvailabilityForRingFence(ringFenceAllocations, date, availPercent, staffingTagSelected) {
    if (!ringFenceAllocations.some(r => staffingTagSelected.includes(r.commitmentTypeCode))) {
      ringFenceAllocations.forEach(ringFenceAllocation => {
        if (date.isSameOrAfter(moment(ringFenceAllocation.startDate), 'day') &&
          date.isSameOrBefore(moment(ringFenceAllocation.endDate), 'day')) {
          availPercent = 0;
        }
      });
    }
    return availPercent;

  }

  private static isActiveCommitment(commitmentStartDate, commitmentEndDate) {
    return (moment(commitmentStartDate).startOf('day').isSameOrBefore(moment().format('LL'))
      && moment(commitmentEndDate).startOf('day').isSameOrAfter(moment().format('LL')));
  }

  private static isFutureCommitment(commitmentStartDate) {
    return moment(commitmentStartDate).startOf('day').isAfter(moment().format('LL'));
  }
  private static isFutureAllocation(allocationStartDate, dateFirstAvailable) {
    return moment(allocationStartDate).isAfter(moment(dateFirstAvailable), 'day');
  }

  private static isActivePEGCommitment(commitmentName: string, commitmentStartDate, commitmentEndDate): boolean {

    return (commitmentName.toUpperCase().includes('PEG') &&
      this.isActiveCommitment(commitmentStartDate, commitmentEndDate));
  }

  private static getAlertMessageForActiveCommitment(messagePrefix, commitmentStartDate, commitmentEndDate) {
    return (`${messagePrefix} ${moment(commitmentStartDate).format('DD-MMM-YYYY')} till ${moment(commitmentEndDate)
      .format('DD-MMM-YYYY')}`);
  }
  private static getAlertsForFutureAllocations(allocation){
    if(allocation.oldCaseCode){
      return (`Allocated on ${allocation.caseName} (${allocation.oldCaseCode} - ${allocation.allocation}%) from ${moment(allocation.startDate).format('DD-MMM-YYYY')} till ${moment(allocation.endDate)
        .format('DD-MMM-YYYY')}`);
    }
    else if (allocation.pipelineId){
      return (`Allocated on ${allocation.opportunityName} (${allocation.allocation}%) from ${moment(allocation.startDate).format('DD-MMM-YYYY')} till ${moment(allocation.endDate)
        .format('DD-MMM-YYYY')}`);
    }
    else{
      if(allocation.planningCardTitle){
      return (`Allocated on Planning Card (${allocation.planningCardTitle} - ${allocation.allocation}%) from ${moment(allocation.startDate).format('DD-MMM-YYYY')} till ${moment(allocation.endDate)
        .format('DD-MMM-YYYY')}`);
      }
      else{
        return (`Allocated on Planning Card (${allocation.allocation}%) from ${moment(allocation.startDate).format('DD-MMM-YYYY')} till ${moment(allocation.endDate)
        .format('DD-MMM-YYYY')}`);
      
      }
    }
  }

  private static getAlertMessageForFutureCommitment(messagePrefix, commitmentStartDate, commitmentEndDate, dateDiff) {
   
    if (commitmentEndDate) {
      return (`${messagePrefix} ${moment(commitmentStartDate).format('DD-MMM-YYYY')} till ${moment(commitmentEndDate)
        .format('DD-MMM-YYYY')} in ${dateDiff} day(s)`);
    } else {
      return (`${messagePrefix} ${moment(commitmentStartDate).format('DD-MMM-YYYY')} in ${dateDiff} day(s)`);
    }

    
  }

  /*Returns an array with distinct items based on column name*/
  public static getDistinctFromArray(array, col) {
    const flags = {};
    const result = array.filter(function (item) {
      const colValue = item[col];
      if (flags[colValue]) {
        return false;
      }
      flags[colValue] = true;
      return true;
    });

    return result;

  }

}
