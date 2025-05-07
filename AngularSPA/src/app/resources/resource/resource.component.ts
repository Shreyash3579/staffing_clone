import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges, OnDestroy } from "@angular/core";
import { Resource } from "src/app/shared/interfaces/resource.interface";
import { ProfileImageService } from "src/app/shared/services/profileImage.service";
import { BsModalService } from "ngx-bootstrap/modal";
import { PopupModalComponent } from "../popup-modal/popup-modal.component";
import { CommitmentType as CommitmentTypeCodeEnum, EmployeeCaseGroupingEnum, NoteTypeCode } from "src/app/shared/constants/enumMaster";
import { ResourceOrCasePlanningViewNote } from "src/app/shared/interfaces/resource-or-case-planning-view-note.interface";
import { DateService } from "src/app/shared/dateService";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { CoreService } from "src/app/core/core.service";
import { CommonService } from "src/app/shared/commonService";
import { Subscription } from "rxjs";
import { ResourceViewCD } from "src/app/shared/interfaces/resource-view-cd.interface";
import { ResourceViewSelectedTab } from "src/app/shared/interfaces/resource-view-selected-tab.interface";
import { ResourceViewCommercialModel } from "src/app/shared/interfaces/resource-view-commercial-model.interface";
import { EmployeePracticeArea } from "src/app/shared/interfaces/employeePracticeArea";

@Component({
  selector: "resources-resource",
  templateUrl: "./resource.component.html",
  styleUrls: ["./resource.component.scss"],
  providers: [ProfileImageService]
})
export class ResourceComponent implements OnInit, OnChanges, OnDestroy {
   // -----------------------Input Events-----------------------------------------------//
  @Input() resource: Resource;
  @Input() rowIndex = "";
  @Input() commitments = [];
  @Input() resourceViewNotes: ResourceOrCasePlanningViewNote[] = [];
  @Input() resourceViewCD: ResourceViewCD[] = [];
  @Input() resourceViewCommercialModel: ResourceViewCommercialModel[] = [];
  @Input() affiliations: EmployeePracticeArea[] = [];
  @Input() objGanttCollapsedRows;
  @Input() selectedEmployeeCaseGroupingOption: string;
  @Input() isLeftSideBarCollapsed: boolean = false;
  @Input() isSelectedPracticeView:boolean;

  // -----------------------Output Events-----------------------------------------------//
  @Output() openResourceDetailsDialog = new EventEmitter();
  @Output() upsertResourceViewNote = new EventEmitter<any>();
  @Output() deleteResourceViewNotes = new EventEmitter<any>();
  @Output() upsertResourceRecentCD = new EventEmitter<any>();
  @Output() deleteResourceRecentCD = new EventEmitter<any>();
  @Output() upsertResourceCommercialModel = new EventEmitter<any>();
  @Output() deleteResourceCommercialModel = new EventEmitter<any>();
  @Output() selectedResourceViewTab = new EventEmitter<any>();
  @Output() expandCollapseGanttRow = new EventEmitter<boolean>();

  isRowCollapsed: boolean = false;
  public lastBillableDate: string;
  l1Affiliations: EmployeePracticeArea[] = [];
  l2Affiliations: EmployeePracticeArea[] = [];
  accessibleFeatures = ConstantsMaster.appScreens.feature;
  isNotesReadonly: boolean = false;
  subscription: Subscription = new Subscription(); 
  constructor(
    private profileImageService: ProfileImageService,
    private modalService: BsModalService,
    private coreService: CoreService
  ) { }

  ngOnInit() {}

  ngOnChanges(changes: SimpleChanges) {
    this.isNotesReadonly = !this.isNotesAccessible();
    this.setLastBillableDate();

    // if (changes.isTopbarCollapsed) {
    //   this.isRowCollapsed = changes.isTopbarCollapsed.currentValue;
    // }
    if (changes.objGanttCollapsedRows && this.objGanttCollapsedRows) {
      if(this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES){
            this.isRowCollapsed = true;
      }else{
          this.isRowCollapsed = this.objGanttCollapsedRows.exceptionRowIndexes.includes(this.resource.employeeCode)
          ? !this.objGanttCollapsedRows.isAllCollapsed : this.objGanttCollapsedRows.isAllCollapsed;
      }
    }
    //if(changes.resourceViewCommercialModel){
    //  console.log(this.resourceViewCommercialModel);
    //}

    if(changes.affiliations && this.affiliations){
      this.l1Affiliations = this.affiliations.filter(x => x.roleCode === '8');
      this.l2Affiliations = this.affiliations.filter(x => x.roleCode === '9');
    }
  }

   l1AffiliationNames(): string {
    if(!this.l1Affiliations || this.l1Affiliations.length === 0){
      return '';
    }
    return this.l1Affiliations.map(practice => practice.practiceAreaName).join(', ');
  }

  l2AffiliationNames(): string {
    if(!this.l2Affiliations || this.l2Affiliations.length === 0){
      return '';
    }
    return this.l2Affiliations.map(practice => practice.practiceAreaName).join(', ');
  }

  get getFormattedAffiliation(): string {
    const l1AffilaitionText = this.l1Affiliations?.length ? `L1: ${this.l1AffiliationNames()}` : '';
    const l2TAffilaitionext = this.l2Affiliations?.length ? `L2: ${this.l2AffiliationNames()}` : '';
    const affiliationText = [l1AffilaitionText, l2TAffilaitionext].filter(text => text).join(', ');
    const separator = this.isRowCollapsed ? ' | ' : ', ';
  
    return [l1AffilaitionText, l2TAffilaitionext].filter(text => text).join(separator);

  }

  // Open Commitments Detail Popup
  openCommitmentsDetailPopup(event, commitments) {
    const positionObj = {
      top: event.clientY,
      left: event.clientX
    };

    this.modalService.show(PopupModalComponent, {
      animated: true,
      backdrop: false,
      ignoreBackdropClick: false,
      initialState: {
        commitments: commitments.filter(x => x.commitmentTypeCode === CommitmentTypeCodeEnum.PEG)
      },
      class: `commitments-detail-popup left-${positionObj.left} top-${positionObj.top}`
    });
  }

  showPegIcon(commitments): boolean {
    return commitments.some(this.checkPegAllocation);
  }

  checkPegAllocation(commitment) {
    return commitment.commitmentTypeCode === CommitmentTypeCodeEnum.PEG;
  }

  getImageUrl() {
    this.profileImageService.getImage(this.resource.profileImageUrl);

    this.subscription.add(this.profileImageService.imgUrl.subscribe((imgUrl) => {
      this.resource.profileImageUrl = imgUrl;
    }));
  }

  openResourceDetailsDialogHandler(employeeCode) {
    this.openResourceDetailsDialog.emit(employeeCode);
  }

  // Expand & Collapse Row
  toggleExpandCollapse(event, rowIndex) {
    this.isRowCollapsed = !this.isRowCollapsed;
    this.expandCollapseGanttRow.emit(this.isRowCollapsed);
  }

  setLastBillableDate() {
    this.lastBillableDate = DateService.convertDateInBainFormat(this.resource.lastBillable?.lastBillableDate);

    var date = new Date();
    date.setDate(date.getDate() + 1);

    if (!this.lastBillableDate) {
      this.lastBillableDate = 'N/A';
    }
    else if (DateService.isSameOrAfter(this.lastBillableDate, date)) {
      this.lastBillableDate = 'Staffed'
    }
  }

  upsertResourceViewNoteHandler(event){
    let noteToBeUpserted : ResourceOrCasePlanningViewNote = {
      id: event.id,
      note: event.note,
      employeeCode: this.resource.employeeCode,
      lastUpdated: event.lastUpdated,
      createdBy: event.createdBy,
      createdByName: event.createdByName,
      sharedWith: event.sharedWith,
      sharedWithDetails: event.sharedWithDetails,
      isPrivate: event.isPrivate,
      noteTypeCode: NoteTypeCode.RESOURCE_ALLOCATION_NOTE,   
      lastUpdatedBy: event.lastUpdatedBy
    }
    this.upsertResourceViewNote.emit(noteToBeUpserted);
  }
 
  deleteResourceViewNotesHandler(event){
    this.deleteResourceViewNotes.emit(event);
  }

  upsertResourceRecentCDHandler(event){
    let recentCdToBeUpserted : ResourceViewCD = {
      id: event.id,
      recentCD: event.recentCD,
      employeeCode: this.resource.employeeCode,
      lastUpdated: event.lastUpdated,
      createdBy: event.createdBy,
      createdByName: event.createdByName,   
      lastUpdatedBy: event.lastUpdatedBy
    }
    this.upsertResourceRecentCD.emit(recentCdToBeUpserted);
  }
  upsertResourceCommercialModelHandler(event){
    let recentCdToBeUpserted : ResourceViewCommercialModel = {
      id: event.id,
      commercialModel: event.commercialModel,
      employeeCode: this.resource.employeeCode,
      lastUpdated: event.lastUpdated,
      createdBy: event.createdBy,
      createdByName: event.createdByName,   
      lastUpdatedBy: event.lastUpdatedBy
    }
    this.upsertResourceCommercialModel.emit(recentCdToBeUpserted);
  }

  deleteResourceRecentCDHandler(event){
    
    this.deleteResourceRecentCD.emit(event);
  }
  deleteResourceCommercialModelHandler(event){
    
    this.deleteResourceCommercialModel.emit(event);
  }

  selectedResourceViewTabHandler(event){
    let resourceViewSelectedTab: ResourceViewSelectedTab ={
      employeeCode: this.resource.employeeCode,
      selectedTab: event
    }
    this.selectedResourceViewTab.emit(resourceViewSelectedTab);
  }



  isNotesAccessible() {
    const featureName = this.accessibleFeatures.addResourceViewNotes;

    const accessibleFeaturesForUser = this.coreService.loggedInUserClaims.FeatureAccess;
    const isAccessable = CommonService.isAccessToFeatureReadOnlyOrNone(featureName, accessibleFeaturesForUser);
    return isAccessable;
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

}
