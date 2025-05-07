import {
  Component,
  OnInit,
  Inject,
  HostListener,
  Output,
  EventEmitter,
  OnDestroy,
  ElementRef
} from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Router, RouterEvent, NavigationStart, ActivatedRoute } from "@angular/router";
import { filter } from "rxjs/operators";
import { CommitmentType } from "src/app/shared/interfaces/commitmentType.interface";
import { GanttService } from "../gantt/gantt.service";
import { InvestmentCategory } from "src/app/shared/interfaces/investmentCateogry.interface";
import { CaseRoleType } from "src/app/shared/interfaces/caseRoleType.interface";
import { DatePipe } from "@angular/common";
import { NotificationService } from "src/app/shared/notification.service";
import { ValidationService } from "src/app/shared/validationService";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { CommonService } from "src/app/shared/commonService";
import { OverlayService } from "../overlay.service";
import { SystemconfirmationFormComponent } from "src/app/shared/systemconfirmation-form/systemconfirmation-form.component";
import { DateService } from "src/app/shared/dateService";
import { CommitmentType as CommitmentTypeCodeEnum, NoteTypeCode } from "../../shared/constants/enumMaster";
import * as moment from "moment";
import { ResourceAllocationService } from "src/app/shared/services/resourceAllocation.service";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { ResourceAllocation } from "src/app/shared/interfaces/resourceAllocation.interface";
import { GridOptions } from "ag-grid-community";
import { CoreService } from "src/app/core/core.service";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { UpdateDateFormComponent } from "src/app/shared/update-date-form/update-date-form.component";
import { LevelGradeTransactionModel } from "src/app/shared/interfaces/level-grade-transaction.interface";
import { Training } from "src/app/shared/interfaces/training";
import { Language } from "src/app/shared/interfaces/employeeLanguage.interface";
import { Certification, EmployeeCertification } from "src/app/shared/interfaces/employeeCertification.interface";
import { EmployeeSchoolHistory } from "src/app/shared/interfaces/employeeSchoolHistory";
import { EmployeeWorkHistory } from "src/app/shared/interfaces/employeeWorkHistory";
import { CommitmentType as CommitmentTypeEnum } from "../../shared/constants/enumMaster";
import { EmployeeStaffingPreferences } from "src/app/shared/interfaces/employeeStaffingPreferences";
import { StaffableAsRole } from "src/app/shared/interfaces/staffableAsRole.interface";
import { ResourceReviewRating } from "src/app/shared/interfaces/resourceReviewRating.interface";
import { TransferTransactionModel } from "src/app/shared/interfaces/tranfer-transaction.interface";
import { EmployeeWorkAndSchoolHistory } from "src/app/shared/interfaces/employeeWorkAndSchoolHistory.interface";
import { UrlService } from "src/app/core/services/url.service";
import { ResourceOrCasePlanningViewNote } from "src/app/shared/interfaces/resource-or-case-planning-view-note.interface";
import { SMAPMissionNote } from "src/app/shared/interfaces/SMAPMissionNote";
import { EmployeeStaffingInfo } from "src/app/shared/interfaces/employeeStaffingInfo";
import { DomSanitizer } from "@angular/platform-browser";

import { Store, select } from "@ngrx/store";
import * as fromResourceOverlay from "../State/reducer/resource-overlay.reducer";
import * as ResourceOverlayActions from "../State/actions/resource-overlay.actions"
import * as fromUserSpecificDetailsStore from 'src/app/state/reducers/user-specific-details.reducer';
import * as UserSpecificDetailsAction from 'src/app/state/actions/user-specific-details.action';

import { Observable, Subscription } from "rxjs";

@Component({
  selector: "app-resource-overlay",
  templateUrl: "./resource-overlay.component.html",
  styleUrls: ["./resource-overlay.component.scss"]
})
export class ResourceOverlayComponent implements OnInit, OnDestroy {
  // -----------------------Local Variables--------------------------------------------//

  // For dragging
  public grabber: boolean = false;
  public height: number = 320;
  public oldYCoord: number = 0;
  public yCoord: number = 100;

  public resourceDetails: any = {};
  public showOverlay: boolean;
  public openedTab: string;
  public commitmentTypes: CommitmentType[];
  public investmentCategories: InvestmentCategory[];
  public caseRoleTypes: CaseRoleType[];
  public commitmentStartDate;
  public today = new Date();
  public bsModalRef: BsModalRef;
  public calendarRadioSelected: string = ConstantsMaster.ganttCalendarScaleOptions.defaultSelection;
  public emptyLogMessage = "";
  public gridOptions: GridOptions;
  public resourceNotes : ResourceOrCasePlanningViewNote[] = [];
  public smapMissionNotes : SMAPMissionNote;
  public resourceRatings: ResourceReviewRating[];

  public columnDefs;
  public defaultColDef;
  public domLayout;
  public components;
  public frameworkComponents;
  public agGridRowData: any[];
  public gridApi;
  public gridColumnApi;
  private isAllocationValid = false;
  private isStartDateValid = false;
  private isEndDateValid = false;
  private resourceData: any;
  private routerSubscription: any;
  public levelGradeHistoryData: LevelGradeTransactionModel[];
  public globalTrainingsData: Training[];
  public languages: Language[];
  public certificates: Certification[];
  public employeeSchoolHistory: EmployeeSchoolHistory[];
  public employeeWorkHistory: EmployeeWorkHistory[];
  public staffableAsRoles: StaffableAsRole[];
  public activeStaffableAsRoleName: string = "";
  public employeeStaffingDetails: EmployeeStaffingInfo ;
  public isLoading = true;
  public employeeStaffingPreferences: EmployeeStaffingPreferences[];
  public tranferData: TransferTransactionModel[];
  public routeQueryParamSubsciption: any;
  apiDown = false;

  
  isStaffingPreferencesToolEnabled: boolean = false;
  auditLogStartIndex = 0;
  logLoading = false;
  scrollDistance: number; // how much percentage the scroll event should fire ( 2 means (100 - 2*10) = 80%)
  pageScrolled = false;
  pageSize: number;
  appScreens: any;
  isAGGridReadOnly = false;
  accessibleFeatures = ConstantsMaster.appScreens.feature;
  employeeStaffingDetails$: any;
  storeSub: Subscription = new Subscription();
  // ------------------------Ouput Events-----------------------------------------------//
  @Output() openResourceDetailsFromProjectDialog = new EventEmitter();
  @Output() openProjectDetailsFromResourceDialog = new EventEmitter();
  @Output() updateResourceCommitment = new EventEmitter();
  @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();
  @Output() deleteResourceCommitment = new EventEmitter();
  @Output() openQuickAddForm = new EventEmitter<any>();
  @Output() updateResourceDataForProject = new EventEmitter();
  @Output() deleteResourceAllocationFromCase = new EventEmitter();
  @Output() deleteResourceAllocationFromCases = new EventEmitter();
  @Output() deleteResourceAllocationsCommitmentsFromCase = new EventEmitter();
  @Output() openNotesDialog = new EventEmitter();
  @Output() openSplitAllocationDialog = new EventEmitter();
  @Output() getResourceNotesEmitter = new EventEmitter();
  @Output() addResourceNoteEmitter = new EventEmitter();
  @Output() updateResourceNoteEmitter = new EventEmitter();
  @Output() deleteStaffableAsRoleEmitter = new EventEmitter();
  @Output() upsertStaffableAsRoleEmitter = new EventEmitter();
   
  // ------------------------Ag-grid Properties-----------------------------------------------//

  constructor(
      public dialogRef: MatDialogRef<ResourceOverlayComponent>,
      @Inject(MAT_DIALOG_DATA) public data: any,
      private _overlayService: OverlayService,
      private _ganttService: GanttService,
      private modalService: BsModalService,
      private notifyService: NotificationService,
      private router: Router,
      private route: ActivatedRoute,
      private _resourceAllocationService: ResourceAllocationService,
      private coreService: CoreService,
      private localStorageService: LocalStorageService,
      private urlService: UrlService,
      private elementRef: ElementRef,
      private sanitizer : DomSanitizer,
      private resourceOverlayStore: Store<fromResourceOverlay.State>,
      private userSpecificDetailsStore: Store<fromUserSpecificDetailsStore.State>
    
  ) {
      this.appScreens = ConstantsMaster.appScreens;
      this.investmentCategories = this.localStorageService.get(ConstantsMaster.localStorageKeys.investmentCategories);
      this.caseRoleTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseRoleTypes);

      this.commitmentStartDate = new Date();
      this.commitmentStartDate.setDate(
          this.commitmentStartDate.getDate() - ConstantsMaster.ganttConstants.initialDaysDeduction
      );

      // emitting null value to prevent emitting last value
      this._ganttService.resourceCommitments.next(null);

      this.isAGGridReadOnly = CommonService.isReadOnlyAccessToFeature(
          this.accessibleFeatures.casesTab,
          this.coreService.loggedInUserClaims.FeatureAccess
      );
      this.loadAgGridConfigurations();
      // domLayout is used to automatically set the height of the grid according to the height of the screen provided
      // but it removes the position:fixed property of grid headers
      // this.domLayout = "autoHeight";

      this.dialogRef.disableClose = true; // dialogRef property set to disable dialogBox from closing using esc button.
      this.showOverlay = this.data.showOverlay;
      this.getDetailsForResource(this.data.employeeCode);
      this.getCommitmentTypes();
      this.coreService.setShowHideNotification(false);
  }
  // Added to provide animation to dialog box while closing using esc button.
  @HostListener("document:keydown", ["$event"])
  public handleKeyboardEvent(event: KeyboardEvent): void {
      // if-else condition added to close only case popup when both resource popup and project detail popup are open
      // and user presses esc button.
      if (event.key === "Escape") {
          if ((<HTMLInputElement>event.target).childElementCount > 0) {
              if (
                  (<HTMLInputElement>event.target).firstElementChild.firstElementChild &&
                  ((<HTMLInputElement>event.target).firstElementChild.firstElementChild.id === "user-card" ||
                      (<HTMLInputElement>event.target).firstElementChild.firstElementChild.id === "")
              ) {
                  this.closeDialog();
              }
          } else {
              this.closeDialog();
          }
      }
  }

  @HostListener("window:mousemove", ["$event"])
  onMousemove(event: MouseEvent) {
      document.querySelector(".gantt_tooltip")?.remove();
  }

  //-----------------------Store Subscriptions ------------------------------//

  setStoreSubscriptions(){
    this.storeSub.add(this.resourceOverlayStore
    .pipe(
        select(fromResourceOverlay.getResourceStaffingInfo),
        filter(employeeStaffingDetails => employeeStaffingDetails != null && employeeStaffingDetails.employeeCode === this.data.employeeCode)) // exclude null on inital store subscription and exclude old data of previously opened employee overlay
    .subscribe((employeeStaffingDetails: any) => {
        this.employeeStaffingDetails = employeeStaffingDetails;
    }));
  }
  
  // -------------------Component LifeCycle Events and Functions----------------------//

  ngOnInit() {
      this.routerSubscription = (this.router.events as Observable<RouterEvent>)
          .pipe(
              filter((event: RouterEvent) => event instanceof NavigationStart),
              filter(() => !!this.dialogRef)
          )
          .subscribe(() => {
              this.dialogRef.close();
          });
      this.openedTab = "calendar";
      this.pageSize = this.coreService.appSettings.resourcesPerPage;
      this.scrollDistance = this.coreService.appSettings.scrollDistance;

      this.setStoreSubscriptions();
      this.setUserAuthorizationForStaffingPreferences();
  }

  ngAfterViewInit() {
      // Event Listener for dragging and expanding column Div
      this.elementRef.nativeElement.querySelector("#gr").addEventListener("mousedown", this.onMouseDown.bind(this));
  }

  ngOnDestroy(): void {
      this.routerSubscription.unsubscribe();
      this.storeSub.unsubscribe();
      this.resourceOverlayStore.dispatch(new ResourceOverlayActions.ClearResourceOverlayState());
  }

  // On mouse move up or down
  @HostListener("document:mousemove", ["$event"])
  onMouseMove(event: MouseEvent) {
      if (!this.grabber) {
          return;
      } else {
          this.resizer(event.clientY - this.oldYCoord);
          this.oldYCoord = event.clientY;
      }
  }

  // Set height variable assigned to columns container to offsetY value
  resizer(offsetY: number) {
      this.height += offsetY;
  }

  // When mouse click is released
  @HostListener("document:mouseup", ["$event"])
  onMouseUp() {
      this.grabber = false;
  }

  // When mouse is clicked down
  onMouseDown(event: MouseEvent) {
      this.grabber = true;
      this.oldYCoord = event.clientY;
  }

  changeCalendarOptionHandler(changedCalendarOption) {
      this.calendarRadioSelected = changedCalendarOption;
  }

  oldCaseCodeToOpenHandler(data) {
      this.openProjectDetailsFromResourceDialog.emit({ oldCaseCode: data.oldCaseCode, pipelineId: data.pipelineId });
  }

  getHeight() {
      return {
          height: `calc(100% - ${this.height}px)`
      };
  }

  loadAgGridConfigurations() {
      this.gridOptions = <GridOptions>{
          context: {
              componentParent: this
          }
      };
      this.defaultColDef = {
          resizable: true,
          sortable: true,
          filter: true,
          filterParams: { clearButton: true }
      };

      this.columnDefs = [
          {
              checkboxSelection: !this.isAGGridReadOnly,
              headerCheckboxSelection: !this.isAGGridReadOnly,
              headerCheckboxSelectionFilteredOnly: !this.isAGGridReadOnly,
              headerName: "",
              field: "",
              maxWidth: 70,
              filter: false,
              cellRenderer: function (params) {
                  return '<span><i class="fa fa-trash" title="Delete"></i></span>';
              },
              hide: this.isAGGridReadOnly
          },
          {
              headerName: "Notes",
              field: "notes",
              filter: false,
              maxWidth: 30,
              cellRenderer: function (params) {
                  return params.data.notes
                      ? '<span><i class="fas fa-edit" title="' + params.data.notes + '"></i></span>'
                      : '<span><i class="fas fa-plus" title="Add notes"></i></span>';
              },
              cellStyle: { cursor: "pointer", "text-align": "center" },
              hide: this.isAGGridReadOnly
          },
          {
              headerName: "Case Code",
              field: "oldCaseCode",
              maxWidth: 100
          },
          {
              headerName: "Case/Opp Name",
              field: "caseName",
              minWidth: 150,
              cellRenderer: function (params) {
                  if (params.data.oldCaseCode) {
                      return params.data.caseName;
                  } else if (!params.data.oldCaseCode && params.data.pipelineId) {
                      return params.data.opportunityName;
                  }
                  return params.value;
              },
              cellStyle: { cursor: "pointer", color: "#005999" }
          },
          { headerName: "Client", field: "clientName", minWidth: 150 },
          {
              headerName: "Case Manager",
              field: "caseManagerName",
              minWidth: 150,
              cellStyle: { cursor: "pointer", color: "#005999" }
          },
          { headerName: "LG", field: "currentLevelGrade", maxWidth: 80 },
          {
              headerName: "Alloc",
              field: "allocation",
              maxWidth: 70,
              editable: !this.isAGGridReadOnly,
              cellStyle: function (params) {
                  let paramValue = params.value.toString();
                  if (paramValue.indexOf("%") !== -1) {
                      paramValue = paramValue.slice(0, paramValue.indexOf("%"));
                  }
                  if (!ValidationService.isAllocationValid(paramValue)) {
                      return { backgroundColor: "lightcoral" };
                  } else {
                      return { backgroundColor: "" };
                  }
              },
              cellRenderer: function (params) {
                  let paramValue = params.value.toString();
                  if (paramValue.indexOf("%") === -1) {
                      paramValue += "%";
                  }
                  return paramValue;
              },
              // comparator is used for custom sorting. Here we are converting the allocation, which is in string,
              // to Int and comparing the values for the order given
              comparator: function (valueA, valueB, nodeA, nodeB, isInverted) {
                  return (
                      parseInt(valueA.slice(0, valueA.indexOf("%"))) - parseInt(valueB.slice(0, valueB.indexOf("%")))
                  );
              },
              valueGetter: (params) => {
                  return params.data.allocation;
              },
              valueSetter: function (params) {
                  //params.context.componentParent.isCellDataValidForUpdate();
                  const existingUserAllocation = params.data;
                  const updatedAllocation = Object.assign({}, params.data);
                  updatedAllocation.allocation = params.newValue;

                  updatedAllocation.allocation = params.newValue;
                  const currentAllocationDecidingParamsForSplit =
                      params.context.componentParent.allocationDecidingParamsForSplit(updatedAllocation);
                  let [canSplitForMonthClose, validationMessage] =
                      params.context.componentParent._resourceAllocationService.canSplitForMonthClose(
                          existingUserAllocation,
                          currentAllocationDecidingParamsForSplit
                      );
                  if (!canSplitForMonthClose && validationMessage) {
                      params.context.componentParent.notifyService.showValidationMsg(validationMessage);
                      return false;
                  }
                  params.data.allocation = updatedAllocation.allocation;
                  return true;
              }
          },
          {
              headerName: "Investment",
              field: "investmentCode",
              maxWidth: 140,
              editable: !this.isAGGridReadOnly,
              cellEditor: "agRichSelectCellEditor",
              cellEditorParams: {
                  cellHeight: 35,
                  values: this.investmentCategories.map((investmentCategory) => investmentCategory.investmentName)
              },
              valueSetter: function (params) {
                  /**When we select a value from our drop down list, this function will make sure
        that our row's record receives the "investmentCode" of the chosen selection.*/
                  const existingUserAllocation = params.data;
                  const updatedAllocation = Object.assign({}, params.data);
                  updatedAllocation.investmentCode = params.context.componentParent.investmentCategories.find(
                      (investmentCategory) => investmentCategory.investmentName == params.newValue
                  )?.investmentCode;

                  const isInvestmentTypeValid = ValidationService.isInvestmentTypeValid(updatedAllocation);
                  if (!isInvestmentTypeValid.isValid) {
                      params.data.investmentCode = params.context.componentParent.investmentCategories.find(
                          (investmentCategory) => investmentCategory.investmentName == params.oldValue
                      )?.investmentCode;
                      params.context.componentParent.notifyService.showValidationMsg(
                          isInvestmentTypeValid.errorMessage
                      );
                      return false;
                  } else {
                      updatedAllocation.allocation = params.newValue;
                      const currentAllocationDecidingParamsForSplit =
                          params.context.componentParent.allocationDecidingParamsForSplit(updatedAllocation);
                      let [canSplitForMonthClose, validationMessage] =
                          params.context.componentParent._resourceAllocationService.canSplitForMonthClose(
                              existingUserAllocation,
                              currentAllocationDecidingParamsForSplit
                          );
                      if (!canSplitForMonthClose && validationMessage) {
                          params.context.componentParent.notifyService.showValidationMsg(validationMessage);
                          return false;
                      }
                  }

                  params.data.investmentCode = updatedAllocation.investmentCode;
                  return true;
              },

              valueGetter: function (params) {
                  /**  We don't want to display the raw "investmentCode" value.. we actually want
        the "investmentName" string for that code. */
                  let investmentCategoryName = "";
                  if (!!params.data.investmentCode) {
                      investmentCategoryName = params.context.componentParent.investmentCategories.find(
                          (investmentCategory) => investmentCategory.investmentCode === params.data.investmentCode
                      )?.investmentName;
                  }
                  return investmentCategoryName;
              }
          },
          {
              headerName: "Role",
              field: "caseRoleCode",
              editable: !this.isAGGridReadOnly,
              maxWidth: 70,
              cellEditor: "agRichSelectCellEditor",
              cellEditorParams: {
                  cellHeight: 45,
                  values: this.caseRoleTypes.map((caseRoleType) => caseRoleType.caseRoleName)
              },
              valueSetter: function (params) {
                  /**When we select a value from our drop down list, this function will make sure
        that our row's record receives the "caseRoleCode" of the chosen selection.*/
                  const existingUserAllocation = params.data;
                  const updatedAllocation = Object.assign({}, params.data);
                  updatedAllocation.caseRoleCode = params.context.componentParent.caseRoleTypes.find(
                      (caseRoleType) => caseRoleType.caseRoleName === params.newValue
                  )?.caseRoleCode;

                  updatedAllocation.allocation = params.newValue;
                  const currentAllocationDecidingParamsForSplit =
                      params.context.componentParent.allocationDecidingParamsForSplit(updatedAllocation);
                  let [canSplitForMonthClose, validationMessage] =
                      params.context.componentParent._resourceAllocationService.canSplitForMonthClose(
                          existingUserAllocation,
                          currentAllocationDecidingParamsForSplit
                      );
                  if (!canSplitForMonthClose && validationMessage) {
                      params.context.componentParent.notifyService.showValidationMsg(validationMessage);
                      return false;
                  }

                  params.data.caseRoleCode = updatedAllocation.caseRoleCode;
                  return true;
              },

              valueGetter: function (params) {
                  /**  We want to display the raw "caseRoleCode" value */
                  let caseRoleTypeCode = "";
                  if (!!params.data.caseRoleCode) {
                      caseRoleTypeCode = params.context.componentParent.caseRoleTypes.find(
                          (caseRoleType) => caseRoleType.caseRoleCode === params.data.caseRoleCode
                      )?.caseRoleCode;
                  }
                  return caseRoleTypeCode;
              }
          },
          {
              headerName: "Start Date",
              field: "startDate",
              maxWidth: 95,
              editable: !this.isAGGridReadOnly,
              // cellEditor: 'datePickerRenderer',
              cellEditor: "datePicker",
              comparator: function (valueA, valueB, nodeA, nodeB, isInverted) {
                  return Date.parse(valueA) - Date.parse(valueB);
              },
              cellStyle: function (params) {
                  if (
                      !ValidationService.isValidDate(params.value) ||
                      Date.parse(params.data.startDate) > Date.parse(params.data.endDate) ||
                      moment(params.data.joiningDate).isAfter(params.data.startDate, "day")
                  ) {
                      return { backgroundColor: "lightcoral" };
                  } else {
                      return { backgroundColor: "" };
                  }
              },
              valueGetter: (params) => {
                  return params.data.startDate;
              },
              valueSetter: function (params) {
                  //params.context.componentParent.isCellDataValidForUpdate();
                  const existingUserAllocation = params.data;
                  const updatedAllocation = Object.assign({}, params.data);
                  updatedAllocation.startDate = params.newValue;

                  const [isValidaAllocation, errorMessage] =
                      params.context.componentParent._resourceAllocationService.validateMonthCloseForUpdates(
                          updatedAllocation,
                          existingUserAllocation
                      );
                  if (!isValidaAllocation) {
                      params.context.componentParent.notifyService.showValidationMsg(errorMessage);
                      return false;
                  }
                  params.data.startDate = updatedAllocation.startDate;
                  return true;
              },
              filter: "agDateColumnFilter",
              filterParams: {
                  applyButton: true,
                  clearButton: true,
                  comparator: function (filterLocalDateAtMidnight, cellValue) {
                      const cellDate = new Date(cellValue);
                      if (filterLocalDateAtMidnight.getTime() === cellDate.getTime()) {
                          return 0;
                      }
                      if (cellDate < filterLocalDateAtMidnight) {
                          return -1;
                      }
                      if (cellDate > filterLocalDateAtMidnight) {
                          return 1;
                      }
                  }
              }
          },
          {
              headerName: "End Date",
              field: "endDate",
              maxWidth: 95,
              editable: !this.isAGGridReadOnly,
              cellEditor: "datePicker",
              comparator: function (valueA, valueB, nodeA, nodeB, isInverted) {
                  return Date.parse(valueA) - Date.parse(valueB);
              },
              cellStyle: function (params) {
                  if (
                      !ValidationService.isValidDate(params.value) ||
                      Date.parse(params.data.startDate) > Date.parse(params.data.endDate)
                  ) {
                      return { backgroundColor: "lightcoral" };
                  } else {
                      return { backgroundColor: "" };
                  }
              },
              valueGetter: (params) => {
                  return params.data.endDate;
              },
              valueSetter: function (params) {
                  //params.context.componentParent.isCellDataValidForUpdate();
                  const existingUserAllocation = params.data;
                  const updatedAllocation = Object.assign({}, params.data);
                  updatedAllocation.endDate = params.newValue;

                  const [isValidaAllocation, errorMessage] =
                      params.context.componentParent._resourceAllocationService.validateMonthCloseForUpdates(
                          updatedAllocation,
                          existingUserAllocation
                      );
                  if (!isValidaAllocation) {
                      params.context.componentParent.notifyService.showValidationMsg(errorMessage);
                      return false;
                  }
                  params.data.endDate = updatedAllocation.endDate;
                  return true;
              },
              filter: "agDateColumnFilter",
              filterParams: {
                  applyButton: true,
                  clearButton: true,
                  comparator: function (filterLocalDateAtMidnight, cellValue) {
                      const cellDate = new Date(cellValue);
                      if (filterLocalDateAtMidnight.getTime() === cellDate.getTime()) {
                          return 0;
                      }
                      if (cellDate < filterLocalDateAtMidnight) {
                          return -1;
                      }
                      if (cellDate > filterLocalDateAtMidnight) {
                          return 1;
                      }
                  }
              }
          },
          { headerName: "Industry", field: "primaryIndustry" },
          { headerName: "PEG Industry", field: "pegIndustryTerm" },
          { headerName: "Capability", field: "primaryCapability" }
      ];
      this.components = { datePicker: this.getDatePicker() };
      // this.frameworkComponents = {
      //   datePickerRenderer: AgGridDatepickerComponent
      // };
  }

  getDatePicker() {
      function Datepicker() {}
      Datepicker.prototype.init = function (params) {
          this.eInput = document.createElement("input");
          this.eInput.id = "datePicker";
          this.eInput.type = "date";
          this.eInput.classList.add("aggrid-datePicker");
          this.eInput.value = DateService.formatDate(params.value);
          this.eInput.style.height = "100%";
          this.eInput.setAttribute("data-date", params.value);
          this.eInput.setAttribute("data-date-format", "DD-MMM-YYYY");
          this.eInput.onchange = function () {
              const el = <HTMLInputElement>document.getElementById("datePicker");
              const formattedDate = moment(el.value, "YYYY-MM-DD").format(el.getAttribute("data-date-format"));
              el.setAttribute("data-date", formattedDate);
          };
      };
      Datepicker.prototype.getGui = function () {
          return this.eInput;
      };
      Datepicker.prototype.afterGuiAttached = function () {
          this.eInput.focus();
          this.eInput.select();
      };
      Datepicker.prototype.getValue = function () {
          return DateService.convertDateInBainFormat(this.eInput.value);
      };
      Datepicker.prototype.destroy = function () {};
      Datepicker.prototype.isPopup = function () {
          return true;
      };
      return Datepicker;
  }

  // onRowDataChanged(params) {
  //   params.api.sizeColumnsToFit();
  //   //params.api.refreshCells();
  //   //this.keepFocusOnCellAfterAgGridRefresh(params);
  // }

  private keepFocusOnCellAfterAgGridRefresh(gridOptions) {
      const cell = gridOptions.api.getFocusedCell();
      if (cell) {
          if (
              cell.column.colId === "startDate" ||
              cell.column.colId === "endDate" ||
              cell.column.colId === "allocation"
          ) {
              gridOptions.api.setFocusedCell(cell.rowIndex, cell.column.colId);
          } else if (cell.column.colId === "investmentCode" || cell.column.colId === "caseRoleCode") {
              gridOptions.api.startEditingCell({ rowIndex: cell.rowIndex, colKey: cell.column.colId });
          }
      }
  }

  allocationDecidingParamsForSplit(allocation) {
      return {
          allocation: allocation.allocation,
          investmentCode: allocation.investmentCode,
          caseRoleCode: allocation.caseRoleCode,
          startDate: allocation.startDate,
          endDate: allocation.endDate
      };
  }

  onCellValueChanged(params) {
      if (!(params.newValue === params.oldValue)) {
          /*The following if statement is used to handle the issue for investment category header.
    If a resource does not have any investemt category and user reselects "select any" field, then the newValue
    and oldValue are "null" and null respectively */
          if (!(params.newValue === "null" && params.oldValue == null)) {
              this.resourceData = params.data;
              let allocation = this.resourceData.allocation;
              if (this.resourceData.allocation.indexOf("%") !== -1) {
                  allocation = this.resourceData.allocation.slice(0, this.resourceData.allocation.indexOf("%"));
              }
              if (!ValidationService.isAllocationValid(allocation)) {
                  this.notifyService.showValidationMsg(ValidationService.numberInvalidMessage);
                  this.isAllocationValid = false;
              } else {
                  this.isAllocationValid = true;
                  params.data.allocation = allocation;
              }
              const startDate = this.resourceData.startDate;
              if (!ValidationService.isValidDate(startDate)) {
                  this.notifyService.showValidationMsg(ValidationService.dateInvalidMessage);
                  this.isStartDateValid = false;
              } else if (moment(this.resourceData.joiningDate).isAfter(moment(startDate), "day")) {
                  this.notifyService.showValidationMsg(
                      ValidationService.employeeJoiningDateGreaterThanStartDate.replace(
                          "[joiningDate]",
                          DateService.convertDateInBainFormat(this.resourceData.joiningDate)
                      )
                  );
                  this.isStartDateValid = false;
              } else {
                  this.isStartDateValid = true;
              }
              const endDate = this.resourceData.endDate;
              if (!ValidationService.isValidDate(endDate)) {
                  this.notifyService.showValidationMsg(ValidationService.dateInvalidMessage);
                  this.isEndDateValid = false;
              } else {
                  this.isEndDateValid = true;
              }
              if (this.isStartDateValid && this.isEndDateValid) {
                  if (Date.parse(startDate) > Date.parse(endDate)) {
                      this.isStartDateValid = false;
                      this.isEndDateValid = false;
                      this.notifyService.showValidationMsg(ValidationService.startDateGreaterThanEndDate);
                  } else {
                      this.isStartDateValid = true;
                      this.isEndDateValid = true;
                  }
              }

              if (this.isAllocationValid && this.isStartDateValid && this.isEndDateValid) {
                  const resourceDataToBeUpdated = params.data;
                  /**
                   * NOTE:  Here params.data is the data we get from ResourceHistory model and we need ResourceAllocation model for updating data.
                   * LevelGrade and CurrentLevelGrade keys are different in both the models.
                   * Similarly, in ResourceHistory model, allocation is get as a string with % sign in it but in ResourceAllocation model,
                   * it is get as int without % sign.
                   * This is just a work around as we have to discuss it before changing it.
                   * */
                  resourceDataToBeUpdated.currentLevelGrade = params.data.currentLevelGrade;
                  resourceDataToBeUpdated.allocation =
                      params.data.allocation.indexOf("%") !== -1
                          ? params.data.allocation.slice(0, params.data.allocation.indexOf("%"))
                          : params.data.allocation;
                  if (resourceDataToBeUpdated.investmentCode === "null") {
                      resourceDataToBeUpdated.investmentCode = null;
                  } else {
                      resourceDataToBeUpdated.investmentCode = params.data.investmentCode;
                  }
                  if (resourceDataToBeUpdated.caseRoleCode === "null") {
                      resourceDataToBeUpdated.caseRoleCode = null;
                  } else {
                      resourceDataToBeUpdated.caseRoleCode = params.data.caseRoleCode;
                  }

                  let allocationbeforeupdate = this.resourceDetails.staffingHistory.find(
                      (x) => x.id === resourceDataToBeUpdated.id
                  );
                  allocationbeforeupdate.allocation = allocationbeforeupdate.allocation.substring(
                      0,
                      allocationbeforeupdate.allocation.indexOf("%")
                  );
                  let allocationsToUpdate: ResourceAllocation[] = [];
                  let successMessage = null;
                  let isValidAllocation = true;
                  const currentAllocationDecidingParamsForSplit =
                      this.allocationDecidingParamsForSplit(resourceDataToBeUpdated);

                  let [canSplitForMonthClose, validationMessage] =
                      this._resourceAllocationService.canSplitForMonthClose(
                          allocationbeforeupdate,
                          currentAllocationDecidingParamsForSplit
                      );
                  if (canSplitForMonthClose) {
                      [allocationsToUpdate, successMessage] =
                          this._resourceAllocationService.splitAlloctionForMonthClose(
                              allocationbeforeupdate,
                              resourceDataToBeUpdated
                          );
                  } else {
                      [isValidAllocation, validationMessage] =
                          this._resourceAllocationService.validateMonthCloseForUpdates(
                              resourceDataToBeUpdated,
                              allocationbeforeupdate
                          );
                      allocationsToUpdate.push(resourceDataToBeUpdated);
                  }

                  if (validationMessage) {
                      this.notifyService.showValidationMsg(validationMessage);
                      return;
                  }
                  this.checkForPrePostAndUpsertResourceAllocation(allocationsToUpdate, successMessage);
              }
          }
      }
  }

  checkForPrePostAndUpsertResourceAllocation(resourceAllocations: ResourceAllocation[], successMessage?) {
      const projectStartDate = DateService.convertDateInBainFormat(
          resourceAllocations[0].oldCaseCode
              ? resourceAllocations[0].caseStartDate
              : resourceAllocations[0].opportunityStartDate
      );
      const projectEndDate = DateService.convertDateInBainFormat(
          resourceAllocations[0].oldCaseCode
              ? resourceAllocations[0].caseEndDate
              : resourceAllocations[0].opportunityEndDate
      );
      let allocationsData: ResourceAllocation[] = [];

      if (projectStartDate && projectEndDate) {
          if (Array.isArray(resourceAllocations)) {
              resourceAllocations.forEach((resourceAllocation) => {
                  allocationsData = allocationsData.concat(
                      this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(
                          resourceAllocation
                      )
                  );
              });
          } else {
              allocationsData = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(
                  resourceAllocations
              );
          }
      } else {
          allocationsData = resourceAllocations;
      }

      this.upsertResourceAllocationsToProject.emit({
          resourceAllocation: allocationsData,
          event: "casesGrid",
          showMoreThanYearWarning: false,
          splitSuccessMessage: successMessage
      });
  }

  // -------------------Service/API Calls -----------------------------------------------

  getDetailsForResource(employeeCode) {
      this.getEmployeeInfoForHeader(employeeCode);
      this.getResourceActiveStaffableAsByEmployeeCode(employeeCode);
      this.getResourceStaffingResponsibleDataByEmployeeCode(employeeCode);
      this.getAllCommitmentsForEmployee(employeeCode, this.commitmentStartDate);
      if (this.agGridRowData) {
          this.getStaffingHistoryForEmployee();
      }
      this.getEmployeeRatings(employeeCode);
  }

  getEmployeeInfoForHeader(employeeCode) {
      this._overlayService.getEmployeeInfoWithGxcAffiliations(employeeCode).subscribe((details) => {
          this.resourceDetails.resource = details.employee;
          this.resourceDetails.practiceAreaAffiliations = details.affiliationsByEmployeeCodesAndPracticeAreaCodes;
          this.resourceDetails.advisor = details.employeeAdvisor;
          this.resourceDetails.mentees = details.employeeMentees;
          this.resourceDetails.timeInLevelData = details.employeeTimeInLevel[0]; //API returns in Array format
          //this.resourceDetails.levelGrade = details.employee.levelGrade;
      });
  }

  getEmployeeRatings(employeeCode) {
      this._overlayService.getEmployeeRatings(employeeCode).subscribe((ratings) => {
          this.resourceRatings = ratings.sort((a, b) => <any>new Date(b.lastUpdated) - <any>new Date(a.lastUpdated));
      });
  }

  getAllCommitmentsForEmployee(employeeCode, effectiveDate?) {
      let effectiveFromDate = effectiveDate
          ? DateService.convertDateInBainFormat(effectiveDate)
          : DateService.convertDateInBainFormat(this.today);
      effectiveFromDate = DateService.convertDateInBainFormat(effectiveFromDate);
      this._overlayService.getAllCommitmentsForEmployee(employeeCode, effectiveFromDate).subscribe((details) => {
          const commitments = this.getCommitments(details);

          this.resourceDetails.commitments = commitments;

          // Added resource allocations to subject to use on gantt chart
          this._ganttService.resourceCommitments.next(commitments);
      });
  }

  populateGanttData() {
      this.getAllCommitmentsForEmployee(this.data.employeeCode, this.commitmentStartDate);
  }

  getStaffingHistoryForEmployee() {
      const employeeCode = this.data.employeeCode;
      this._overlayService.getHistoricalStaffingAllocationsByEmployee(employeeCode).subscribe((staffingHistory) => {
          this.resourceDetails.staffingHistory = staffingHistory;
          this.resourceDetails.staffingHistory.forEach((history) => {
              history.allocation = history.allocation + "%";
              history.startDate = new DatePipe("en-US").transform(history.startDate, "dd-MMM-yyyy");
              history.endDate = new DatePipe("en-US").transform(history.endDate, "dd-MMM-yyyy");
              history.joiningDate = this.resourceDetails.resource?.startDate;
          });

          const historicalAllocationsData = JSON.parse(JSON.stringify(this.resourceDetails.staffingHistory));
          if (!this.agGridRowData) {
              this.agGridRowData = historicalAllocationsData;
          } else {
              const rowsToBeInserted = historicalAllocationsData.filter(
                  (o) => !this.agGridRowData.some((r) => o.id === r.id)
              );

              if (rowsToBeInserted.length > 0) {
                  this.agGridRowData = this.agGridRowData.concat(rowsToBeInserted);
                  this.gridApi.applyTransaction({ add: rowsToBeInserted });
              }

              const rowsToBeDeleted = this.agGridRowData.filter(
                  (allocation) => !historicalAllocationsData.some((r) => allocation.id === r.id)
              );

              if (rowsToBeDeleted.length > 0) {
                  rowsToBeDeleted.forEach((r) => {
                      this.agGridRowData.splice(this.agGridRowData.indexOf(r), 1);
                  });
                  this.gridApi.applyTransaction({ remove: rowsToBeDeleted });
              }

              const itemsToUpdate = [];

              const updatedData = historicalAllocationsData;

              if (updatedData) {
                  this.gridApi.forEachNodeAfterFilterAndSort(function (rowNode, index) {
                      const updatedRowData = updatedData.find((r) => r.id === rowNode.data.id);
                      if (updatedRowData) {
                          const data = rowNode.data;
                          data.notes = updatedRowData.notes;
                          data.startDate = updatedRowData.startDate;
                          data.endDate = updatedRowData.endDate;
                          data.allocation = updatedRowData.allocation;
                          data.investmentCode = updatedRowData.investmentCode;
                          data.caseRoleCode = updatedRowData.caseRoleCode;
                          itemsToUpdate.push(data);
                      }
                  });
                  this.gridApi.applyTransaction({ update: itemsToUpdate });
              }
          }
      });
  }

  getAuditTrailForEmployee() {
      const employeeCode = this.data.employeeCode;
      this.logLoading = true;

      this._overlayService
          .getAuditTrailForEmployee(employeeCode, this.pageSize, this.auditLogStartIndex)
          .subscribe((auditLog) => {
              if (!this.pageScrolled) {
                  this.resourceDetails.auditTrails = [];
              }

              if (auditLog.length > 0) {
                  this.resourceDetails.auditTrails = this.resourceDetails.auditTrails.concat(auditLog);
              } else {
                  this.emptyLogMessage = ConstantsMaster.ganttConstants.emptyTemplateText;
              }
              this.logLoading = false;
          });
  }

  onAuditLogScrolled() {
      // if total logs displayed are less than the page size, it means there are not more logs that needs to be fetched
      if (this.resourceDetails.auditTrails.length < this.pageSize + this.auditLogStartIndex) {
          return;
      }

      this.getMoreLogs();
  }

  getMoreLogs() {
      this.auditLogStartIndex = this.auditLogStartIndex + this.pageSize;
      this.pageScrolled = true;

      this.getAuditTrailForEmployee();
  }

  getEmployeeStaffingPreferences() {
      const employeeCode = this.data.employeeCode;
      this._overlayService.getEmployeeStaffingPreferences(employeeCode).subscribe((employeeStaffingPreferences) => {
          this.employeeStaffingPreferences = employeeStaffingPreferences;
      });
  }

  updateEmployeeStaffingPreferencesHandler(event) {
      this._overlayService.upsertEmployeeStaffingPreferences(event).subscribe((employeeStaffingPreferences) => {
          if (employeeStaffingPreferences) {
              this.notifyService.showSuccess("Preference list successfully updated", "Success");
          } else {
              this.notifyService.showError("An error occurred while updating preference list", "Error");
          }
      });
  }

getSmapMissionNotes(){
    this._overlayService.getSmapMissionNotes(this.data.employeeCode).subscribe((smapMissionNotes) =>{

        this.smapMissionNotes = smapMissionNotes? smapMissionNotes[0] :{};
    })
}

getResourceNotes() {
    const employeeCode = this.data.employeeCode;
    const noteTypeCode = NoteTypeCode.RESOURCE_PROFILE_NOTE;
    this._overlayService.getResourceNotes(employeeCode,noteTypeCode).subscribe((resourceNotes) => {
        this.resourceNotes = resourceNotes;
        })
    }


    getResourceAboutData() {
        this._overlayService.getLevelGradeHistoryData(this.data.employeeCode).subscribe((data) => {
            this.isLoading = false;
            this.levelGradeHistoryData = data;
        });

        this._overlayService
            .getGlobalTrainingsData(this.data.employeeCode)
            .subscribe((data) => {

                this.isLoading = false;
                this.globalTrainingsData = data.sort((a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime());
            });

        this._overlayService
            .getEmployeeLanguagesByEmployeeCodes(this.data.employeeCode)
            .subscribe((employeesLanguages) => {
                this.isLoading = false;
                this.languages = employeesLanguages?.find((x) => x.employeeCode === this.data.employeeCode)?.languages;
            });

        this._overlayService.getEmployeeTransfersByEmployeeCodes(this.data.employeeCode).subscribe((data) => {
            this.isLoading = false;
            this.tranferData = data;
        });

        this._overlayService
            .getCertificationsByEmployeeCodes(this.data.employeeCode)
            .subscribe((employeesCertificates: EmployeeCertification[]) => {
                this.isLoading = false;
                this.certificates = employeesCertificates?.find(
                    (x) => x.employeeCode === this.data.employeeCode
                )?.certifications;
            });

        this._overlayService
            .getEmployeeWorkAndSchoolHistory(this.data.employeeCode)
            .subscribe((employeeWorkAndSchoolHistory: EmployeeWorkAndSchoolHistory) => {
                this.isLoading = false;
                this.employeeSchoolHistory = employeeWorkAndSchoolHistory.educationHistory;
                this.employeeWorkHistory = employeeWorkAndSchoolHistory.employmentHistory;
            },
                err => {
                    console.log("ERROR");
                    this.apiDown = true;
                });
    }

  getResourceActiveStaffableAsByEmployeeCode(employeeCode: string) {
      this._overlayService
          .getResourceActiveStaffableAsByEmployeeCode(employeeCode)
          .subscribe((staffableAsRoleApiResponse: StaffableAsRole[]) => {
              if (staffableAsRoleApiResponse?.length > 0) {
                  this.setStaffableAsRoleForAboutTab(staffableAsRoleApiResponse);
              } else {
                  this.activeStaffableAsRoleName = "";
              }
              this.staffableAsRoles = staffableAsRoleApiResponse;
          });
  }

  getResourceStaffingResponsibleDataByEmployeeCode(employeeCode: string) {
    this.resourceOverlayStore.dispatch(
        new ResourceOverlayActions.GetResourceStaffingInfo(employeeCode)
    );

   
}   

  removeStaffableAsRoleEventEmitterHandler(event) {
      this.deleteStaffableAsRoleEmitter.emit(event);
  }

  upsertStaffableAsRoleEventEmitterHandler(staffableRoles) {
      this.upsertStaffableAsRoleEmitter.emit({
          staffableRoles: staffableRoles,
          resource: this.resourceDetails.resource
      });
  }

  setStaffableAsRoleForAboutTab(updateStaffableAsRolesApiResponse) {
      let activeStaffableAsRole = updateStaffableAsRolesApiResponse.find((x) => x.isActive);
      let staffableAsTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffableAsTypes);
      this.activeStaffableAsRoleName = staffableAsTypes.find(
          (x) => x.staffableAsTypeCode === activeStaffableAsRole.staffableAsTypeCode
      ).staffableAsTypeName;
      this.staffableAsRoles = [activeStaffableAsRole];
  }

  getCommitmentTypes() {
      this._overlayService.getCommitmentTypes().subscribe((commitmentTypesData) => {
          this.commitmentTypes = commitmentTypesData;

          // add a "select type" item to the list
          const dummyCategory = { commitmentTypeCode: "", commitmentTypeName: "Select Type", precedence: 0 };
          this.commitmentTypes.splice(0, 0, dummyCategory);
      });
  }

  getCommitments(details) {
    const placeholderAllocatons = this.getPlaceholderCommitments(details.placeholderAllocations);
    const caseCommitments = this.getCaseOppCommitments(details.staffingAllocations);
    const otherCommitments = this.getOtherCommitments(details)

    let commitments = [];

    commitments = commitments
        .concat(placeholderAllocatons)
        .concat(caseCommitments)
        .concat(otherCommitments)

    return commitments;
  }

  isNotConfirmedAllocation(allocation) {
    return (allocation.oldCaseCode || allocation.pipelineId) && !allocation.isConfirmed;
  }

  isPlanningCardShared(allocation) {
    return allocation.planningCardId && allocation.isPlanningCardShared;
  }

  getPlaceholderCommitments(placeholderAllocationDetails){
    const placeholderParentId = CommonService.generate_UUID();
    //exclude confirmed placeholders on Case/Opps
    const placeholderAllocationsExceptConfirmed = placeholderAllocationDetails.filter( x=> this.isNotConfirmedAllocation(x) || this.isPlanningCardShared(x));
    let placeholderAllocations = placeholderAllocationsExceptConfirmed.map((a)=>{
          return {
              parent: placeholderParentId,
              id: a.id,
              billingOfficeAbbreviation: a.billingOfficeAbbreviation,
              billingOfficeCode: a.billingOfficeCode,
              billingOfficeName: a.billingOfficeName,
              caseCode: a.caseCode,
              planningCardTitle : a.planningCardTitle,
              caseName: a.caseName,
              clientName: a.clientName,
              clientCode: a.clientCode,
              levelGrade: a.currentLevelGrade,
              oldCaseCode: a.oldCaseCode,
              pipelineId: a.pipelineId,
              caseTypeCode: a.caseTypeCode,
              opportunityName: a.opportunityName,
              startDate: a.startDate,
              endDate: a.endDate,
              commitmentTypeCode: CommitmentTypeEnum.NAMED_PLACEHOLDER,
              type: !a.planningCardId ? (!a.oldCaseCode ? "Opportunity" : "Case") : "PlanningCard",
              status: !a.oldCaseCode ? "" : "Active",
              description: !a.oldCaseCode
                ? !a.planningCardId ? a.probabilityPercent
                    ? `${a.probabilityPercent}% - ${a.clientName} - ${a.opportunityName}`
                    : `${a.clientName} - ${a.opportunityName}`
                    :`${a.planningCardTitle}`
                : `${a.oldCaseCode} - ${a.clientName} - ${a.caseName}`,
              allocation: a.allocation,
              investmentCode: a.investmentCode,
              caseRoleCode: a.caseRoleCode,
              source: "staffing",
              caseStartDate: a.caseStartDate,
              caseEndDate: a.caseEndDate,
              opportunityStartDate: a.opportunityStartDate,
              opportunityEndDate: a.opportunityEndDate,
              notes: a.notes,
              caseRoleName: a.caseRoleName,
              planningCardId: a.planningCardId,
              isPlaceholderAllocation: a.isPlaceholderAllocation,
              isPlanningCardShared: a.isPlanningCardShared,
              isConfirmed: a.isConfirmed
          };
    });

    placeholderAllocations.sort((a, b) => {
        return <any>new Date(a.startDate) - <any>new Date(b.startDate);
    });

    if (placeholderAllocations.length > 0) {
        placeholderAllocations.push({
            id: placeholderParentId,
            startDate: placeholderAllocations[0].startDate,
            endDate: placeholderAllocations[placeholderAllocations.length - 1].endDate,
            commitmentTypeCode: '',
            type: "Placeholders"
        });
    }


    return placeholderAllocations;
  }

  getCaseOppCommitments(staffingAllocationDetails){
    const caseOppCommitmentsParentId = CommonService.generate_UUID();
    let caseCommitments = staffingAllocationDetails.map((a) => {
        return {
            id: a.id,
            parent: caseOppCommitmentsParentId,
            caseCode: a.caseCode,
            caseName: a.caseName,
            clientName: a.clientName,
            clientCode: a.clientCode,
            levelGrade: a.currentLevelGrade,
            oldCaseCode: a.oldCaseCode,
            pipelineId: a.pipelineId,
            caseTypeCode: a.caseTypeCode,
            opportunityName: a.opportunityName,
            startDate: a.startDate,
            endDate: a.endDate,
            commitmentTypeCode: CommitmentTypeEnum.CASE_OPP,
            type: !a.oldCaseCode ? "Opportunity" : "Case",
            status: !a.oldCaseCode ? "" : "Active",
            description: !a.oldCaseCode
                ? a.probabilityPercent
                    ? `${a.probabilityPercent}% - ${a.clientName} - ${a.opportunityName}`
                    : `${a.clientName} - ${a.opportunityName}`
                : `${a.oldCaseCode} - ${a.clientName} - ${a.caseName}`,
            allocation: a.allocation,
            investmentCode: a.investmentCode,
            caseRoleCode: a.caseRoleCode,
            source: "staffing",
            caseStartDate: a.caseStartDate,
            caseEndDate: a.caseEndDate,
            opportunityStartDate: a.opportunityStartDate,
            opportunityEndDate: a.opportunityEndDate,
            notes: a.notes,
            caseRoleName: a.caseRoleName
        };
    });

    caseCommitments = caseCommitments
        ? caseCommitments?.filter(
            (f) => <any>new Date(f.endDate) >= <any>this.commitmentStartDate.setHours(0, 0, 0, 0)
        )
        : [];

    caseCommitments.sort((a, b) => {
        return <any>new Date(a.startDate) - <any>new Date(b.startDate);
    });

    if (caseCommitments.length > 0) {
        caseCommitments.push({
            id: caseOppCommitmentsParentId,
            startDate: caseCommitments[0].startDate,
            endDate: caseCommitments[caseCommitments.length - 1].endDate,
            commitmentTypeCode: '',
            type: "Confirmed"
        });
    }

    return caseCommitments;
  }

  getOtherCommitments(allDetails){
    const otherCommitmentsId = CommonService.generate_UUID();
    // Trainings From BVU
    const trainings = allDetails.trainings.map((t) => {
        return {
            parent: otherCommitmentsId,
            startDate: t.startDate,
            endDate: t.endDate,
            commitmentTypeCode: CommitmentTypeEnum.TRAINING,
            type: t.type,
            description: `${t.role} - ${t.trainingName}`,
            allocation: "100%"
        };
    });

    // Commitments created in BOSS system except vacations and short term available
    // Vacations will be shown as split task in one row
    // Short term available will be checked for overlapping with LOA
    const commitmentsSavedInStaffing = allDetails.commitmentsSavedInStaffing
        ?.filter(
            (x) =>
                x.commitmentType.commitmentTypeCode !== CommitmentTypeCodeEnum.VACATION &&
                x.commitmentType.commitmentTypeCode !== CommitmentTypeCodeEnum.SHORT_TERM_AVAILABLE
        )
        .map((c) => {
            return {
                parent: otherCommitmentsId,
                id: c.id,
                startDate: c.startDate,
                endDate: c.endDate,
                type: c.commitmentType.commitmentTypeName,
                commitmentTypeCode: c.commitmentType.commitmentTypeCode,
                commitmentTypePrecedence: c.commitmentType.precedence,
                commitmentTypeReasonCode: c.commitmentTypeReasonCode,
                description: c.notes,
                allocation: c.allocation,
                source: c.isSourceStaffing ? "staffing" : "Other",
                employeeCode: c.employeeCode,
                notes: c.notes
            };
        });

    // Filter out short term availability if overlapped by LOA saved in workday
    let shortTermCommitmentSavedInStaffing = allDetails.commitmentsSavedInStaffing
        ?.filter((x) => x.commitmentType.commitmentTypeCode === CommitmentTypeCodeEnum.SHORT_TERM_AVAILABLE)
        .map((c) => {
            return {
                parent: otherCommitmentsId,
                id: c.id,
                startDate: c.startDate,
                endDate: c.endDate,
                type: c.commitmentType.commitmentTypeName,
                commitmentTypeCode: c.commitmentType.commitmentTypeCode,
                commitmentTypePrecedence: c.commitmentType.precedence,
                description: c.notes,
                source: c.isSourceStaffing ? "staffing" : "Other",
                employeeCode: c.employeeCode,
                notes: c.notes
            };
        });

    allDetails.loa.forEach((loa) => {
        shortTermCommitmentSavedInStaffing = shortTermCommitmentSavedInStaffing?.filter((st) => {
            const isOverlapped =
                moment(st.startDate).isSameOrAfter(moment(loa.startDate)) &&
                moment(st.endDate).isSameOrBefore(moment(loa.endDate));
            return !isOverlapped;
        });
    });

    // Vacation Requests from VRS
    const vacationId = CommonService.generate_UUID();
    const vacationSavedInVRS = allDetails.vacationRequests.map((v) => {
        return {
            parent: vacationId,
            startDate: v.startDate,
            endDate: v.endDate,
            commitmentTypeCode: CommitmentTypeEnum.VACATION,
            type: v.type,
            description: `${v.status} - ${v.description}`,
            allocation: "100%"
        };
    });

    const timeOffsSavedInWorkday = allDetails.employeeTimeOffs.map((v) => {
        return {
            parent: vacationId,
            startDate: v.startDate,
            endDate: v.endDate,
            commitmentTypeCode: CommitmentTypeEnum.VACATION,
            type: v.type,
            description:v.approvedBy && v.status == 'Approved' ? `Approved By: ${v.approvedBy}` :  v.status == 'Submitted'? "Pending Approval":`${v.status}`,
            allocation: "100%"
        };
    });

    const vacationsSavedInStaffing = allDetails.commitmentsSavedInStaffing
        ?.filter((x) => x.commitmentType.commitmentTypeCode === CommitmentTypeCodeEnum.VACATION)
        .map((c) => {
            return {
                id: c.id,
                parent: vacationId,
                startDate: c.startDate,
                endDate: c.endDate,
                type: c.commitmentType.commitmentTypeName,
                commitmentTypeCode: c.commitmentType.commitmentTypeCode,
                commitmentTypePrecedence: c.commitmentType.precedence,
                description: c.notes,
                source: c.isSourceStaffing ? "staffing" : "Other",
                employeeCode: c.employeeCode,
                notes: c.notes
            };
        });

    let vacationRequests = vacationSavedInVRS
        .concat(vacationsSavedInStaffing)
        .sort((a, b) => <any>new Date(a.startDate) - <any>new Date(b.startDate));

    vacationRequests = vacationRequests
        .concat(timeOffsSavedInWorkday)
        .sort((a, b) => <any>new Date(a.startDate) - <any>new Date(b.startDate));

    if (vacationRequests.length > 0) {
        vacationRequests.push({
            parent: otherCommitmentsId,
            id: vacationId,
            startDate: vacationRequests[0].startDate,
            endDate: vacationRequests[vacationRequests.length - 1].endDate,
            commitmentTypeCode: CommitmentTypeEnum.VACATION,
            type: "Vacation",
            render: "split"
        });
    }

    // Office Holidays
    const holidayId = CommonService.generate_UUID();
    const officeHolidays = allDetails.officeHolidays
        .map((h) => {
            return {
                parent: holidayId,
                startDate: h.startDate,
                endDate: h.endDate,
                commitmentTypeCode: CommitmentTypeEnum.HOLIDAY,
                type: h.type,
                description: h.description,
                allocation: "100%"
            };
        })
        .sort((a, b) => <any>new Date(a.startDate) - <any>new Date(b.startDate));

    if (officeHolidays.length > 0) {
        officeHolidays.push({
            parent: otherCommitmentsId,
            id: holidayId,
            startDate: officeHolidays[0].startDate,
            endDate: officeHolidays[officeHolidays.length - 1].endDate,
            commitmentTypeCode: CommitmentTypeEnum.HOLIDAY,
            type: "Holiday",
            render: "split"
        });


    }

    // Transfers From Workday
    const transfers = allDetails.employeeTransfers
        .map((t) => {
            return {
                parent: otherCommitmentsId,
                startDate: t.startDate,
                endDate: t.startDate, // end Date is required by gantt
                commitmentTypeCode: CommitmentTypeEnum.TRANSFER,
                type: t.type,
                description: `Transfer from ${t.operatingOfficeCurrent.officeName} to ${t.operatingOfficeProposed.officeName}`
            };
        })
        .sort((a, b) => <any>new Date(a.startDate) - <any>new Date(b.startDate));

    const employeeTransition =
        allDetails.employeeTransition.startDate === null
            ? [ ]
            : {
                // For Termination we are getting endDate only which is equivalent to termination effective date
                parent: otherCommitmentsId,
                startDate: allDetails.employeeTransition.startDate,
                endDate: allDetails.employeeTransition.endDate,
                commitmentTypeCode: CommitmentTypeEnum.TRANSITION,
                type: allDetails.employeeTransition.type
            };

    // Termination From Workday
    const employeeTermination =
        allDetails.employeeTermination.endDate === null
            ? []
            : {
                  // For Termination we are getting endDate only which is equivalent to termination effective date
                  parent: otherCommitmentsId,
                  startDate: allDetails.employeeTermination.endDate,
                  endDate: allDetails.employeeTermination.endDate,
                  commitmentTypeCode: CommitmentTypeEnum.TERMINATION,
                  type: allDetails.employeeTermination.type
              };

    //LOA from Workday
    const loas =
        allDetails.loa
        .map((t) => {
            return {
                parent: otherCommitmentsId,
                startDate: t.startDate,
                endDate: t.endDate,
                type: t.type,
                commitmentTypeCode: CommitmentTypeEnum.LOA,
                description: t.description
            };
        })
        .sort((a, b) => <any>new Date(a.startDate) - <any>new Date(b.startDate));

    let otherCommitments = [];
    otherCommitments = otherCommitments
        .concat(vacationRequests)
        .concat(officeHolidays)
        .concat(trainings)
        .concat(loas)
        .concat(employeeTransition)
        .concat(employeeTermination)
        .concat(commitmentsSavedInStaffing)
        .concat(shortTermCommitmentSavedInStaffing)
        .concat(transfers);

    // Sort commitments by start date Asc
    otherCommitments.sort((a, b) => {
      return <any>new Date(a.startDate) - <any>new Date(b.startDate);
    });

    if(otherCommitments.length > 0){
        otherCommitments.push({
            id: otherCommitmentsId,
            startDate: otherCommitments[0].startDate,
            endDate: otherCommitments[otherCommitments.length - 1].endDate,
            commitmentTypeCode: '',
            type: "Commitments"
        });
    }

    return otherCommitments;

  }
  // -------------------Component Event Handlers-------------------------------------//

  closeDialog() {
      this.coreService.setShowHideNotification(false);
      const self = this;

      self.showOverlay = false;

      /*
       * Set timeout is required to close the animation with animation and
       * override/delay the defaut closing of material dialog.
       * If not used, the overlay will not close as per our requored animation
       * dialogref.close is required to unload the overlay component.
       */
      setTimeout(function () {
          if (self.router.url.toLowerCase().includes("/overlay?employee")) {
              this.routeQueryParamSubsciption = self.route.queryParamMap.subscribe((queryParams) => {
                  const reportType = queryParams.get("reportType");

                  switch (reportType) {
                      case "1":
                          self.router.navigate([`/${ConstantsMaster.appScreens.report.staffingAllocation}`]);
                          break;
                      case "2":
                          self.router.navigate([`/${ConstantsMaster.appScreens.report.dailyDeploymentDetails}`]);
                          break;
                      case "3":
                          self.router.navigate([
                              `/${ConstantsMaster.appScreens.report.weeklyDeploymentIndividualResourceDetails}`
                          ]);
                          break;
                      case "4":
                          self.router.navigate([`/${ConstantsMaster.appScreens.report.weeklyDeploymentSummaryView}`]);
                          break;
                      // case '5':
                      //   self.router.navigate(['/analytics/resourceAllocationsToday']);
                      //   break;
                      case "6":
                          self.router.navigate([`/${ConstantsMaster.appScreens.report.ringfenceStaffing}`]);
                          break;
                      case "12":
                          self.router.navigate([`/${ConstantsMaster.appScreens.report.monthlyDeployment}`]);
                          break;
                      case "13":
                          self.router.navigate([`/${ConstantsMaster.appScreens.report.practiceStaffing}`]);
                          break;
                      default:{
                        const redirectUrl = self.urlService.redirectURL || '/home';
                        self.router.navigate([redirectUrl]);
                      }

                  }
                  self.dialogRef.close();
              });
              this.routeQueryParamSubsciption.unsubscribe();
          } else {
              self.dialogRef.close();
          }
      }, 1000);
  }

  openTab(tabName) {
      this.openedTab = tabName;
  }

  openProjectDetailsDialog($event) {
      if ($event.colDef.field === "notes") {
          this.gridApi.deselectAll();
          this.openNotesDialog.emit({
              projectData: $event.data
          });
      } else if ($event.colDef.field === "") {
          this.deleteSelectedProjectsConfirmation($event.data);
      } else if ($event.colDef.field === "caseName") {
          // event.detail defines the type of click event if it double click (2) or single click (1)
          if ($event.event.detail === 2) {
              return false;
          }
          this.openProjectDetailsFromResourceDialog.emit($event.data);
      } else if ($event.colDef.field === "caseManagerName") {
          this.openResourceDetailsFromProjectDialog.emit($event.data.caseManagerCode);
      }
  }

  deleteSelectedProjectsConfirmation(projectData) {
      const [isValidAllocation, errorMessage] =
          this._resourceAllocationService.validateMonthCloseForInsertAndDelete(projectData);

      if (!isValidAllocation) {
          this.notifyService.showValidationMsg(errorMessage);
          return;
      } else {
          const selectedIds = projectData.length > 0 ? projectData.map((elem) => elem.id).join(",") : projectData.id;
          const confirmationPopUpBodyMessage =
              "You are about to remove selected allocation(s) for " +
              this.resourceDetails.resource.fullName +
              ". Are you sure you want to remove the selected allocation(s) ?";
          this.openSystemConfirmationToDeleteSelectedProjects({
              allocationIds: selectedIds,
              projectName: projectData.caseName,
              confirmationPopUpBodyMessage: confirmationPopUpBodyMessage
          });
      }
  }

  deleteSelectetAllocationsCommitmentsConfirmation(data) {
      const commitmentIds = data
          .filter((r) => r.type.toLowerCase() !== "case" && r.type.toLowerCase() !== "opportunity")
          .map((commitment) => commitment.id)
          .join(",");
      const allocationIds = data
          .filter((r) => r.type.toLowerCase() === "case" || r.type.toLowerCase() === "opportunity")
          .map((allocation) => allocation.id)
          .join(",");

      if (allocationIds.length > 0) {
          const allocationsData = data.filter(
              (r) => r.type.toLowerCase() === "case" || r.type.toLowerCase() === "opportunity"
          );
          const [isValidAllocation, errorMessage] =
              this._resourceAllocationService.validateMonthCloseForInsertAndDelete(allocationsData);

          if (!isValidAllocation) {
              this.notifyService.showValidationMsg(errorMessage);
              return;
          }
      }
      const config = {
          class: "modal-dialog-centered",
          ignoreBackdropClick: true,
          initialState: {
              confirmationPopUpBodyMessage:
                  " Are you sure you want to remove the selected allocation(s)/commitment(s) for " +
                  this.resourceDetails.resource.fullName,
              allocationIds: allocationIds,
              commitmentIds: commitmentIds,
              resourceAllocation: data,
              isBulkDelete: true
          }
      };
      this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);
      this.bsModalRef.content.deleteResourcesFromProject.subscribe((dataToDelete) => {
          this.deleteResourceAllocationsCommitmentsFromCases(dataToDelete);
      });
  }

  openSystemConfirmationToDeleteSelectedProjects(event) {
      const config = {
          class: "modal-dialog-centered",
          ignoreBackdropClick: true,
          initialState: {
              confirmationPopUpBodyMessage: event.confirmationPopUpBodyMessage,
              allocationIds: event.allocationIds,
              resourceAllocation: event.resourceData
          }
      };
      this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);
      this.bsModalRef.content.deleteResourcesFromProject.subscribe((projectData) => {
          this.deleteResourceAllocationFromCasesHandler(projectData.allocationIds);
      });
  }

  openUpdateDateForm(event) {
      const config = {
          class: "modal-dialog-centered",
          ignoreBackdropClick: true,
          initialState: {
              resourceAllocations: event.resourceAllocations,
              title: event.title
          }
      };
      this.bsModalRef = this.modalService.show(UpdateDateFormComponent, config);

      this.bsModalRef.content.updateDataForSelectedAllocations.subscribe((upsertData) => {
          this.updateDateForSelectedAllocationsHandler(upsertData);
      });
  }

  deleteResourceCommitmentHandler(commitmentId) {
      this.deleteResourceCommitment.emit(commitmentId);
  }

  deleteResourceAllocationFromCaseHandler(allocationIds) {
      this.deleteResourceAllocationFromCases.emit(allocationIds);
  }

  deleteResourceAllocationFromCasesHandler(allocationIds) {
      this.deleteResourceAllocationFromCases.emit({allocationIds: allocationIds});
  }

  deleteResourceAllocationsCommitmentsFromCases(dataToDelete) {
      this.deleteResourceAllocationsCommitmentsFromCase.emit(dataToDelete);
  }

  updateResourceCommitmentHandler(event) {
      this.updateResourceCommitment.emit(event);
  }

  upsertResourceAllocationsToProjectHandler(upsertedAllocations) {
      this.upsertResourceAllocationsToProject.emit(upsertedAllocations);
  }

  updateDateForSelectedAllocationsHandler(upsertedAllocations) {
      if (
          upsertedAllocations &&
          upsertedAllocations.selectedAllocations &&
          upsertedAllocations.updatedDate &&
          upsertedAllocations.title
      ) {
          const updatedAllocations = this._resourceAllocationService.updateDateForSelectedAllocations(
              upsertedAllocations.selectedAllocations,
              upsertedAllocations.updatedDate,
              upsertedAllocations.title
          );

          const [isValidAllocation, errorMessage] = this._resourceAllocationService.validateMonthCloseForUpdates(
              updatedAllocations,
              upsertedAllocations.selectedAllocations
          );

          if (!isValidAllocation) {
              this.notifyService.showValidationMsg(errorMessage);
              return;
          } else {
              const updatedAllocationsWithPrePost =
                  this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(updatedAllocations);

              this.upsertResourceAllocationsToProjectHandler({
                  resourceAllocation: updatedAllocationsWithPrePost,
                  event: "updateDatePopUp",
                  showMoreThanYearWarning: false
              });
          }
      }
  }

  onFirstDataRendered(params) {
      params.api.sizeColumnsToFit();
  }

  setResourceDataInUpsertedCommitment(resourceAllocationObj: ResourceAllocation): ResourceAllocation {
      resourceAllocationObj.employeeCode =
          resourceAllocationObj.employeeCode || this.resourceDetails.resource.employeeCode;
      resourceAllocationObj.employeeName =
          resourceAllocationObj.employeeName || this.resourceDetails.resource.fullName;
      resourceAllocationObj.currentLevelGrade =
          resourceAllocationObj.currentLevelGrade || this.resourceDetails.resource.levelGrade;
      resourceAllocationObj.operatingOfficeCode =
          resourceAllocationObj.operatingOfficeCode || this.resourceDetails.resource.schedulingOffice.officeCode;
      resourceAllocationObj.operatingOfficeAbbreviation =
          resourceAllocationObj.operatingOfficeAbbreviation ||
          this.resourceDetails.resource.schedulingOffice.officeAbbreviation;
      resourceAllocationObj.serviceLineCode =
          resourceAllocationObj.serviceLineCode || this.resourceDetails.resource.serviceLine.serviceLineCode;
      resourceAllocationObj.serviceLineName =
          resourceAllocationObj.serviceLineName || this.resourceDetails.resource.serviceLine.serviceLineName;
      // because !!0 will return false and we're allowing allocation percentage to be 0
      resourceAllocationObj.allocation =
          resourceAllocationObj.allocation !== undefined
              ? resourceAllocationObj.allocation
              : this.resourceDetails.resource.fte * 100;
      resourceAllocationObj.joiningDate = this.resourceDetails.resource.startDate;

      return resourceAllocationObj;
  }

  openQuickAddFormHandler(modalData) {
      modalData.resourceAllocationData = this.setResourceDataInUpsertedCommitment(modalData.resourceAllocationData);

      this.openQuickAddForm.emit(modalData);
  }

  addResourceCommitment() {
      if (this.resourceDetails.resource.isTerminated) {
          this.notifyService.showValidationMsg(ValidationService.terminatedEmployeeCommitment);
      } else {
          const resourceAllocationData = this.setResourceDataInUpsertedCommitment({} as ResourceAllocation);
          this.openQuickAddForm.emit({
              commitmentTypeCode: "",
              resourceAllocationData: resourceAllocationData,
              isUpdateModal: false
          });
      }
  }

  getCommitmentForEmployeeOnOrAfterSelectedDateHandler(event) {
      this.commitmentStartDate = event.selectedDate;
      this.getAllCommitmentsForEmployee(this.resourceDetails.resource.employeeCode, this.commitmentStartDate);
  }

  getContextMenuItems(params) {
      if (!!params.node) {
          let selectedRows = [];
          selectedRows =
              params.api.getSelectedRows().length > 0
                  ? selectedRows.concat(params.api.getSelectedRows())
                  : selectedRows.concat(params.node.data);
          const result = [
              "copy",
              "copyWithHeaders",
              "export",
              {
                  name: "Split Allocation",
                  action: function () {
                      params.context.componentParent.openSplitAllocationDialog.emit({
                          allocationData: params.node.data
                      });
                  },
                  disabled: selectedRows.length > 1 || params.node.data.startDate === params.node.data.endDate,
                  tooltip:
                      selectedRows.length > 1
                          ? "Cannot split multiple allocation at once"
                          : params.node.data.startDate === params.node.data.endDate
                          ? "Cannot split one day allocation"
                          : "Split this allocation"
              },
              {
                  name: "Delete",
                  action: function () {
                      if (params.api.getSelectedRows().length > 0) {
                          params.context.componentParent.deleteSelectedProjectsConfirmation(selectedRows);
                      } else {
                          params.context.componentParent.deleteSelectedProjectsConfirmation(params.node.data);
                      }
                  },
                  tooltip: "To delete the selected allocation(s)"
              },
              {
                  name: "Update Start Date",
                  action: function () {
                      params.context.componentParent.openUpdateDateForm({
                          title: "Start",
                          resourceAllocations: selectedRows
                      });
                  }
              },
              {
                  name: "Update End Date",
                  action: function () {
                      params.context.componentParent.openUpdateDateForm({
                          title: "End",
                          resourceAllocations: selectedRows
                      });
                  }
              }
          ];
          return result;
      }
  }

  onGridReady(params) {
      this.gridApi = params.api;
      this.gridColumnApi = params.columnApi;
  }

  upsertNoteHandler(resourceNote : ResourceOrCasePlanningViewNote) {
    resourceNote.employeeCode = this.resourceDetails.resource.employeeCode;
    resourceNote.noteTypeCode = NoteTypeCode.RESOURCE_PROFILE_NOTE;
    this._overlayService.upsertResourceNote(resourceNote).subscribe((insertedNote) => {
        //added to solve the id problem
        this.getResourceNotes();
        this.userSpecificDetailsStore.dispatch(new UserSpecificDetailsAction.GetMostRecentSharedWithEmployeeGroups());

        this.notifyService.showSuccess(
            `Note added ${resourceNote.note.length === 1000 ? "with 1000 char" : ""}`
        );
    });

  }

  deleteNoteHandler(resourceNoteId) {
    this.deleteResourceNote(resourceNoteId);
  }

  private deleteResourceNote(deletedResourceNoteId) {
      this._overlayService.deleteResourceNote(deletedResourceNoteId).subscribe(
          (success) => {
            this.resourceNotes = this.resourceNotes.filter((note) => note.id !== deletedResourceNoteId);
            this.userSpecificDetailsStore.dispatch(new UserSpecificDetailsAction.GetMostRecentSharedWithEmployeeGroups());
            this.notifyService.showSuccess("Note deleted");
          },
          (error) => {
            this.notifyService.showError("Error while deleting note");
          }
      );
  }

  openSplitAllocationDialogHandler(data) {
      this.openSplitAllocationDialog.emit({ allocationData: data });
  }

  openUpdateAllocationsDatesDialogHandler(data) {
      this.openUpdateDateForm({ title: data.selectedOption, resourceAllocations: data.selectedAllocations });
  }

  UpdateEmployeeStaffingResponsibleEmitter(event){
    
    this.resourceOverlayStore.dispatch(
        new ResourceOverlayActions.UpsertResourceStaffingInfo(event)
    );
    
  }
  setUserAuthorizationForStaffingPreferences() {
    const tabs = [
        "staffingInsightsPriorities", 
        "staffingInsightsPD", 
        "staffingInsightsIndustries", 
        "staffingInsightsCapabilities",
        "staffingInsightsExperience",  
        "staffingInsightsTravel"  
    ]
    const accessibleFeaturesForUser = this.coreService.loggedInUserClaims.FeatureAccess;
    tabs.forEach(tab => {
        const featureName = this.accessibleFeatures[tab];
        if (CommonService.isAccessToFeatureReadOnlyOrNone(featureName, accessibleFeaturesForUser) ||
            this.coreService.loggedInUser.hasAccessToStaffingInsightsTool) {
                this.isStaffingPreferencesToolEnabled = true;
            }
    });
  }

  public activeTab: string = 'Calendar'; 
  onTabChange(selectedTab){
    this.activeTab = selectedTab.heading;
  }
}
