import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { BS_DEFAULT_CONFIG } from 'src/app/shared/constants/bsDatePickerConfig';
import { UserNotification } from 'src/app/shared/interfaces/userNotification.interface';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { CoreService } from '../core.service';
import { ThemePalette } from '@angular/material/core';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.scss']
})
export class NotificationComponent implements OnInit, OnDestroy {

  // -----------------------Local Variables--------------------------------------------//
  public showDialog: boolean;
  public userNotifications: UserNotification[];
  public preponeCaseAllocationsAudit = [];
  public TAB_INDEX_ENUM = {
    NOTIFICATION : 0,
    CASE_CHANGES_AUDIT : 1
  }

  public selectedDateRange: any;
  public bsConfig: Partial<BsDatepickerConfig>;
  public isCaseChangesAuditDataLoaded = false;
  color: ThemePalette = "warn";
  
  constructor(public dialogRef: MatDialogRef<NotificationComponent>,
    private _coreService: CoreService,
    @Inject(MAT_DIALOG_DATA) public data: any) {
    this.dialogRef.disableClose = true; // dialogRef property set to disable dialogBox from closing using esc button.
    this.showDialog = this.data.showDialog;
    this.userNotifications = this.data.userNotifications;
  }

  // -----------------------Component LifeCycle Events and Functions--------------------------------------------//
  ngOnInit() {
    this._coreService.getShowHideNotification().subscribe(value => {
      if (!value) {
        this.closeDialog();
      }
    });

    this.setFilters();
    this.initializeDateConfig();
    
  }

  // -------------------Component Event Handlers-------------------------------------//

  private initializeDateConfig() {
    this.bsConfig = Object.assign({}, BS_DEFAULT_CONFIG);
    this.bsConfig.containerClass = "theme-red calendar-dropdown calendar-align-left";
  }

  setFilters(){
    const endDate = new Date();
    let startDate = new Date();
    startDate.setDate(startDate.getDate() -14);

    this.selectedDateRange = [startDate , endDate];
  }

  closeDialog() {
    const self = this;

    self.showDialog = false;

    /*
     * Set timeout is required to close the animation with animation and override/delay the defaut closing of material dialog.
     * If not used, the overlay will not close as per our requored animation
     * dialogref.close is required to unload the overlay component.
    */
    setTimeout(function () {
      self.dialogRef.close();
    }, 1000);
  }

  updateUserNotificationStatus(event, notification: UserNotification) {
    const updateItem = this.userNotifications.find(x => x.notificationId === notification.notificationId);
    const index = this.userNotifications.indexOf(updateItem);
    this.userNotifications[index].notificationStatus = 'R';
    this._coreService.updateUserNotificationStatus(notification.notificationId, 'R').subscribe(status => {
      this._coreService.userNotifications.next(this.userNotifications);
    });
  }

  openCaseOpp(description: string) {
    this.closeDialog();
    this._coreService.triggerOpenCaseOppOverlayFromNotifications({ oldCaseCode: description });
  }

  onTabChanged(selectedTab){
    if(selectedTab.index === this.TAB_INDEX_ENUM.CASE_CHANGES_AUDIT){
      this.getPreponedCaseAllocationsAudit();
    };
  }

  shiftDateRange(shiftDate) {
    if (shiftDate === "left") {
        const startDate = this.selectedDateRange[0];
        const endDate = this.selectedDateRange[1];

        startDate.setDate(startDate.getDate() - 7);
        endDate.setDate(endDate.getDate() - 7);

        this.selectedDateRange = [startDate, endDate];
    } else {
        const startDate = this.selectedDateRange[0];
        const endDate = this.selectedDateRange[1];

        startDate.setDate(startDate.getDate() + 7);
        endDate.setDate(endDate.getDate() + 7);

        this.selectedDateRange = [startDate, endDate];
    }

    this.getPreponedCaseAllocationsAudit();

  }

  onDateRangeChange(selectedDateRange) {
      // To avoid API call during initialization we check for non nullable start and end dates
      if (!selectedDateRange || this.selectedDateRange.toString() === selectedDateRange.toString()) {
          return;
      }

      this.selectedDateRange = selectedDateRange;

      this.getPreponedCaseAllocationsAudit();
  }

  getPreponedCaseAllocationsAudit(){
    this.isCaseChangesAuditDataLoaded = false;
    const userPreferences : UserPreferences = this._coreService.getUserPreferencesValue();
    const filterObj = {
      "startDate" : this.selectedDateRange[0],
      "endDate" : this.selectedDateRange[1],
      "serviceLineCodes": userPreferences.supplyViewStaffingTags,
      "officeCodes": userPreferences.supplyViewOfficeCodes
    };

    this._coreService.getPreponedCaseAllocationsAudit(filterObj).subscribe(data => {
      this.preponeCaseAllocationsAudit = data;
      this.isCaseChangesAuditDataLoaded = true;
    });
  }

  ngOnDestroy() {
  }
}
