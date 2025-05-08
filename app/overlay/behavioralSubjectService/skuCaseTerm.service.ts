// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { MatDialogRef } from '@angular/material/dialog';
import { takeUntil } from 'rxjs/operators';

// ----------------------- component References ----------------------------------//
import { ResourceOverlayComponent } from '../resource-overlay/resource-overlay.component';
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';

// ----------------------- Service References ----------------------------------//
import { OverlayService } from '../overlay.service';
import { ResourceAssignmentService } from './resourceAssignment.service';
import { OverlayMessageService } from './overlayMessage.service';
import { NotificationService } from 'src/app/shared/notification.service';

@Injectable({ providedIn: 'root' })
export class SkuCaseTermService {
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

  insertSKUCaseTerms(skuTabData: any, resourceDialogRef, caseDialogRef) {
    this.resourceDialogRef = resourceDialogRef;
    this.caseDialogRef = caseDialogRef;
    if (skuTabData.skuTab) {
      this.overlayService.insertSKUCaseTerms(skuTabData.skuTab).pipe(takeUntil(this.destroy$)).subscribe(
        data => {
          skuTabData.skuTab.id = data.id;
          this.notifyService.showSuccess('SKU Term added successfully');
          this.overlayMessageService.triggerCaseAndOpportunityRefresh();
          if (this.caseDialogRef != null) {
            const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
            this.caseDialogRef.componentInstance.getProjectDetails(projectData);
          }
          if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
            this.resourceDialogRef.componentInstance.getDetailsForResource(this.resourceDialogRef.componentInstance.data.employeeCode);
          }
        },
        error => {
          this.notifyService.showError('Error while adding SKU term');
        }
      );
    }
  }

  updateSKUCaseTerms(skuTabData: any, resourceDialogRef, caseDialogRef) {
    this.resourceDialogRef = resourceDialogRef;
    this.caseDialogRef = caseDialogRef;
    if (skuTabData) {
      this.overlayService.updateSKUCaseTerms(skuTabData.skuTab).pipe(takeUntil(this.destroy$)).subscribe(
        data => {
          this.notifyService.showSuccess('SKU Term updated successfully');
          this.overlayMessageService.triggerCaseAndOpportunityRefresh();
          if (this.caseDialogRef != null) {
            const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
            this.caseDialogRef.componentInstance.getProjectDetails(projectData);
          }
          if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
            this.resourceDialogRef.componentInstance.getDetailsForResource(this.resourceDialogRef.componentInstance.data.employeeCode);
          }
        },
        error => {
          this.notifyService.showError('Error while updated SKU term');
        }
      );
    }
  }

  deleteSKUCaseTerms(skuTab: any, resourceDialogRef, caseDialogRef) {
    this.resourceDialogRef = resourceDialogRef;
    this.caseDialogRef = caseDialogRef;
    if (skuTab) {
      this.overlayService.deleteSKUCaseTerms(skuTab.id).pipe(takeUntil(this.destroy$)).subscribe(
        data => {
          this.notifyService.showSuccess('SKU Tab deleted');
          this.overlayMessageService.triggerCaseAndOpportunityRefresh();
          if (this.caseDialogRef != null) {
            const projectData = this.caseDialogRef.componentInstance.project.projectDetails;
            this.caseDialogRef.componentInstance.getProjectDetails(projectData);
          }
          if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
            this.resourceDialogRef.componentInstance.getDetailsForResource(this.resourceDialogRef.componentInstance.data.employeeCode);
          }
        },
        error => {
          this.notifyService.showError('Error while deleted SKU tab');
        }
      );
    }
  }

}
