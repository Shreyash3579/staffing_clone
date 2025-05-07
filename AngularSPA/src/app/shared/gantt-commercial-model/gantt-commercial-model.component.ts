import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { CoreService } from 'src/app/core/core.service';
import { AllocationNotesContextMenuComponent } from 'src/app/resources/allocation-notes-context-menu/allocation-notes-context-menu.component';
import { ResourceViewCD } from '../interfaces/resource-view-cd.interface';
import { ResourceViewCommercialModel } from '../interfaces/resource-view-commercial-model.interface';

@Component({
  selector: 'app-shared-gantt-commercial-model',
  templateUrl: './gantt-commercial-model.component.html',
  styleUrls: ['./gantt-commercial-model.component.scss']
})
export class GanttCommercialModelComponent implements OnInit {
  // -----------------------Input Events-----------------------------------------------//
  @Input() rowIndex = "";
  @Input() commercialModels: ResourceViewCommercialModel[] = [];
  @Input() hideAddNewCommercialModel: boolean = true;
  @Input() isCommercialModelReadonly: boolean = false;
  @Input() isHideCloseIcon: boolean = true;

  // -----------------------Output Events-----------------------------------------------//
  @Output() upsertCommercialModel = new EventEmitter<any>();
  @Output() deleteCommercialModel = new EventEmitter<any>();
  @Output() closeCommercialModelWrapperEmitter = new EventEmitter<boolean>();

  public userCommercialModel = "";
  public editCommercialModelMode = false;
  public loggedInUser: string;
  public loggedInUserName: string;
  public commercialModelWrapperCollapsed: boolean = false;
  public editedcommercialModelObj: ResourceViewCommercialModel = {
    commercialModel: "",
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
    this.setRecentCommercialModelsIfNull();
    this.loggedInUser = this.coreService.loggedInUser.employeeCode;
    this.loggedInUserName = this.coreService.loggedInUser.fullName;
     this.updateAddRecentCommercialModelsSectionVisibility();
  }

  updateAddRecentCommercialModelsSectionVisibility() {
    if (this.isCommercialModelReadonly) {
        this.hideAddNewCommercialModel = true;
    }
}

  setRecentCommercialModelsIfNull() {
    if (!this.commercialModels) {
      this.commercialModels = [];
    }
  }


  onEditRecentCommercialModel(recentCommercialModel){
    this.editCommercialModel(recentCommercialModel);
    this.changeDetector.detectChanges();
  }

  onDeleteCommercialModel(cm, index){
  this.commercialModels.forEach((item) => {
    if (item.id === cm.id) {
      this.commercialModels.splice(index, 1);
      this.deleteCommercialModel.emit(cm.id);
    }
    });
  }

  // Handles Editing Recent CD
  editCommercialModel(recentCommercialModelObj) {
    this.cancelCommercialModelHandler();
    this.hideAddNewCommercialModel = false;
    this.editCommercialModelMode = !this.editCommercialModelMode;
    this.userCommercialModel = recentCommercialModelObj.commercialModel;

    this.editedcommercialModelObj = {
      id: recentCommercialModelObj.id,
      commercialModel: this.userCommercialModel,
      lastUpdated: recentCommercialModelObj.lastUpdated,
      createdBy: recentCommercialModelObj.createdBy,
      createdByName: recentCommercialModelObj.createdByName,
      employeeCode: recentCommercialModelObj.employeeCode,
      lastUpdatedBy: recentCommercialModelObj.lastUpdatedBy,
    };

  }


  // Toggle Add CD Input
  toggleAddNewCommercialModel() {
    this.cancelCommercialModelHandler();
    this.hideAddNewCommercialModel = !this.hideAddNewCommercialModel;
  }

  // Add Recent CD Handler
  addRecentCommercialModelHandler() {
    const cd = this.setCommercialModelObject();

    if (this.userCommercialModel.length >= 1 || this.userCommercialModel !== "") {
      this.commercialModels.unshift(cd);
      this.cancelCommercialModelHandler();
      this.upsertCommercialModel.emit(cd);
    }
  }

  setCommercialModelObject() {
    const commercialModel: ResourceViewCommercialModel = {
      commercialModel: this.userCommercialModel,
      lastUpdated: new Date(),
      createdBy: this.loggedInUser,
      createdByName: this.loggedInUserName,
      lastUpdatedBy: this.loggedInUser
    };
    return commercialModel;
  }

  // Saving edited Recent CD
  saveRecentCommercialModelEditHandler() {
    this.editedcommercialModelObj.commercialModel = this.userCommercialModel;


    this.commercialModels.forEach((item) => {
      if (item.id === this.editedcommercialModelObj.id) {
        item.commercialModel = this.userCommercialModel,
          item.lastUpdated = new Date(),
          item.createdBy = this.loggedInUser,
          item.createdByName = this.loggedInUserName,
          item.lastUpdatedBy = this.loggedInUser
      }
    });
    this.upsertCommercialModel.emit(this.editedcommercialModelObj);

    this.editCommercialModelMode = !this.editCommercialModelMode;
    this.cancelCommercialModelHandler();
  }

  // Cancel Changes
  cancelCommercialModelHandler() {
    this.hideAddNewCommercialModel = true;
    this.editCommercialModelMode = false;
    this.userCommercialModel = "";
  }

  closeCommercialModelWrapper(){
    this.closeCommercialModelWrapperEmitter.emit(true);
  }


}
