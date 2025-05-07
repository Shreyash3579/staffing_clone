import { Injectable } from "@angular/core";
import { MatDialogRef } from "@angular/material/dialog";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import { StaffableAsRole } from "src/app/shared/interfaces/staffableAsRole.interface";
import { NotificationService } from "src/app/shared/notification.service";
import { OverlayService } from "../overlay.service";
import { ResourceOverlayComponent } from "../resource-overlay/resource-overlay.component";
import { OverlayMessageService } from "./overlayMessage.service";


@Injectable({ providedIn: 'root' })
export class ResourceStaffableAsService {
    private destroy$: Subject<boolean> = new Subject<boolean>();
    private resourceDialogRef: MatDialogRef<ResourceOverlayComponent, any>;

    constructor(
        private notifyService: NotificationService,
        private overlayMessageService: OverlayMessageService,
        private overlayService: OverlayService) { }

    deleteStaffableAsRole(event, resourceDialogRef) {
        this.resourceDialogRef = resourceDialogRef;
        this.overlayService.deleteResourceStaffableAsById(event).pipe(takeUntil(this.destroy$)).subscribe(() => {
            this.notifyService.showSuccess('Staffable As role deleted', 'Success');
            this.overlayMessageService.triggerResourceRefresh();
            if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
                this.resourceDialogRef.componentInstance.activeStaffableAsRoleName = '';
            }
        });
    }

    upsertStaffableAsRoleEventEmitterHandler(event, resourceDialogRef) {
        this.resourceDialogRef = resourceDialogRef;
        let staffableRoles = event.staffableRoles;
        let resource = event.resource;
        staffableRoles.map(x => {
            x.employeeCode = resource.employeeCode,
                x.levelGrade = resource.levelGrade
        });
        this.overlayService.upsertResourceStaffableAs(staffableRoles)
            .pipe(takeUntil(this.destroy$)).subscribe((updateStaffableAsRolesApiResponse: StaffableAsRole[]) => {
                if (updateStaffableAsRolesApiResponse) {
                    this.notifyService.showSuccess('Staffable As role updated', 'Success');
                    this.overlayMessageService.triggerResourceRefresh();
                    if (this.resourceDialogRef && this.resourceDialogRef.componentInstance) {
                        this.resourceDialogRef.componentInstance.setStaffableAsRoleForAboutTab(updateStaffableAsRolesApiResponse);
                    }
                } else {
                    this.notifyService.showError('An error occurred while updating staffable role(s)', 'Error');
                }
            });
    }
}