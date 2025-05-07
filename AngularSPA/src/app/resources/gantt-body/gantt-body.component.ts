// ----------------------- Angular Package References ----------------------------------//
import {
    Component,
    OnInit,
    Input,
    OnChanges,
    QueryList,
    ViewChildren,
    Output,
    EventEmitter,
    ChangeDetectionStrategy,
    SimpleChanges,
    HostListener,
    ChangeDetectorRef,
    ViewChild
  } from "@angular/core";
  
  // ----------------------- Component References ----------------------------------//
  import { ResourceComponent } from "../resource/resource.component";
  import { GanttTaskComponent } from "../gantt-task/gantt-task.component";
//   import { MembersPopupComponent } from "../members-popup/members-popup.component";
  
  // --------------------------Interfaces -----------------------------------------//
  import { ResourceStaffing } from "src/app/shared/interfaces/resourceStaffing.interface";
  import { ResourceAllocation } from "src/app/shared/interfaces/resourceAllocation.interface";
  import { Commitment } from "src/app/shared/interfaces/commitment.interface";
  // ----------------------- Service References ----------------------------------//
  import { DateService } from "src/app/shared/dateService";
  import { BsModalService } from "ngx-bootstrap/modal";
  import { PopupModalComponent } from "../popup-modal/popup-modal.component";
  import { EmployeeCaseGroupingEnum, WeeklyMonthlyGroupingEnum } from "src/app/shared/constants/enumMaster";
import { ProjectBasic } from "src/app/shared/interfaces/project.interface";
import { Dayjs } from "dayjs";
import { PlaceholderAllocation } from "src/app/shared/interfaces/placeholderAllocation.interface";
  
  @Component({
    selector: "[resources-gantt-body]",
    templateUrl: "./gantt-body.component.html",
    styleUrls: ["./gantt-body.component.scss"]
  })
  export class GanttBodyComponent implements OnInit, OnChanges {
    public perDayClass = [];
    public commitmentMatrix = [];
    public isRowCollapsed = false;
    public readonly weeklyMonthlyGroupingEnum = WeeklyMonthlyGroupingEnum;

  
    // -----------------------Input Events-----------------------------------------------//
  
    @Input() resourceStaffing: ResourceStaffing;
    @Input() case: ProjectBasic;
    @Input() dateRange: [Date, Date];
    @Input() selectedCommitmentTypes: string[];
    @Input() thresholdRangeValue; // Threshold input

    @Input() isLeftSideBarCollapsed = false;
    @Input() objGanttCollapsedRows;
    @Input() rowIndex: number;
    @Input() selectedEmployeeCaseGroupingOption: string;
    @Input() selectedWeeklyMonthlyGroupingOption: string;
    @Input() isSelectedPracticeView: boolean;
    @Input() isTopbarCollapsed: boolean;
  
    // ------------------------Output Events---------------------------------------
    @Output() updateResourceAssignmentToProject =
      new EventEmitter<ResourceAllocation>();
    @Output() upsertPlaceholderAllocationsToProject = new EventEmitter<any>();
    @Output() upsertResourceAllocationsToProject = new EventEmitter<
      ResourceAllocation[]
    >();
    @Output() updateResourceCommitment = new EventEmitter<Commitment>();
    @Output() openQuickAddForm = new EventEmitter<any>();
    @Output() openResourceDetailsDialog = new EventEmitter();
    @Output() openSplitAllocationPopup = new EventEmitter<any>();
    @Output() openCaseDetailsDialog = new EventEmitter<any>();
    @Output() upsertResourceViewNote = new EventEmitter<any>();
    @Output() deleteResourceViewNotes = new EventEmitter<any>();
    @Output() upsertResourceRecentCD= new EventEmitter<any>();
    @Output() deleteResourceRecentCD = new EventEmitter<any>();
    @Output() upsertResourceCommercialModel= new EventEmitter<any>();
    @Output() deleteResourceCommercialModel = new EventEmitter<any>();
    @Output() selectedResourceViewTab = new EventEmitter<any>();
    @Output() isGroupCollapsedEmitter = new EventEmitter<boolean>();
    // Export functionality: future requirements, should not be removed
    // @Output() ganttBodyLoadedEmitter = new EventEmitter<any>();
  
    @ViewChildren(ResourceComponent)
    resourceChildren: QueryList<ResourceComponent>;
    @ViewChildren(GanttTaskComponent)
    resourceTaskChildren: QueryList<GanttTaskComponent>;
  
    @ViewChild('ganttResource') ganttResourceComponent: ResourceComponent;
    @ViewChild('ganttTask') ganttTaskomponent: GanttTaskComponent;
    
    constructor(
      public changeDetectorRef: ChangeDetectorRef,
      private modalService: BsModalService
    ) {}
  
    ngOnInit() {
    }

    ngOnChanges(changes: SimpleChanges) {
      if (changes.dateRange && this.dateRange || changes.selectedWeeklyMonthlyGroupingOption && this.selectedWeeklyMonthlyGroupingOption) {
        this.perDayClass = [];
        this.getClassNameForEachDay();
      }
  
      if (changes.objGanttCollapsedRows && this.objGanttCollapsedRows) {

        if(this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES){
          this.isRowCollapsed = true;
        }else{
          this.isRowCollapsed = this.objGanttCollapsedRows.exceptionRowIndexes.includes(this.resourceStaffing.resource?.employeeCode)
          ? !this.objGanttCollapsedRows.isAllCollapsed : this.objGanttCollapsedRows.isAllCollapsed;
        }
      }
    }

    expandCollapseGanttRowHandler(isGanttRowCollapsed){
      this.isRowCollapsed = isGanttRowCollapsed;
      this.ganttResourceComponent.isRowCollapsed = isGanttRowCollapsed; //needed to set the correct value of property on indidual expan/collapse of row
      this.ganttTaskomponent.isRowCollapsed = isGanttRowCollapsed;
      if(!this.objGanttCollapsedRows.exceptionRowIndexes.includes(this.resourceStaffing.resource.employeeCode)){
        this.objGanttCollapsedRows.exceptionRowIndexes.push(this.resourceStaffing.resource.employeeCode);
      }else{
        const index  =  this.objGanttCollapsedRows.exceptionRowIndexes.indexOf(this.resourceStaffing.resource.employeeCode);
        this.objGanttCollapsedRows.exceptionRowIndexes.splice(index , 1);
      }
      
    }
  
    collapseExpandResource(event) {
      this.isLeftSideBarCollapsed = event;
    }
  
    openCaseDetailsDialogHandler(event) {
      this.openCaseDetailsDialog.emit(event);
    }
  
    getClassNameForEachDay(): any {
      const projectStartDate = DateService.getFormattedDate(this.dateRange[0]);
      const projectEndDate = DateService.getFormattedDate(this.dateRange[1]);
      const datesBetweenRange: Dayjs[] = DateService.getDates(
        projectStartDate,
        projectEndDate
      );

      if(this.selectedWeeklyMonthlyGroupingOption === this.weeklyMonthlyGroupingEnum.MONTHLY) {
        datesBetweenRange.forEach((date) => {
          const day = DateService.getDayAbbreviationsInLowerCase(date);
          const className = "";
          if(day === "mon") {
            this.perDayClass.push(className);
          }
        });
      } else {
        datesBetweenRange.forEach((date) => {
          const day = DateService.getDayAbbreviationsInLowerCase(date);
          const className = day === "sat" || day === "sun" ? "weekend" : "";
          this.perDayClass.push(className);
        });
      }
    }
  
    getClass(index: number): any {
      const baseClass = this.selectedWeeklyMonthlyGroupingOption === this.weeklyMonthlyGroupingEnum.MONTHLY ? 'week' : 'day';
      const additionalClass = this.perDayClass ? this.perDayClass[index] : '';
      return {
        [baseClass]: true,
        [additionalClass]: additionalClass !== ''
      };
    }
    
    toggleHighlightPercentRow(event) {
      const percentRow = event.currentTarget.querySelector(".percent-area");
  
      if (event.type === "mouseenter") {
        percentRow.classList.add("show");
      } else if (event.type === "mouseleave") {
        percentRow.classList.remove("show");
      }
    }
  
    openPopUpDialogHandler(event) {
      const windowWidth = window.innerWidth;
      const positionObj = event.positionObj;
      const commitmentsData = event.commitment;
  
      let classToUse = `commitments-detail-popup left-${positionObj.left} top-${positionObj.top}`;
  
      if (windowWidth - positionObj.left < 270) {
        positionObj.right = windowWidth - positionObj.left;
        positionObj.left = 0;
        classToUse = `commitments-detail-popup right-${positionObj.right} top-${positionObj.top}`;
      }
  
      this.modalService.show(PopupModalComponent, {
        animated: true,
        backdrop: false,
        ignoreBackdropClick: false,
        initialState: {
          commitments: commitmentsData
        },
        class: classToUse
      });
    }
  
    updateResourceAssignmentToProjectHandler(resourceAllocation) {
      this.updateResourceAssignmentToProject.emit(resourceAllocation);
    }
    upsertPlaceholderAllocationsToProjectHandler(placeholderAllocations) {
      this.upsertPlaceholderAllocationsToProject.emit( placeholderAllocations );
    }
  
    upsertResourceAllocationsToProjectHandler(resourceAllocations) {
      this.upsertResourceAllocationsToProject.emit(resourceAllocations);
    }
  
    updateResourceCommitmentHandler(resourceCommitment) {
      this.updateResourceCommitment.emit(resourceCommitment);
    }
  
    openQuickAddFormHandler(event) {
      this.openQuickAddForm.emit(event);
    }

    openResourceDetailsDialogHandler(event) {
      this.openResourceDetailsDialog.emit(event);
    }
  
    openSplitAllocationPopupHandler(event) {
      this.openSplitAllocationPopup.emit(event);
    }

    upsertResourceViewNoteHandler(event) {
        this.upsertResourceViewNote.emit(event);
    }
  
    deleteResourceViewNotesHandler(event) {
        this.deleteResourceViewNotes.emit(event);
    }

    upsertResourceRecentCDHandler(event) {
      this.upsertResourceRecentCD.emit(event);
    }

    upsertResourceCommercialModelHandler(event) {
      this.upsertResourceCommercialModel.emit(event);
    }

    deleteResourceCommercialModelHandler(event) {
      this.deleteResourceCommercialModel.emit(event);
   }
   deleteResourceRecentCDHandler(event) {
    this.deleteResourceRecentCD.emit(event);
 }
   selectedResourceViewTabHandler(event){
    this.selectedResourceViewTab.emit(event);
   }
  
    // start: to hide context menu on scroll
    divScroll(event) {
      document.dispatchEvent(new Event("click"));
    }
  
    @HostListener("window:keyup", ["$event"])
    keyEvent(event: KeyboardEvent) {
      // for pageDown and pageUp key code mapping
      if (event.keyCode === 33 || event.keyCode === 34) {
        document.dispatchEvent(new Event("click"));
      }
    }
    // end
  }
