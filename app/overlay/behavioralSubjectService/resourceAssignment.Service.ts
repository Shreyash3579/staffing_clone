// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

// ----------------------- component References ----------------------------------//
import { ResourceOverlayComponent } from '../resource-overlay/resource-overlay.component';
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';

// ----------------------- Service References ----------------------------------//
import { OverlayService } from '../overlay.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { OverlayMessageService } from './overlayMessage.service';
import { SharedService } from 'src/app/shared/shared.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { UserPlaygroundSession } from 'src/app/shared/interfaces/userPlaygroundSession';
import { CasePlanningWhiteboardService } from 'src/app/case-planning-whiteboard/case-planning-whiteboard.service';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { CasePlanningBoardPlaygroundAllocation } from 'src/app/shared/interfaces/case-planning-board-playground-allocation.interface';
import { DateService } from 'src/app/shared/dateService';
// --------------------------Utils -----------------------------------------//

@Injectable({ providedIn: 'root' })
export class ResourceAssignmentService {
  // -----------------------Local Variables--------------------------------------------//
  private destroy$: Subject<boolean> = new Subject<boolean>();
  private resourceDialogRef: MatDialogRef<ResourceOverlayComponent, any>;
  private caseDialogRef: MatDialogRef<ProjectOverlayComponent, any>; 

  // -----------------------Constructor--------------------------------------------//
  constructor(
    private notifyService: NotificationService,
    private overlayMessageService: OverlayMessageService,
    private overlayService: OverlayService, 
    private sharedService: SharedService,
    private planningBoardService: CasePlanningWhiteboardService,
    private localStorageService: LocalStorageService
    ) { }

  // -----------------------Helper Function--------------------------------------------//
  updateResourceAssignmentToCase(resource, resourceDialogRef, caseDialogRef) {
    this.resourceDialogRef = resourceDialogRef;
    this.caseDialogRef = caseDialogRef;
    const updatedResource = resource.resourceAllocation;
    this.overlayService.updateResourceAssignmentToCase(updatedResource).pipe(takeUntil(this.destroy$)).subscribe(
      updateData => {
        if (updatedResource.employeeName !== undefined && updatedResource.employeeName != null) {
          this.notifyService.showSuccess('Resource Updated - ' + updatedResource.employeeName);
        } else {
          this.notifyService.showSuccess('Resource Data Updated');
        }

        this.overlayMessageService.triggerResourceRefresh();

        if (resource.event !== 'dragdrop' && resource.event !== 'editResource') {
          this.overlayMessageService.triggerCaseAndOpportunityRefresh();

        }
        if (this.caseDialogRef && this.caseDialogRef.componentInstance) {
          const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
          this.caseDialogRef.componentInstance.getProjectDetails(projectData);
        }
        if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
          this.resourceDialogRef.componentInstance.getDetailsForResource(this.resourceDialogRef.componentInstance.data.employeeCode);
        }
      },
      error => {
        // TODO: Assign Employee back to resource view
        this.notifyService.showError('Error while updating resource - ' + updatedResource.employeeName);
      }
    );
  }

  upsertResourceAllocationsToProject(resource, resourceDialogRef, caseDialogRef, lastUpdatedBy = null) {
    this.resourceDialogRef = resourceDialogRef;
    this.caseDialogRef = caseDialogRef;
    let addedResourceAsArray = [];
    const addedResource = resource.resourceAllocation;
    if (Array.isArray(addedResource)) {
      addedResourceAsArray = addedResource;
    } else {
      addedResourceAsArray.push(addedResource);
    }
    this.overlayService.upsertResourceAllocations(addedResourceAsArray, lastUpdatedBy).pipe(takeUntil(this.destroy$)).subscribe(
      (upsertedData) => {
        if (resource.splitSuccessMessage) {
          this.notifyService.showStickySuccess(resource.splitSuccessMessage);
        } else if (resource.showMoreThanYearWarning) {
          this.notifyService.showWarning('Assignment Saved for one month.');
        } else {
          this.notifyService.showSuccess('Assignment Saved');
        }
        this.overlayMessageService.triggerResourceRefresh();

        if (resource.event !== 'dragdrop' && resource.event !== 'editResource') {
          this.overlayMessageService.triggerCaseAndOpportunityRefresh();

        } else {
          this.overlayMessageService.reosurceAssignmentToCase(upsertedData);
        }
        if (this.caseDialogRef && this.caseDialogRef.componentInstance) {
          const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
          this.caseDialogRef.componentInstance.getProjectDetails(projectData);
        }
        if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
          if (this.resourceDialogRef.componentInstance.resourceDetails?.resource?.employeeCode) {
            const employeeCode = this.resourceDialogRef.componentInstance.resourceDetails.resource.employeeCode;
            this.resourceDialogRef.componentInstance.getDetailsForResource(employeeCode);
          }
        }
        
      if(resource?.allocationDataBeforeSplitting != null) {
        let allocationData = resource.allocationDataBeforeSplitting;
        if (allocationData.every((r)=> r.oldCaseCode))
        {
          this.sharedService.checkPegRingfenceAllocationAndInsertDownDayCommitments(allocationData).subscribe(commitments => {
          if(commitments?.length > 0)
          {
            this.notifyService.showSuccess(ConstantsMaster.Messages.DownDaySaved);
          }
        });
      }
    }   
      },
      error => {
        this.notifyService.showError('Error while saving assignment for ' + addedResourceAsArray[0].employeeName);
      }
    );
  }

  public upsertPlaygroundAllocationsForCasePlanningMetrics(resourceAllocations: ResourceAllocation[]) { 
   const userPlaygroundSessionInfo = this.localStorageService.get(ConstantsMaster.localStorageKeys.userPlaygroundSession);
    if (userPlaygroundSessionInfo?.playgroundId) {
      const playgroundAllocations: CasePlanningBoardPlaygroundAllocation[] = [];

      resourceAllocations.forEach((selectedResource) => {
        const casePlanningBoardPlaygroundAllocation: CasePlanningBoardPlaygroundAllocation = {
          employeeCode: selectedResource.employeeCode,
          newStartDate: DateService.convertDateInBainFormat(selectedResource.startDate),
          newEndDate: DateService.convertDateInBainFormat(selectedResource.endDate),
          newAllocation: selectedResource.allocation,
          newInvestmentCode: selectedResource.investmentCode,
          previousStartDate: DateService.convertDateInBainFormat(selectedResource.previousStartDate),
          previousEndDate: DateService.convertDateInBainFormat(selectedResource.previousEndDate),
          previousAllocation: selectedResource.previousAllocation,
          previousInvestmentCode: selectedResource.previousInvestmentCode,
          isOpportunity: !!selectedResource.pipelineId && !selectedResource.oldCaseCode,
          caseEndDate: DateService.convertDateInBainFormat(selectedResource.caseEndDate)
        };

        playgroundAllocations.push(casePlanningBoardPlaygroundAllocation);
      });

      this.planningBoardService.upsertCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations(userPlaygroundSessionInfo.playgroundId, playgroundAllocations)
      .subscribe((result) => 
      {
        console.log(result); 
      });
    }
  }

  public deletePlaygroundAllocationsForCasePlanningMetrics(resourceAllocations: ResourceAllocation[]) { 
    const userPlaygroundSessionInfo = this.localStorageService.get(ConstantsMaster.localStorageKeys.userPlaygroundSession);
    if (userPlaygroundSessionInfo?.playgroundId) {
      const playgroundAllocations: CasePlanningBoardPlaygroundAllocation[] = [];

      resourceAllocations.forEach((selectedResource) => {
        const casePlanningBoardPlaygroundAllocation: CasePlanningBoardPlaygroundAllocation = {
          employeeCode: selectedResource.employeeCode,
          newStartDate: null,
          newEndDate: null,
          newAllocation: null,
          newInvestmentCode: null,
          previousStartDate: DateService.convertDateInBainFormat(selectedResource.startDate),
          previousEndDate: DateService.convertDateInBainFormat(selectedResource.endDate),
          previousAllocation: selectedResource.allocation,
          previousInvestmentCode: selectedResource.previousInvestmentCode,
          isOpportunity: !!selectedResource.pipelineId && !selectedResource.oldCaseCode,
          caseEndDate: DateService.convertDateInBainFormat(selectedResource.caseEndDate)
        };

        playgroundAllocations.push(casePlanningBoardPlaygroundAllocation);
      });

      this.planningBoardService.upsertCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations(userPlaygroundSessionInfo.playgroundId, playgroundAllocations)
      .subscribe((result) => 
      {
        console.log(result); 
      });
    }
  }

  deleteResourceAssignmentFromCase(allocationId, resourceDialogRef?, caseDialogRef?) {
    this.resourceDialogRef = resourceDialogRef;
    this.caseDialogRef = caseDialogRef;
    this.overlayService.deleteResourceAssignmentFromProject(allocationId).pipe(takeUntil(this.destroy$)).subscribe(
      () => {
        this.notifyService.showSuccess('Resource Deleted');
        this.overlayMessageService.triggerResourceRefresh();
        this.overlayMessageService.triggerHighlightedResourcesRefresh(allocationId);

        // need this check as sometimes after making change in case or resource pop-up if
        // response is delayed and user closes the pop-up in the meanwhile
        // then dialogRef's component instance becomes null
        if (this.resourceDialogRef || this.caseDialogRef) {
          this.overlayMessageService.triggerCaseAndOpportunityRefresh();
        }

        if (this.caseDialogRef && this.caseDialogRef.componentInstance) {
          const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
          this.caseDialogRef.componentInstance.getProjectDetails(projectData);
        }
        if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
          const employeeCode = this.resourceDialogRef.componentInstance.data.employeeCode;
          this.resourceDialogRef.componentInstance.getDetailsForResource(employeeCode);
        }

      },
      () => {
        // console.log('Error while deleting Resource!!');
        this.notifyService.showError('Error while deleting resource');
      }
    );
  }

  deleteResourcesAssignmentsFromCase(allocationIds, resourceDialogRef?, caseDialogRef?, lastUpdatedBy = null) {
    this.resourceDialogRef = resourceDialogRef;
    this.caseDialogRef = caseDialogRef;
    this.overlayService.deleteResourcesAssignmentsFromProject(allocationIds, lastUpdatedBy).pipe(takeUntil(this.destroy$)).subscribe(
      () => {
        this.notifyService.showSuccess('Resources Deleted');
        this.overlayMessageService.triggerResourceRefresh();

        // need this check as sometimes after making change in case or resource pop-up if
        // response is delayed and user closes the pop-up in the meanwhile
        // then dialogRef's component instance becomes null
        if (this.resourceDialogRef || this.caseDialogRef) {
          this.overlayMessageService.triggerCaseAndOpportunityRefresh();
        }

        if (this.caseDialogRef && this.caseDialogRef.componentInstance) {
          const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
          this.caseDialogRef.componentInstance.getProjectDetails(projectData);
        }
        if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
          const employeeCode = this.resourceDialogRef.componentInstance.data.employeeCode;
          this.resourceDialogRef.componentInstance.getDetailsForResource(employeeCode);
        }

      },
      () => {
        // console.log('Error while deleting Resource!!');
        this.notifyService.showError('Error while deleting resource');
      }
    );
  }

  deleteResourcesAllocationsCommitments(dataToDelete, resourceDialogRef?, caseDialogRef?) {
    this.resourceDialogRef = resourceDialogRef;
    this.caseDialogRef = caseDialogRef;
    this.overlayService.deleteResourcesAllocationsCommitments(dataToDelete).pipe(takeUntil(this.destroy$)).subscribe(
      () => {
        this.notifyService.showSuccess('Allocations/Commitments Deleted');
        this.overlayMessageService.triggerResourceRefresh();

        // need this check as sometimes after making change in case or resource pop-up if
        // response is delayed and user closes the pop-up in the meanwhile
        // then dialogRef's component instance becomes null
        if (this.resourceDialogRef || this.caseDialogRef) {
          this.overlayMessageService.triggerCaseAndOpportunityRefresh();
        }

        if (this.caseDialogRef && this.caseDialogRef.componentInstance) {
          const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
          this.caseDialogRef.componentInstance.getProjectDetails(projectData);
        }
        if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
          const employeeCode = this.resourceDialogRef.componentInstance.data.employeeCode;
          this.resourceDialogRef.componentInstance.getDetailsForResource(employeeCode);
        }

      },
      () => {
        // console.log('Error while deleting Resource!!');
        this.notifyService.showError('Error while deleting resource');
      }
    );
  }

}
