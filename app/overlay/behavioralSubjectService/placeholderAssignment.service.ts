// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
// ----------------------- Service References ----------------------------------//
import { NotificationService } from 'src/app/shared/notification.service';
import { OverlayMessageService } from './overlayMessage.service';
import { OverlayService } from '../overlay.service';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { CaseOppCortexTeamSize } from 'src/app/shared/interfaces/case-opp-cortex-team-size.interface';

@Injectable({ providedIn: 'root' })
export class PlaceholderAssignmentService {
  // -----------------------Local Variables--------------------------------------------//
  private destroy$: Subject<boolean> = new Subject<boolean>();

  // -----------------------Constructor--------------------------------------------//
  constructor(
    private notifyService: NotificationService,
    private overlayMessageService: OverlayMessageService,
    private overlayService: OverlayService) { }

  // -----------------------Helper Function--------------------------------------------//

  deletePlaceHoldersByIds(event, caseDialogRef?) {
    this.overlayService.deletePlaceholdersByIds(event.placeholderIds).pipe(takeUntil(this.destroy$)).subscribe(
      (updatedData) => {
        if (event.notifyMessage) {
          this.notifyService.showSuccess(event.notifyMessage);
        }

        this.overlayMessageService.triggerCaseAndOpportunityRefresh();
        this.overlayMessageService.triggerHighlightedResourcesRefresh(event.placeholderIds)

        if (caseDialogRef && caseDialogRef.componentInstance) {
          const projectData = caseDialogRef.componentInstance.project.projectDetails;
          caseDialogRef.componentInstance.getProjectDetails(projectData);
        }
      }
    );
  }

  upsertPlcaeholderAllocations(allocations: PlaceholderAllocation[], resourceDialogRef, caseDialogRef) {
    this.overlayService.upsertPlaceholderAllocations(allocations).pipe(takeUntil(this.destroy$)).subscribe(
      (updatedData) => {

        this.notifyService.showSuccess(`Placeholder assignment is created/updated/deleted`);

        this.overlayMessageService.triggerCaseAndOpportunityRefresh();
        this.overlayMessageService.triggerCaseAndOpportunityRefreshOnCasePlanning(updatedData);

        if (caseDialogRef && caseDialogRef.componentInstance) {
          const projectData = caseDialogRef.componentInstance.project.projectDetails;
          caseDialogRef.componentInstance.getProjectDetails(projectData);
        }
        // if (resourceDialogRef && resourceDialogRef.componentInstance) {
        //   if (resourceDialogRef.componentInstance.resourceDetails?.resource?.employeeCode) {
        //     const employeeCode = resourceDialogRef.componentInstance.resourceDetails.resource.employeeCode;
        //     resourceDialogRef.componentInstance.getDetailsForResource(employeeCode);
        //   }
        // }
      }
    );
  }

  upsertPlaceholderCreatedForCortexInfo(caseOppCortexTeamSize: CaseOppCortexTeamSize) {
    this.overlayService.upsertPlaceholderCreatedForCortexInfo(caseOppCortexTeamSize).pipe(takeUntil(this.destroy$)).subscribe(
      (updatedData) => { });

  }
}
