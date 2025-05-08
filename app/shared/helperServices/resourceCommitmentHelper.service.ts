// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Interfaces ----------------------------------//
import { Holiday } from '../interfaces/holiday';
import { ResourceAllocation } from '../interfaces/resourceAllocation.interface';
import { ResourceLoA } from '../interfaces/resourceLoA';
import { StaffingCommitment } from '../interfaces/staffingCommitment.interface';
import { ResourceTimeOff } from '../interfaces/resourceTimeOff.interface';
import { Training } from '../interfaces/training';
import { Vacation } from '../interfaces/vacation';

// ----------------------- Service References ----------------------------------//
import { LocalStorageService } from '../local-storage.service';

// --------------------------Constants/Enums -----------------------------------------//
import { ConstantsMaster } from '../constants/constantsMaster';
import { CommitmentType as  CommitmentTypeCodeEnum} from 'src/app/shared/constants/enumMaster';
import { CommitmentView } from '../interfaces/commitmentView';
import { PlaceholderAllocation } from '../interfaces/placeholderAllocation.interface';


@Injectable({
  providedIn: 'root'
})
export class ResourceCommitmentHelperService {

  constructor(private localStorageService: LocalStorageService,) { }

  public convertResourceAllocationsToStaffingCommitmentModel(resourceAllocations: ResourceAllocation[]) : StaffingCommitment[]{
    const commitmentArray: StaffingCommitment[] = [];

    resourceAllocations.forEach(allocation => {

      //TODO: fetch investment name from API
      let investmentName = null;
      if(allocation.investmentCode){
        investmentName = this.localStorageService.get(ConstantsMaster.localStorageKeys.investmentCategories)
        .find(x => x.investmentCode === allocation.investmentCode).investmentName;
      }

      const staffingCommitment: StaffingCommitment = {
          id: allocation.id,
          oldCaseCode: allocation.oldCaseCode,
          caseName: allocation.caseName,
          clientName: allocation.clientName,
          caseTypeCode: allocation.caseTypeCode,
          pipelineId: allocation.pipelineId,
          opportunityName: allocation.opportunityName,
          employeeCode: allocation.employeeCode,
          employeeName: allocation.employeeName,
          internetAddress: allocation.internetAddress,
          operatingOfficeCode: allocation.operatingOfficeCode,
          operatingOfficeAbbreviation: allocation.operatingOfficeAbbreviation,
          currentLevelGrade: allocation.currentLevelGrade,
          serviceLineCode: allocation.serviceLineCode,
          serviceLineName: allocation.serviceLineName,
          allocation: allocation.allocation,
          startDate: allocation.startDate,
          endDate: allocation.endDate,
          investmentCode: allocation.investmentCode,
          investmentName: investmentName,
          caseRoleCode: allocation.caseRoleCode,
          lastUpdatedBy: allocation.lastUpdatedBy,
          caseStartDate: allocation.caseStartDate,
          caseEndDate: allocation.caseEndDate,
          opportunityStartDate: allocation.opportunityStartDate,
          opportunityEndDate: allocation.opportunityEndDate,
          notes: allocation.notes,
          type: ConstantsMaster.CommitmentType.Allocation,
          commitmentTypeCode: CommitmentTypeCodeEnum.CASE_OPP,
          commitmentTypeName: ConstantsMaster.CommitmentType.Allocation,
          commitmentTypeReasonCode: allocation?.commitmentTypeReasonCode,
          commitmentTypeReasonName: allocation?.commitmentTypeReasonName,
          // commitmentType: ConstantsMaster.CommitmentType.Allocation,
          isEditable: allocation.isPlaceholderAllocation ? false : true,
          caseRoleName: allocation.caseRoleName,
          billCode: null,
          description: null,
          fte: null,
          lastUpdated: null,
          operatingOffice: null,
          operatingOfficeCurrent: null,
          operatingOfficeProposed: null,
          position: null,
          probabilityPercent: allocation.probabilityPercent,
          isPlaceholderAllocation: allocation.isPlaceholderAllocation,
      };

      commitmentArray.push(staffingCommitment);
    });

    return commitmentArray;
  }

  public convertPlaceholderAllocationsToStaffingCommitmentModel(placeholderAllocations: PlaceholderAllocation[]) : StaffingCommitment[]{
    const commitmentArray: StaffingCommitment[] = [];

    placeholderAllocations.forEach(allocation => {

      const staffingCommitment: StaffingCommitment = {
          placeholderId: allocation.id,
          planningCardId: allocation.planningCardId,
          planningCardName: allocation.planningCardTitle,
          oldCaseCode: allocation.oldCaseCode,
          caseName: allocation.caseName,
          clientName: allocation.clientName,
          caseTypeCode: allocation.caseTypeCode,
          pipelineId: allocation.pipelineId,
          opportunityName: allocation.opportunityName,
          employeeCode: allocation.employeeCode,
          employeeName: allocation.employeeName,
          internetAddress: allocation.internetAddress,
          operatingOfficeCode: allocation.operatingOfficeCode,
          operatingOfficeAbbreviation: allocation.operatingOfficeAbbreviation,
          currentLevelGrade: allocation.currentLevelGrade,
          serviceLineCode: allocation.serviceLineCode,
          serviceLineName: allocation.serviceLineName,
          allocation: allocation.allocation,
          startDate: allocation.startDate,
          endDate: allocation.endDate,
          lastUpdatedBy: allocation.lastUpdatedBy,
          notes: allocation.notes,
          type: allocation.oldCaseCode || allocation.pipelineId ? ConstantsMaster.CommitmentType.NamedPlaceholder  : ConstantsMaster.CommitmentType.PlanningCard,
          commitmentTypeCode: allocation.oldCaseCode || allocation.pipelineId ? CommitmentTypeCodeEnum.NAMED_PLACEHOLDER : CommitmentTypeCodeEnum.PLANNING_CARD,
          commitmentTypeName: allocation.oldCaseCode || allocation.pipelineId ? ConstantsMaster.CommitmentType.NamedPlaceholder  : ConstantsMaster.CommitmentType.PlanningCard,
          // commitmentType: allocation.oldCaseCode || allocation.pipelineId ? ConstantsMaster.CommitmentType.NamedPlaceholder  : ConstantsMaster.CommitmentType.PlanningCard,
          commitmentTypeReasonCode: allocation?.commitmentTypeReasonCode || null,
          commitmentTypeReasonName: allocation?.commitmentTypeReasonName || null,
          isEditable: allocation.planningCardId ? true: false,
          investmentCode: null,
          investmentName: null,
          caseRoleCode: null,
          caseStartDate: null,
          caseEndDate: null,
          opportunityStartDate: null,
          opportunityEndDate: null,
          caseRoleName: null,
          billCode: null,
          description: null,
          fte: null,
          lastUpdated: null,
          operatingOffice: null,
          operatingOfficeCurrent: null,
          operatingOfficeProposed: null,
          position: null,
          probabilityPercent: allocation.probabilityPercent,
          includeInCapacityReporting: allocation.includeInCapacityReporting,
      };

      commitmentArray.push(staffingCommitment);
    });

    return commitmentArray;
  }

  convertAllCommitmentsCreatedinBOSSToStaffingCommitmentModel(commitmentssavedinBOSS: CommitmentView[], selectedCommitmentTypes) : StaffingCommitment[]{
    const commitmentArray: StaffingCommitment[] = [];

    commitmentssavedinBOSS
    .filter(item => selectedCommitmentTypes.includes(item.commitmentTypeCode))
    .forEach(commitment => {
      const commitmentTypeName =
        this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes)
            .find(x => x.commitmentTypeCode === commitment.commitmentTypeCode).commitmentTypeName;

      const staffingCommitment: StaffingCommitment = {} as StaffingCommitment;

      staffingCommitment.id = commitment.id;
      staffingCommitment.employeeCode = commitment.employeeCode;
      staffingCommitment.startDate = commitment.startDate;
      staffingCommitment.endDate = commitment.endDate;
      staffingCommitment.allocation = commitment.allocation;
      staffingCommitment.description = commitment.description;
      staffingCommitment.type = commitmentTypeName;
      staffingCommitment.commitmentTypeCode = commitment.commitmentTypeCode;
      staffingCommitment.commitmentTypeName = commitmentTypeName;
      staffingCommitment.commitmentTypeReasonCode = commitment.commitmentTypeReasonCode;
      staffingCommitment.commitmentTypeReasonName = commitment.commitmentTypeReasonName;
      // staffingCommitment.commitmentType = commitmentTypeName;
      staffingCommitment.isEditable = commitment.isSourceStaffing;
      commitmentArray.push(staffingCommitment);
    });

    return commitmentArray;
  }

  convertLOAsFromSourceToStaffingCommitmentModel(loAs: ResourceLoA[]) : StaffingCommitment[]{

    const commitmentArray: StaffingCommitment[] = [];

    //LOA from Workday
    loAs.forEach(loa => {
      const staffingCommitment: StaffingCommitment = {} as StaffingCommitment;

      staffingCommitment.employeeCode = loa.employeeCode;
      staffingCommitment.employeeName = loa.employeeName;
      staffingCommitment.startDate = loa.startDate;
      staffingCommitment.endDate = loa.endDate;
      staffingCommitment.description = loa.description;
      staffingCommitment.status = loa.status;
      staffingCommitment.type = loa.type;
      staffingCommitment.commitmentTypeCode = CommitmentTypeCodeEnum.LOA;
      staffingCommitment.commitmentTypeName = ConstantsMaster.CommitmentType.Loa;
      // staffingCommitment.commitmentType = ConstantsMaster.CommitmentType.Loa;
      staffingCommitment.isEditable = false;

      commitmentArray.push(staffingCommitment);
    });

    return commitmentArray;
  }

  convertVacationsFromSourceToStaffingCommitmentModel(vacationsFromVRS: Vacation[], timeOffsFromWorkday : ResourceTimeOff[]) : StaffingCommitment[]{

    const commitmentArray: StaffingCommitment[] = [];

    //vacation created in VRS
    vacationsFromVRS.forEach(vacation => {
      const staffingCommitment: StaffingCommitment = {} as StaffingCommitment;

      staffingCommitment.employeeCode = vacation.employeeCode;
      staffingCommitment.startDate = vacation.startDate;
      staffingCommitment.endDate = vacation.endDate;
      staffingCommitment.description = vacation.description;
      staffingCommitment.status = vacation.status;
      staffingCommitment.type = vacation.type;
      staffingCommitment.commitmentTypeCode = CommitmentTypeCodeEnum.VACATION;
      staffingCommitment.commitmentTypeName = ConstantsMaster.CommitmentType.Vacation;
      // staffingCommitment.commitmentType = ConstantsMaster.CommitmentType.Vacation;
      staffingCommitment.isEditable = false;

      commitmentArray.push(staffingCommitment);
    });

    //vacation created in Workday
    timeOffsFromWorkday.forEach(timeOff => {
      const staffingCommitment: StaffingCommitment = {} as StaffingCommitment;

      staffingCommitment.employeeCode = timeOff.employeeCode;
      staffingCommitment.startDate = timeOff.startDate;
      staffingCommitment.endDate = timeOff.endDate;
      staffingCommitment.description = '';
      staffingCommitment.status = timeOff.status;
      staffingCommitment.type = timeOff.type;
      staffingCommitment.commitmentTypeCode = CommitmentTypeCodeEnum.VACATION;
      staffingCommitment.commitmentTypeName = ConstantsMaster.CommitmentType.Vacation;
      staffingCommitment.approvedBy = timeOff.approvedBy;
      // staffingCommitment.commitmentType = ConstantsMaster.CommitmentType.Vacation;
      staffingCommitment.isEditable = false;

      commitmentArray.push(staffingCommitment);
    });

    return commitmentArray;
  }

  convertTrainingsFromSourceToStaffingCommitmentModel(trainings : Training[]) : StaffingCommitment[]{

    const commitmentArray: StaffingCommitment[] = [];

    //Trainings created in BVU
    trainings.forEach(training => {

      const staffingCommitment: StaffingCommitment = {} as StaffingCommitment;

      staffingCommitment.employeeCode = training.employeeCode;
      staffingCommitment.role = training.role;
      staffingCommitment.trainingName = training.trainingName;
      staffingCommitment.description = training.trainingName;
      staffingCommitment.startDate = training.startDate;
      staffingCommitment.endDate = training.endDate;
      staffingCommitment.type = training.type;
      staffingCommitment.commitmentTypeCode = CommitmentTypeCodeEnum.TRAINING;
      staffingCommitment.commitmentTypeName = ConstantsMaster.CommitmentType.Training;
      // staffingCommitment.commitmentType = ConstantsMaster.CommitmentType.Training;
      staffingCommitment.isEditable = false;

      commitmentArray.push(staffingCommitment);
    });

    return commitmentArray;
  }

  convertHolidaysFromSourceToStaffingCommitmentModel(holidays: Holiday[]) : StaffingCommitment[]{

    const commitmentArray: StaffingCommitment[] = [];

    //Holidays created in Basis
    holidays.forEach(holiday => {
      const staffingCommitment: StaffingCommitment = {} as StaffingCommitment;

      staffingCommitment.employeeCode = holiday.employeeCode;
      staffingCommitment.startDate = holiday.startDate;
      staffingCommitment.endDate = holiday.endDate;
      staffingCommitment.description = holiday.description;
      staffingCommitment.status = '';
      staffingCommitment.type = holiday.type;
      staffingCommitment.commitmentTypeCode = CommitmentTypeCodeEnum.HOLIDAY;
      staffingCommitment.commitmentTypeName = ConstantsMaster.CommitmentType.Holiday;
      // staffingCommitment.commitmentType = ConstantsMaster.CommitmentType.Holiday;
      staffingCommitment.isEditable = false;

      commitmentArray.push(staffingCommitment);
    });

    return commitmentArray;
  }

}
