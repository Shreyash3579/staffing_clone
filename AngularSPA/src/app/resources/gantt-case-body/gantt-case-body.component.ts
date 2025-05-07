// ----------------------- Angular Package References ----------------------------------//
import {
    Component,
    OnInit,
    Input,
    OnChanges,
    Output,
    EventEmitter,
    ChangeDetectionStrategy,
    SimpleChanges  } from "@angular/core";
  
  // ----------------------- Component References ----------------------------------//
  
  // --------------------------Interfaces -----------------------------------------//
  // ----------------------- Service References ----------------------------------//
  import { BsModalService } from "ngx-bootstrap/modal";
  import { PopupModalComponent } from "../popup-modal/popup-modal.component";
  import * as moment from "moment";
  import { StaffingCommitment } from "src/app/shared/interfaces/staffingCommitment.interface";
  import { ResourcesCount } from "src/app/shared/interfaces/resourcesCount.interface";
import { WeeklyMonthlyGroupingEnum } from "src/app/shared/constants/enumMaster";

  @Component({
    changeDetection: ChangeDetectionStrategy.OnPush,
    selector: "[resources-gantt-case-body]",
    templateUrl: "./gantt-case-body.component.html",
    styleUrls: ["./gantt-case-body.component.scss"]
  })
  export class GanttCaseBodyComponent implements OnInit, OnChanges {
    public isCaseGroupCollapsed = false;
    className = "";
    public commitmentDurationInDays: any;
    offsetLeft;
    gridSize = 32; 
    gridCellMargin = 6;
    numClicks = 0;  
    gridSpace = 2;
    public readonly weeklyMonthlyGroupingEnum = WeeklyMonthlyGroupingEnum;
   
  
    // -----------------------Input Events-----------------------------------------------//
  
    @Input() resourceStaffing: any;
    @Input() isLeftSideBarCollapsed = false;
    @Input() objGanttCollapsedRows;
    @Input() rowIndex: number;
    @Input() commitment: StaffingCommitment;
    @Input() dateRange: [Date, Date];
    @Input() resourcesCountOnCaseOpp: ResourcesCount[];
    @Input() selectedWeeklyMonthlyGroupingOption: string;
    @Input() isSelectedPracticeView:boolean;
    @Input() isTopbarCollapsed: boolean = true;
  
    // ------------------------Output Events---------------------------------------
    @Output() collapseExpandCaseGroupEmitter = new EventEmitter<boolean>();
    @Output() openQuickAddForm = new EventEmitter<any>();
    @Output() openOverlappedTeamsForm = new EventEmitter<any>();

    constructor(
      private modalService: BsModalService
    ) {}
  
    ngOnInit() {
      this.setCommitmentClasses();
    }

    ngOnChanges(changes: SimpleChanges) {
  
      if (changes.objGanttCollapsedRows && this.objGanttCollapsedRows) {
        this.isCaseGroupCollapsed = this.objGanttCollapsedRows.isAllCaseGroupsCollapsed;
      }
      if (changes.selectedWeeklyMonthlyGroupingOption && this.selectedWeeklyMonthlyGroupingOption) {    
      this.setCommitmentClasses();
      }
      
    }

    collapseExpandCaseGroupHandler(isCaseGroupCollapsed) {
      this.isCaseGroupCollapsed = isCaseGroupCollapsed;
      this.collapseExpandCaseGroupEmitter.emit(isCaseGroupCollapsed);

    }

    openMembersPopup(event,caseDetails, members){
      //DO not open pop-up for NOT Allocated case group
      if(caseDetails.oldCaseCode === "NA" || caseDetails.clientName === "Not Allocated")
        return;

      const positionObj = {
          top: event.clientY,
          left: event.clientX,
          right: 0
        };

        let memberCommitments = [];

        members.forEach( x=>{
          const allocations = x.allocations.filter( y=> (y.oldCaseCode && y.oldCaseCode === caseDetails.oldCaseCode) || (y.pipelineId && y.pipelineId === caseDetails.pipelineId));

          allocations.forEach( z => {
            memberCommitments.push({
              commitmentTypeName: 'Resource',
              employeeName: x.resource.fullName,
              startDate: z.startDate,
              endDate: z.endDate,
              allocation: z.allocation
            });
          });
        });

        this.openPopUpDialogHandler({positionObj, commitment: memberCommitments})
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

    private setCommitmentClasses(reAdjustClass = false, $event = null) {
      let dayOrWeek;
      let daysOrWeeks;
      if(this.selectedWeeklyMonthlyGroupingOption == this.weeklyMonthlyGroupingEnum.MONTHLY) {
        dayOrWeek = "week";
        daysOrWeeks = "weeks";
      } else {
        dayOrWeek = "day";
        daysOrWeeks = "days";
      }
      const caseStartDate = this.resourceStaffing.caseDetails?.startDate;
      let caseEndDate; 

      if (this.resourceStaffing.caseDetails?.endDate) {
        caseEndDate = this.resourceStaffing.caseDetails?.endDate;
      } else {
        // In case when end date is null, set end date as date range end date to avoid UI breaking
        caseEndDate =  moment(this.dateRange[1]).startOf(dayOrWeek).format();
      }

      let startCount = 0;
      const commitmentStartDate = moment(caseStartDate).startOf(
        dayOrWeek
      );
      const dateRangeStartDate = moment(this.dateRange[0]).startOf(dayOrWeek);
      const commitmentEndDate = moment(caseEndDate);
      const dateRangeEndDate = moment(this.dateRange[1]).startOf(dayOrWeek);
      this.commitmentDurationInDays = commitmentEndDate.diff(
        commitmentStartDate,
        daysOrWeeks
      );
  
      if (commitmentStartDate.isAfter(dateRangeStartDate)) {
        startCount =
          commitmentStartDate.diff(dateRangeStartDate, daysOrWeeks) + 1;
      }
  
      const end = commitmentEndDate.isAfter(dateRangeEndDate)
        ? dateRangeEndDate
        : commitmentEndDate;
      const start = commitmentStartDate.isAfter(dateRangeStartDate)
        ? commitmentStartDate
        : dateRangeStartDate;
        const duration = 
        this.selectedWeeklyMonthlyGroupingOption == this.weeklyMonthlyGroupingEnum.MONTHLY && end.day() == 0 ?
        end.diff(start, daysOrWeeks): end.diff(start, daysOrWeeks) + 1;
      //this.commitment["duration"] = duration;
      if (reAdjustClass) {
        this.offsetLeft =
          (startCount - 1) * this.gridSize + this.gridCellMargin;
        $event.host.classList.add(`duration-${duration}`);
      }
  
      let border = "";
      this.selectedWeeklyMonthlyGroupingOption == this.weeklyMonthlyGroupingEnum.MONTHLY ?
      this.className =
      "start-" +
      startCount +"-" + this.gridSpace +
      " duration-" +
      duration +"-" + this.gridSpace +
      " commitment-" +
      " " +
      border :
      this.className =
      "start-" +
      startCount +
      " duration-" +
      duration +
      " commitment-" +
      " " +
      border
    } 

    openOverlappedTeamsPopupHandler(event) {
      this.openOverlappedTeamsForm.emit(event);
    }
  }
