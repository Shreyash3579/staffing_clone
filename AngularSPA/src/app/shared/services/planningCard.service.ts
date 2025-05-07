import { Injectable } from '@angular/core';
import { ResourceAllocationService } from './resourceAllocation.service';
import { NotificationService } from '../notification.service';
import { DateService } from '../dateService';
import { ValidationService } from '../validationService';
import { ResourceAllocation } from '../interfaces/resourceAllocation.interface';
import { ConstantsMaster } from '../constants/constantsMaster';
import { PlaceholderAllocation } from '../interfaces/placeholderAllocation.interface';
import { PlanningCard } from '../interfaces/planningCard.interface';

import { Store } from '@ngrx/store';
import * as ProjectAllocationsActions from '../../state/actions/project-allocations.action';
import * as fromProjectAllocationsStore from '../../state/reducers/project-allocations.reducer';
import * as fromPlanningCardStore from 'src/app/state/reducers/planning-card-overlay.reducer';
import * as PlanningCardOverlayActions from 'src/app/state/actions/planning-card-overlay.action';

@Injectable({
  providedIn: 'root'
})
export class PlanningCardService {
  constructor(
    private projectAllocationsStore: Store<fromProjectAllocationsStore.State>,
    private _resourceAllocationService: ResourceAllocationService,
    private _notifyService: NotificationService,
    private planningCardStore: Store<fromPlanningCardStore.State>) { }

    
  showMoreThanYearWarning = false;

  mergePlanningcardToCaseOppEmitterHandler(event) {
    const project = event.project;
    const planningCard = event.planningCard;

    if (planningCard.placeholderAllocations?.length === 0 && planningCard.regularAllocations?.length === 0) {
      this._notifyService.showWarning(`Allocate resources in order to merge planning card`);
      return true;
    }
    const today = new Date().toLocaleDateString('en-US');
    const startDate = Date.parse(project.startDate) > Date.parse(today)
      ? project.startDate
      : Date.parse(project.endDate) < Date.parse(today)
        ? project.startDate
        : today;

    /*
      * NOTE: We are calculating opportunityEndDate if a resource is allocated to an opportunity that does not any end date or a duration.
      * For an opportunity that is going to start in future,
      * we have set the end date for the allocation as opportunity start date + 30 days.
      *
      * For an opportunuty that has already started, we have set the end date for the allocation as today + 30 days.
      *
      * TODO: Change the logic once Brittany comes up with the solution
    */

    let proposedEndDate = new Date(startDate);
    const defaultAllocationStartDate = new Date(startDate);
    const defaultAllocationEndDate = new Date(project.endDate);
    proposedEndDate.setDate(proposedEndDate.getDate() + 30);

    proposedEndDate = project.endDate !== null
      ? proposedEndDate
      : (!!planningCard.endDate ? new Date(planningCard.endDate) : proposedEndDate);

    const opportunityEndDate = proposedEndDate.toLocaleDateString('en-US');
    const maxEndDate = DateService.getMaxEndDateForAllocation(defaultAllocationStartDate, defaultAllocationEndDate);
    this.showMoreThanYearWarning = ValidationService.checkIfAllocationIsOfOneYear(defaultAllocationStartDate, defaultAllocationEndDate);

    const allocationEndDate = maxEndDate.toLocaleDateString('en-US');

    // if resource being dropped does not have an id that means resource is being dropped from resources list,
    // else its being dropped from one of the cards

    const resourceAllocations: ResourceAllocation[] = planningCard.regularAllocations
      .map(item => {
        return {
          oldCaseCode: project.oldCaseCode,
          caseName: project.caseName,
          clientName: project.clientName,
          pipelineId: project.pipelineId,
          caseTypeCode: project.caseTypeCode,
          opportunityName: project.opportunityName,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          operatingOfficeCode: item.operatingOfficeCode,
          operatingOfficeAbbreviation: item.operatingOfficeAbbreviation,
          currentLevelGrade: item.currentLevelGrade,
          serviceLineCode: item.serviceLineCode,
          serviceLineName: item.serviceLineName,
          allocation: item.allocation,
          startDate: this.getAllocationStartDate(item.startDate, project.startDate),
          endDate: this.getAllocationEndDate(allocationEndDate, opportunityEndDate),
          previousStartDate: null,
          previousEndDate: null,
          previousAllocation: null,
          investmentCode: item.investmentCode,
          investmentName: item.investmentName,
          caseRoleCode: item.caseRoleCode,
          caseStartDate: project.oldCaseCode ? project.startDate : null,
          caseEndDate: project.oldCaseCode ? project.endDate : null,
          opportunityStartDate: !project.oldCaseCode ? project.startDate : null,
          opportunityEndDate: !project.oldCaseCode ? project.endDate : null,
          lastUpdatedBy: null
        };
      });

    let allocationsToUpsert : ResourceAllocation[] ;

    if (resourceAllocations.length > 0) {
      resourceAllocations.forEach(alloc => {
        if (Date.parse(alloc.endDate) < Date.parse(alloc.startDate)) {
          alloc.endDate = alloc.startDate;
        }
      });

      const [isValidAllocation, monthCloseErrorMessage] = this._resourceAllocationService.validateMonthCloseForInsertAndDelete(resourceAllocations);

      if (!isValidAllocation) {
        this._notifyService.showValidationMsg(monthCloseErrorMessage);
        return;
      }

      allocationsToUpsert = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocations)

    }

    if (event.action === ConstantsMaster.PlanningCardMergeActions.CopyAndMerge) {
      //TODO: update this logic and make it better
      if(allocationsToUpsert?.length > 0) {

        this.projectAllocationsStore.dispatch(
            new ProjectAllocationsActions.UpsertResourceAllocations({
              resourceAllocation: allocationsToUpsert,
              showMoreThanYearWarning: this.showMoreThanYearWarning,
              allocationDataBeforeSplitting: resourceAllocations,
            })
          );
      }
      this.copyAndMergePlanningCard(planningCard, project, allocationEndDate, opportunityEndDate);
    } else {
      this.mergeAndUpdatePlanningCard(planningCard, allocationsToUpsert, project, allocationEndDate, opportunityEndDate, resourceAllocations);
    }
  }

  private copyAndMergePlanningCard(planningCard, project, allocationEndDate, opportunityEndDate) {
    const placeholderAllocations: PlaceholderAllocation[] = Object.assign([], planningCard).placeholderAllocations
      .map(item => {
        return {
          id: null,
          planningCardId: null,
          oldCaseCode: project.oldCaseCode,
          caseName: project.caseName,
          clientName: project.clientName,
          caseTypeCode: project.caseTypeCode,
          pipelineId: project.pipelineId,
          opportunityName: project.opportunityName,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          operatingOfficeCode: item.operatingOfficeCode,
          operatingOfficeAbbreviation: item.operatingOfficeAbbreviation,
          currentLevelGrade: item.currentLevelGrade,
          serviceLineCode: item.serviceLineCode,
          serviceLineName: item.serviceLineName,
          allocation: item.allocation,
          startDate: this.getAllocationStartDate(item.startDate, project.startDate),
          endDate: this.getAllocationEndDate(allocationEndDate, opportunityEndDate),
          investmentCode: item.investmentCode,
          investmentName: item.investmentName,
          caseRoleCode: item.caseRoleCode,
          caseStartDate: project.oldCaseCode ? project.startDate : null,
          caseEndDate: project.oldCaseCode ? project.endDate : null,
          opportunityStartDate: !project.oldCaseCode ? project.startDate : null,
          opportunityEndDate: !project.oldCaseCode ? project.endDate : null,
          isPlaceholderAllocation: item.isPlaceholderAllocation,
          positionGroupCode: item.positionGroupCode,
          lastUpdatedBy: null
        };
      });

    placeholderAllocations?.forEach(alloc => {
      if (Date.parse(alloc.endDate) < Date.parse(alloc.startDate)) {
        alloc.endDate = alloc.startDate;
      }
    });
    this.upsertPlaceholderHandler(placeholderAllocations, true, true);
  }

  private mergeAndUpdatePlanningCard(planningCard : PlanningCard, regularAllocationsToUpsert: ResourceAllocation[], project, allocationEndDate, opportunityEndDate, resourceAllocations) {
    const placeholderAllocations: PlaceholderAllocation[] = planningCard.placeholderAllocations
      .map(item => {
        return {
          id: item.id,
          planningCardId: null,
          oldCaseCode: project.oldCaseCode,
          caseTypeCode: project.caseTypeCode,
          caseName: project.caseName,
          clientName: project.clientName,
          pipelineId: project.pipelineId,
          opportunityName: project.opportunityName,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          operatingOfficeCode: item.operatingOfficeCode,
          operatingOfficeAbbreviation: item.operatingOfficeAbbreviation,
          currentLevelGrade: item.currentLevelGrade,
          serviceLineCode: item.serviceLineCode,
          serviceLineName: item.serviceLineName,
          allocation: item.allocation,
          startDate: this.getAllocationStartDate(item.startDate, project.startDate),
          endDate: this.getAllocationEndDate(allocationEndDate, opportunityEndDate),
          investmentCode: item.investmentCode,
          investmentName: item.investmentName,
          caseRoleCode: item.caseRoleCode,
          caseStartDate: project.oldCaseCode ? project.startDate : null,
          caseEndDate: project.oldCaseCode ? project.endDate : null,
          opportunityStartDate: !project.oldCaseCode ? project.startDate : null,
          opportunityEndDate: !project.oldCaseCode ? project.endDate : null,
          isPlaceholderAllocation: item.isPlaceholderAllocation,
          positionGroupCode: item.positionGroupCode,
          lastUpdatedBy: null
        };
      });

    placeholderAllocations?.forEach(alloc => {
      if (Date.parse(alloc.endDate) < Date.parse(alloc.startDate)) {
        alloc.endDate = alloc.startDate;
      }
    });

    //Update  planning card as merged and save case
    planningCard.mergedCaseCode = project.oldCaseCode;
    planningCard.isMerged = true;

    var payload = {
      resourceAllocations: regularAllocationsToUpsert ?? [],
      placeholderAllocations : placeholderAllocations ?? [],
      planningCard: planningCard,
      allocationDataBeforeSplitting: resourceAllocations
    }
    //this.mergePlanningCardAndAllocations.emit(payload);

    this.planningCardStore.dispatch(
      new PlanningCardOverlayActions.MergePlanningCards({
        planningCard : planningCard,
        allocationDataBeforeSplitting: resourceAllocations,
        resourceAllocations: regularAllocationsToUpsert ?? [],
        placeholderAllocations : placeholderAllocations ?? []
      })
    );
  }

  upsertPlaceholderHandler(payload, isMergeFromPlanningCard = false, isCopyAndMerge = false) {
    payload = payload.placeholderAllocation ? payload.placeholderAllocation : payload;

    let placeholderAllocations: PlaceholderAllocation[] = [];
    placeholderAllocations = placeholderAllocations.concat(payload);
    if (placeholderAllocations.length <= 0) {
      return true;
    }
     this.projectAllocationsStore.dispatch(
       new ProjectAllocationsActions.UpsertPlaceholderAllocations({
            isMergeFromPlanningCard: isMergeFromPlanningCard, 
            isCopyAndMerge: isCopyAndMerge,
            placeholderAllocations: placeholderAllocations
         })
     );
  }

  private getAllocationStartDate(allocationStartDate, caseOppStartDate) {
    const today = new Date().toLocaleDateString('en-US');
    const date7DaysAgo = new Date(new Date().setDate(new Date().getDate() - 7)).toLocaleDateString('en-US');
    if (caseOppStartDate === null && allocationStartDate === null) {
      return today;
    }
    if (caseOppStartDate === null && allocationStartDate !== null) {
      return DateService.convertDateInBainFormat(allocationStartDate);
    }
    if (Date.parse(caseOppStartDate) < Date.parse(date7DaysAgo)) {
      return today;
    }
    if (Date.parse(caseOppStartDate) >= Date.parse(date7DaysAgo) && Date.parse(caseOppStartDate) <= Date.parse(today)) {
      return DateService.convertDateInBainFormat(caseOppStartDate);
    }
    if (Date.parse(caseOppStartDate) > Date.parse(today) && allocationStartDate) {
      return DateService.convertDateInBainFormat(allocationStartDate);
    }
    return today;
  }

  private getAllocationEndDate(allocationEndDate, opportunityEndDate) {
    return allocationEndDate !== null
      ? DateService.convertDateInBainFormat(allocationEndDate)
      : DateService.convertDateInBainFormat(opportunityEndDate);
  }
}


