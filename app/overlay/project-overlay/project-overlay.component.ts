import { Component, Inject, OnInit, HostListener, Output, EventEmitter, OnDestroy, ElementRef } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Router, RouterEvent, NavigationStart, ActivatedRoute } from "@angular/router";
import { filter } from "rxjs/operators";
import { BsModalService, BsModalRef } from "ngx-bootstrap/modal";
import { SystemconfirmationFormComponent } from "../../shared/systemconfirmation-form/systemconfirmation-form.component";
import { GanttService } from "../gantt/gantt.service";
import { NotificationService } from "src/app/shared/notification.service";
import { ValidationService } from "src/app/shared/validationService";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { DateService } from "src/app/shared/dateService";
import { UserPreferences } from "src/app/shared/interfaces/userPreferences.interface";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { OverlayService } from "../overlay.service";
import { ResourceAllocationService } from "src/app/shared/services/resourceAllocation.service";
import { ResourceAllocation } from "src/app/shared/interfaces/resourceAllocation.interface";
import { GridOptions, GridSizeChangedEvent } from "ag-grid-community";
import { CoreService } from "src/app/core/core.service";
import { InvestmentCategory } from "src/app/shared/interfaces/investmentCateogry.interface";
import { CaseRoleType } from "src/app/shared/interfaces/caseRoleType.interface";
import * as moment from "moment";
import { Observable, Subscription } from "rxjs";
import { OverlayMessageService } from "../behavioralSubjectService/overlayMessage.service";
import { UpdateDateFormComponent } from "src/app/shared/update-date-form/update-date-form.component";
import { CaseRoleAllocation } from "src/app/shared/interfaces/caseRoleAllocation.interface";
import { CommonService } from "src/app/shared/commonService";
import { PlaceholderAllocation } from "src/app/shared/interfaces/placeholderAllocation.interface";
import { UrlService } from "src/app/core/services/url.service";
import { ProjectOverlayViewModel } from "src/app/shared/interfaces/projectOverlayViewModel";
import { PegOpportunityDialogService } from "../dialogHelperService/peg-opportunity-dialog.service";
import { ResourceCommitmentService } from "../behavioralSubjectService/resourceCommitment.service";
import { Commitment } from "src/app/shared/interfaces/commitment.interface";
import { CommitmentType } from "src/app/shared/interfaces/commitmentType.interface";

@Component({
    selector: "app-project-overlay",
    templateUrl: "./project-overlay.component.html",
    styleUrls: ["./project-overlay.component.scss"]
})
export class ProjectOverlayComponent implements OnInit, OnDestroy {
    // -----------------------Local Variables--------------------------------------------//

    // For dragging
    public grabber: boolean = false;
    public height: number = 370;
    public oldYCoord: number = 0;
    public yCoord: number = 100;

    public project: ProjectOverlayViewModel = {
        projectDetails: {},
        projectResourceAllocations: [],
        projectPlaceholderAllocations: [],
        auditTrails: [],
        skuTabList: []
    };
    public showDialog: boolean;
    public showCaseIntake = false;
    public skuTermList: any = [];
    public openedTab: String;
    public columnDefs;
    public defaultColDef;
    public components;
    public projectData: any;
    public isPinned = false;
    public isHidden = false;
    public isCaseOnRoll = false;
    public isSTACommitmentCreated = false;
    public today = new Date();
    public calendarRadioSelected: string = ConstantsMaster.ganttCalendarScaleOptions.defaultSelection;
    public emptyLogMessage = "";
    public gridOptions: GridOptions;
    private investmentCategories: InvestmentCategory[];
    private commitmentTypes: CommitmentType[];
    private caseRoleTypes: CaseRoleType[];
    public frameworkComponents;
    private isAllocationValid = false;
    private isStartDateValid = false;
    private isEndDateValid = false;
    public activeResourcesEmailAddresses = "";
    private routerSubscription: any;
    public activeResourcesStartDate;
    private gridApi;
    public gridColumnApi;
    public agGridRowData: any[];
    subscription: Subscription = new Subscription();
    public caseRoleAllocations = [];
    public isTableauDashboardLoadTrigerred = false;
    accessibleFeatures = ConstantsMaster.appScreens.feature;
    isAGGridReadOnly = false;
    isGanttCalendarReadOnly = false;
    dateMessage = "Does not exist";
    bsModalRef: BsModalRef;
    auditLogStartIndex = 0;
    logLoading = false;
    scrollDistance: number; // how much percentage the scroll event should fire ( 2 means (100 - 2*10) = 80%)
    pageScrolled = false;
    pageSize: number;
    // -----------------------Output Events--------------------------------------------//

    @Output() openResourceDetailsFromProjectDialog = new EventEmitter();
    @Output() updateResourceAssignmentToProject = new EventEmitter();
    @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();
    @Output() upsertPlaceholderAllocationsToProject = new EventEmitter<any>();
    @Output() deleteResourcesFromProject = new EventEmitter();
    @Output() deletePlaceholdersFromProject = new EventEmitter();
    @Output() openQuickAddForm = new EventEmitter();
    @Output() insertSkuCaseTermsForProject = new EventEmitter();
    @Output() updateSkuCaseTermsForProject = new EventEmitter();
    @Output() deleteSkuCaseTermsForProject = new EventEmitter();
    @Output() addProjectToUserExceptionShowList = new EventEmitter();
    @Output() removeProjectFromUserExceptionShowList = new EventEmitter();
    @Output() addProjectToUserExceptionHideList = new EventEmitter();
    @Output() removeProjectFromUserExceptionHideList = new EventEmitter();
    @Output() openCaseRollForm = new EventEmitter();
    @Output() openStaCommitmentForm = new EventEmitter();
    @Output() openPlaceholderForm = new EventEmitter();
    @Output() updateProjectChanges = new EventEmitter();
    @Output() openNotesDialog = new EventEmitter();
    @Output() openSplitAllocationDialog = new EventEmitter();
    @Output() updateResourceCommitment = new EventEmitter();
 

    constructor(
        public dialogRef: MatDialogRef<ProjectOverlayComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any,
        private _overlayService: OverlayService,
        private modalService: BsModalService,
        private _ganttService: GanttService,
        private _notifyService: NotificationService,
        private localStorageService: LocalStorageService,
        private router: Router,
        private route: ActivatedRoute,
        private _resourceAllocationService: ResourceAllocationService,
        private _resourceCommitmentService: ResourceCommitmentService,
        private coreService: CoreService,
        private overlayMessageService: OverlayMessageService,
        private urlService: UrlService,
        private elementRef: ElementRef,
        private pegOpportunityDialogService: PegOpportunityDialogService
    ) {
        this.isAGGridReadOnly = CommonService.isReadOnlyAccessToFeature(
            this.accessibleFeatures.caseOverlayTeam,
            this.coreService.loggedInUserClaims.FeatureAccess
        );
        this.isGanttCalendarReadOnly = CommonService.isReadOnlyAccessToFeature(
            this.accessibleFeatures.caseOverlayGantt,
            this.coreService.loggedInUserClaims.FeatureAccess
        );

        this.investmentCategories = this.localStorageService.get(ConstantsMaster.localStorageKeys.investmentCategories);
        this.commitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes);
        this.caseRoleTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseRoleTypes);

        this.activeResourcesStartDate = new Date();
        this.activeResourcesStartDate.setDate(
            this.activeResourcesStartDate.getDate() - ConstantsMaster.ganttConstants.initialDaysDeduction
        );
        // emitting null value to prevent emitting last value
        this._ganttService.resourceAndPlaceholderAllocations.next(null);
        this.dialogRef.disableClose = true; // dialogRef property set to disable dialogBox from closing using esc button.
        this.loadAgGridConfigurations();
        this.showDialog = this.data.showDialog;
        this.getProjectDetails(this.data.projectData);
        this.setProjectActions(this.data.projectData.oldCaseCode, this.data.projectData.pipelineId);
    }

    getHeight() {
        return {
            height: `calc(100vh - (${this.height}px + 120px))`
        };
    }

    @HostListener("window:mousemove", ["$event"])
    onMousemove(event: MouseEvent) {
        document.querySelector(".gantt_tooltip")?.remove();
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

        this.columnDefs = this.getAGGridColumnDefination();

        this.components = { datePicker: this.getDatePicker() };
        // this.frameworkComponents = {
        //   datePickerRenderer: AgGridDatepickerComponent
        // }
    }

    getAGGridColumnDefination() {
        // let accessibleFeatures = this.coreService.loggedInUserClaims.FeatureAccess.map(x => x.FeatureName);
        // let hasNotesAccess = CommonService.hasAccessToFeature('allocationNotes', accessibleFeatures);

        return [
            {
                checkboxSelection: !this.isAGGridReadOnly,
                headerCheckboxSelection: !this.isAGGridReadOnly,
                headerCheckboxSelectionFilteredOnly: !this.isAGGridReadOnly,
                headerName: "",
                field: "",
                maxWidth: 60,
                minWidth: 60,
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
                headerName: "Name",
                field: "employeeName",
                editable: false,
                cellStyle: { cursor: "pointer", color: "#005999" }
            },
            {
                headerName: "Service Line",
                field: "serviceLineName",
                editable: false,
                maxWidth: 120
            },
            {
                headerName: "Office",
                field: "operatingOfficeAbbreviation",
                editable: false,
                maxWidth: 120
            },
            {
                headerName: "Level",
                field: "currentLevelGrade",
                editable: false,
                maxWidth: 120
            },
            {
                headerName: "Start",
                field: "startDate",
                maxWidth: 120,
                editable: !this.isAGGridReadOnly,
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
                        params.context.componentParent._notifyService.showValidationMsg(errorMessage);
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
                headerName: "End",
                field: "endDate",
                maxWidth: 120,
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
                        params.context.componentParent._notifyService.showValidationMsg(errorMessage);
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
            {
                headerName: "Allocation",
                field: "allocation",
                maxWidth: 120,
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
                    if (paramValue.indexOf("%") === -1 && !isNaN(params.value)) {
                        paramValue += "%";
                    }
                    return paramValue;
                },
                valueGetter: (params) => {
                    return params.data.allocation;
                },
                valueSetter: function (params) {
                    //params.context.componentParent.isCellDataValidForUpdate();
                    const existingUserAllocation = params.data;
                    const updatedAllocation = Object.assign({}, params.data);
                    updatedAllocation.allocation = params.newValue;
                    const currentAllocationDecidingParamsForSplit =
                        params.context.componentParent.allocationDecidingParamsForSplit(updatedAllocation);
                    let [canSplitForMonthClose, validationMessage] =
                        params.context.componentParent._resourceAllocationService.canSplitForMonthClose(
                            existingUserAllocation,
                            currentAllocationDecidingParamsForSplit
                        );
                    if (!canSplitForMonthClose && validationMessage) {
                        params.context.componentParent._notifyService.showValidationMsg(validationMessage);
                        return false;
                    }
                    params.data.allocation = updatedAllocation.allocation;
                    return true;
                }
            },
            {
                headerName: "Investment",
                field: "investmentCode",
                editable: !this.isAGGridReadOnly,
                cellEditor: "agRichSelectCellEditor",
                cellEditorParams: {
                    cellHeight: 35,
                    values: this.investmentCategories?.map((investmentCategory) => investmentCategory.investmentName)
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
                        params.context.componentParent._notifyService.showValidationMsg(
                            isInvestmentTypeValid.errorMessage
                        );
                        return false;
                    } else {
                        const currentAllocationDecidingParamsForSplit =
                            params.context.componentParent.allocationDecidingParamsForSplit(updatedAllocation);
                        let [canSplitForMonthClose, validationMessage] =
                            params.context.componentParent._resourceAllocationService.canSplitForMonthClose(
                                existingUserAllocation,
                                currentAllocationDecidingParamsForSplit
                            );

                        if (!canSplitForMonthClose && validationMessage) {
                            params.context.componentParent._notifyService.showValidationMsg(validationMessage);
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
                cellEditor: "agRichSelectCellEditor",
                cellEditorParams: {
                    cellHeight: 45,
                    values: this.caseRoleTypes?.map((caseRoleType) => caseRoleType.caseRoleName)
                },
                valueSetter: function (params) {
                    /**When we select a value from our drop down list, this function will make sure
                    that our row's record receives the "caseRoleCode" of the chosen selection.*/
                    const existingUserAllocation = params.data;
                    const updatedAllocation = Object.assign({}, params.data);
                    updatedAllocation.caseRoleCode = params.context.componentParent.caseRoleTypes.find(
                        (caseRoleType) => caseRoleType.caseRoleName === params.newValue
                    )?.caseRoleCode;

                    const currentAllocationDecidingParamsForSplit =
                        params.context.componentParent.allocationDecidingParamsForSplit(updatedAllocation);
                    let [canSplitForMonthClose, validationMessage] =
                        params.context.componentParent._resourceAllocationService.canSplitForMonthClose(
                            existingUserAllocation,
                            currentAllocationDecidingParamsForSplit
                        );

                    if (!canSplitForMonthClose && validationMessage) {
                        params.context.componentParent._notifyService.showValidationMsg(validationMessage);
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
            }
        ];
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

    // Added to provide animation to dialog box while closing using esc button.
    @HostListener("document:keydown", ["$event"])
    public handleKeyboardEvent(event: KeyboardEvent): void {
        // if-else condition added to close only resource popup when both resource popup and project detail popup are open
        // and user presses esc button.
        if (event.key === "Escape") {
            if ((<HTMLInputElement>event.target).childElementCount > 0) {
                if (
                    (<HTMLInputElement>event.target).firstElementChild.firstElementChild &&
                    ((<HTMLInputElement>event.target).firstElementChild.firstElementChild.id === "project-card" ||
                        (<HTMLInputElement>event.target).firstElementChild.firstElementChild.id === "")
                ) {
                    this.closeDialog();
                }
            } else {
                this.closeDialog();
            }
        }
    }

    // -----------------------Component LifeCycle Events and Functions--------------------------------------------//

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
            this.skuTermList = this.localStorageService.get(ConstantsMaster.localStorageKeys.skuTermList);
            this.coreService.setShowHideNotification(false);
            this.subscription.add(
                this.overlayMessageService.refreshProjectForCaseRoll().subscribe((result) => {
                    if (result !== null) {
                        this.project.projectDetails.caseRoll = result.caseRoll;
                    }
            })
        );
    }

    ngAfterViewInit() {
        // Event Listener for dragging and expanding column Div
        this.elementRef.nativeElement.querySelector("#gr").addEventListener("mousedown", this.onMouseDown.bind(this));
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

    ngOnDestroy(): void {
        this.routerSubscription.unsubscribe();
        this.subscription.unsubscribe();
    }

    changeCalendarOptionHandler(changedCalendarOption) {
        this.calendarRadioSelected = changedCalendarOption;
    }

    // ------------------------Service/API calls-------------------------------------------------------------------//

    getProjectDetails(projectData) {
        if (projectData.oldCaseCode) {
            this.getCaseInfoForHeader(projectData.oldCaseCode);
            this.populateCaseGantt(projectData.oldCaseCode, this.activeResourcesStartDate);
            this.getSkuTermsForCase(projectData.oldCaseCode);
        } else {
            this.getOpportunityInfoForHeader(projectData.pipelineId);
            this.populateOpportunityGantt(projectData.pipelineId, this.activeResourcesStartDate);
            this.getSkuTermsForOpportunity(projectData.pipelineId);
        }
        if (this.agGridRowData) {
            this.populateAGGrid();
        }
        this.getCaseRoleAllocations();
    }

    getCaseRoleAllocations() {
        const oldCaseCode = this.data.projectData.oldCaseCode;
        const pipelineId = this.data.projectData.pipelineId;
        let getCaseRoleAllocationApiCall = oldCaseCode
            ? this._overlayService.getCaseRoleAllocationsByOldCaseCodes(oldCaseCode)
            : this._overlayService.getCaseRoleAllocationsByPipelineIds(pipelineId);
        getCaseRoleAllocationApiCall.subscribe((response) => {
            if (response.length > 0) {
                this.caseRoleAllocations = [];
                let uniqueCaseRoleAllocations = CommonService.getUniqueArrayOfObjects(
                    response,
                    "employeeCode",
                    "caseRoleCode"
                );
                let groupedCaseRoleAllocationsByCaseRoleCode =
                    this.groupCaseRoleAllocationsByCaseRoleCode(uniqueCaseRoleAllocations);
                this.createCaseRoleAllocationsList(groupedCaseRoleAllocationsByCaseRoleCode);
            }
        });
    }

    groupCaseRoleAllocationsByCaseRoleCode(caseRoleAllocations) {
        return caseRoleAllocations.reduce((obj, allocation) => {
            obj[allocation.caseRoleCode] = [
                ...(obj[allocation.caseRoleCode] || []),
                allocation.employeeName + " - " + allocation.officeAbbreviation
            ];
            return obj;
        }, {});
    }

    createCaseRoleAllocationsList(groupedCaseRoleAllocationsByCaseRoleCode) {
        let uniqueCaseRoleCodes = Object.keys(groupedCaseRoleAllocationsByCaseRoleCode);
        uniqueCaseRoleCodes.forEach((key) => {
            let item = {};
            item["title"] = key;
            item["data"] = groupedCaseRoleAllocationsByCaseRoleCode[key].join(", ");
            this.caseRoleAllocations.push(item);
        });
    }

    populateAGGrid() {
        const oldCaseCode = this.data.projectData.oldCaseCode;
        const pipelineId = this.data.projectData.pipelineId;

        this._overlayService.getHistoricalAllocationsForProject(oldCaseCode, pipelineId).subscribe((allocations) => {
            this.assignAllocationToProject(allocations);
            const historicalAllocationsData = JSON.parse(JSON.stringify(this.project.projectResourceAllocations));
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

    private assignAllocationToProject(allocations: ResourceAllocation[]): void {
        this.project.projectResourceAllocations = allocations?.filter(
            (allocation) => !allocation.isPlaceholderAllocation
        );
        this.project.projectPlaceholderAllocations = allocations?.filter(
            (allocation) =>
                allocation.isPlaceholderAllocation &&
                allocation.serviceLineCode &&
                allocation.operatingOfficeCode &&
                allocation.currentLevelGrade
        );

        this.project.projectResourceAllocations.forEach((allocatedResource) => {
            this.addActiveResourceToEmailString(allocatedResource);
        });

        this.convertDateToBainFormat(this.project.projectResourceAllocations);
        this.convertDateToBainFormat(this.project.projectPlaceholderAllocations);

        this.project.projectResourceAllocations = this.sortAllocations(this.project.projectResourceAllocations);

        this.project.projectPlaceholderAllocations = this.sortAllocations(this.project.projectPlaceholderAllocations);
    }

    sortAllocations(allocations) {
        return allocations?.sort((firstAllocation, secondAllocation) => {
            if (firstAllocation.startDate > secondAllocation.startDate) {
                return 1;
            }
            if (firstAllocation.startDate < secondAllocation.startDate) {
                return -1;
            }
            return 0;
        });
    }

    convertDateToBainFormat(allocations) {
        allocations?.forEach((plceholderAllocation) => {
            plceholderAllocation.startDate = DateService.convertDateInBainFormat(plceholderAllocation.startDate);
            plceholderAllocation.endDate = DateService.convertDateInBainFormat(plceholderAllocation.endDate);
        });
    }

    setProjectActions(oldCaseCode, pipelineId) {
        const userSettings: UserPreferences = JSON.parse(
            this.localStorageService.get(ConstantsMaster.localStorageKeys.userPreferences)
        );

        this.isPinned =
            (userSettings.caseExceptionShowList &&
                userSettings.caseExceptionShowList.split(",").includes(oldCaseCode)) ||
            (userSettings.opportunityExceptionShowList &&
                userSettings.opportunityExceptionShowList.split(",").includes(pipelineId));
        this.isHidden =
            (userSettings.caseExceptionHideList &&
                userSettings.caseExceptionHideList.split(",").includes(oldCaseCode)) ||
            (userSettings.opportunityExceptionHideList &&
                userSettings.opportunityExceptionHideList.split(",").includes(pipelineId));
        this.isCaseOnRoll = this.project && this.project.projectDetails && this.project.projectDetails.caseRoll;
        this.isSTACommitmentCreated = this.project && this.project.projectDetails && this.project.projectDetails.isSTACommitmentCreated;
    }

    getCaseInfoForHeader(oldCaseCode) {
        this._overlayService.getCaseDetails(oldCaseCode).subscribe((caseDetails) => {
            this.project.projectDetails = caseDetails;
            this.setProjectActions(oldCaseCode, null);

            if (this.project.projectDetails.startDate === "0001-01-01T00:00:00") {
                this.project.projectDetails.startDate = this.dateMessage;
            } else {
                this.project.projectDetails.startDate = DateService.convertDateInBainFormat(
                    this.project.projectDetails.startDate
                );
            }
            if (this.project.projectDetails.endDate === "0001-01-01T00:00:00") {
                this.project.projectDetails.endDate = this.dateMessage;
            } else {
                this.project.projectDetails.endDate = DateService.convertDateInBainFormat(
                    this.project.projectDetails.endDate
                );
            }
        });
    }

    getOpportunityInfoForHeader(pipelineId) {
        this._overlayService.getOpportunityDetails(pipelineId).subscribe((opportunityDetails) => {
            this.project.projectDetails = opportunityDetails;
            this.setProjectActions(null, pipelineId);

            this.project.projectDetails.startDate = DateService.convertDateInBainFormat(opportunityDetails.startDate);
            this.project.projectDetails.endDate = DateService.convertDateInBainFormat(opportunityDetails.endDate);
        });
    }

    addActiveResourceToEmailString(resource) {
        if (
            Date.parse(resource.endDate) >= DateService.getToday() &&
            !this.activeResourcesEmailAddresses.includes(resource.internetAddress)
        ) {
            this.activeResourcesEmailAddresses += resource.internetAddress + ";";
        }
    }

    populateCaseGantt(oldCaseCode, effectiveDate?) {
        const effectiveFromDate = effectiveDate
            ? DateService.convertDateInBainFormat(effectiveDate)
            : DateService.convertDateInBainFormat(this.today);
        this._overlayService.getCaseAllocations(oldCaseCode, effectiveFromDate).subscribe((allocations) => {
            this.assignAllocationToProject(allocations);
            var placeholderAndResourceAllocations = this.project.projectPlaceholderAllocations.concat(
                this.project.projectResourceAllocations
            );
            this._ganttService.resourceAndPlaceholderAllocations.next(placeholderAndResourceAllocations);
        });
    }

    populateOpportunityGantt(pipelineId, effectiveDate?) {
        const effectiveFromDate = effectiveDate
            ? DateService.convertDateInBainFormat(effectiveDate)
            : DateService.convertDateInBainFormat(this.today);
        this._overlayService.getOpportunityAllocations(pipelineId, effectiveFromDate).subscribe((allocations) => {
            this.assignAllocationToProject(allocations);
            var placeholderAndResourceAllocations = this.project.projectPlaceholderAllocations.concat(
                this.project.projectResourceAllocations
            );
            this._ganttService.resourceAndPlaceholderAllocations.next(placeholderAndResourceAllocations);
        });
    }

    populateGanttData() {
        if (this.data.projectData.oldCaseCode) {
            this.populateCaseGantt(this.data.projectData.oldCaseCode, this.activeResourcesStartDate);
        } else {
            this.populateOpportunityGantt(this.data.projectData.pipelineId, this.activeResourcesStartDate);
        }
    }

    getProjectAuditTrails() {
        const oldCaseCode = this.data.projectData.oldCaseCode;
        const pipelineId = this.data.projectData.pipelineId;

        this.logLoading = true;
        this._overlayService
            .getprojectAuditTrails(oldCaseCode, pipelineId, this.pageSize, this.auditLogStartIndex)
            .subscribe((auditTrails) => {
                if (!this.pageScrolled) {
                    this.project.auditTrails = [];
                }

                if (auditTrails.length > 0) {
                    this.project.auditTrails = this.project.auditTrails.concat(auditTrails);
                } else {
                    this.emptyLogMessage = ConstantsMaster.ganttConstants.emptyTemplateText;
                }
                this.logLoading = false;
            });
    }

    onAuditLogScrolled() {
        // if total logs displayed are less than the page size, it means there are not more logs that needs to be fetched
        if (this.project.auditTrails.length < this.pageSize + this.auditLogStartIndex) {
            return;
        }

        this.getMoreLogs();
    }

    getMoreLogs() {
        this.auditLogStartIndex = this.auditLogStartIndex + this.pageSize;
        this.pageScrolled = true;

        this.getProjectAuditTrails();
    }

    loadCaseEconomicsDashboard() {
        this.isTableauDashboardLoadTrigerred = true;
    }

    getSkuTermsForCase(oldCaseCode) {
        this._overlayService.getSKUTermsForCase(oldCaseCode).subscribe((skuTabList) => {
            this.project.skuTabList = skuTabList;
        });
    }

    getSkuTermsForOpportunity(pipelineId) {
        this._overlayService.getSKUTermsForOpportunity(pipelineId).subscribe((skuTabList) => {
            this.project.skuTabList = skuTabList;
        });
    }



    // -------------------Component Event Handlers-------------------------------------//

    closeDialog() {
        this.coreService.setShowHideNotification(false);
        const self = this;

        self.showDialog = false;

        /*
         * Set timeout is required to close the animation with animation and override/delay the defaut closing of material dialog.
         * If not used, the overlay will not close as per our requored animation
         * dialogref.close is required to unload the overlay component.
         */
        setTimeout(function () {
            if (
                self.router.url.toLowerCase().includes("/overlay?oldcasecode") ||
                self.router.url.toLowerCase().includes("/overlay?pipelineid")
            ) {
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

    updateResourceToProjectHandler(updatedResource) {
        this.updateResourceAssignmentToProject.emit(updatedResource);
    }

    upsertResourceAllocationsToProjectHandler(upsertedAllocations) {
        this.upsertResourceAllocationsToProject.emit(upsertedAllocations);
    }

    upsertPlaceholderAllocationsToProjectHandler(upsertedAllocations) {
        this.upsertPlaceholderAllocationsToProject.emit(upsertedAllocations);
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
                this._notifyService.showValidationMsg(errorMessage);
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

    updateDateForSelectedCommitmentsHandler(upsertedCommitments) {
        if (
            upsertedCommitments &&
            upsertedCommitments.selectedCommitments &&
            upsertedCommitments.updatedDate &&
            upsertedCommitments.title
        ) {

            const updatedUserCommitments: Commitment[] = upsertedCommitments.selectedCommitments.map(item => ({
                ...item,
                startDate: upsertedCommitments.updatedDate,
                endDate: upsertedCommitments.updatedDate,
              }));

              this.updateResourceCommitment.emit({
                resourceAllocation: updatedUserCommitments,
                event: 'ganttResource'
            });

        }
    }

    openSystemConfirmationToDeleteSelectedResources(event) {
        const projectName =
            this.project.projectDetails.type === "Opportunity"
                ? this.project.projectDetails.opportunityName
                : this.project.projectDetails.caseName;
        const confirmationPopUpBodyMessage =
            event.confirmationPopUpBodyMessage +
            projectName +
            ". Are you sure you want to remove selected resource(s) ?";

        // class is required to center align the modal on large screens
        const config = {
            class: "modal-dialog-centered",
            ignoreBackdropClick: true,
            initialState: {
                confirmationPopUpBodyMessage: confirmationPopUpBodyMessage,
                allocationIds: event.projectIds,
                resourceAllocation: [].concat(event.resourceData.resourceAllocations).concat(event.resourceData.resourceCommitments??[]),
                commitmentIds: event.commitmentIds
            }
        };
        this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);
        this.bsModalRef.content.deleteResourcesFromProject.subscribe((resourceData) => {
            this.deleteResourcesFromProject.emit(resourceData);
        });
    }

    openSystemConfirmationToDeleteSelectedPlaceholders(event) {
        const projectName =
            this.project.projectDetails.type === "Opportunity"
                ? this.project.projectDetails.opportunityName
                : this.project.projectDetails.caseName;
        const confirmationPopUpBodyMessage =
            event.confirmationPopUpBodyMessage +
            projectName +
            ". Are you sure you want to remove selected placeholder(s) ?";

        // class is required to center align the modal on large screens
        const config = {
            class: "modal-dialog-centered",
            ignoreBackdropClick: true,
            initialState: {
                confirmationPopUpBodyMessage: confirmationPopUpBodyMessage,
                placeholderIds: event.placeholderIds,
                placeholderAllocation: event.placeholderAllocations
            }
        };
        this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);
        this.bsModalRef.content.deletePlaceholdersFromProject.subscribe((placeholderData) => {
            this.deletePlaceholdersFromProject.emit(placeholderData);
        });
    }

    openUpdateDateForm(event) {
        const config = {
            class: "modal-dialog-centered",
            ignoreBackdropClick: true,
            initialState: {
                resourceAllocations: event.resourceAllocations,
                title: event.title,
                placeholderAllocations: event.placeholderAllocations,
                resourceCommitments: event.resourceCommitments
            }
        };
        this.bsModalRef = this.modalService.show(UpdateDateFormComponent, config);

        this.bsModalRef.content.updateDataForSelectedAllocations.subscribe((upsertData) => {
            this.updateDateForSelectedAllocationsHandler(upsertData);
        });

        this.bsModalRef.content.updateDataForSelectedCommitments.subscribe((upsertData) => {
            this.updateDateForSelectedCommitmentsHandler(upsertData);
        });

        this.bsModalRef.content.updateDataForSelectedPlaceholders.subscribe((upsertData) => {
            this.upsertPlaceholderAllocationsToProjectHandler(upsertData);
        });
    }

    setProjectDataInUpsertedCommitment(resourceAllocationObj: ResourceAllocation): ResourceAllocation {
        resourceAllocationObj.oldCaseCode =
            resourceAllocationObj.oldCaseCode || this.project.projectDetails.oldCaseCode;
        resourceAllocationObj.caseName = resourceAllocationObj.caseName || this.project.projectDetails.caseName;
        resourceAllocationObj.clientName = resourceAllocationObj.clientName || this.project.projectDetails.clientName;
        resourceAllocationObj.caseTypeCode =
            resourceAllocationObj.caseTypeCode || this.project.projectDetails.caseTypeCode;
        resourceAllocationObj.pipelineId = resourceAllocationObj.pipelineId || this.project.projectDetails.pipelineId;
        resourceAllocationObj.opportunityName =
            resourceAllocationObj.opportunityName || this.project.projectDetails.opportunityName;
        resourceAllocationObj.caseStartDate =
            resourceAllocationObj.caseStartDate ||
            (this.project.projectDetails.oldCaseCode ? this.project.projectDetails.startDate : null);
        resourceAllocationObj.caseEndDate =
            resourceAllocationObj.caseEndDate ||
            (this.project.projectDetails.oldCaseCode ? this.project.projectDetails.endDate : null);
        resourceAllocationObj.opportunityStartDate =
            resourceAllocationObj.opportunityStartDate ||
            (!this.project.projectDetails.oldCaseCode ? this.project.projectDetails.startDate : null);
        resourceAllocationObj.opportunityEndDate =
            resourceAllocationObj.opportunityEndDate ||
            (!this.project.projectDetails.oldCaseCode ? this.project.projectDetails.endDate : null);

        return resourceAllocationObj;
    }

    openQuickAddFormHandler(modalData) {
        modalData.resourceAllocationData = this.setProjectDataInUpsertedCommitment(modalData.resourceAllocationData);

        this.openQuickAddForm.emit(modalData);
    }

    addResource() {
        const resourceAllocationData = this.setProjectDataInUpsertedCommitment({} as ResourceAllocation);

        this.openQuickAddForm.emit({
            commitmentTypeCode: "C",
            resourceAllocationData: resourceAllocationData,
            isUpdateModal: false
        });
    }

    resourceCodeToOpenHandler(resourceCodeToOpen) {
        this.openResourceDetailsFromProjectDialog.emit(resourceCodeToOpen.resourceCode);
    }

    deleteSelectedPlaceholdersConfirmation(placeholdersData: PlaceholderAllocation | PlaceholderAllocation[]) {
        const placeholderAllocations = [].concat(placeholdersData);
        const selectedIds = placeholderAllocations.map((elem) => elem.id).join(",");
        const confirmationPopUpBodyMessage = "You are about to remove selected placeholder(s) from project ";
        this.openSystemConfirmationToDeleteSelectedPlaceholders({
            placeholderIds: selectedIds,
            placeholderAllocations: placeholderAllocations,
            confirmationPopUpBodyMessage: confirmationPopUpBodyMessage
        });
    }

    deleteSelectedResourcesConfirmation(resourceData) {
        const [isValidAllocation, errorMessage] =
            this._resourceAllocationService.validateMonthCloseForInsertAndDelete(resourceData.resourceAllocations);

        if (!isValidAllocation) {
            this._notifyService.showValidationMsg(errorMessage);
            return;
        } else {
            const allocationIds = this.extractAllocationOrCommitmentIds(resourceData.resourceAllocations);
            const commitmentIds = this.extractAllocationOrCommitmentIds(resourceData.resourceCommitments);

            const confirmationPopUpBodyMessage = "You are about to remove the selected resource(s) from project ";
            this.openSystemConfirmationToDeleteSelectedResources({
                projectIds: allocationIds ?? "",
                commitmentIds: commitmentIds ?? "",
                confirmationPopUpBodyMessage: confirmationPopUpBodyMessage,
                resourceData: resourceData
            });
        }
    }

    private extractAllocationOrCommitmentIds(data) : string
    {
        if (!data) {
            return '';
        }
    
        if (Array.isArray(data)) {
            return data.length > 0 ? data.map(function(elem) { return elem.id; }).join(',') : '';
        }
    
        return data.id || '';
    }


    insertSkuCaseTermsHandler(skuTab) {
        this.insertSkuCaseTermsForProject.emit(skuTab);
    }

    updateSkuCaseTermsHandler(skuTab) {
        this.updateSkuCaseTermsForProject.emit(skuTab);
    }

    deleteSkuCaseTermsHandler(skuTab) {
        this.deleteSkuCaseTermsForProject.emit(skuTab);
    }

    onFirstDataRendered(params) {
        params.api.sizeColumnsToFit();
    }

    // onRowDataChanged(params) {
    //   params.api.sizeColumnsToFit();
    //   params.api.refreshCells();
    // }

    openResourceDetailsDialogHandler(params) {
        if (params.context.componentParent.isAGGridReadOnly) return false;

        if (params.colDef.field === "employeeName") {
            this.openResourceDetailsFromProjectDialog.emit(params.data.employeeCode);
        } else if (params.colDef.field === "") {
            this.deleteSelectedResourcesConfirmation({resourceAllocations : params.data});
        } else if (params.colDef.field === "notes") {
            this.gridApi.deselectAll();
            this.openNotesDialog.emit({
                projectData: params.data
            });
        }
    }

    openPegRFPopUpHandler(pegOpportunityId) {
        this.pegOpportunityDialogService.openPegOpportunityDetailPopUp(pegOpportunityId);
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
        // tslint:disable-next-line: triple-equals
        if (!(params.newValue == params.oldValue)) {
            /*The following if statement is used to handle the issue for investment category and case role headers.
            If a resource does not have any investemt category or case role and user reselects "select any" field, then the newValue
            and oldValue are "null" and null respectively */
            if (!(params.newValue === "null" && params.oldValue == null)) {
                this.projectData = params.data;
                let allocation = this.projectData.allocation.toString();
                if (allocation.indexOf("%") !== -1) {
                    allocation = allocation.slice(0, allocation.indexOf("%"));
                }
                if (!ValidationService.isAllocationValid(allocation)) {
                    this._notifyService.showValidationMsg(ValidationService.numberInvalidMessage);
                    this.isAllocationValid = false;
                } else {
                    this.isAllocationValid = true;
                    params.data.allocation = allocation;
                }
                const startDate = this.projectData.startDate;
                if (!ValidationService.isValidDate(startDate)) {
                    this._notifyService.showValidationMsg(ValidationService.dateInvalidMessage);
                    this.isStartDateValid = false;
                } else if (moment(this.projectData.joiningDate).isAfter(moment(startDate), "day")) {
                    this._notifyService.showValidationMsg(
                        ValidationService.employeeJoiningDateGreaterThanStartDate.replace(
                            "[joiningDate]",
                            DateService.convertDateInBainFormat(this.projectData.joiningDate)
                        )
                    );
                    this.isStartDateValid = false;
                } else {
                    this.isStartDateValid = true;
                }
                const endDate = this.projectData.endDate;
                if (!ValidationService.isValidDate(endDate)) {
                    this._notifyService.showValidationMsg(ValidationService.dateInvalidMessage);
                    this.isEndDateValid = false;
                } else {
                    this.isEndDateValid = true;
                }
                if (this.isStartDateValid && this.isEndDateValid) {
                    if (Date.parse(startDate) > Date.parse(endDate)) {
                        this.isStartDateValid = false;
                        this.isEndDateValid = false;
                        this._notifyService.showValidationMsg(ValidationService.startDateGreaterThanEndDate);
                    } else {
                        this.isStartDateValid = true;
                        this.isEndDateValid = true;
                    }
                }

                if (this.isAllocationValid && this.isStartDateValid && this.isEndDateValid) {
                    const resourceDataToBeUpdated = params.data;
                    if (!resourceDataToBeUpdated.investmentCode || resourceDataToBeUpdated.investmentCode === "null") {
                        resourceDataToBeUpdated.investmentCode = null;
                    } else {
                        resourceDataToBeUpdated.investmentCode = parseInt(params.data.investmentCode);
                    }
                    if (!resourceDataToBeUpdated.caseRoleCode || resourceDataToBeUpdated.caseRoleCode === "null") {
                        resourceDataToBeUpdated.caseRoleCode = null;
                    } else {
                        resourceDataToBeUpdated.caseRoleCode = params.data.caseRoleCode;
                    }

                    const allocationbeforeupdate = this.project.projectResourceAllocations.find(
                        (x) => x.id === resourceDataToBeUpdated.id
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
                        this._notifyService.showValidationMsg(validationMessage);
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
            event: "teamsGrid",
            showMoreThanYearWarning: false,
            splitSuccessMessage: successMessage
        });
    }

    onTogglePinHandler(isPinned: boolean) {
        const pipelineId = this.project.projectDetails.pipelineId || null;
        const oldCaseCode = this.project.projectDetails.oldCaseCode || null;

        if (isPinned) {
            this.addProjectToUserExceptionShowList.emit({ pipelineId, oldCaseCode });
        } else {
            this.removeProjectFromUserExceptionShowList.emit({ pipelineId, oldCaseCode });
        }
    }

    onToggleHideHandler(isHidden: boolean) {
        const pipelineId = this.project.projectDetails.pipelineId || null;
        const oldCaseCode = this.project.projectDetails.oldCaseCode || null;

        if (isHidden) {
            this.addProjectToUserExceptionHideList.emit({ pipelineId, oldCaseCode });
        } else {
            this.removeProjectFromUserExceptionHideList.emit({ pipelineId, oldCaseCode });
        }
    }

    onCaseRollHandler() {
        if (!ValidationService.isCaseEligibleForRoll(this.project.projectDetails.endDate)) {
            this._notifyService.showValidationMsg(ValidationService.caseRollNotAllowedForInActiveCasesMessage);
        } else {
            this.openCaseRollForm.emit({ project: this.project.projectDetails });
        }
    }

    onSTACommitmentHandler(){
        this.openStaCommitmentForm.emit({ project: this.project.projectDetails });
    }

    openPlaceholderFormhandler(event) {
        this.openPlaceholderForm.emit({
            project: this.project.projectDetails,
            placeholderAllocationData: event?.placeholderAllocationData,
            isUpdateModal: event?.isUpdateModal
        });
    }

    updateProjectChangesHandler(event) {
        this.updateProjectChanges.emit(event);
    }

    getActiveResourcesForProjectOnOrAfterSelectedDateHandler(event) {
        this.activeResourcesStartDate = event.selectedDate;
        if (this.project.projectDetails.oldCaseCode) {
            this.populateCaseGantt(this.project.projectDetails.oldCaseCode, this.activeResourcesStartDate);
        } else if (this.project.projectDetails.pipelineId) {
            this.populateOpportunityGantt(this.project.projectDetails.pipelineId, this.activeResourcesStartDate);
        }
    }

    getContextMenuItems(params) {
        if (!!params.node) {
            const componentInstance = params.context.componentParent;

            let selectedRows = [];
            selectedRows =
                params.api.getSelectedRows().length > 0
                    ? selectedRows.concat(params.api.getSelectedRows())
                    : selectedRows.concat(params.node.data);

            const customContextMenuItems = [
                {
                    name: "Split Allocation",
                    action: function () {
                        componentInstance.openSplitAllocationDialog.emit({ allocationData: params.node.data });
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
                            componentInstance.deleteSelectedResourcesConfirmation({resourceAllocations : selectedRows});
                        } else {
                            componentInstance.deleteSelectedResourcesConfirmation({resourceAllocations : params.node.data});
                        }
                    },
                    tooltip: "To delete the selected allocation(s)"
                },
                {
                    name: "Update Start Date",
                    action: function () {
                        componentInstance.openUpdateDateForm({ title: "Start", resourceAllocations: selectedRows });
                    }
                },
                {
                    name: "Update End Date",
                    action: function () {
                        componentInstance.openUpdateDateForm({ title: "End", resourceAllocations: selectedRows });
                    }
                }
            ];

            let result = [
                "copy",
                "copyWithHeaders",
                "export",
                ...(componentInstance.isAGGridReadOnly ? [] : customContextMenuItems)
            ];

            return result;
        }
    }

    onGridReady(params) {
        this.gridApi = params.api;
        this.gridColumnApi = params.columnApi;
    }

    onGridSizeChanged(params: GridSizeChangedEvent) {
        params.api.sizeColumnsToFit();
    }

    openSplitAllocationDialogHandler(data) {
        this.openSplitAllocationDialog.emit({ allocationData: data });
    }

    updateResourceCommitmentHandler(event) {
        this.updateResourceCommitment.emit(event);
    }
}
