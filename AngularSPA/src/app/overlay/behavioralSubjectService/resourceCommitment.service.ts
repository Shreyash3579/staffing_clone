// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

// ----------------------- component References ----------------------------------//
import { ResourceOverlayComponent } from '../resource-overlay/resource-overlay.component';
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';

// ----------------------- Service References ----------------------------------//
import { OverlayService } from '../overlay.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { OverlayMessageService } from './overlayMessage.service';
import { ResourceAssignmentService } from './resourceAssignment.service';

@Injectable({ providedIn: 'root' })
export class ResourceCommitmentService {
  // -----------------------Local Variables--------------------------------------------//
  private destroy$: Subject<boolean> = new Subject<boolean>();
  private resourceDialogRef: MatDialogRef<ResourceOverlayComponent, any>;
  private caseDialogRef: MatDialogRef<ProjectOverlayComponent, any>;

  // -----------------------Constructor--------------------------------------------//
  constructor(
    private notifyService: NotificationService,
    private overlayMessageService: OverlayMessageService,
    private resourceAssignmentService: ResourceAssignmentService,
    private overlayService: OverlayService) { }

  // -----------------------Helper Function--------------------------------------------//
  insertResourcesCommitments(commitments, resourceDialogRef) {
    this.resourceDialogRef = resourceDialogRef;
    this.overlayService.insertResourcesCommitments(commitments).pipe(takeUntil(this.destroy$)).subscribe(
      addedData => {
        this.notifyService.showSuccess('Commitment Added Successfully');
        this.overlayMessageService.triggerResourceRefresh();
        if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
          const employeeCode = commitments[0].employeeCode;
          this.resourceDialogRef.componentInstance.getDetailsForResource(employeeCode);
        }
      },
      error => {
        this.notifyService.showError('Error while adding commitment');
      }
    );
  }

  updateResourceCommitment(updatedResource, resourceDialogRef, caseDialogRef) {
    this.resourceDialogRef = resourceDialogRef;
    this.caseDialogRef = caseDialogRef;
    const updatedCommitment = updatedResource.resourceAllocation;
    if (!updatedCommitment.commitmentType) {
      this.resourceAssignmentService.updateResourceAssignmentToCase(updatedResource, this.resourceDialogRef, this.caseDialogRef);
      return true;
    }
    this.overlayService.insertResourcesCommitments([updatedCommitment]).pipe(takeUntil(this.destroy$)).subscribe(
      updatedData => {
        this.notifyService.showSuccess('Commitment Updated');
        this.overlayMessageService.triggerResourceRefresh();
        if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
          const employeeCode = updatedCommitment.employeeCode;
          this.resourceDialogRef.componentInstance.getDetailsForResource(employeeCode);
        }
      },
      error => {
        // TODO: Assign Employee back to resource view
        this.notifyService.showError('Error while updating commitment');
      }
    );
  }

  deleteResourceCommitment(commitmentId, resourceDialogRef) {
    this.resourceDialogRef = resourceDialogRef;
    this.overlayService.deleteResourceCommitments(commitmentId).pipe(takeUntil(this.destroy$)).subscribe(
      () => {
        this.notifyService.showSuccess('Commitment Deleted');
        this.overlayMessageService.triggerResourceRefresh();

        if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
          const employeeCode = this.resourceDialogRef.componentInstance.data.employeeCode;
          this.resourceDialogRef.componentInstance.getDetailsForResource(employeeCode);
        }
      },
      () => {
        this.notifyService.showError('Error while deleting commitment');
      }
    );
  }

}
