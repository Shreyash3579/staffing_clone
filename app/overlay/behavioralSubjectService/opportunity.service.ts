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

@Injectable({ providedIn: 'root' })
export class OpportunityService {
  // -----------------------Local Variables--------------------------------------------//
  private destroy$: Subject<boolean> = new Subject<boolean>();
  private caseDialogRef: MatDialogRef<ProjectOverlayComponent, any>;

  // -----------------------Constructor--------------------------------------------//
  constructor(
    private notifyService: NotificationService,
    private overlayMessageService: OverlayMessageService,
    private overlayService: OverlayService) { }

  // -----------------------Helper Function--------------------------------------------//

  updateProjectChangesHandler(updatedProjectData, caseDialogRef = null) {
    this.caseDialogRef = caseDialogRef;
    if (updatedProjectData.pipelineId) {
      this.overlayService.updateOppChanges(updatedProjectData).pipe(takeUntil(this.destroy$)).subscribe(
        data => {
          this.notifyService.showSuccess('Pipeline data updated successfully');
          this.overlayMessageService.triggerCaseAndOpportunityRefresh();
          if (this.caseDialogRef != null) {
            const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
            this.caseDialogRef.componentInstance.getProjectDetails(projectData);
          }
        },
        error => {
          this.notifyService.showError('Error while updating pipeline data');
        }
      );
    } else if (updatedProjectData.oldCaseCode) {
      this.overlayService.updateCaseChanges(updatedProjectData).pipe(takeUntil(this.destroy$)).subscribe(
        data => {
          this.notifyService.showSuccess('Case data updated successfully');
        },
        error => {
          this.notifyService.showError('Error while updating Case data');
        }
      );
    }
  }
}
