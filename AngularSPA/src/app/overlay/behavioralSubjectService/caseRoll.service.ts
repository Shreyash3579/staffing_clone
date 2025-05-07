// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { MatDialogRef } from '@angular/material/dialog';
import { takeUntil } from 'rxjs/operators';

// ----------------------- component References ----------------------------------//
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';

// ----------------------- Service References ----------------------------------//
import { OverlayService } from '../overlay.service';
import { OverlayMessageService } from './overlayMessage.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { CaseRoll } from 'src/app/shared/interfaces/caseRoll.interface';
import { ResourceAllocation } from '../../shared/interfaces/resourceAllocation.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { SharedService } from 'src/app/shared/shared.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ResourceAssignmentService } from './resourceAssignment.service';

@Injectable()
export class CaseRollService {
    // -----------------------Local Variables--------------------------------------------//
    private destroy$: Subject<boolean> = new Subject<boolean>();
    private caseDialogRef: MatDialogRef<ProjectOverlayComponent, any>;

    // -----------------------Constructor--------------------------------------------//
    constructor(
        private notifyService: NotificationService,
        private overlayMessageService: OverlayMessageService,
        private overlayService: OverlayService,
        private sharedService: SharedService,
        private resourceAssignmentService: ResourceAssignmentService) { }

    // -----------------------Helper Function--------------------------------------------//

    upsertCaseRollAndAllocationsHandler(caseRoll: CaseRoll, resourceAllocations: ResourceAllocation[], caseDialogRef,
         project?: Project, allocationDataBeforeSplitting?: ResourceAllocation[]) {
        this.caseDialogRef = caseDialogRef;
        this.overlayMessageService.triggerProjectRefreshForCaseRoll(resourceAllocations, project, caseRoll);

        const caseRollArray : CaseRoll[] = [];
        caseRollArray.push(caseRoll);

        this.overlayService.upsertCaseRollsAndAllocations(caseRollArray, resourceAllocations)
            .pipe(takeUntil(this.destroy$)).subscribe(
                () => {

                    this.notifyService.showSuccess('Case Rolled Successfully');
                    if (this.caseDialogRef?.componentInstance) {
                        const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
                        this.caseDialogRef.componentInstance.getProjectDetails(projectData);
                    }
                    if(allocationDataBeforeSplitting) {
                        if (allocationDataBeforeSplitting.every((r)=> r.oldCaseCode))
                        {
                            this.sharedService.checkPegRingfenceAllocationAndInsertDownDayCommitments(allocationDataBeforeSplitting).subscribe(commitments => 
                            {
                                if(commitments?.length > 0)
                                {
                                    this.notifyService.showSuccess(ConstantsMaster.Messages.DownDaySaved);
                                }
                            });
                        }
                    }
                
                    this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(resourceAllocations);
                    this.overlayMessageService.triggerResourceRefresh();
                    this.overlayMessageService.triggerCaseAndOpportunityRefresh();
                },
                () => {
                    this.notifyService.showError('Error while rolling case');
                }
            );
    }

    upsertCaseRollAndPlaceholderAllocationsHandler(caseRoll: CaseRoll, resourceAllocations: ResourceAllocation[], caseDialogRef,
        project?: Project) {
       this.caseDialogRef = caseDialogRef;
       this.overlayMessageService.triggerProjectRefreshForCaseRoll(resourceAllocations, project, caseRoll);

       const caseRollArray : CaseRoll[] = [];
       caseRollArray.push(caseRoll);

       this.overlayService.upsertCaseRollsAndPlaceholderAllocations(caseRollArray, resourceAllocations)
           .pipe(takeUntil(this.destroy$)).subscribe(
               () => {

                   this.notifyService.showSuccess('Case Rolled Successfully');
                   if (this.caseDialogRef?.componentInstance) {
                       const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
                       this.caseDialogRef.componentInstance.getProjectDetails(projectData);
                   }
                   this.overlayMessageService.triggerResourceRefresh();
                   this.overlayMessageService.triggerCaseAndOpportunityRefresh();
                   this.overlayMessageService.triggerPlanningCardRefresh();
               },
               () => {
                   this.notifyService.showError('Error while rolling case');
               }
           );
   }

    revertCaseRollAndAllocationsHandler(caseRoll: CaseRoll, resourceAllocations: ResourceAllocation[], caseDialogRef, project?: Project) {
        this.caseDialogRef = caseDialogRef;
        this.overlayMessageService.triggerProjectRefreshForCaseRoll(resourceAllocations, project, caseRoll);

        this.overlayService.revertCaseRollAndAllocations(caseRoll, resourceAllocations)
            .pipe(takeUntil(this.destroy$)).subscribe(
                () => {

                    this.notifyService.showSuccess('Case Roll reverted Successfully');
                    if (this.caseDialogRef?.componentInstance) {
                        const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
                        this.caseDialogRef.componentInstance.getProjectDetails(projectData);
                    }

                    this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(resourceAllocations);          
                    this.overlayMessageService.triggerResourceRefresh();
                    this.overlayMessageService.triggerCaseAndOpportunityRefresh();
                },
                () => {
                    this.notifyService.showError('Error while reverting case roll');
                }
            );
    }

}
