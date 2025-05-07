import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { CoreService } from 'src/app/core/core.service';
import { AllocationNotesContextMenuComponent } from 'src/app/resources/allocation-notes-context-menu/allocation-notes-context-menu.component';
import { ResourceViewCD } from '../interfaces/resource-view-cd.interface';

@Component({
  selector: 'app-shared-gantt-recent-cd',
  templateUrl: './gantt-recent-cd.component.html',
  styleUrls: ['./gantt-recent-cd.component.scss']
})
export class GanttRecentCdComponent implements OnInit {
  // -----------------------Input Events-----------------------------------------------//
  @Input() rowIndex = "";
  @Input() recentCDs: ResourceViewCD[] = [];
  @Input() hideAddNewRecentCD: boolean = true;
  @Input() isRecentCdReadonly: boolean = false;
  @Input() isHideCloseIcon: boolean = true;

  // -----------------------Output Events-----------------------------------------------//
  @Output() upsertRecentCD = new EventEmitter<any>();
  @Output() deleteRecentCD = new EventEmitter<any>();
  @Output() closeRecentCDWrapperEmitter = new EventEmitter<boolean>();

  public userCD = "";
  public editCDMode = false;
  public loggedInUser: string;
  public loggedInUserName: string;
  public recentCDWrapperCollapsed: boolean = false;
  public editedrecentCdObj: ResourceViewCD = {
    recentCD: "",
    lastUpdated: new Date(),
    createdBy: "",
    createdByName: "",
    id: "",
    lastUpdatedBy: ""
  };


  bsModalRef: BsModalRef;
  CdsContextMenuDialogRef: MatDialogRef<AllocationNotesContextMenuComponent, any>;

  constructor(
    private coreService: CoreService,
    private modalService: BsModalService,
    private readonly changeDetector: ChangeDetectorRef,
  ) { }

  ngOnInit(): void {
    this.setRecentCDsIfNull();
    this.loggedInUser = this.coreService.loggedInUser.employeeCode;
    this.loggedInUserName = this.coreService.loggedInUser.fullName;
     this.updateAddRecentCdSectionVisibility();
  }

  updateAddRecentCdSectionVisibility() {
    if (this.isRecentCdReadonly) {
        this.hideAddNewRecentCD = true;
    }
}

  setRecentCDsIfNull() {
    if (!this.recentCDs) {
      this.recentCDs = [];
    }
  }


  onEditRecentCD(recentCd){
    this.editCD(recentCd);
    this.changeDetector.detectChanges();
  }

  onDeleteCD(cd, index){
  this.recentCDs.forEach((item) => {
    if (item.id === cd.id) {
      this.recentCDs.splice(index, 1);
      this.deleteRecentCD.emit(cd.id);
    }
    });
  }

  // Handles Editing Recent CD
  editCD(recentCdObj) {
    this.cancelRecentCDHandler();
    this.hideAddNewRecentCD = false;
    this.editCDMode = !this.editCDMode;
    this.userCD = recentCdObj.recentCD;

    this.editedrecentCdObj = {
      id: recentCdObj.id,
      recentCD: this.userCD,
      lastUpdated: recentCdObj.lastUpdated,
      createdBy: recentCdObj.createdBy,
      createdByName: recentCdObj.createdByName,
      employeeCode: recentCdObj.employeeCode,
      lastUpdatedBy: recentCdObj.lastUpdatedBy,
    };

  }


  // Toggle Add CD Input
  toggleAddNewRecentCD() {
    this.cancelRecentCDHandler();
    this.hideAddNewRecentCD = !this.hideAddNewRecentCD;
  }

  // Add Recent CD Handler
  addRecentCDHandler() {
    const cd = this.setCDObject();

    if (this.userCD.length >= 1 || this.userCD !== "") {
      this.recentCDs.unshift(cd);
      this.cancelRecentCDHandler();
      this.upsertRecentCD.emit(cd);
    }
  }

  setCDObject() {
    const cd: ResourceViewCD = {
      recentCD: this.userCD,
      lastUpdated: new Date(),
      createdBy: this.loggedInUser,
      createdByName: this.loggedInUserName,
      lastUpdatedBy: this.loggedInUser
    };
    return cd;
  }

  // Saving edited Recent CD
  saveRecentCdEditHandler() {
    this.editedrecentCdObj.recentCD = this.userCD;


    this.recentCDs.forEach((item) => {
      if (item.id === this.editedrecentCdObj.id) {
        item.recentCD = this.userCD,
          item.lastUpdated = new Date(),
          item.createdBy = this.loggedInUser,
          item.createdByName = this.loggedInUserName,
          item.lastUpdatedBy = this.loggedInUser
      }
    });
    this.upsertRecentCD.emit(this.editedrecentCdObj);

    this.editCDMode = !this.editCDMode;
    this.cancelRecentCDHandler();
  }

  // Cancel Changes
  cancelRecentCDHandler() {
    this.hideAddNewRecentCD = true;
    this.editCDMode = false;
    this.userCD = "";
  }

  closeRecentCDWrapper(){
    this.closeRecentCDWrapperEmitter.emit(true);
  }


}
