import { AfterViewInit, Component, ElementRef, EventEmitter, HostListener, OnDestroy, OnInit, Output, ViewEncapsulation } from '@angular/core';
import { CoreService } from 'src/app/core/core.service';
import { DemandFilterCriteria } from 'src/app/shared/interfaces/demandFilterCriteria.interface';
import { Office } from 'src/app/shared/interfaces/office.interface';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { CasePlanningWhiteboardService } from '../case-planning-whiteboard.service';
import { CaseType as CaseTypeEnum, ProjectType, ServiceLine, CasePlanningBoardPlaygroundSessionOptions, CasePlanningBoardBucketEnum } from 'src/app/shared/constants/enumMaster';
import { DateService } from 'src/app/shared/dateService';
import { transferArrayItem } from '@angular/cdk/drag-drop';
import { forkJoin, of, Subject, Subscription } from 'rxjs';
import { NotificationService } from 'src/app/shared/notification.service';
import { ValidationService } from 'src/app/shared/validationService';
import { OpportunityService } from 'src/app/overlay/behavioralSubjectService/opportunity.service';
import { PlanningCardService } from 'src/app/core/services/planning-card.service';
import { UpdateCaseCardComponent } from '../update-case-card/update-case-card.component';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { CaseOppChanges } from 'src/app/shared/interfaces/caseOppChanges.interface';
import { AddTeamSkuComponent } from 'src/app/case-planning/add-team-sku/add-team-sku.component';
import { PlaceholderAssignmentService } from 'src/app/overlay/behavioralSubjectService/placeholderAssignment.service';
import { OverlayMessageService } from 'src/app/overlay/behavioralSubjectService/overlayMessage.service';
import { GridApi,GridOptions, ICellRendererParams } from 'ag-grid-enterprise';
import { SupplyFilterCriteria } from 'src/app/shared/interfaces/supplyFilterCriteria.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { CasePlanningBoardBucket } from 'src/app/shared/interfaces/case-planning-board-bucket.interface';
import { CasePlanningBoardBucketPreferences } from 'src/app/shared/interfaces/case-planning-board-bucket-preferences.interface';
import { PegOpportunityDialogService } from 'src/app/overlay/dialogHelperService/peg-opportunity-dialog.service';
import { JoinPlaygroundPopUpComponent } from '../join-playground-pop-up/join-playground-pop-up.component';
import { QuickAddFormComponent } from 'src/app/shared/quick-add-form/quick-add-form.component';
import { ResourceAssignmentService } from 'src/app/overlay/behavioralSubjectService/resourceAssignment.service';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { UserPlaygroundSession } from 'src/app/shared/interfaces/userPlaygroundSession';
import { CasePlanningPlaygroundService } from 'src/app/core/services/case-planning-playground.service';
import { StaffableTeamsModalComponent } from '../staffable-teams-modal/staffable-teams-modal.component';
import { CasePlanningBoardStaffableTeamsColumn } from 'src/app/shared/interfaces/case-planning-board-staffable-teams-column';
import { CasePlanningBoardStaffableTeamViewModel } from 'src/app/shared/interfaces/case-planning-board-staffable-team-view-model';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { AgGridDatepickerComponent } from 'src/app/shared/ag-grid-datepicker/ag-grid-datepicker.component';
import { AgGridOfficeDropdownComponent } from 'src/app/shared/ag-grid-office-dropdown/ag-grid-office-dropdown.component';
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { CommonService } from 'src/app/shared/commonService';
import { BackfillFormComponent } from 'src/app/shared/backfill-form/backfill-form.component';
import { AgGridNoteCellRendererComponent } from "../ag-grid-note-cell-renderer/ag-grid-note-cell-renderer.component";
import { DemandNotesModalComponent } from '../demand-notes-modal/demand-notes-modal.component';
import { ProjectsGanttBodyComponent } from 'src/app/case-planning/projects-gantt-body/projects-gantt-body.component';
import { CortexSkuMapping } from 'src/app/shared/interfaces/cortex-sku-mapping.interface';
import { CortexSkuService } from 'src/app/shared/cortex-sku.service';

import { Store, select } from '@ngrx/store';
import * as fromProjectssGlobalStore from "../../../app/state/reducers/project-allocations.reducer";
import * as ProjectsGlobalStoreActions from "../../../app/state/actions/project-allocations.action";

@Component({
  selector: 'app-planning-board-stage',
  templateUrl: './planning-board-stage.component.html',
  styleUrls: ['./planning-board-stage.component.scss']
})
export class PlanningBoardStageComponent implements OnInit, AfterViewInit, OnDestroy {
  // Column container grabber
  public height = 600;
  public y = 100;
  private oldY = 0;
  private grabber = false;

  // AG Grid
  public rowData: any[] = [];
  public columnDefs;
  public gridApi;
  public gridColumnApi;
  public gridOptions: GridOptions;
  public defaultColDef;
  public localCellStyles = {
    "color": "#000",
    "font-size": "12px",
  };

  // Bucket Headers
  public planningBoardBucketLookUp: CasePlanningBoardBucket[] = [];
  public bucketIdsToIncludeInDemand = "";

  private supplyMetricsData;
  private isSupplyMetricsLodaed = false;
  private isDemandMetricsLoaded = false;
  private isStaffableTeamsLoaded = false;
  private updatedProjectIds = [];

  private userPreferences: UserPreferences;
  private demandFilterCriteriaObj: DemandFilterCriteria = {} as DemandFilterCriteria;
  private supplyFilterCriteriaObj: SupplyFilterCriteria = {} as SupplyFilterCriteria;
  private homeOffice: Office;
  public planningBoard = [];
  public planningBoardColumnMetrics = [];
  private subPreferences: Subscription;
  private subData: Subscription = new Subscription();
  private overlayMessageServiceSub: Subscription = new Subscription();
  contextMenuOptions = [];
  tableContextMenuOptions = [];
  public hiddenColumns = [];
  public demandMetricsProjects;
  private NEW_DEMAND_COLUMN_TITLE = "New Demands";
  private DEMAND_METRICS_PROJECTS = "Projects To Be Included In Demand Metrics";
  private showProgressBar$: Subject<boolean> = new Subject();
  private showModalCloseButton$: Subject<boolean> = new Subject();
  public userPlaygroundSessionInfo: UserPlaygroundSession;
  public playgroundValidationObj = {
    isInValid: false,
    errorMessage: ""
  };
  bsModalRef: BsModalRef;
  staffableTeams: CasePlanningBoardStaffableTeamsColumn[];
  isGCStaffableTeamCountVisible: boolean = false;
  isPEGStaffableTeamCountVisible: boolean = false;
  newDemandsData: {
    title: string;
    projects: any[];
    count: number,
    buckets: any,
  } = {
      title: "",
      projects: [],
      count: 0,
      buckets: null
    };
  isPreviousWeekData = false;
  enableBulkSkuButton = false;
  disableBulkSkuButton = false;
  moveDemands = false;
  isCountOfIndividualResourcesToggle = false;
  enableNewlyAvailableHighlighting = false;
  enableMemberGrouping = false;
  officesFlat: Office[];
  officeHierarchy: OfficeHierarchy;
  latestNote: any = {};
  cortexSkuMappings: CortexSkuMapping[] = []; 
  storeSub: Subscription = new Subscription();

  @Output() showProgressBarEmitter = new EventEmitter<boolean>();
  @Output() showModalCloseButtonEmitter = new EventEmitter<boolean>();
  project: any;

  constructor(private planningBoardService: CasePlanningWhiteboardService,
    private coreService: CoreService,
    private opportunityService: OpportunityService,
    private planningCardService: PlanningCardService,
    private notifyService: NotificationService,
    private modalService: BsModalService,
    private placeholderAssignmentService: PlaceholderAssignmentService,
    private overlayMessageService: OverlayMessageService,
    private elementRef: ElementRef,
    private localStorageService: LocalStorageService,
    private pegOpportunityDialogService: PegOpportunityDialogService,
    private resourceAssignmentService: ResourceAssignmentService,
    private casePlanningPlaygroundService: CasePlanningPlaygroundService,
    private cortexSkuService: CortexSkuService,
    private projectsGlobalStore: Store<fromProjectssGlobalStore.State>
  ) { }

  ngOnInit(): void {
    this.homeOffice = this.coreService.loggedInUser.office;
    this.getLookupListFromLocalStorage();
    this.subscribeEvents();

    this.subscribeuserPrefences();
    // Load AG Grid Configurations
    this.loadGridConfigurations();
  }

  @HostListener("window:beforeunload", ["$event"])
  beforeWindowUnloadHandler($event) {
    if (this.userPlaygroundSessionInfo?.playgroundId) {
      $event.preventDefault();
      // $event.returnValue = "The whiteboard session in active. Please leave/end this session before leaving this page!!";
      // var result =  confirm("The whiteboard session in active. Please leave/end this session before leaving this page!!");
      // return result;
    }
  }

  private getLookupListFromLocalStorage() {
    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.officesFlat = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
    this.planningBoardBucketLookUp = this.localStorageService.get(ConstantsMaster.localStorageKeys.casePlanningBoardBuckets);
    this.bucketIdsToIncludeInDemand = this.planningBoardBucketLookUp.filter(x => x.includeInDemand).map(y => y.id).join(",");
  }

  ngAfterViewInit() {
    // Event Listener for dragging and expanding column Div
    this.elementRef.nativeElement
      .querySelector("#gr")
      .addEventListener("mousedown", this.onMouseDown.bind(this));

  }

  // On mouse move up or down
  @HostListener("document:mousemove", ["$event"])
  onMouseMove(event: MouseEvent) {
    if (!this.grabber) {
      return;
    } else {
      this.resizer(event.clientY - this.oldY);
      this.oldY = event.clientY;
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
    this.oldY = event.clientY;
  }

  // Set Grip and Grid Column APIs
  onGridReady(params) {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;
  }

  // Function to load set AG Grid options
  loadGridConfigurations() {
    this.gridOptions = <GridOptions>{
      enableBrowserTooltips: true,
      context: {
        componentParent: this,
      },
      rowMultiSelectWithClick: true,
      components: {
        AgGridNoteCellRendererComponent: AgGridNoteCellRendererComponent
      },
    };

    this.defaultColDef = {
      sortable: true, // can sort by client, project name, etc
      filter: true, // can search for case
      flex: 1
    };

    this.columnDefs = [

      {
        headerCheckboxSelection: true,
        checkboxSelection: true,
        width: 1,
        maxWidth: 40
      },
      {
        dndSource: true, width: 1, maxWidth: 40
      },
      {
        field: "type",
        cellRenderer: this.projectTypeRenderer,
        menuTabs: ["filterMenuTab"],
        tooltipField: 'type'
      },
      {
        field: "client",
        cellStyle: this.localCellStyles,
        menuTabs: ["filterMenuTab"],
        tooltipField: 'client'
      },
      {
        field: "projectName",
        cellStyle: this.localCellStyles,
        menuTabs: ["filterMenuTab"],
        tooltipField: 'projectName'
      },
      {
        field: "pegRFOppIcon",
        headerName: "",
        // cellStyle: this.localCellStyles,
        cellRenderer: this.pegRFOppIconRenderer,
        minWidth: 60,
        filter: false,
        menuTabs: ["filterMenuTab"],
      },
      {
        headerName: "Start Date",
        field: "originalStartDate",
        cellStyle: this.localCellStyles,
        menuTabs: ["filterMenuTab"],
        // Comparator is used for sorting the ag-grid on the basis of dates instead of just sorting as string values
        comparator: function (valueA, valueB, nodeA, nodeB, isInverted) {
          return Date.parse(DateService.formatDate(valueA)) - Date.parse(DateService.formatDate(valueB));
        },
        tooltipField: 'originalStartDate',
      },
      {
        headerName: "Override",
        headerClass: 'cozy-cottage',
        field: "overrideStartDate",
        editable: (params) => {
          return this.isDateEditable(params);
        },
        cellEditor: AgGridDatepickerComponent,
        menuTabs: ["filterMenuTab"],
        // Comparator is used for sorting the ag-grid on the basis of dates instead of just sorting as string values
        comparator: function (valueA, valueB, nodeA, nodeB, isInverted) {
          return Date.parse(DateService.formatDate(valueA)) - Date.parse(DateService.formatDate(valueB));
        },
        cellStyle: function (params) {
          if (params.data.oldCaseCode)
            return { backgroundColor: "#dddddd" };
          else if (!params.value)
            return;
          else if (!ValidationService.isValidDate(params.value) ||
            Date.parse(params.value || params.data.originalStartDate) > Date.parse(params.data.overrideEndDate || params.data.originalEndDate)
          ) {
            return { backgroundColor: "lightcoral" };
          } else {
            return { backgroundColor: "", color: "#cc0000" };
          }

        },
        tooltipField: 'overrideStartDate',
      },
      {
        headerName: "End Date",
        field: "originalEndDate",
        cellStyle: this.localCellStyles,
        menuTabs: ["filterMenuTab"],
        comparator: function (valueA, valueB, nodeA, nodeB, isInverted) {
          return Date.parse(DateService.formatDate(valueA)) - Date.parse(DateService.formatDate(valueB));
        },
        tooltipField: 'originalEndDate',
      },
      {
        headerName: "Override",
        headerClass: 'cozy-cottage',
        field: "overrideEndDate",
        editable: (params) => {
          return this.isDateEditable(params);
        },
        cellEditor: AgGridDatepickerComponent,
        menuTabs: ["filterMenuTab"],
        comparator: function (valueA, valueB, nodeA, nodeB, isInverted) {
          return Date.parse(DateService.formatDate(valueA)) - Date.parse(DateService.formatDate(valueB));
        },
        cellStyle: function (params) {
          if (params.data.oldCaseCode)
            return { backgroundColor: "#dddddd" };
          else if (!params.value)
            return;
          else if (!ValidationService.isValidDate(params.value) ||
            Date.parse(params.data.overrideStartDate || params.data.originalStartDate) > Date.parse(params.data.overrideEndDate || params.data.originalEndDate)
          ) {
            return { backgroundColor: "lightcoral" };
          } else {
            return { backgroundColor: "", color: "#cc0000" };
          }

        },
        tooltipField: 'overrideEndDate',
      },
      {
        field: "sku",
        cellStyle: this.localCellStyles,
        cellRenderer: this.skuRenderer,
        menuTabs: ["filterMenuTab"],
        tooltipField: 'sku',
      },
      {
        field: "manager",
        cellStyle: this.localCellStyles,
        menuTabs: ["filterMenuTab"],
        tooltipField: 'manager',
      },
      {
        field: "office",
        maxWidth: 150,
        cellStyle: this.localCellStyles,
        menuTabs: ["filterMenuTab"],
        tooltipField: 'office',
      },
      {
        headerName: "Override",
        headerClass: 'cozy-cottage',
        field: "overrideOfficeCodes",
        editable: true,
        cellEditorSelector: function (params) {
          const projectType = params.data.type;
          if (projectType === "Planning Card") {
            return {
              component: AgGridOfficeDropdownComponent,
              params: {
                cellHeight: 45,
                values: params.context.componentParent.officeHierarchy,
                popup: true
              }
            };
          } else {
            return {
              component: 'agRichSelectCellEditor',
              params: { values: params.context.componentParent.officesFlat.map(x => x.officeName) },
              popup: true
            };
          }
        },
        valueGetter: function (params) {
          if (params.data.overrideOfficeCodes) {
            const officeCodes = params.data.overrideOfficeCodes.toString();
            const officeAbbreviation = params.context.componentParent.officesFlat.filter(x => officeCodes.includes(x.officeCode)).map(x => x.officeAbbreviation).join(",");
            return officeAbbreviation;
          }

        },
        valueSetter: function (params) {
          if (params.newValue) {
            params.data.overrideOfficeCodes = Array.isArray(params.newValue)
              ? params.newValue.join(",")
              : params.context.componentParent.officesFlat.filter(x => params.newValue.includes(x.officeName)).map(x => x.officeCode).join(",");
          } else {
            params.data.overrideOfficeCodes = '';

            return true;
          }
        },
        maxWidth: 150,
        menuTabs: ["filterMenuTab"],
        cellStyle: function (params) {
          if (!params.value)
            return;
          return { backgroundColor: "", color: "#cc0000" };
        },
        tooltipField: 'overrideOfficeAbbreviations',
      },
      {
        field: "originalProbabilityPercent",
        headerName: "%",
        maxWidth: 110,
        cellStyle: this.localCellStyles,
        // comparator is used for custom sorting. Here we are converting the allocation, which is in string,
        // to Int and comparing the values for the order given
        comparator: function (valueA, valueB, nodeA, nodeB, isInverted) {
          if (!valueB) {
            valueB = '0%';
          }
          return parseInt(valueA.slice(0, valueA.indexOf('%'))) - parseInt(valueB.slice(0, valueB.indexOf('%')));
        },
        menuTabs: [],
        tooltipField: 'originalProbabilityPercent',
      },
      {
        field: "overrideProbabilityPercent",
        headerClass: 'cozy-cottage',
        headerName: "Override",
        editable: (params) => {
          return this.isProbabilityEditable(params);
        },
        maxWidth: 110,
        // comparator is used for custom sorting. Here we are converting the allocation, which is in string,
        // to Int and comparing the values for the order given
        comparator: function (valueA, valueB, nodeA, nodeB, isInverted) {
          if (!valueB) {
            valueB = '0%';
          }
          return parseInt(valueA.slice(0, valueA.indexOf('%'))) - parseInt(valueB.slice(0, valueB.indexOf('%')));
        },
        menuTabs: [],
        cellStyle: function (params) {
          if (!params.data.pipelineId)
            return { backgroundColor: "#dddddd" };
          if (!params.value)
            return;
          let paramValue = params.value.toString();
          if (!!paramValue && paramValue.indexOf("%") !== -1) {
            paramValue = paramValue.slice(0, paramValue.indexOf("%"));
          }
          if (!ValidationService.validateProbablePercentage(paramValue).isValid) {
            return { backgroundColor: "lightcoral" };
          } else {
            return { backgroundColor: "", color: "#cc0000" };
          }
        },
        cellRenderer: function (params) {
          if (!!params.data.pipelineId) {
            let paramValue = params.value.toString();
            if (!!paramValue && paramValue.indexOf("%") === -1) {
              paramValue += "%";
            }
            return paramValue;
          }
        },
        tooltipField: 'overrideProbabilityPercent',
      },
      {
        headerName: "Case Code",
        field: "oldCaseCode",
        maxWidth: 150,
        cellStyle: this.localCellStyles,
        menuTabs: ["filterMenuTab"],
        tooltipField: 'oldCaseCode',
      },
      {
        field: "notes",
        maxWidth: 100,
        cellStyle: this.localCellStyles,
        cellRenderer: "AgGridNoteCellRendererComponent",
        menuTabs: ["filterMenuTab"],
      }

    ];
  }

  onCellClicked($event) {
    if ($event.colDef.field === "notes") {
      this.gridApi.deselectAll();
      this.openNotesModalHandler($event.data);
    }
  }

  isDateEditable(params) {
    return params.data.type != 'New Case';
  }

  isProbabilityEditable(params) {
    return params.data.type === ProjectType.Opportunity;
  }

  // Renders cell for SKU Term or SKU Selection
  pegRFOppIconRenderer(params: ICellRendererParams) {
    const pegRFOppIconElement = document.createElement("span");
    const componentInstance = params.context.componentParent;
    const project = params.data;

    // Show icon if value exists
    if (componentInstance.coreService.loggedInUserClaims.PegC2CAccess && params.value) {
      pegRFOppIconElement.innerHTML = `
      <a title="PEG Opportunity Details" class="rf-overlay-icon-container">
        <img src="assets/img/fence-icon.png" alt="PEG Opportunity Details" style="width: 20px; opacity: 0.4 !important;" />
      </a>
      `;
    } else {
      // Else show nothing
      pegRFOppIconElement.innerHTML = ``;
    }

    pegRFOppIconElement.style.cursor = "pointer"; // set cursor to show cell is clickable
    pegRFOppIconElement.onclick = () => {
      const finalProject = componentInstance.findProjectInNewDemands([].concat(project));
      componentInstance.openPegRFPopUpHandler(finalProject[0].pegOpportunityId);
    };

    return pegRFOppIconElement;
  }

  onSelectionChanged() {
    if (this.gridApi.getSelectedRows().length > 0)
      this.enableBulkSkuButton = true;
    else
      this.enableBulkSkuButton = false;
  }


  onCellValueChanged(params) {
    if (!(params.newValue === params.oldValue)) {

      if (!this.IsValidObject(params.data)) {
        return;
      }

      if (params.data.planningCardId) {
        params.data.startDate = params.data.overrideStartDate;
        params.data.endDate = params.data.overrideEndDate;
        params.data.office = params.context.componentParent.officesFlat.filter(x => params.data.overrideOfficeCodes.includes(x.officeCode)).map(x => x.officeAbbreviation).join(",");

        params.data.originalStartDate = params.data.overrideStartDate;
        params.data.originalEndDate = params.data.overrideEndDate;

        let updatedPlanningCard: PlanningCard = {
          id: params.data.planningCardId,
          name: params.data.projectName,
          startDate: DateService.convertDateInBainFormat(params.data.overrideStartDate),
          endDate: DateService.convertDateInBainFormat(params.data.overrideEndDate),
          sharedOfficeCodes: params.data.overrideOfficeCodes,
          sharedStaffingTags: params.data.sharedStaffingTags,
          isShared: true
        }

        this.updatePlanningCardEmitterHandler(updatedPlanningCard);
      } else {

        params.data.startDate = params.data.overrideStartDate || params.data.originalStartDate;
        params.data.endDate = params.data.overrideEndDate || params.data.originalEndDate;
        params.data.probabilityPercent = params.data.overrideProbabilityPercent || params.data.originalProbabilityPercent;

        const updatedCaseOppChanges: CaseOppChanges = {
          pipelineId: params.data.pipelineId,
          oldCaseCode: params.data.oldCaseCode,
          startDate: params.data.overrideStartDate,
          endDate: params.data.overrideEndDate,
          probabilityPercent: params.data.overrideProbabilityPercent ? parseInt(params.data.overrideProbabilityPercent.replace('%', '')) : null,
          notes: params.data.notes,
          caseServedByRingfence: params.data.caseServedByRingfence,
          staffingOfficeCode: params.data.overrideOfficeCodes
        }

        this.opportunityService.updateProjectChangesHandler(updatedCaseOppChanges);
        this.updateNewDemandObjectForCaseOrOpp(updatedCaseOppChanges);
      }

      this.removeRowIfNotPartOfDemandFilter(params);
      this.gridApi.refreshCells();
    }
  }

  updateNewDemandObjectForCaseOrOpp(updatedCaseOppChanges) {
    let upsertedProject = this.newDemandsData.projects.find(x => (updatedCaseOppChanges.pipelineId && x.pipelineId == updatedCaseOppChanges.pipelineId
      || updatedCaseOppChanges.oldCaseCode && x.oldCaseCode == updatedCaseOppChanges.oldCaseCode));

    if (upsertedProject) {
      upsertedProject.overrideStartDate = updatedCaseOppChanges.startDate;
      upsertedProject.overrideEndDate = updatedCaseOppChanges.endDate;
      upsertedProject.overrideProbabilityPercent = updatedCaseOppChanges.probabilityPercent;
      upsertedProject.staffingOfficeCode = updatedCaseOppChanges.staffingOfficeCode;
      upsertedProject.staffingOfficeAbbreviation = this.officesFlat.find(x => x.officeCode == updatedCaseOppChanges.staffingOfficeCode)?.officeAbbreviation;
    }
  }

  removeRowIfNotPartOfDemandFilter(params) {
    const updatedRowData = params.data;
    if (!updatedRowData) {
      return;
    }
    if (!!updatedRowData.overrideOfficeCodes
      && !CommonService.isCommonElementsInTwoCommaSeparatedStrings(updatedRowData.overrideOfficeCodes, this.demandFilterCriteriaObj.officeCodes)) {
      this.removeRowFromAgGrid(updatedRowData);
      this.removeRowFromNewDemandObj(updatedRowData);
      this.updateNewCasesCount();
    }
  }

  removeRowFromAgGrid(updatedRowData) {
    const rowsToBeDeleted = this.rowData.filter(project =>
      updatedRowData.id === project.id);

    if (rowsToBeDeleted.length > 0) {
      rowsToBeDeleted.forEach(r => {
        this.rowData.splice(this.rowData.indexOf(r), 1);
      })
      this.gridApi.applyTransaction({ remove: rowsToBeDeleted });
    }
  }

  removeRowFromNewDemandObj(updatedRowData) {
    this.newDemandsData.projects = this.newDemandsData.projects.filter(project =>
      updatedRowData.type == 'Planning Card'
        ? project.planningCardId !== updatedRowData.id
        : updatedRowData.type == 'Opportunity'
          ? project.pipelineId !== updatedRowData.id
          : project.oldCaseCode !== updatedRowData.id);
  }

  IsValidObject(obj) {
    let isValidObj = true;
    if (
      (obj.overrideStartDate && !ValidationService.isValidDate(obj.overrideStartDate))
      || (obj.overrideEndDate && !ValidationService.isValidDate(obj.overrideEndDate))
    ) {
      isValidObj = false;
      this.notifyService.showValidationMsg(ValidationService.dateInvalidMessage);
    }

    if (
      Date.parse(obj.overrideStartDate || obj.originalStartDate) > Date.parse(obj.overrideEndDate || obj.originalEndDate)
    ) {
      isValidObj = false;
      this.notifyService.showValidationMsg(ValidationService.startDateGreaterThanEndDate);
    }

    if (obj.overrideProbabilityPercent) {
      const probabilityPercentValidObj = ValidationService.validateProbablePercentage(parseInt(obj.overrideProbabilityPercent.replace('%', '')));

      if (!probabilityPercentValidObj.isValid) {
        isValidObj = false;
        this.notifyService.showValidationMsg(probabilityPercentValidObj.errorMessage);
      }
    }

    return isValidObj;
  }

  setDefaultValuesForSKUs(selectedRows = null) {
    this.disableBulkSkuButton = true;
    const rowsSelection = this.gridApi.getSelectedRows().length > 0 ? this.gridApi.getSelectedRows() : selectedRows;
    let rowsNeedsToBeUpdated = rowsSelection.filter(x => !x.sku);
    if (rowsNeedsToBeUpdated.length > 0) {
      this.getPlaceholderAllocations(rowsNeedsToBeUpdated);
    }
    else {
      this.disableBulkSkuButton = false;
      this.notifyService.showInfo(`Placeholder assignment already exists for selected row`);
    }
  }

  disableMoveDemandButton() {
    return this.gridApi?.getSelectedRows().length > 0 ? true : false;
  }

  getPlaceholderAllocations(rowsNeedsToBeUpdated) {
    let caseCodes = [];
    let pipelineIds = [];
    let planningCardIds = [];
    rowsNeedsToBeUpdated.filter(x => x.caseCode).forEach(x => {
      caseCodes.push(x.caseCode)
    })
    rowsNeedsToBeUpdated.filter(x => x.pipelineId).forEach(x => {
      pipelineIds.push(x.pipelineId)
    })
    rowsNeedsToBeUpdated.filter(x => x.planningCardId).forEach(x => {
      planningCardIds.push(x.planningCardId)
    })
    forkJoin([
      caseCodes.length > 0 ? this.planningBoardService.getAllocationsByOldCaseCodes(caseCodes?.toString()) : of(null),
      pipelineIds.length > 0 ? this.planningBoardService.getAllocationsByPipelineIds(pipelineIds?.toString()) : of(null),
      planningCardIds.length > 0 ? this.planningBoardService.getAllocationsByPlanningCardIds(planningCardIds?.toString()) : of(null)
    ]).subscribe(result => {
      this.getProjectsWithNoSku(result[0], result[1], result[2], rowsNeedsToBeUpdated);
    });
  }

  getProjectsWithNoSku(caseAllocations, opportunityAllocations, planningCardAllocations, rowsNeedsToBeUpdated) {
    let placeholderAllocations: PlaceholderAllocation[] = [];
    const levelGrades = ["M1", "A1", "A1", "C1", "C1"];
    rowsNeedsToBeUpdated.filter(x => caseAllocations?.forEach(cas => x.caseCode !== cas.oldCaseCode)
      && opportunityAllocations?.forEach(opp => !x.oldCaseCode && x.pipelineId !== opp.pipelineId)
      && planningCardAllocations?.ForEach(pc => !x.oldCaseCode && x.planningCardId !== pc.planningCardId));
    rowsNeedsToBeUpdated.forEach(selectedRow => {
      const project = this.newDemandsData.projects.find(x => x.oldCaseCode === selectedRow.id
        || x.pipelineId === selectedRow.id
        || x.planningCardId === selectedRow.id);
      levelGrades.forEach(levelGrade => {
        if (project) {
          const placeholderAllocation = this.populatePlacholderAllocationProperties(project, levelGrade);
          placeholderAllocations.push(placeholderAllocation);
        }
      });
    });
    this.placeholderAssignmentService.upsertPlcaeholderAllocations(placeholderAllocations, null, null);
    this.disableBulkSkuButton = false;
  }

  private populatePlacholderAllocationProperties(project, levelGrade) {
    const planningCardOfficeCode = project.sharedOfficeCodes?.split(',')[0];
    const planningCardOfficeAbbreviation = project.sharedOfficeAbbreviations?.split(',')[0];
    const projectBillingOfficeCode = project.staffingOfficeCode ?? project?.billingOfficeCode;
    const projectBillingOfficeAbbreviation = project.staffingOfficeAbbreviation ?? project?.billingOfficeAbbreviation;

    let placeholderAllocation: PlaceholderAllocation = {
      id: null,
      planningCardId: project.planningCardId,
      caseName: project.caseName,
      caseTypeCode: project.caseTypeCode,
      clientName: project.clientName,
      oldCaseCode: project.oldCaseCode,
      employeeCode: null,
      employeeName: "",
      currentLevelGrade: levelGrade,
      commitmentTypeCode: null,
      serviceLineCode: "SL0001",
      serviceLineName: "General Consulting",
      operatingOfficeCode: project?.planningCardId ? planningCardOfficeCode : projectBillingOfficeCode,
      operatingOfficeAbbreviation: project?.planningCardId ? planningCardOfficeAbbreviation : projectBillingOfficeAbbreviation,
      pipelineId: project.pipelineId,
      opportunityName: project.opportunityName,
      investmentCode: null,
      investmentName: null,
      caseRoleCode: null,
      allocation: 100,
      startDate: project.startDate,
      endDate: project.endDate,
      lastUpdatedBy: null,
      isPlaceholderAllocation: true,
      isConfirmed: false
    };
    return placeholderAllocation;
  }

  // Renders cell for SKU Term or SKU Selection
  skuRenderer(params: ICellRendererParams) {
    const skuElement = document.createElement("span");
    const componentInstance = params.context.componentParent;
    const project = params.data;

    // If value exists, show value
    if (params.value !== null && params.value !== "") {
      skuElement.innerHTML = `
        <a title="${params.value}">${params.value}</a>
      `;
    } else {
      // Else show user icon to add SKU
      skuElement.innerHTML = `
        <p class="m-0" style="width: fit-content;"><i class="fas fa-user-plus" title="Add Placeholder"></i></p>
      `;
    }

    skuElement.style.cursor = "pointer"; // set cursor to show cell is clickable
    skuElement.onclick = () => {
      const finalProject = componentInstance.findProjectInNewDemands([].concat(project));
      componentInstance.openAddTeamSkuFormHandler(finalProject[0]);
    };

    return skuElement;
  }

  // Renders cell for Project Type
  projectTypeRenderer(params: ICellRendererParams) {
    const typeElement = document.createElement("span");
    typeElement.innerText = params.value;
    typeElement.style.padding = "4px 8px";
    typeElement.style.borderRadius = "4px";
    typeElement.style.fontWeight = "bold";
    typeElement.style.fontSize = "12px";
    typeElement.style.color = "#000";

    // Set background color of label based on project type
    if (params.value == "Opportunity") {
      typeElement.style.backgroundColor = "#fae62a";
    } else if (params.value == "New Case") {
      typeElement.style.backgroundColor = "#f9a740";
      typeElement.style.color = "#fff";
    } else if (params.value == "ActiveCase") {
      typeElement.style.backgroundColor = "#4cb6fb";
    } else {
      typeElement.style.backgroundColor = "#07c29f";
      typeElement.style.color = "#fff";
    }

    return typeElement;
  }

  private subscribeuserPrefences() {
    // this is done so that whenever user changes their user settings, it reflects in the projects and resources data
    this.subPreferences = this.coreService.getUserPreferences().subscribe((userPreferences) => {
      this.userPreferences = userPreferences;

      // If user accidently closed the playgroundSession, then resume it once they open the page again
      // user to conitue session after close or refresh
      this.userPlaygroundSessionInfo = this.localStorageService.get(ConstantsMaster.localStorageKeys.userPlaygroundSession);
      if (this.userPlaygroundSessionInfo?.playgroundId) {
        this.getPlaygroundSessionFiltersAndLoadPlanningBoardDataForPlayground(this.userPlaygroundSessionInfo.playgroundId);
      } else {
        this.updateSupplyAndDemandSettings();
        this.loadPlanningBoardData();
        this.setStaffableRowVisibility();
      }

    });
  }

  loadPlanningBoardData() {
    this.getCasePlanningBoardDataBySelectedValues();
    this.getNewDemandsDataBySelectedValues();
    this.getAvailabilityMetricsByFilterValues();
    this.getStaffableTeamsDataBySupply();
    this.getCortexSkuMappings();
  }

  getCortexSkuMappings() {
    this.cortexSkuMappings = this.localStorageService.get(ConstantsMaster.localStorageKeys.cortexSkuMappings);

    if (!this.cortexSkuMappings || !this.cortexSkuMappings.length) {
      this.subData.add(
        this.cortexSkuService.getCortexSkuMappings()
          .subscribe((mappings) => {
            this.cortexSkuMappings = [];
            this.cortexSkuMappings = mappings;
            this.localStorageService.set(ConstantsMaster.localStorageKeys.cortexSkuMappings, this.cortexSkuMappings, "weekly");
          })
      );
    }
  }

  getStaffableTeamsDataBySupply() {
    this.isStaffableTeamsLoaded = false;

    let startDate = DateService.convertDateInBainFormat(DateService.getStartOfWeek());
    if (this.isPreviousWeekData) {
      startDate = DateService.addWeeks(startDate, -6);
    }

    const officeCodes = this.supplyFilterCriteriaObj.officeCodes;
    const startWeek = DateService.convertDateInBainFormat(startDate);
    const endWeek = DateService.addWeeks(startWeek, 6);

    this.subData.add(
      this.planningBoardService.getCasePlanningBoardStaffableTeams(officeCodes, startWeek, endWeek)
        .subscribe((staffableTeams) => {
          this.staffableTeams = [];
          this.staffableTeams = staffableTeams
          this.isStaffableTeamsLoaded = true;

          this.calculateCountForOfficeHierarchy();
          this.createMetricsForSupplyAndDemand();
        })
    );
  }

  private calculateCountForOfficeHierarchy() {
    if (!!this.staffableTeams && this.staffableTeams.length > 0)
      this.staffableTeams.forEach(weekColumn => {
        this.updateGCAndPegTeamCountByWeekColumn(weekColumn);
      });
  }

  updateGCAndPegTeamCountByWeekColumn(weekColumn) {
    if (!!weekColumn) {
      const children = weekColumn.staffableTeams.staffableTeamChildren;
      const childrenForCountCalculation = this.getChildrenForCountCalculation(weekColumn.staffableTeams);
      weekColumn.staffableTeams.gcTeamCount = this.calculateGCCount(childrenForCountCalculation);
      weekColumn.staffableTeams.pegTeamCount = this.calculatePegCount(childrenForCountCalculation);

      if (children.length > 0) {
        this.setGCAndPegCountForHierarchy(children);
      }
    }
  }

  setGCAndPegCountForHierarchy(children) {
    if (!!children) {
      if (Array.isArray(children) && children.length > 0) {
        children.forEach(child => {
          if (child.staffableTeamChildren.length > 0) {
            const childrenForCountCalculation = this.getChildrenForCountCalculation(child);
            child.gcTeamCount = this.calculateGCCount(childrenForCountCalculation);
            child.pegTeamCount = this.calculatePegCount(childrenForCountCalculation);

            this.setGCAndPegCountForHierarchy(child.staffableTeamChildren);
          }
        });
      }
    }
  }

  getChildrenForCountCalculation(staffableTeam) {
    const parent = staffableTeam.staffableTeamChildren;
    let children = this.getChildren(parent);

    return children.length > 0 ? [...new Set(children)] : null;
  }

  calculateGCCount(children) {
    return !!children && children.length > 0
      ? children.map((x) => x?.gcTeamCount)
        ?.reduce((prev, curr) => prev + curr, 0)
      : 0;
  }

  calculatePegCount(children) {
    return !!children && children.length > 0
      ? children.map((x) => x?.pegTeamCount)
        ?.reduce((prev, curr) => prev + curr, 0)
      : 0;
  }

  getChildren(parent, returnValue = []) {
    if (!!parent) {
      let children = parent.staffableTeamChildren;
      if (!children)
        children = parent;
      if (!!children && children.length > 0) {
        children.forEach(x => {
          let grandChildren = this.getChildren(x, returnValue);
        });
      }
      else {
        if (Array.isArray(parent)) {
          parent.forEach(x => {
            if (x.staffableTeamChildren.length == 0) {
              returnValue.push(x);
            }
            else {
              let grandChildren = this.getChildren(x, returnValue);
            }
          });
        }
        else {
          returnValue.push(parent);
        }
      }
    }
    return returnValue;
  }

  private updateSupplyAndDemandSettings() {
    var startOfWeek = DateService.getStartOfWeek();

    let endOfDuration = new Date(startOfWeek);
    endOfDuration.setDate(endOfDuration.getDate() + 5 * 7 + 4); //need data till end of 6 weeks

    // Default date range will be today + 40 days on load
    const defaultDateRange = DateService.getFormattedDateRange({
      startDate: startOfWeek,
      endDate: endOfDuration,
    });

    if (this.userPreferences && typeof this.userPreferences === 'object') {

      //----------------Supply Criteria --------------------------------//
      this.supplyFilterCriteriaObj.startDate = defaultDateRange.startDate;
      this.supplyFilterCriteriaObj.endDate = defaultDateRange.endDate;
      this.supplyFilterCriteriaObj.officeCodes = this.userPreferences.supplyViewOfficeCodes || this.homeOffice.officeCode.toString();
      this.supplyFilterCriteriaObj.levelGrades = this.userPreferences.levelGrades;
      this.supplyFilterCriteriaObj.staffingTags = this.userPreferences.supplyViewStaffingTags;
      this.supplyFilterCriteriaObj.availabilityIncludes = this.userPreferences.availabilityIncludes;
      this.supplyFilterCriteriaObj.groupBy = this.userPreferences.groupBy;
      this.supplyFilterCriteriaObj.sortBy = this.userPreferences.sortBy;
      this.supplyFilterCriteriaObj.affiliationRoleCodes = this.userPreferences.affiliationRoleCodes;


      //Don't need these variables for now
      this.supplyFilterCriteriaObj.practiceAreaCodes = this.userPreferences.practiceAreaCodes;
      this.supplyFilterCriteriaObj.employeeStatuses = '';
      this.supplyFilterCriteriaObj.positionCodes = this.userPreferences.positionCodes;
      this.supplyFilterCriteriaObj.staffableAsTypeCodes = '';

      //----------------------Demand Criteria ------------------------------//
      this.demandFilterCriteriaObj.startDate = defaultDateRange.startDate;
      this.demandFilterCriteriaObj.endDate = defaultDateRange.endDate;
      this.demandFilterCriteriaObj.officeCodes = this.userPreferences.demandViewOfficeCodes || this.homeOffice.officeCode.toString();
      this.demandFilterCriteriaObj.caseTypeCodes = this.userPreferences.caseTypeCodes || CaseTypeEnum.Billable;
      this.demandFilterCriteriaObj.demandTypes = "Opportunity,NewDemand,PlanningCards";// this.userPreferences.demandTypes; TODO: remove hardcoding once this is converted into a filter on planning board page
      this.demandFilterCriteriaObj.opportunityStatusTypeCodes = this.userPreferences.opportunityStatusTypeCodes;
      this.demandFilterCriteriaObj.caseAttributeNames = this.userPreferences.caseAttributeNames;
      this.demandFilterCriteriaObj.minOpportunityProbability = this.userPreferences.minOpportunityProbability;
      this.demandFilterCriteriaObj.industryPracticeAreaCodes = this.userPreferences.industryPracticeAreaCodes;
      this.demandFilterCriteriaObj.capabilityPracticeAreaCodes = this.userPreferences.capabilityPracticeAreaCodes;

      //Don't need these variables for now
      this.demandFilterCriteriaObj.caseExceptionShowList = '';
      this.demandFilterCriteriaObj.caseExceptionHideList = '';
      this.demandFilterCriteriaObj.opportunityExceptionShowList = '';
      this.demandFilterCriteriaObj.opportunityExceptionHideList = '';
      this.demandFilterCriteriaObj.caseAllocationsSortBy = '';
      this.demandFilterCriteriaObj.planningCardsSortOrder = '';
      this.demandFilterCriteriaObj.caseOppSortOrder = '';

    } else {
      this.supplyFilterCriteriaObj.startDate = defaultDateRange.startDate;
      this.supplyFilterCriteriaObj.endDate = defaultDateRange.endDate;
      this.supplyFilterCriteriaObj.officeCodes = this.homeOffice.officeCode.toString();
      this.supplyFilterCriteriaObj.staffingTags = ServiceLine.GeneralConsulting;

      this.demandFilterCriteriaObj.startDate = defaultDateRange.startDate;
      this.demandFilterCriteriaObj.endDate = defaultDateRange.endDate;
      this.demandFilterCriteriaObj.officeCodes = this.homeOffice.officeCode.toString();
      this.demandFilterCriteriaObj.caseTypeCodes = CaseTypeEnum.Billable;
    }

  }

  getAvailabilityMetricsByFilterValues() {
    this.isSupplyMetricsLodaed = false;
    let supplyFilterCriteriaObj = JSON.parse(JSON.stringify(this.supplyFilterCriteriaObj));

    if (this.isPreviousWeekData) {
      supplyFilterCriteriaObj.startDate = DateService.addWeeks(this.supplyFilterCriteriaObj.startDate, -7);
      supplyFilterCriteriaObj.endDate = DateService.addWeeks(this.supplyFilterCriteriaObj.startDate, 7);
    }
    else {
      supplyFilterCriteriaObj.startDate = DateService.addWeeks(DateService.convertDateInBainFormat(DateService.getStartOfWeek()), -1);
      let endOfDuration = new Date(supplyFilterCriteriaObj.startDate);
      supplyFilterCriteriaObj.endDate = DateService.convertDateInBainFormat(new Date(endOfDuration.setDate(endOfDuration.getDate() + 6 * 7 + 4)));
    }
    this.subData.add(
      this.planningBoardService.getAvailabilityMetricsByFilterValues(supplyFilterCriteriaObj)
        .subscribe((supplyMetricsData) => {
          this.supplyMetricsData = [];
          this.supplyMetricsData = supplyMetricsData;
          this.isSupplyMetricsLodaed = true;

          this.createMetricsForSupplyAndDemand();
        })
    );
  }

  getAvailabilityMetricsForPlaygroundById(playgroundId: string) {
    this.isSupplyMetricsLodaed = false;
    this.subData.add(
      this.planningBoardService.getAvailabilityMetricsForPlaygroundById(playgroundId)
        .subscribe((supplyMetricsData) => {
          this.supplyMetricsData = [];
          this.supplyMetricsData = supplyMetricsData;
          this.isSupplyMetricsLodaed = true;

          this.createMetricsForSupplyAndDemand();
        })
    );
  }

  getNewDemandsDataBySelectedValues() {
    this.showProgressBar$.next(true);
    this.subData.add(
      this.planningBoardService.getNewDemandsDataBySelectedValues(this.demandFilterCriteriaObj)
        .subscribe((newDemandsData) => {
          if (!!newDemandsData) {
            this.newDemandsData.title = this.NEW_DEMAND_COLUMN_TITLE;
            this.newDemandsData.projects = newDemandsData.projects;
            this.newDemandsData.buckets = this.getBucketsData(newDemandsData.projects, this.NEW_DEMAND_COLUMN_TITLE),

              this.updateNewCasesCount();

            // Send projects to generateRowData for AG Grid
            this.generateRowData(this.newDemandsData.projects);
            this.updatedProjectIds = [];
          }
          this.showProgressBar$.next(false);
        })
    );
  }

  getCasePlanningBoardDataBySelectedValues() {
    this.isDemandMetricsLoaded = false;
    this.showProgressBar$.next(true);

    let demandFilterCriteriaObj = JSON.parse(JSON.stringify(this.demandFilterCriteriaObj));

    if (this.isPreviousWeekData) {
      demandFilterCriteriaObj.startDate = DateService.addWeeks(this.demandFilterCriteriaObj.startDate, -6);
      demandFilterCriteriaObj.endDate = DateService.addWeeks(this.demandFilterCriteriaObj.startDate, 6);
    }
    else {
      demandFilterCriteriaObj.startDate = DateService.convertDateInBainFormat(DateService.getStartOfWeek());
      let endOfDuration = new Date(demandFilterCriteriaObj.startDate);
      demandFilterCriteriaObj.endDate = DateService.convertDateInBainFormat(new Date(endOfDuration.setDate(endOfDuration.getDate() + 5 * 7 + 4)));
    }
    this.subData.add(
      this.planningBoardService.getCasePlanningBoardDataBySelectedValues(demandFilterCriteriaObj)
        .subscribe((planningBoardData) => {
          this.isDemandMetricsLoaded = true;
          this.hiddenColumns = [];

          if (this.updatedProjectIds.length > 0 && !this.isUpdatedProjectInNewDemands()) {
            let updatedPlanningBoardColumn = planningBoardData.filter(x => x.projects.some(y =>
              this.updatedProjectIds.includes(y.oldCaseCode) ||
              this.updatedProjectIds.includes(y.pipelineId) ||
              this.updatedProjectIds.includes(y.planningCardId)));

            updatedPlanningBoardColumn.forEach(element => {
              if (element.title != this.DEMAND_METRICS_PROJECTS) {
                let title = DateService.convertDateInBainFormat(element.title);
                let planningBoardColumnData = this.planningBoard.find(data => data.title === title);
                planningBoardColumnData.buckets = this.getBucketsData(element.projects, element.title);

                // Send projects to generateRowData for AG Grid
                if (this.planningBoardColumnMetrics.length) {
                  //let projectsToIncludeInDemand = element.projects.filter(project => this.bucketIdsToIncludeInDemand.includes(project.bucketId.toString()));
                  let projectsToIncludeInDemand = element.projects.filter(project => project.bucketId == CasePlanningBoardBucketEnum.STAFF_FROM_MY_SUPPLY
                                                  || (project.bucketId == CasePlanningBoardBucketEnum.ACTION_NEEDED && project.includeInDemand == true));
                  const colIndex = this.planningBoardColumnMetrics.findIndex(x => x.date == element.title);
                  //const metricsData = [].concat(this.createSupplyMetricsData(element.title));

                  projectsToIncludeInDemand.forEach(project => {
                    let upsertedDemandMetricsProject = this.demandMetricsProjects.find(x => x.planningBoardId == project.planningBoardId);
                    upsertedDemandMetricsProject.skuTerm = project.skuTerm;
                  });
                  this.createMetricsForSupplyAndDemand();

                  //const metricsData = [].concat(this.createMetricsData(element.title, this.getSkuUsedForCalculatingDemand(projectsToIncludeInDemand)));
                  //this.planningBoardColumnMetrics.splice(colIndex, 1, { 'title': '', 'date': element.title, 'metrics': metricsData });

                }
              } else {
                this.demandMetricsProjects = planningBoardData.find(x => x.title === this.DEMAND_METRICS_PROJECTS).projects;
                this.createMetricsForSupplyAndDemand();
              }
            });

          }
          else {
            this.planningBoard = [];
            this.createPlanningBoardData(planningBoardData);
            this.setContextmenuOptions();
          }
          this.updatedProjectIds = [];
          this.showProgressBar$.next(false);

        }));
  }

  // Finding case from list of new cases, that matches selected case row
  findProjectInNewDemands(projectObjs) {
    var finalProjects = [];
    projectObjs.forEach(projectObj => {
      const finalProject = this.newDemandsData.projects.find(
        (project) => {
          return (
            (project.oldCaseCode === projectObj["id"] ||
              project.pipelineId === projectObj["id"] ||
              project.planningCardId === projectObj["id"])
          );
        }
      );
      finalProjects.push(finalProject);
    });


    return finalProjects;
  }

  // AG Grid - When row is dragged over to column grid
  onDragOver(event) {
    var dragSupported = event.dataTransfer.length;
    if (dragSupported) {
      event.dataTransfer.dropEffect = "move";
    }
    event.preventDefault();
  }

  // AG Grid - When row is dropped into column
  onDrop(event, bucket) {
    event.preventDefault();
    const caseData = JSON.parse(event.dataTransfer.getData("text"));

    // Finding case from list that matches selected case row
    let finalProject = this.findProjectInNewDemands([].concat(caseData));


    // Set project and week object
    let finalEvent = {
      project: finalProject,
      week: new Date(bucket.date)
        .toLocaleDateString("en-GB", {
          day: "2-digit",
          month: "short",
          year: "numeric",
        })
        .replace(/ /g, "-"),
    };

    let startingBucket = this.newDemandsData.buckets[0];
    let previousIndex = startingBucket.projects.findIndex(
      (x) => x == finalProject
    );
    let currentIndex = bucket.projects.length;

    // When moving case from New Cases table to column bucket
    if (
      DateService.convertDateInBainFormat(startingBucket.date) ===
      finalEvent.week &&
      startingBucket.bucketId === bucket.id
    ) {
      this.notifyService.showValidationMsg(
        ValidationService.sameBucketMovementMsg
      );
    } else {
      // Perform case move to new bucket if no errors
      this.moveCaseToNewBucket(
        [].concat(finalProject),
        startingBucket,
        bucket,
        previousIndex,
        currentIndex
      );
    }
  }

  setContextmenuOptions() {
    // Used for Case Cards, has options for all buckets
    if (this.isPreviousWeekData) {
      this.contextMenuOptions = this.planningBoard
        .map((data) => data.title);
      const extendedContextMenuOptions = this.planningBoard
        .map((data) => DateService.addWeeks(data.title, 6));

      extendedContextMenuOptions.forEach(element => {
        this.contextMenuOptions.push(element);
      });
    } else {
      this.contextMenuOptions = this.planningBoard
        .map((data) => data.title);
    }

    // Used for AG Grid
    this.tableContextMenuOptions = [...this.contextMenuOptions]
  }

  createPlanningBoardData(planningBoardData) {
    planningBoardData.forEach((element) => {
      if (element.title === this.DEMAND_METRICS_PROJECTS) {
        this.demandMetricsProjects = element.projects;
      } else {
        var data = {
          title:
            element.title === this.DEMAND_METRICS_PROJECTS
              ? element.title
              : DateService.convertDateInBainFormat(element.title),
          date: element.title === this.DEMAND_METRICS_PROJECTS
            ? null : element.title,
          buckets: this.getBucketsData(element.projects, element.title),
          count: null,
          metrics: null,
        };

        data.count = data.buckets
          .map((x) => x.projects.length)
          .reduce((prev, curr) => prev + curr, 0);
        this.planningBoard.push(data);
      }
    });

    this.createMetricsForSupplyAndDemand();
  }

  createMetricsForSupplyAndDemand() {
    if (!this.isSupplyMetricsLodaed || !this.isDemandMetricsLoaded || !this.isStaffableTeamsLoaded)
      return;

    this.planningBoardColumnMetrics = [];
    this.planningBoard.forEach((planningBoardColumnData) => {
      if (planningBoardColumnData.title != this.DEMAND_METRICS_PROJECTS) {

        const columnDate = new Date(planningBoardColumnData.title).getTime();

        //const bucketsToIncludeInDemand = this.demandMetricsProjects.filter(project => this.bucketIdsToIncludeInDemand.includes(project.bucketId));
        const bucketsToIncludeInDemand = this.demandMetricsProjects.filter(project => project.bucketId == CasePlanningBoardBucketEnum.STAFF_FROM_MY_SUPPLY
                                        || (project.bucketId == CasePlanningBoardBucketEnum.ACTION_NEEDED && project.includeInDemand == true));
        const projectsToIncludeInDemand = bucketsToIncludeInDemand.filter(project => {
          const fromProjectDate = new Date(project.date).getTime();
          const toProjectDate = new Date(project.endDate).getTime();

          return (columnDate >= fromProjectDate && columnDate <= toProjectDate) || columnDate == fromProjectDate
        });

        let demandMetricsData = this.getSkuUsedForCalculatingDemand(projectsToIncludeInDemand);
        const metrics = [].concat(this.createMetricsData(planningBoardColumnData.title, demandMetricsData));
        this.planningBoardColumnMetrics.push({ 'title': planningBoardColumnData.title, 'date': planningBoardColumnData.date, 'metrics': metrics });
      }
    });
  }

  createMetricsData(colName, demandMetricsDataForColumn) {
    let metricsData = [
      { name: 'Supply', id: 'supply', data: this.createSupplyMetricsData(colName) },
      { name: 'Demand', id: 'demand', data: this.createDemandMetricsData(demandMetricsDataForColumn) },
      { name: 'Balance', id: 'balance', data: this.createBalanceMetricsData() }
    ];

    if (this.isGCStaffableTeamCountVisible || this.isPEGStaffableTeamCountVisible) {
      metricsData.push({ name: 'Staffable Teams', id: 'staffableTeams', data: this.createStaffableTeamsData(colName) });
    }

    return metricsData;
  }

  createStaffableTeamsData(colName) {

    const staffableTeamForColumn: CasePlanningBoardStaffableTeamsColumn = this.staffableTeams?.find(staffableTeam => DateService.isSame(colName, staffableTeam.weekOf));
    const totalGCStaffableTeamCount = this.isGCStaffableTeamCountVisible ? staffableTeamForColumn?.staffableTeams.gcTeamCount : 0;
    const totalPEGStaffableTeamCount = this.isPEGStaffableTeamCountVisible ? staffableTeamForColumn?.staffableTeams.pegTeamCount : 0;
    //count of data inside each childrn recursively
    // staffableTeamForColumn.staffableTeams.gcTeamCount = staffableTeamForColumn.staffableTeams.staffableTeamChildren?.reduce((acc, val) => acc + (val.staffableTeamChildren ? this.countChildren(val.staffableTeamChildren, 'gc') : 0), 0);
    // staffableTeamForColumn.pegTeamCount = staffableTeamForColumn?.reduce((acc, val) => acc + (val.children ? this.countChildren(val.children, 'peg') : 0), 0);
    // this.countChildren(staffableTeamForColumn.staffableTeams, 'gc');
    const staffableTeamReturnObject =
      [
        { name: "Staffable Teams", id: "staffableTeams", sum: totalGCStaffableTeamCount + totalPEGStaffableTeamCount, levels: [], visible: true }
      ];

    return staffableTeamReturnObject;
  }

  calculateGCCount1(children) {
    return !!children && children.length > 0
      ? children.map((x) => x?.gcTeamCount)
        ?.reduce((prev, curr) => prev + curr, 0)
      : 0;
  }
  countChildren(children: CasePlanningBoardStaffableTeamViewModel, type) {
    //   children.map((x) => x?.gcTeamCount)
    //       ?.reduce((prev, curr) => prev + curr, 0)
    //     : 0;
    // }
    children.gcTeamCount = children.staffableTeamChildren.reduce((acc, val) => acc + (val.staffableTeamChildren ? this.countChildren(val, type) : val.gcTeamCount), 0);
    return children.gcTeamCount;
  }

  getSkuUsedForCalculatingDemand(staffFromMySupplyProjects) {
    var skuTerms = [];
    staffFromMySupplyProjects.forEach(project => {
      if (project.skuTerm != null) {
        skuTerms.push(...project.skuTerm);
      }
    });
    return skuTerms;
  }

  removeElementFromDemandArray(demandMetricsData, element) {
    let index = demandMetricsData.findIndex(x => x.currentLevelGrade == element.currentLevelGrade);
    if (index >= 0) {
      demandMetricsData.splice(index, 1);
    }
  }

  getPlanningBoardColumnMetrics() {
    this.planningBoardColumnMetrics.find(x => x.metrics.find(x => x.id == 'supply').data)
  }

  createSupplyMetricsData(colName) {

    var levels = [];
    var levelGrades = [];

    let supplyReturnObject =
      [
        { name: "Team", id: "team", visible: false, available: [{ name: "Available", id: "avail", levels: [] }], prospective: [{ name: "Prospective", id: "prospective", levels: [] }] },
        { name: "SMAP", id: "smap", visible: false, available: [{ name: "Available", id: "avail", levels: [] }], prospective: [{ name: "Prospective", id: "prospective", levels: [] }] },
        { name: "Partner", id: "partner", visible: false, available: [{ name: "Available", id: "avail", levels: [] }], prospective: [{ name: "Prospective", id: "prospective", levels: [] }] },
        { name: "Additional Expertise", id: "additionalExpertise", visible: false, available: [{ name: "Available", id: "avail", levels: [] }], prospective: [{ name: "Prospective", id: "prospective", levels: [] }] }
      ]

    const smapAvailableLevels = supplyReturnObject.find(x => x.id == 'smap').available[0].levels;
    const smapProspectiveLevels = supplyReturnObject.find(x => x.id == 'smap').prospective[0].levels;
    const teamAvailableLevels = supplyReturnObject.find(x => x.id == 'team').available[0].levels;
    const teamProspectiveLevels = supplyReturnObject.find(x => x.id == 'team').prospective[0].levels;
    const partnerAvailableLevels = supplyReturnObject.find(x => x.id == 'partner').available[0].levels;
    const partnerProspectiveLevels = supplyReturnObject.find(x => x.id == 'partner').prospective[0].levels;
    const additionalExpertiseAvailableLevels = supplyReturnObject.find(x => x.id == 'additionalExpertise').available[0].levels;
    const additionalExpertiseProspectiveLevels = supplyReturnObject.find(x => x.id == 'additionalExpertise').prospective[0].levels;


    let weekData = this.supplyMetricsData.availabilityMetrics?.filter(x => new Date(x.week_Of).getTime() == new Date(colName).getTime());

    let weekDataDetails = this.supplyMetricsData.availabilityMetrics_Nupur?.filter(x => new Date(x.week_Of).getTime() == new Date(colName).getTime());

    let previousweekDataDetails = this.supplyMetricsData.availabilityMetrics_Nupur?.filter(x => new Date(x.week_Of).getTime() == new Date(DateService.addWeeks(new Date(colName), -1)).getTime());

    //create selected levels array from staffing settings
    if (this.supplyFilterCriteriaObj.levelGrades == "") {
      levels = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGradesHierarchy).map(item => item.value);
    } else {
      this.supplyFilterCriteriaObj.levelGrades?.split(",").forEach(level => {
        levels.push(level.replace(/[\d.+]/g, ''));
        levelGrades.push(level);
      })
      levels = [...new Set(levels)]
    }

    levels.forEach(levelGrade => {
      switch (levelGrade) {
        case "M":
          if (!(smapAvailableLevels?.find(x => x.name == levelGrade))) {
            smapAvailableLevels.push({
              name: levelGrade,
              levelGrades: [{
                name: null,
                supply: 0,
                members: null
              }],
            })
          }
          if (!(smapProspectiveLevels?.find(x => x.name == levelGrade))) {
            smapProspectiveLevels.push({
              name: levelGrade,
              levelGrades: [{
                name: null,
                supply: 0,
                members: null
              }],
            })
          }
          break;
        case "A":
        case "C":
          if (!(teamAvailableLevels?.find(x => x.name == levelGrade))) {
            teamAvailableLevels.push({
              name: levelGrade,
              levelGrades: [{
                name: null,
                supply: 0,
                members: null
              }],
            })
          }
          if (!(teamProspectiveLevels?.find(x => x.name == levelGrade))) {
            teamProspectiveLevels.push({
              name: levelGrade,
              levelGrades: [{
                name: null,
                supply: 0,
                members: null
              }],
            })
          }
          break;
        case "V":
          if (!(partnerAvailableLevels?.find(x => x.name == levelGrade))) {
            partnerAvailableLevels.push({
              name: levelGrade,
              levelGrades: [{
                name: null,
                supply: 0,
                members: null
              }],
            })
          }
          if (!(partnerProspectiveLevels?.find(x => x.name == levelGrade))) {
            partnerProspectiveLevels.push({
              name: levelGrade,
              levelGrades: [{
                name: null,
                supply: 0,
                members: null
              }],
            })
          }
          break;
        default:
          if (!(additionalExpertiseAvailableLevels?.find(x => x.name == levelGrade))) {
            additionalExpertiseAvailableLevels.push({
              name: levelGrade,
              levelGrades: [{
                name: null,
                supply: 0,
                members: null
              }],
            })
          }
          if (!(additionalExpertiseProspectiveLevels?.find(x => x.name == levelGrade))) {
            additionalExpertiseProspectiveLevels.push({
              name: levelGrade,
              levelGrades: [{
                name: null,
                supply: 0,
                members: null
              }],
            })
          }
          break;
      }
    })

    weekData?.forEach(element => {
      let membersInMetrics = this.getMembers(weekDataDetails, element);
      this.highlightNewlyAvailableMembers(weekDataDetails, previousweekDataDetails);
      let supplyCount = this.getSupplyCount(membersInMetrics, element);

      switch (element.level) {
        case "M": {
          if (element.capacitySubCategory == "Available") {
            if ((smapAvailableLevels?.find(x => x.name == element.level))) {
              smapAvailableLevels?.find(x => x.name == element.level).levelGrades.push({
                name: element.currentLevelGrade,
                supply: supplyCount,
                members: membersInMetrics
              })
            }
            else {
              smapAvailableLevels.push({
                name: element.level,
                levelGrades: [{
                  name: element.currentLevelGrade,
                  supply: supplyCount,
                  members: membersInMetrics
                }],
              })
            }
          }
          else {
            if ((smapProspectiveLevels?.find(x => x.name == element.level))) {
              smapProspectiveLevels?.find(x => x.name == element.level).levelGrades.push({
                name: element.currentLevelGrade,
                supply: supplyCount,
                members: membersInMetrics
              })
            }
            else {
              smapProspectiveLevels?.push({
                name: element.level,
                levelGrades: [{
                  name: element.currentLevelGrade,
                  supply: supplyCount,
                  members: membersInMetrics
                }],
              })
            }
          }
        }
          break;
        case "A":
        case "C": {
          if (element.capacitySubCategory == "Available") {
            if ((teamAvailableLevels?.find(x => x.name == element.level))) {
              teamAvailableLevels?.find(x => x.name == element.level).levelGrades.push({
                name: element.currentLevelGrade,
                supply: supplyCount,
                members: membersInMetrics
              })
            }
            else {
              teamAvailableLevels?.push({
                name: element.level,
                levelGrades: [{
                  name: element.currentLevelGrade,
                  supply: supplyCount,
                  members: membersInMetrics
                }],
              })
            }
          }
          else {
            if ((teamProspectiveLevels?.find(x => x.name == element.level))) {
              teamProspectiveLevels?.find(x => x.name == element.level).levelGrades.push({
                name: element.currentLevelGrade,
                supply: supplyCount,
                members: membersInMetrics
              })
            }
            else {
              teamProspectiveLevels?.push({
                name: element.level,
                levelGrades: [{
                  name: element.currentLevelGrade,
                  supply: supplyCount,
                  members: membersInMetrics
                }],
              })
            }
          }
        }
          break;
        case "V": {
          if (element.capacitySubCategory == "Available") {
            if ((partnerAvailableLevels?.find(x => x.name == element.level))) {
              partnerAvailableLevels?.find(x => x.name == element.level).levelGrades.push({
                name: element.currentLevelGrade,
                supply: supplyCount,
                members: membersInMetrics
              })
            }
            else {
              partnerAvailableLevels?.push({
                name: element.level,
                levelGrades: [{
                  name: element.currentLevelGrade,
                  supply: supplyCount,
                  members: membersInMetrics
                }],
              })
            }
          }
          else {
            if ((partnerProspectiveLevels?.find(x => x.name == element.level))) {
              partnerProspectiveLevels?.find(x => x.name == element.level).levelGrades.push({
                name: element.currentLevelGrade,
                supply: supplyCount,
                members: membersInMetrics
              })
            }
            else {
              partnerProspectiveLevels?.push({
                name: element.level,
                levelGrades: [{
                  name: element.currentLevelGrade,
                  supply: supplyCount,
                  members: membersInMetrics
                }],
              })
            }
          }
        }
          break;
        default: {
          if (element.capacitySubCategory == "Available") {
            if ((additionalExpertiseAvailableLevels?.find(x => x.name == element.level))) {
              additionalExpertiseAvailableLevels?.find(x => x.name == element.level).levelGrades.push({
                name: element.currentLevelGrade,
                supply: supplyCount,
                members: membersInMetrics
              })
            }
            else {
              additionalExpertiseAvailableLevels?.push({
                name: element.level,
                levelGrades: [{
                  name: element.currentLevelGrade,
                  supply: supplyCount,
                  members: membersInMetrics
                }],
              })
            }
          }
          else {
            if ((additionalExpertiseProspectiveLevels?.find(x => x.name == element.level))) {
              additionalExpertiseProspectiveLevels?.find(x => x.name == element.level).levelGrades.push({
                name: element.currentLevelGrade,
                supply: supplyCount,
                members: membersInMetrics
              })
            }
            else {
              additionalExpertiseProspectiveLevels?.push({
                name: element.level,
                levelGrades: [{
                  name: element.currentLevelGrade,
                  supply: supplyCount,
                  members: membersInMetrics
                }],
              })
            }
          }
        }
          break;
      }
    });
    this.setVisibilityOfLevelsInMetrics(supplyReturnObject, levels);

    return supplyReturnObject;
  }

  createDemandMetricsData(demandMetricsData) {
    var levels = [];

    let demandReturnObject =
      [
        { name: "Team", id: "team", levels: [], visible: false },
        { name: "SMAP", id: "smap", levels: [], visible: false },
        { name: "Partner", id: "partner", levels: [], visible: false },
        { name: "Additional Expertise", id: "additionalExpertise", levels: [], visible: false }
      ]

    const teamLevels = demandReturnObject.find(x => x.id == "team").levels;
    const smapLevels = demandReturnObject.find(x => x.id == "smap").levels;
    const partnerLevels = demandReturnObject.find(x => x.id == "partner").levels;
    const additionalExpertiseLevels = demandReturnObject.find(x => x.id == "additionalExpertise").levels;

    levels = this.getLevelGradesSelectedInSupply();
    this.showDefaultValueForSelectedLevelGrades(levels, smapLevels, teamLevels, partnerLevels, additionalExpertiseLevels);

    //sum for each level
    demandMetricsData.forEach(skuData => {
      //ADD SKU's for level grades that are selected by users. Ignore for others.

      if (levels.includes(skuData.level) && skuData.aggregateDemand) {
        switch (skuData.level) {
          case "M": {
            if (smapLevels?.find(x => x.name == skuData.level)) {
              let level = smapLevels?.find(x => x.name == skuData.level);
              level.sum += Number(parseFloat(skuData.aggregateDemand));
            }
            else {
              smapLevels.push({
                name: skuData.level,
                sum: Number(parseFloat(skuData.aggregateDemand))
              })
            }
            break;
          }
          case "A":
          case "C": {
            if (teamLevels?.find(x => x.name == skuData.level)) {
              let level = teamLevels?.find(x => x.name == skuData.level);
              level.sum += Number(parseFloat(skuData.aggregateDemand));
            }
            else {
              teamLevels.push({
                name: skuData.level,
                sum: Number(parseFloat(skuData.aggregateDemand))
              })
            }
            break;
          }
          case "V": {
            if (partnerLevels?.find(x => x.name == skuData.level)) {
              let level = partnerLevels?.find(x => x.name == skuData.level);
              level.sum += Number(parseFloat(skuData.aggregateDemand));
            }
            else {
              partnerLevels.push({
                name: skuData.level,
                sum: Number(parseFloat(skuData.aggregateDemand))
              })
            }
            break;
          }
          default: {
            if (additionalExpertiseLevels?.find(x => x.name == skuData.level)) {
              let level = additionalExpertiseLevels?.find(x => x.name == skuData.level);
              level.sum += Number(parseFloat(skuData.aggregateDemand));
            }
            else {
              additionalExpertiseLevels.push({
                name: skuData.level,
                sum: Number(parseFloat(skuData.aggregateDemand))
              })
            }
            break;
          }
        }
      }
    });
    this.setVisibilityOfLevelsInMetrics(demandReturnObject, levels);

    return demandReturnObject;
  }

  createBalanceMetricsData() {
    var levels = [];

    let balanceReturnObject =
      [
        { name: "Team", id: "team", levels: [], visible: false },
        { name: "SMAP", id: "smap", levels: [], visible: false },
        { name: "Partner", id: "partner", levels: [], visible: false },
        { name: "Additional Expertise", id: "additionalExpertise", levels: [], visible: false }
      ]
    const teamLevels = balanceReturnObject.find(x => x.id == "team").levels;
    const smapLevels = balanceReturnObject.find(x => x.id == "smap").levels;
    const partnerLevels = balanceReturnObject.find(x => x.id == "partner").levels;
    const additionalExpertiseLevels = balanceReturnObject.find(x => x.id == "additionalExpertise").levels;

    levels = this.getLevelGradesSelectedInSupply();
    this.showDefaultValueForSelectedLevelGrades(levels, smapLevels, teamLevels, partnerLevels, additionalExpertiseLevels);
    this.setVisibilityOfLevelsInMetrics(balanceReturnObject, levels);

    return balanceReturnObject;
  }

  getLevelGradesSelectedInSupply() {
    var levels = [];
    var levelGrades = [];

    //create selected levels array from staffing settings
    if (this.supplyFilterCriteriaObj.levelGrades == "") {
      levels = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGradesHierarchy).map(item => item.value);
    } else {
      this.supplyFilterCriteriaObj.levelGrades?.split(",").forEach(level => {
        levels.push(level.replace(/[\d.+]/g, ''));
        levelGrades.push(level);
      })
      levels = [...new Set(levels)]
    }
    return levels;
  }

  showDefaultValueForSelectedLevelGrades(levels, smapLevels, teamLevels, partnerLevels, additionalExpertiseLevels) {
    levels.forEach(level => {
      switch (level) {
        case "M":
          if (!(smapLevels?.find(x => x.name == level)))
            smapLevels.push({
              name: level,
              sum: 0
            })
          break;
        case "A":
        case "C":
          if (!(teamLevels?.find(x => x.name == level)))
            teamLevels.push({
              name: level,
              sum: 0
            })
          break;
        case "V":
          if (!(partnerLevels?.find(x => x.name == level)))
            partnerLevels.push({
              name: level,
              sum: 0
            })
          break;
        default:
          if (!(additionalExpertiseLevels?.find(x => x.name == level)))
            additionalExpertiseLevels.push({
              name: level,
              sum: 0
            })
          break;
      }
    })
  }

  setVisibilityOfLevelsInMetrics(returnObject, levels) {
    //create return object and set visibility based on selected levels in settings
    if (!this.supplyFilterCriteriaObj.levelGrades || levels.includes('A') || levels.includes('C')) {
      returnObject.find(x => x.id == "team").visible = true;
    }
    if (!this.supplyFilterCriteriaObj.levelGrades || levels.includes('M')) {
      returnObject.find(x => x.id == "smap").visible = true;
    }
    if (!this.supplyFilterCriteriaObj.levelGrades || levels.includes('V')) {
      returnObject.find(x => x.id == "partner").visible = true;
    }
    if (!this.supplyFilterCriteriaObj.levelGrades || levels.filter(levels => (!levels.includes('A') && !levels.includes('C') && !levels.includes('M') && !levels.includes('V'))).length > 0) {
      returnObject.find(x => x.id == "additionalExpertise").visible = true;
    }
  }

  // Creats context menu for AG Grid
  getContextMenuItems(params) {
    if (!!params.node) {
      let selectedRows = {};
      selectedRows =
        params.api.getSelectedRows().length > 0
          ? Object.assign(params.api.getSelectedRows())
          : Object.assign(params.node.data);

      const componentInstance = params.context.componentParent;
      const fromBucket = componentInstance.newDemandsData.buckets[0];
      const menuOptions: any[] = componentInstance.tableContextMenuOptions;

      var result = [];

      // Finding case from list that matches selected case row
      var finalProject = componentInstance.findProjectInNewDemands([].concat(selectedRows));

      menuOptions.forEach((item) => {
        result.push({
          name: item,
          action: function () {
            // Set project and week object
            var fullProject = { project: finalProject, week: item };
            componentInstance.contextMenuClickHandler(fullProject, fromBucket);
          },
        });
      });

      // Add "Send to:" label as first option
      result.unshift({
        name: "Send to:",
        disabled: true,
      });

      result.unshift({
        name: "+Bulk SKU (M+4)",
        action: function () {
          componentInstance.setDefaultValuesForSKUs([].concat(selectedRows));
        }
      });

      return result;
    }
  }

  // Generates row data for AG Grid
  generateRowData(projects) {
    let clientName;
    let agGridRowData = [];

    // Loops through all projects from New Cases bucket
    projects.forEach((project) => {
      // Set client name
      clientName = project.clientName;
      if (project.clientPriority) {
        clientName += ` (${project.clientPriority})`;
      }

      // Create object for each case row
      agGridRowData.push({
        id: project.oldCaseCode || project.pipelineId || project.planningCardId,
        client: clientName,
        projectName: project.caseName || project.opportunityName || project.name,
        pegRFOppIcon: project.pegOpportunityId,
        startDate: DateService.convertDateInBainFormat(project.startDate),
        originalStartDate: (project.oldCaseCode || project.planningCardId) ? DateService.convertDateInBainFormat(project.startDate) : DateService.convertDateInBainFormat(project.originalStartDate),
        overrideStartDate: (project.planningCardId) ? DateService.convertDateInBainFormat(project.startDate) : DateService.convertDateInBainFormat(project.overrideStartDate),
        endDate: DateService.convertDateInBainFormat(project.endDate),
        originalEndDate: (project.oldCaseCode || project.planningCardId) ? DateService.convertDateInBainFormat(project.endDate) : DateService.convertDateInBainFormat(project.originalEndDate),
        overrideEndDate: project.planningCardId ? DateService.convertDateInBainFormat(project.endDate) : DateService.convertDateInBainFormat(project.overrideEndDate),
        sku: project.combinedSkuTerm,
        manager: project.caseManagerName,
        office:
          project.managingOfficeAbbreviation ||
          project.sharedOfficeAbbreviations,
        overrideOfficeAbbreviations:
          project.staffingOfficeAbbreviation ||
          project.sharedOfficeAbbreviations,
        overrideOfficeCodes:
          project.staffingOfficeCode ||
          project.sharedOfficeCodes,
        sharedStaffingTags: project.sharedStaffingTags,
        probabilityPercent: project.probabilityPercent ? `${project.probabilityPercent}%` : '',
        originalProbabilityPercent: project.originalProbabilityPercent ? `${project.originalProbabilityPercent}%` : '',
        overrideProbabilityPercent: project.overrideProbabilityPercent ? `${project.overrideProbabilityPercent}%` : '',
        oldCaseCode: project.oldCaseCode,
        pipelineId: project.pipelineId,
        planningCardId: project.planningCardId,
        type: project.type
          ? project.type === ProjectType.NewDemand
            ? "New Case"
            : project.type === ProjectType.Opportunity
              ? "Opportunity"
              : project.type
          : "Planning Card",
        notes: project.notes,
        caseServedByRingfence: project.caseServedByRingfence,
        latestCasePlanningBoardViewNote: project?.latestCasePlanningBoardViewNote
      })
    });

    // The following logic refreshes on the required rows in ag-grid without affecting the scroll/sorting/filterting applied by user
    this.setAgGridRowData(agGridRowData);
  }

  setAgGridRowData(agGridRowData) {
    if (!this.rowData || this.rowData.length === 0) {
      this.rowData = agGridRowData;
    } else {
      const rowsToBeInserted = agGridRowData.filter(o =>
        !this.rowData.some((r) => o.id === r.id));

      if (rowsToBeInserted.length > 0) {
        this.rowData = this.rowData.concat(rowsToBeInserted);
        this.gridApi.applyTransaction({ add: rowsToBeInserted });
      }

      const rowsToBeDeleted = this.rowData.filter(allocation =>
        !agGridRowData.some((r) => allocation.id === r.id));

      if (rowsToBeDeleted.length > 0) {
        rowsToBeDeleted.forEach(r => {
          this.rowData.splice(this.rowData.indexOf(r), 1);
        })
        this.gridApi.applyTransaction({ remove: rowsToBeDeleted });
      }
      const itemsToUpdate = [];

      const updatedData = agGridRowData;

      if (updatedData) {
        this.gridApi.forEachNodeAfterFilterAndSort(function (rowNode, index) {
          const updatedRowData = updatedData.find(r => r.id === rowNode.data.id);
          if (updatedRowData) {
            const data = rowNode.data;
            data.id = updatedRowData.id;
            data.client = updatedRowData.client;
            data.projectName = updatedRowData.projectName;
            data.pegRFOppIcon = updatedRowData.pegRFOppIcon;
            data.startDate = updatedRowData.startDate;
            data.originalStartDate = updatedRowData.originalStartDate;
            data.overrideStartDate = updatedRowData.overrideStartDate;
            data.endDate = updatedRowData.endDate;
            data.originalEndDate = updatedRowData.originalEndDate;
            data.overrideEndDate = updatedRowData.overrideEndDate;
            data.endDate = updatedRowData.overrideEndDate;
            data.sku = updatedRowData.sku;
            data.manager = updatedRowData.manager;
            data.office = updatedRowData.office;
            data.overrideOfficeAbbreviations = updatedRowData.overrideOfficeAbbreviations;
            data.overrideOfficeCodes = updatedRowData.overrideOfficeCodes;
            data.sharedStaffingTags = updatedRowData.sharedStaffingTags;
            data.probabilityPercent = updatedRowData.probabilityPercent;
            data.originalProbabilityPercent = updatedRowData.originalProbabilityPercent;
            data.overrideProbabilityPercent = updatedRowData.overrideProbabilityPercent;
            data.oldCaseCode = updatedRowData.oldCaseCode;
            data.pipelineId = updatedRowData.pipelineId;
            data.planningCardId = updatedRowData.planningCardId;
            data.type = updatedRowData.type;
            data.notes = updatedRowData.notes;
            data.caseServedByRingfence = updatedRowData.caseServedByRingfence;
            data.latestCasePlanningBoardViewNote = updatedRowData.latestCasePlanningBoardViewNote;
            itemsToUpdate.push(data);
          }
        });
        this.gridApi.applyTransaction({ update: itemsToUpdate });
      }
    }
  }

  getBucketsData(projects, colName) {
    let bucketList = [];

    if (colName === this.NEW_DEMAND_COLUMN_TITLE) {
      let bucketrow = {
        bucketId: this.planningBoardBucketLookUp[0].id,
        bucketName: this.planningBoardBucketLookUp[0].bucketName,
        includeInDemand: this.planningBoardBucketLookUp[0].includeInDemand,
        isPartiallyChcked: this.planningBoardBucketLookUp[0].isPartiallyChecked,
        date: null,
        projects: projects
      }

      bucketList.push(bucketrow);

    } else {
      this.planningBoardBucketLookUp.forEach(element => {

        let bucketedProjects = projects.filter(x => x.bucketId === element.id);

        let bucketrow = {
          bucketId: element.id,
          bucketName: element.bucketName,
          includeInDemand: element.includeInDemand,
          isPartiallyChecked: element.isPartiallyChecked,
          date: colName,
          projects: bucketedProjects
        }

        bucketList.push(bucketrow);
      });
    }

    return bucketList;
  }

  onCaseDrop(event) {
    if (event.previousContainer === event.container) {
      this.notifyService.showValidationMsg(ValidationService.sameBucketMovementMsg);
    }
    else {

      this.moveCaseToNewBucket([].concat(event.item.data), event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
    }
  }

  getToBucket(startDate) {
    const startDateOfPlanningBoard = DateService.convertDateInBainFormat(DateService.getStartOfWeek());
    const startDateOfLastWeek = DateService.addWeeks(startDateOfPlanningBoard, 5);
    const endDateOfPlanningBoard = DateService.addDays(startDateOfLastWeek, 6);
    if (startDate == null || (new Date(startDate) < new Date(startDateOfPlanningBoard))) {
      startDate = startDateOfPlanningBoard;
    } else if (new Date(startDate) > new Date(endDateOfPlanningBoard)) {
      startDate = startDateOfLastWeek;
    } else {
      const startDateOfWeek = DateService.convertDateInBainFormat(DateService.getStartOfWeek(startDate));
      startDate = startDateOfWeek;
    }
    return this.convertToBucketRowObj(startDate);
  }

  upsertCaseCards(draggedProjects, droppedContainerData) {
    let dataToUpdate = [];
    let data;
    draggedProjects.forEach(draggedProject => {
      if (droppedContainerData == null) {
        let toBucket = this.getToBucket(draggedProject.overrideStartDate || draggedProject.startDate);
        draggedProject.bucketId = toBucket?.bucketId;
        data = this.populateDataToUpdate(draggedProject, toBucket);
      } else {
        draggedProject.bucketId = droppedContainerData?.bucketId;
        data = this.populateDataToUpdate(draggedProject, droppedContainerData);
      }
      dataToUpdate.push(data);
    });
    this.upsertOrDeleteCasePlanningBoardCard(draggedProjects, dataToUpdate);
  }

  populateDataToUpdate(draggedProject, droppedContainerData) {
    const dataToUpdate = {
      id: draggedProject.planningBoardId || undefined,
      bucketId: droppedContainerData?.date ? droppedContainerData?.bucketId : null,
     date: droppedContainerData?.date,
      pipelineId: draggedProject?.pipelineId,
      oldCaseCode: draggedProject?.oldCaseCode,
      planningCardId: draggedProject?.planningCardId,
      projectEndDate: draggedProject?.endDate
    }
    return dataToUpdate;
  }

  upsertOrDeleteCasePlanningBoardCard(projects, dataToUpdate) {
    if (dataToUpdate[0].date) {
      this.planningBoardService.upsertCasePlanningBoardData(dataToUpdate).subscribe(upsertedCards => {
        projects.forEach(project => {
          project.planningBoardId = upsertedCards.find(upsertedCard => (project.oldCaseCode && upsertedCard.oldCaseCode == project.oldCaseCode)
            || (project.pipelineId && upsertedCard.pipelineId == project.pipelineId)
            || (project.planningCardId && upsertedCard.planningCardId == project.planningCardId)).id;
        })
        this.notifyService.showSuccess(ValidationService.casePlanningBoardUpdatedSuccessfullyMsg);
        this.overlayMessageService.triggerCaseAndOpportunityRefreshOnCasePlanning(upsertedCards);
        upsertedCards.forEach(upsertedData => {
          let upsertedDemandMetricsProject = this.demandMetricsProjects.find(x => (upsertedData.oldCaseCode && x.oldCaseCode == upsertedData.oldCaseCode)
            || (upsertedData.pipelineId && x.pipelineId == upsertedData.pipelineId)
            || (upsertedData.planningCardId && x.planningCardId == upsertedData.planningCardId));

          if (!upsertedDemandMetricsProject) {
            let upsertedColumn = this.planningBoard.find(x => new Date(x.title).getTime() === new Date(upsertedData.date).getTime());
            let upsertedProject
            if (!!upsertedColumn) {
              upsertedProject = upsertedColumn.buckets.find(x => x.bucketId == upsertedData.bucketId).projects.find(x => x.planningBoardId == upsertedData.id);
              if (!upsertedProject) {
                upsertedProject = projects.find(project => (upsertedData.oldCaseCode && project.oldCaseCode == upsertedData.oldCaseCode)
                  || (upsertedData.pipelineId && project.pipelineId == upsertedData.pipelineId)
                  || (upsertedData.planningCardId && project.planningCardId == upsertedData.planningCardId));
              }
              upsertedProject.bucketId = upsertedData.bucketId;
              if(upsertedProject.bucketId == CasePlanningBoardBucketEnum.ACTION_NEEDED && this.planningBoardBucketLookUp.find(x => x.id == CasePlanningBoardBucketEnum.ACTION_NEEDED).includeInDemand)
              {
                upsertedProject.includeInDemand = true;
              }
              upsertedProject.planningBoardId = upsertedData.id;
              upsertedProject.date = upsertedData.date;
              upsertedProject.endDate = new Date(upsertedData.projectEndDate);
              this.demandMetricsProjects.push(upsertedProject);
            }
          } else {
            upsertedDemandMetricsProject.bucketId = upsertedData.bucketId;
            if(upsertedDemandMetricsProject.bucketId == CasePlanningBoardBucketEnum.ACTION_NEEDED && this.planningBoardBucketLookUp.find(x => x.id == CasePlanningBoardBucketEnum.ACTION_NEEDED).includeInDemand)
              {
                upsertedDemandMetricsProject.includeInDemand = true;
              }
            upsertedDemandMetricsProject.planningBoardId = upsertedData.id;
            upsertedDemandMetricsProject.date = upsertedData.date;
            upsertedDemandMetricsProject.endDate = upsertedData.projectEndDate;
          }
        });
        this.createMetricsForSupplyAndDemand();
      });
    } else {
      this.planningBoardService.deleteCasePlanningBoardByIds(projects[0].planningBoardId).subscribe(deletedData => {
        this.notifyService.showSuccess(ValidationService.casePlanningBoardUpdatedSuccessfullyMsg);
      });
    }
  }

  contextMenuClickHandler(event, fromBucket) {
    let project = event.project;
    let toColumn = event.week;

    let toBucket = this.convertToBucketRowObj(toColumn);
    let previousIndex = fromBucket.projects.findIndex(x => x == project);
    let currentIndex = !toBucket.projects ? 0 : toBucket.projects.length; //Move the project to the end of Staff from my supply bucket in the selected column

    if (DateService.convertDateInBainFormat(fromBucket.date) === toColumn && fromBucket.bucketId == toBucket.bucketId) {
      this.notifyService.showValidationMsg(ValidationService.sameBucketMovementMsg);
    }
    else {
      this.moveCaseToNewBucket([].concat(project), fromBucket, toBucket, previousIndex, currentIndex);
    }
  }

  moveNewDemandClickHandler() {
    this.moveDemands = true;
    let fromBucket = this.newDemandsData?.buckets[0];
    let projects = Object.assign([], this.newDemandsData.projects);
    this.moveCaseToNewBucket(projects, fromBucket, null, null, null);
  }

  isProjectAvailableonBoard(selectedProject) {
    let isProjectExists = false;

    this.planningBoard.every(x => {
      x.buckets.every(y => {
        if (y.projects.findIndex(z => z.oldCaseCode === selectedProject.oldCaseCode && z.pipelineId === selectedProject.pipelineId) > -1) {
          isProjectExists = true;
          return false; //used to break out of the loop
        }
        return true; //used to conitnue in the loop
      })

      if (isProjectExists)
        return false;
      else
        return true;
    });

    return isProjectExists;
  }

  addSelectedProjectToBoardHandler(selectedData) {

    //update staffing office withe selected office
    selectedData.selectedProject.staffingOfficeCode = selectedData.selectedOffice?.officeCode;
    selectedData.selectedProject.staffingOfficeAbbreviation = selectedData.selectedOffice?.officeAbbreviation;

    //show the added case on board
    let bucketToAdd = this.convertToBucketRowObj(selectedData.selectedColumn);
    bucketToAdd?.projects?.push(selectedData.selectedProject);

    selectedData.selectedProject.bucketId = bucketToAdd.bucketId;
    selectedData.selectedProject.date = selectedData.selectedColumn;

    this.updateProjectChanges(selectedData.selectedProject);

    //this.increaseCountOfBoardColumns(bucketToAdd);
    //this.upsertCaseCard(selectedData.selectedProject, bucketToAdd);
  }

  private convertToBucketRowObj(toColumn: string, bucketId = null) {
    if (!bucketId) bucketId = CasePlanningBoardBucketEnum.STAFF_FROM_MY_SUPPLY;

    let toBucket = this.planningBoard?.find(x => x.title == toColumn)?.buckets?.find(y => y.bucketId == bucketId);
    if (!toBucket) {
      toBucket = this.setDefaultBucketDataForColumn(toColumn);
    }
    return toBucket;
  }

  private setDefaultBucketDataForColumn(toColumn) {
    return {
      bucketId: this.planningBoardBucketLookUp[0].id,
      bucketName: this.planningBoardBucketLookUp[0].bucketName,
      date: toColumn,
      projects: null
    }
  }

  private moveCaseToNewBucket(projects, fromBucket, toBucket, previousIndex, currentIndex) {
    if (fromBucket.date != null && this.planningBoard.some(column => column.date === toBucket?.date)) {
      transferArrayItem(
        fromBucket.projects,
        toBucket.projects,
        previousIndex,
        currentIndex
      );
    }
    else {
      projects.forEach(project => {
        if (this.moveDemands) {
          this.newDemandsData.projects.splice(0, 1);

        } else if (this.newDemandsData.projects.some(x =>
          (project.oldCaseCode && x.oldCaseCode === project.oldCaseCode) ||
          (project.pipelineId && x.pipelineId === project.pipelineId) ||
          (project.planningCardId && x.planningCardId === project.planningCardId)
        )) {

          const projectIndex = this.findIndex(project, this.newDemandsData);
          this.newDemandsData.projects.splice(projectIndex, 1);
        }
        else if (this.planningBoard.some(x => column => column.date === fromBucket.date)) {
          const projectIndexInPlanningBoardColumn = this.findIndex(project, fromBucket);
          fromBucket.projects.splice(projectIndexInPlanningBoardColumn, 1);

          const projectIndexInDemandMetricsProjects = this.findIndex(project, this.demandMetricsProjects);
          this.demandMetricsProjects.splice(projectIndexInDemandMetricsProjects, 1);
        }
      });
    }
    this.updateNewCasesCount();
    this.upsertCaseCards(projects, toBucket);
    if (fromBucket.date == null) {
      this.generateRowData(fromBucket.projects);
    }
    //this.updateTotalDemandOnDragDrop(fromBucket, toBucket);
  }

  private findIndex(project, projectArrayToFindIndexFrom) {
    return projectArrayToFindIndexFrom?.projects?.findIndex(x =>
      (project.oldCaseCode && x.oldCaseCode === project.oldCaseCode) ||
      (project.pipelineId && x.pipelineId === project.pipelineId) ||
      (project.planningCardId && x.planningCardId === project.planningCardId));
  }

  updateNewCasesCount() {
    this.newDemandsData.count = this.newDemandsData.projects.length;
  }

  updateTotalDemandOnDragDrop(fromBucket, toBucket) {
    if (!this.planningBoardColumnMetrics.length)
      return;

    if (fromBucket.date === toBucket.date && this.bucketIdsToIncludeInDemand.includes(fromBucket.bucketId) && this.bucketIdsToIncludeInDemand.includes(toBucket.bucketId)) {
      return;
    }

    if (this.bucketIdsToIncludeInDemand.includes(fromBucket.bucketId) && fromBucket.date !== null) {
      const projectsForSkuCalculation = this.getProjectsForSkuCalculationByBucket(fromBucket);
      const fromColIndex = this.planningBoardColumnMetrics.findIndex(x => x.date == fromBucket.date);
      const fromColMetrics = [].concat(this.createMetricsData(fromBucket.date, this.getSkuUsedForCalculatingDemand(projectsForSkuCalculation)));
      const colTitle = DateService.convertDateInBainFormat(fromBucket.date);

      this.planningBoardColumnMetrics.splice(fromColIndex, 1, { 'title': colTitle, 'date': fromBucket.date, 'metrics': fromColMetrics });

    }
    if (this.bucketIdsToIncludeInDemand.includes(toBucket.bucketId) && toBucket.date !== null) {
      const projectsForSkuCalculation = this.getProjectsForSkuCalculationByBucket(toBucket);
      const toColIndex = this.planningBoardColumnMetrics.findIndex(x => x.date == toBucket.date);
      const toColMetrics = [].concat(this.createMetricsData(toBucket.date, this.getSkuUsedForCalculatingDemand(projectsForSkuCalculation)));
      const colTitle = DateService.convertDateInBainFormat(toBucket.date);

      this.planningBoardColumnMetrics.splice(toColIndex, 1, { 'title': colTitle, 'date': toBucket.date, 'metrics': toColMetrics });

    }

    if (fromBucket.date == null) {
      this.generateRowData(fromBucket.projects);
    }

  }

  updateProjectChanges(updatedProject) {
    let updatedProjectData: CaseOppChanges = {
      pipelineId: updatedProject.pipelineId,
      oldCaseCode: updatedProject.oldCaseCode,
      startDate: updatedProject.startDate,
      endDate: updatedProject.endDate,
      probabilityPercent: updatedProject.probabilityPercent,
      notes: updatedProject.notes,
      caseServedByRingfence: updatedProject.caseServedByRingfence,
      staffingOfficeCode: updatedProject.staffingOfficeCode
    }

    this.updateProjectChangesHandler(updatedProjectData, updatedProject);
  }

  updateProjectChangesHandler(updatedCaseOppChanges, project) {
    this.opportunityService.updateProjectChangesHandler(updatedCaseOppChanges);

    const demandMetricsProject = this.demandMetricsProjects.find(x => (project.oldCaseCode && x.oldCaseCode == project.oldCaseCode)
      || (project.pipelineId && x.pipelineId == project.pipelineId)
      || (project.planningCardId && x.planningCardId == project.planningCardId))

    const dataToUpdate = {
      id: project.planningBoardId || undefined,
      bucketId: project?.date ? project?.bucketId : null,
      date: demandMetricsProject ? demandMetricsProject?.date : project?.date,
      pipelineId: project?.pipelineId,
      oldCaseCode: project?.oldCaseCode,
      planningCardId: project?.planningCardId,
      projectEndDate: project?.endDate
    }
    this.upsertOrDeleteCasePlanningBoardCard([].concat(project), [].concat(dataToUpdate))
  }

  updatePlanningCardEmitterHandler(updatedPlanningCard) {
    this.planningCardService.updatePlanningCard(updatedPlanningCard).subscribe(
      data => {
        this.notifyService.showSuccess('Planning Card data updated successfully');

        let upsertedProject = this.demandMetricsProjects.find(x => (updatedPlanningCard.id && x.planningCardId == updatedPlanningCard.id));
        let isUpdateMetrics = true;

        if (!upsertedProject) {
          upsertedProject = this.newDemandsData.projects.find(x => (updatedPlanningCard.id && x.planningCardId == updatedPlanningCard.id));
          isUpdateMetrics = false
        }
        if (upsertedProject) {
          upsertedProject.startDate = updatedPlanningCard.startDate;
          upsertedProject.endDate = updatedPlanningCard.endDate;
          upsertedProject.sharedOfficeCodes = updatedPlanningCard.sharedOfficeCodes;
          upsertedProject.sharedOfficeAbbreviations = this.officesFlat.filter(x => updatedPlanningCard.sharedOfficeCodes.includes(x.officeCode)).map(x => x.officeAbbreviation).join(",");
          if (isUpdateMetrics)
            this.createMetricsForSupplyAndDemand();
        }
      },
      error => {
        this.notifyService.showError('Error while updating Planning Card data');
      }
    );
  }

  onSearchItemSelectHandler(selectedProject) {
    this.openUpdateCaseCardHandler(selectedProject);
  }

  openUpdateCaseCardHandler(selectedProject) {
    if (this.isProjectAvailableonBoard(selectedProject)) {
      this.notifyService.showValidationMsg(ValidationService.projectExistsonBoard);
      return;
    }

    let planningBoardColumnsList = this.planningBoard
      .map((data) => {
        return {
          "text": data.title,
          "value": data.title
        };
      });

    if (this.isPreviousWeekData) {
      let extendedplanningBoardColumnsList = planningBoardColumnsList
        .map((data) => {
          return {
            "text": DateService.addWeeks(data.text, 6),
            "value": DateService.addWeeks(data.value, 6),
          };
        })
      planningBoardColumnsList = planningBoardColumnsList.concat(extendedplanningBoardColumnsList);
    }

    const config = {
      ignoreBackdropClick: true,
      class: 'modal-dialog-centered',
      initialState: {
        projectToAdd: selectedProject,
        planningBoardColumnsList: planningBoardColumnsList
      }
    };

    let bsModalRef = this.modalService.show(UpdateCaseCardComponent, config);

    bsModalRef.content.addSelectedProjectToBoard.subscribe(selectedData => {
      this.addSelectedProjectToBoardHandler(selectedData);
    });
  }

  openAddTeamSkuFormHandler(projectToOpen) {

    if (projectToOpen.oldCaseCode) {
      this.planningBoardService.getPlaceholderAllocationsByOldCaseCodes(projectToOpen.oldCaseCode).subscribe(data => {
        projectToOpen.placeholderAllocations = data;
        this.openAddTeamsForm(projectToOpen);
      });
    } else if (projectToOpen.pipelineId) {
      this.planningBoardService.getPlaceholderAllocationsByPipelineIds(projectToOpen.pipelineId).subscribe(data => {
        projectToOpen.placeholderAllocations = data;
        this.openAddTeamsForm(projectToOpen);
      });
    } else if (projectToOpen.planningCardId) {
      this.planningBoardService.getPlaceholderAllocationsByPlanningCardIds(projectToOpen.planningCardId).subscribe(data => {
        projectToOpen.placeholderAllocations = data;
        this.openAddTeamsForm(projectToOpen);
      });
    }

  }

  openPegRFPopUpHandler(pegOpportunityId) {
    this.pegOpportunityDialogService.openPegOpportunityDetailPopUp(pegOpportunityId);
  }

  openNotesModalHandler(projectToOpen) {
    let casePlanningNotes = projectToOpen.casePlanningNotes;

    if (!!projectToOpen.latestCasePlanningBoardViewNote) {
      casePlanningNotes = [];
      casePlanningNotes = casePlanningNotes.push(projectToOpen.latestCasePlanningBoardViewNote);
    }

    const cardObject = {
      name: projectToOpen.name ?? projectToOpen.projectName,
      data: projectToOpen.data ?? projectToOpen,
      casePlanningNotes: casePlanningNotes
    };

    const modalRef = this.modalService.show(DemandNotesModalComponent, {
      initialState: {
        cardData: cardObject
      },
      class: "demand-notes-modal modal-dialog-centered",
      ignoreBackdropClick: false,
      backdrop: false
    });

    var projectDataToOpen = cardObject.data;

    modalRef.content.setCaseViewNotes.subscribe((latestNotes) => {
      casePlanningNotes = latestNotes;

      this.latestNote = !!casePlanningNotes && casePlanningNotes.length > 0 ? latestNotes[0] : null;
      this.updateLatestNoteForProject(projectDataToOpen, this.latestNote);
    });

    modalRef.content.upsertCaseViewNotes.subscribe((upsertedNote) => {
      if (this.isProjectsSame(upsertedNote, projectDataToOpen)) {
        this.latestNote = upsertedNote;
        this.updateLatestNoteForProject(projectDataToOpen, this.latestNote);
        this.updateLatestNoteForNewDemand(projectDataToOpen, this.latestNote);
      }
    });

    modalRef.content.deleteCaseViewNotes.subscribe((caseNoteIdToDelete) => {
      casePlanningNotes = casePlanningNotes.filter(x => x.id != caseNoteIdToDelete);

      this.latestNote = casePlanningNotes.length == 0 ? null : casePlanningNotes[0];

      this.updateLatestNoteForProject(projectDataToOpen, this.latestNote);
      this.updateLatestNoteForNewDemand(projectDataToOpen, this.latestNote);
    })
  }

  redrawAgGridRows() {
    this.gridApi!.redrawRows();
  }

  updateLatestNoteForProject(project, latestNote) {
    project.latestCasePlanningBoardViewNote = latestNote;
  }

  updateLatestNoteForNewDemand(project, upsertedNote) {
    let newDemandWithUpsertedNote = this.getProjectFromArray(this.newDemandsData.projects, project);
    if (!!newDemandWithUpsertedNote) {
      newDemandWithUpsertedNote.latestCasePlanningBoardViewNote = upsertedNote;
      this.redrawAgGridRows();
    }
  }

  getProjectFromArray(projectArray, project) {
    return projectArray.find(x =>
      this.isProjectsSame(project, x))
  }

  isProjectsSame(project1, project2) {
    return !!project1.oldCaseCode && project2.oldCaseCode === project1.oldCaseCode
      || !!project1.pipelineId && project2.pipelineId === project1.pipelineId
      || !!project1.planningCardId && project2.planningCardId === project1.planningCardId
  }

  openAddTeamsForm(projectToOpen) {

    const modalRef = this.modalService.show(AddTeamSkuComponent, {
      animated: true,
      backdrop: false,
      ignoreBackdropClick: true,
      initialState: {
        selectedCase: projectToOpen,
        autoCalculate: true,
        isCopyCortexHidden: false
      },
      class: "sku-modal modal-dialog-centered"
    });

    // inserts & updates placeholder data when changes are made to placeholder
    this.subData.add(modalRef.content.upsertPlaceholderAllocationsToProject.subscribe(updatedData => {
      this.placeholderAssignmentService.upsertPlcaeholderAllocations(updatedData, null, null);
    }));

    this.subData.add(modalRef.content.deletePlaceHoldersByIds.subscribe(event => {
      this.updatedProjectIds.push(projectToOpen.oldCaseCode || projectToOpen.pipelineId || projectToOpen.planningCardId);
      this.updatedProjectIds = [... new Set(this.updatedProjectIds)];
      this.placeholderAssignmentService.deletePlaceHoldersByIds(event);
    }));

    // inserts & updates if the placeholder data has been created for Cortex SKU
    this.subData.add(modalRef.content.upsertPlaceholderCreatedForCortexPlaceholders.subscribe(upsertedData => {
      this.placeholderAssignmentService.upsertPlaceholderCreatedForCortexInfo(upsertedData);
    }));

  }

  refreshScreen() {
    if (this.updatedProjectIds.length > 0) {
      const isProjectInNewDemands = this.isUpdatedProjectInNewDemands();

      if (!isProjectInNewDemands) {
        this.getCasePlanningBoardDataBySelectedValues();
      }
      else {
        this.getNewDemandsDataBySelectedValues();
      }
    }
  }

  isUpdatedProjectInNewDemands() {
    let isProjectInNewDemands = false;

    if (this.updatedProjectIds.length > 0) {
      isProjectInNewDemands = this.updatedProjectIds.some(id =>
        this.newDemandsData.projects.some(y =>
          y.oldCaseCode === id || y.pipelineId === id || y.planningCardId === id
        ));
    }

    return isProjectInNewDemands;
  }

  subscribeEvents() {
    // this.storeSub.add(this.projectsGlobalStore
    //   .pipe(
    //     select(fromProjectssGlobalStore.getRefreshCasesAndOpoortunities))
    //   .subscribe((refreshCasesAndOpportunities) => {
    //     if (refreshCasesAndOpportunities) {
    //       this.refreshScreen();
    //       this.projectsGlobalStore.dispatch(
    //         new ProjectsGlobalStoreActions.ResetRefreshCasesAndOpportunitiesSuccess(false));
    //     }
    //   }))

    this.overlayMessageServiceSub.add(this.overlayMessageService.refreshCasesAndOpportunityOnCasePlanning().subscribe(result => {
      if (result) {
        this.updatedProjectIds = result.map(x => x.oldCaseCode || x.pipelineId || x.planningCardId);
        this.updatedProjectIds = [... new Set(this.updatedProjectIds)];
        this.refreshScreen();
      }
    }));

    this.overlayMessageServiceSub.add(this.overlayMessageService.refreshCasesAndopportunties().subscribe(result => {
      if (result) {
        this.refreshScreen();
      }
    }));

    this.showProgressBar$.subscribe(showLoader => {
      this.showProgressBarEmitter.emit(showLoader);
    });

    this.showModalCloseButton$.subscribe(showModalCloseButton => {
      this.showModalCloseButtonEmitter.emit(showModalCloseButton);
    });
  }

  //-------------------------------new design Funcs-----------------------------------//
  // Hides column from view
  hideColumnHandler(planningBoardColumnTitle) {
    this.hiddenColumns.push(planningBoardColumnTitle);
  }

  // Reveals columns hidden from view
  revealColumnHandler(planningBoardColumnTitle) {
    const indexOfColumn = this.hiddenColumns.findIndex(x => x === planningBoardColumnTitle);
    this.hiddenColumns.splice(indexOfColumn, 1);
  }

  //-------------------------------new design Funcs-----------------------------------//
  // Expand All Rows
  toggleAllRows() {
    const allRowElements = document.querySelectorAll<HTMLElement>(".metrics-body-row");

    allRowElements.forEach((row) => {
      if (row.classList.contains("collapsed")) {
        row.classList.remove("collapsed");
      } else {
        row.classList.add("collapsed");
      }
    })
  }

  /* Expand / Collapse Top rows which includes
    ** Metrics
    ** Staff from Supply
    ** Action Needed
    ** Others
  */
  toggleCasePlanningBoardRows(event, rowNumber) {

    const rowElements = document.querySelectorAll<HTMLElement>(
      ".data-row-" + rowNumber
    );

    rowElements.forEach((item) => {
      item.style.transition = "all 0.2s ease-in-out";
      item.style.overflow = "hidden";
    });

    // Collapse / Expand Row
    if (event.currentTarget.classList.contains("collapsed")) {
      event.currentTarget.classList.remove("collapsed");
      rowElements.forEach((item) => {
        item.classList.remove("collapsed");
        item.style.maxHeight = "999px";
      });
    } else {
      event.currentTarget.classList.add("collapsed");
      rowElements.forEach((item) => {
        item.classList.add("collapsed");
        item.style.maxHeight = "24px";
      });
    }
  }

  metricsBodyExpandedRowsIds = [];

  /* Toggle Metrics Body Level 1 & Level 2 Rows
    ** Supply, Demand, Balance Rows
    ** Team, SMAP, etc rows within Supply
  */
  toggleMetricsUpperLevelBody(toggleElementName, event, rowIndex) {

    const rowId = `${toggleElementName}-body-row-${rowIndex}`;
    if (this.metricsBodyExpandedRowsIds.includes(rowId)) {
      this.metricsBodyExpandedRowsIds.splice(this.metricsBodyExpandedRowsIds.indexOf(rowId), 1);
    } else {
      this.metricsBodyExpandedRowsIds.push(rowId);
    }

    const rowElements = document.querySelectorAll<HTMLElement>(
      `#${rowId}`
    );

    if (event.currentTarget.classList.contains("collapsed")) {

      rowElements.forEach((row) => {
        // row.classList.remove("collapsed");
      });
    } else {

      rowElements.forEach((row) => {
        // row.classList.add("collapsed");
      });
    }
  }

  /* Toggle Metrics Body Level 3 Rows
    ** Individual Levels like A,C, M etc
  */
  metricsLowerLevelExpandedRowsIdWithHeight = {}; //{"<rowId>": "<rowHeight>"}
  toggleMetricsLowerLevelBody(toggleElementName, event, rowIndex) {

    const rowId = `${toggleElementName}-body-row-${rowIndex}`;
    if (this.metricsBodyExpandedRowsIds.includes(rowId)) {
      this.metricsBodyExpandedRowsIds.splice(this.metricsBodyExpandedRowsIds.indexOf(rowId), 1);
    } else {
      this.metricsBodyExpandedRowsIds.push(rowId);
    }

    const rowElements = document.querySelectorAll<HTMLElement>(
      `#${rowId}`
    );

    const rowHeights = []; // Used for row column heights

    if (event.currentTarget.classList.contains("collapsed")) {

      rowElements.forEach((row) => {
        row.classList.remove("collapsed");
        rowHeights.push(row.offsetHeight); // Get heights of expanded level columns
      });

      const maxHeight = Math.max(...rowHeights); // Get the max value
      // rowElements[0].style.height = maxHeight + "px"; // Set value of first left side column to max height when expanded
      rowElements.forEach((row) => {
        row.style.height = maxHeight + "px";
      });

      this.metricsLowerLevelExpandedRowsIdWithHeight[rowId] = maxHeight + "px";
    } else {
      rowElements[0].style.height = "24px";
      delete this.metricsLowerLevelExpandedRowsIdWithHeight[rowId]; //delete the property from object whene collapsed

      rowElements.forEach((row) => {
        row.classList.add("collapsed");
      });
    }

  }

  // Toggle Include in Demand switch
  toggleIncludeInDemand(bucket: CasePlanningBoardBucket) {    
    bucket.includeInDemand = !bucket.includeInDemand;
    bucket.isPartiallyChecked = false;
    this.updateCasePlanningBoardBucketPreferencesInLocalStorage(bucket);
    this.upsertCasePlanningBoardBucketPreferences(bucket);
    this.planningBoard.forEach(planningBoardColumn => {
      let includeInDemandProjects = planningBoardColumn.buckets.find(x => x.bucketId == CasePlanningBoardBucketEnum.ACTION_NEEDED).projects;
      includeInDemandProjects.forEach(project =>
      {
          project.includeInDemand = bucket.includeInDemand;

          const projectInDemandMetricsProjects = this.demandMetricsProjects.filter(x =>
            (project.bucketId === CasePlanningBoardBucketEnum.ACTION_NEEDED));
          projectInDemandMetricsProjects.forEach(project => {
              project.includeInDemand = bucket.includeInDemand;
          })
        })
      });

    this.createMetricsForSupplyAndDemand();
  }

  toggleIncludeProjectInDemand(project, bucket) {
    bucket.includeInDemand = true;

    const projectInDemandMetricsProjects = this.demandMetricsProjects.find(x =>
      (project.planningBoardId && x.planningBoardId === project.planningBoardId));
      projectInDemandMetricsProjects.includeInDemand = project.includeInDemand;

    this.upsertCasePlanningBoardIncludeInDemandUserPreference(project, bucket);
    this.createMetricsForSupplyAndDemand();
  }

  toggleIndividualCountForSupplyMetrics() {
    this.isCountOfIndividualResourcesToggle = !this.isCountOfIndividualResourcesToggle;

    this.createMetricsForSupplyAndDemand();
  }

  toggleHighlightNewlyAvailable() {
    this.enableNewlyAvailableHighlighting = !this.enableNewlyAvailableHighlighting;
  }

  toggleGroupByDateAvailable() {
    this.enableMemberGrouping = !this.enableMemberGrouping;
  }

  updateCasePlanningBoardBucketPreferencesInLocalStorage(bucket: CasePlanningBoardBucket) {
    this.bucketIdsToIncludeInDemand = this.planningBoardBucketLookUp.filter(x => x.includeInDemand).map(y => y.id).join(",");

    this.localStorageService.set(ConstantsMaster.localStorageKeys.casePlanningBoardBuckets, this.planningBoardBucketLookUp, "weekly");
  }

  upsertCasePlanningBoardBucketPreferences(bucket: CasePlanningBoardBucket) {

    const dataToUpdate: CasePlanningBoardBucketPreferences = {
      employeeCode: this.coreService.loggedInUser.employeeCode,
      bucketId: bucket.id,
      includeInDemand: bucket.includeInDemand,
      isPartiallyChecked: false,
      lastUpdatedBy: null
    }

    this.planningBoardService.upsertCasePlanningBoardBucketPreferences(dataToUpdate).subscribe(upsertedData => {
      var planningBoardBucketLookUpCopy = [...this.planningBoardBucketLookUp];
      this.planningBoardBucketLookUp.find(x => x.id == CasePlanningBoardBucketEnum.ACTION_NEEDED).isPartiallyChecked = false;
      this.planningBoardBucketLookUp = planningBoardBucketLookUpCopy;
      this.notifyService.showSuccess(ValidationService.casePlanningBoardIncludeInDemandSuccessMsg);
    });
  }

  upsertCasePlanningBoardIncludeInDemandUserPreference(project, bucket) {
    const dataToUpdate = {
      planningBoardId: project?.planningBoardId ? project.planningBoardId : null,
      employeeCode: this.coreService.loggedInUser.employeeCode,
      includeProjectInDemand: project.includeInDemand,
      includeBucketInDemand: bucket.includeInDemand
    }; 

    this.planningBoardService.upsertCasePlanningBoardIncludeInDemandUserPreferences(dataToUpdate).subscribe(isPartiallyChecked => {
      var planningBoardBucketLookUpCopy = [...this.planningBoardBucketLookUp];
      this.planningBoardBucketLookUp.find(x => x.id == CasePlanningBoardBucketEnum.ACTION_NEEDED).isPartiallyChecked = isPartiallyChecked;
      this.planningBoardBucketLookUp.find(x => x.id == CasePlanningBoardBucketEnum.ACTION_NEEDED).includeInDemand = isPartiallyChecked;
      this.planningBoardBucketLookUp = planningBoardBucketLookUpCopy;
      bucket.isPartiallyChcked = isPartiallyChecked;
      bucket.includeInDemand = isPartiallyChecked;
      this.updateCasePlanningBoardBucketPreferencesInLocalStorage(bucket);
      this.notifyService.showSuccess(ValidationService.casePlanningBoardIncludeInDemandSuccessMsg);
    });
  }

  applyQuickFiltersHandler(selectedFilters) {
    this.setOrResetSupplyToggles();
    //If no filters are passed, then filter by Staffing Settings
    if (!(selectedFilters.officeCodes || selectedFilters.staffingTags
      || selectedFilters.levelGrades || selectedFilters.practiceAreaCodes || selectedFilters.affiliationRoleCodes)) {
      this.updateSupplyAndDemandSettings();
      this.loadPlanningBoardData();
    } else {
      if (!selectedFilters.officeCodes || !selectedFilters.staffingTags) {
        this.notifyService.showValidationMsg("Please select atleast 1 office and Staffing Tag in Quick Filters");
        return;
      }
      this.supplyFilterCriteriaObj.officeCodes = selectedFilters.officeCodes;
      this.supplyFilterCriteriaObj.staffingTags = selectedFilters.staffingTags;
      this.supplyFilterCriteriaObj.levelGrades = selectedFilters.levelGrades;
      this.supplyFilterCriteriaObj.practiceAreaCodes = selectedFilters.practiceAreaCodes;
      this.supplyFilterCriteriaObj.affiliationRoleCodes = selectedFilters.affiliationRoleCodes;

      this.demandFilterCriteriaObj.officeCodes = selectedFilters.officeCodes;
      this.demandFilterCriteriaObj.caseAttributeNames = selectedFilters.staffingTags;
      this.demandFilterCriteriaObj.industryPracticeAreaCodes = selectedFilters.practiceAreaCodes;
      this.demandFilterCriteriaObj.capabilityPracticeAreaCodes = selectedFilters.practiceAreaCodes;

      this.loadPlanningBoardData();

    }
    this.setStaffableRowVisibility();
  }

  setOrResetSupplyToggles(CountOfIndividualResourcesToggle: boolean = false, enableMemberGroupingToggle: boolean = false,
    enableNewlyAvailableHighlighting: boolean = false) {
    this.isCountOfIndividualResourcesToggle = CountOfIndividualResourcesToggle;
    this.enableMemberGrouping = enableMemberGroupingToggle;
    this.enableNewlyAvailableHighlighting = enableNewlyAvailableHighlighting;
  }

  onPlaygroundOptionSelected(selectedPlaygroundOption) {

    switch (selectedPlaygroundOption) {
      case CasePlanningBoardPlaygroundSessionOptions.CREATE_PLAYGROUND: {
        this.createPlayGroundSession();
        break;
      };
      case CasePlanningBoardPlaygroundSessionOptions.JOIN_PLAYGROUND: {
        this.openJoinPlaygroundPopUp();
        break;
      }
      case CasePlanningBoardPlaygroundSessionOptions.LEAVE_PLAYGROUND: {

        this.showModalCloseButton$.next(true);
        this.resetPlanningBoard();
        this.notifyService.showSuccess("Whiteboard Session Left Successfully!!");
        break;
      }
      case CasePlanningBoardPlaygroundSessionOptions.END_PLAYGROUND: {
        this.showModalCloseButton$.next(true);

        if (this.userPlaygroundSessionInfo?.playgroundId) {
          this.deleteCasePlanningBoardMetricsPlaygroundById(this.userPlaygroundSessionInfo.playgroundId);
        }

        this.resetPlanningBoard();
        break;
      }
    }

  }

  createPlayGroundSession() {
    this.showModalCloseButton$.next(true);
    this.isSupplyMetricsLodaed = false;
    this.showProgressBar$.next(true);

    this.setPlaygroundValidationMsg();

    this.supplyFilterCriteriaObj.startDate = DateService.addWeeks(DateService.convertDateInBainFormat(DateService.getStartOfWeek()), -1);
    this.planningBoardService.createCasePlanningBoardMetricsPlayground(this.demandFilterCriteriaObj, this.supplyFilterCriteriaObj, this.isCountOfIndividualResourcesToggle,
      this.enableMemberGrouping, this.enableNewlyAvailableHighlighting)
      .subscribe(playgroundSupplyMetrics => {
        this.userPlaygroundSessionInfo = {
          playgroundId: playgroundSupplyMetrics.playgroundId,
          isCreatedByLoggedInUser: true
        }

        this.supplyMetricsData = playgroundSupplyMetrics;
        this.isSupplyMetricsLodaed = true;

        this.createMetricsForSupplyAndDemand();
        this.setUserPlaygroundSessionInLocalStorage(this.userPlaygroundSessionInfo);
        this.showProgressBar$.next(false);

        this.notifyService.showSuccess("Whiteboard Session Created Successfully!!");
      });
  }

  setPlaygroundValidationMsg(errorMsg = '') {
    this.playgroundValidationObj.isInValid = !!errorMsg;
    this.playgroundValidationObj.errorMessage = !errorMsg ? '' : errorMsg;
  }

  openJoinPlaygroundPopUp() {
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true
    };

    const bsModalRef = this.modalService.show(JoinPlaygroundPopUpComponent, config);

    bsModalRef.content.joinPlaygroundEmitter.subscribe(
      (playgroundIdToJoin) => {
        if (playgroundIdToJoin) {
          this.getPlaygroundSessionFiltersAndLoadPlanningBoardDataForPlayground(playgroundIdToJoin);
        }
      });
  }

  setUserPlaygroundSessionInLocalStorage(sessionData) {
    this.localStorageService.set(ConstantsMaster.localStorageKeys.userPlaygroundSession, sessionData, "weekly");
    this.casePlanningPlaygroundService.setPlaygroundId(sessionData?.playgroundId);
  }

  clearUserPlaygroundSessionInLocalStorage() {
    this.localStorageService.removeItem(ConstantsMaster.localStorageKeys.userPlaygroundSession);
    this.casePlanningPlaygroundService.setPlaygroundId(null);
  }

  resetPlanningBoard() {
    this.userPlaygroundSessionInfo = null;
    this.clearUserPlaygroundSessionInLocalStorage();
    this.updateSupplyAndDemandSettings();
    this.loadPlanningBoardData();
    this.setStaffableRowVisibility();
  }

  getPlaygroundSessionFiltersAndLoadPlanningBoardDataForPlayground(playgroundIdToJoin: string) {
    this.showModalCloseButton$.next(true);
    this.setPlaygroundValidationMsg();

    this.planningBoardService.getCasePlanningBoardPlaygroundFiltersByPlaygroundId(playgroundIdToJoin)
      .subscribe(playgroundFiltersData => {
        if (!playgroundFiltersData || !playgroundFiltersData.playgroundId) {
          this.clearUserPlaygroundSessionInLocalStorage();
          this.setPlaygroundValidationMsg(ValidationService.invalidWhiteboardIdMsg);

          return;
        } else {

          if (!this.userPlaygroundSessionInfo?.playgroundId) {
            this.userPlaygroundSessionInfo = {
              playgroundId: playgroundIdToJoin,
              isCreatedByLoggedInUser: false
            }
            this.setUserPlaygroundSessionInLocalStorage(this.userPlaygroundSessionInfo);
          }

          this.updateSupplyAndDemandFilterCriteria(playgroundFiltersData);
          this.setOrResetSupplyToggles(playgroundFiltersData.isCountOfIndividualResourcesToggle, playgroundFiltersData.enableMemberGrouping,
            playgroundFiltersData.enableNewlyAvailableHighlighting);
          this.setStaffableRowVisibility();
          this.loadPlanningBoardDataForPlayground();
        }

      });

  }

  updateSupplyAndDemandFilterCriteria(playgroundFiltersData) {

    this.supplyFilterCriteriaObj.startDate = playgroundFiltersData.startDate;
    this.supplyFilterCriteriaObj.endDate = playgroundFiltersData.endDate;
    this.supplyFilterCriteriaObj.officeCodes = playgroundFiltersData.supplyViewOfficeCodes;
    this.supplyFilterCriteriaObj.staffingTags = playgroundFiltersData.supplyViewStaffingTags;
    this.supplyFilterCriteriaObj.levelGrades = playgroundFiltersData.levelGrades;
    this.supplyFilterCriteriaObj.positionCodes = playgroundFiltersData.positionCodes;
    this.supplyFilterCriteriaObj.practiceAreaCodes = playgroundFiltersData.practiceAreaCodes;
    this.supplyFilterCriteriaObj.affiliationRoleCodes = playgroundFiltersData.affiliationRoleCodes;

    this.demandFilterCriteriaObj.startDate = playgroundFiltersData.startDate;
    this.demandFilterCriteriaObj.endDate = playgroundFiltersData.endDate;
    this.demandFilterCriteriaObj.officeCodes = playgroundFiltersData.demandViewOfficeCodes;
    this.demandFilterCriteriaObj.caseAttributeNames = playgroundFiltersData.caseAttributeNames;
    this.demandFilterCriteriaObj.caseTypeCodes = playgroundFiltersData.caseTypeCodes;
    this.demandFilterCriteriaObj.demandTypes = playgroundFiltersData.demandTypes;
    this.demandFilterCriteriaObj.minOpportunityProbability = playgroundFiltersData.minOpportunityProbability;
    this.demandFilterCriteriaObj.opportunityStatusTypeCodes = playgroundFiltersData.opportunityStatusTypeCodes;
    this.demandFilterCriteriaObj.industryPracticeAreaCodes = playgroundFiltersData.industryPracticeAreaCodes;
    this.demandFilterCriteriaObj.capabilityPracticeAreaCodes = playgroundFiltersData.capabilityPracticeAreaCodes;

  }

  refreshPlanningBoardDataForPlayground() {
    this.setOrResetSupplyToggles();
    this.loadPlanningBoardDataForPlayground();
  }

  loadPlanningBoardDataForPlayground() {
    //--Hack to relaod planning board data when user presses refresh
    this.planningBoard = [];
    this.updatedProjectIds = [];

    //---- get planning borad and metrics data----------------

    this.getCasePlanningBoardDataBySelectedValues();
    this.getNewDemandsDataBySelectedValues();
    this.getAvailabilityMetricsForPlaygroundById(this.userPlaygroundSessionInfo.playgroundId);
    this.getStaffableTeamsDataBySupply();
  }

  deleteCasePlanningBoardMetricsPlaygroundById(playgroundIdToDelete: string) {
    this.planningBoardService.deleteCasePlanningBoardMetricsPlaygroundById(playgroundIdToDelete)
      .subscribe(deletedId => {
        this.notifyService.showSuccess("Whiteboard Session Ended Successfully!!");
      });
  }

  showCopyMessage() {
    this.notifyService.showInfo('Whiteboard Session Id Copied to Clipboard!!');
  }

  openQuickAddFormHandler(event) {
    // class is required to center align the modal on large screens
    let initialState = null;

    if (event && event.type !== 'click') {

      initialState = {
        commitmentTypeCode: 'C',
        resourceAllocationData: null,
        isUpdateModal: false,
        employeeCode: event.employeeCode
      };


      const config = {
        class: 'modal-dialog-centered',
        ignoreBackdropClick: true,
        initialState: initialState
      };

      this.bsModalRef = this.modalService.show(QuickAddFormComponent, config);


      this.bsModalRef.content.openBackFillPopUp.subscribe((result) => {
        this.openBackFillFormHandler(result);
      });

      // inserts & updates resource data when changes are made to resource
      this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe(upsertedAllocations => {
        this.resourceAssignmentService.upsertResourceAllocationsToProject(upsertedAllocations, null, null);
        this.upsertPlaygroundAllocationsForCasePlanningMetrics(upsertedAllocations.resourceAllocation);
      });
    }
  }

  private openBackFillFormHandler(event) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        project: event.project,
        resourceAllocation: event.resourceAllocation,
        showMoreThanYearWarning: event.showMoreThanYearWarning,
      },
    };
    this.bsModalRef = this.modalService.show(BackfillFormComponent, config);

    this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe(upsertedAllocations => {
      this.resourceAssignmentService.upsertResourceAllocationsToProject(upsertedAllocations, null, null);
      this.upsertPlaygroundAllocationsForCasePlanningMetrics(upsertedAllocations.resourceAllocation);
    });
  }

  // Open Staffable Teams
  openMetricsStaffableTeams(weekOf) {
    const staffableTeamForColumn = this.staffableTeams?.find(staffableTeam => DateService.isSame(weekOf, staffableTeam.weekOf));
    const modalRef = this.modalService.show(StaffableTeamsModalComponent, {
      animated: true,
      backdrop: false,
      ignoreBackdropClick: true,
      class: "modal-dialog-centered staffing-teams-modal",
      initialState: {
        columnDate: weekOf,
        staffableTeam: staffableTeamForColumn.staffableTeams,
        isGcTeamCountVisible: this.isGCStaffableTeamCountVisible,
        isPegTeamCountVisible: this.isPEGStaffableTeamCountVisible
      }
    });

    this.subData.add(
      modalRef.content.upsertCasePlanningBoardStaffableTeamsEmitter.subscribe((dataToUpsert) => {
        this.planningBoardService.upsertCasePlanningBoardStaffableTeams(dataToUpsert.staffableTeamsToUpsert).subscribe((upsertedData) => {
          const indexStaffableWeekColToUpsert = this.staffableTeams.findIndex(x => DateService.isSame(x.weekOf, dataToUpsert.weekOf));
          this.staffableTeams[indexStaffableWeekColToUpsert].staffableTeams = dataToUpsert.staffableTeam;
          // this.updateGCAndPegTeamCountByWeekColumn(weekColumn);
          this.createMetricsForSupplyAndDemand();
          this.notifyService.showSuccess("Count Updated");
        });
      })
    );
  }

  setStaffableRowVisibility() {
    this.isGCStaffableTeamCountVisible = this.supplyFilterCriteriaObj.staffingTags.split(',').includes(ConstantsMaster.ServiceLine.GeneralConsulting);
    this.isPEGStaffableTeamCountVisible = this.supplyFilterCriteriaObj.staffingTags.split(',').includes(ConstantsMaster.StaffingTag.PEG);
  }

  getDataForPreviousDateRange() {
    this.isPreviousWeekData = true;
    this.getCasePlanningBoardDataBySelectedValues();
    this.getAvailabilityMetricsByFilterValues();
    this.getStaffableTeamsDataBySupply();
  }

  getDataForCurrentDateRange() {
    this.isPreviousWeekData = false;
    this.getCasePlanningBoardDataBySelectedValues();
    this.getAvailabilityMetricsByFilterValues();
    this.getStaffableTeamsDataBySupply();
  }

  private upsertPlaygroundAllocationsForCasePlanningMetrics(resourceAllocations: ResourceAllocation[]) {
    if (this.userPlaygroundSessionInfo.playgroundId) {
      const playgroundAllocations = this.planningBoardService.abc(resourceAllocations);
      this.planningBoardService.upsertCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations(this.userPlaygroundSessionInfo.playgroundId, playgroundAllocations)
        .subscribe(playgroundSupplyMetrics => {
          this.supplyMetricsData = playgroundSupplyMetrics;
          this.isSupplyMetricsLodaed = true;

          this.createMetricsForSupplyAndDemand();
        });
    }
  }

  private getProjectsForSkuCalculationByBucket(bucket) {
    if (!bucket) return 0;
    const actionColumn = this.planningBoard.find(column => column.date === bucket.date).buckets;
    const projectsForSkuCalculation = actionColumn
      .filter(x => this.bucketIdsToIncludeInDemand.includes(x.bucketId))
      .map(x => x.projects)
      .reduce((prev, curr) => prev.concat(curr));
    return projectsForSkuCalculation;
  }

  private getMembers(weekDataDetails, element) {
    if (!!weekDataDetails && !!element)
      return weekDataDetails.filter(x => x.currentLevelGrade == element.currentLevelGrade && x.capacitySubCategory == element.capacitySubCategory);

    return null;
  }

  private highlightNewlyAvailableMembers(weekDataDetails, previousweekDataDetails) {
    weekDataDetails.forEach(x => {
      if (!(previousweekDataDetails.some(previousWeekEmployee => x.employeeCode == previousWeekEmployee.employeeCode &&
                                                                x.capacitySubCategory == previousWeekEmployee.capacitySubCategory))) {
        x.highlight = true;
      }
    })
  }

  private getSupplyCount(membersInMetrics, element) {
    if (!!membersInMetrics && !!element)
      return !this.isCountOfIndividualResourcesToggle ? Number(parseFloat(element.aggregateAvailability)) : membersInMetrics.length;

    return null;
  }


  //---------------------------- Destroy Life Cycle ------------------------------------//
  ngOnDestroy() {
    this.subPreferences?.unsubscribe();
    this.subData?.unsubscribe();
    // this.elementRef.nativeElement.querySelector("#gr").removeEventListener("mousedown",this.onMouseDown.bind(this));
  }

}
