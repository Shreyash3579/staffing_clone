// ----------------------- Angular Package References ----------------------------------//
import {
  Component,
  OnInit,
  Input,
  Output,
  EventEmitter,
  ViewChild,
  SimpleChanges
} from "@angular/core";

// ----------------------- Service References ----------------------------------//
import { DateService } from "src/app/shared/dateService";
import { ResourceAllocationService } from "src/app/shared/services/resourceAllocation.service";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { NotificationService } from "src/app/shared/notification.service";
import { BsModalService } from "ngx-bootstrap/modal";

// --------------------------Interfaces -----------------------------------------//
import { StaffingCommitment } from "src/app/shared/interfaces/staffingCommitment.interface";
import { Commitment } from "src/app/shared/interfaces/commitment.interface";
import { ResourceAllocation } from "src/app/shared/interfaces/resourceAllocation.interface";

// ----------------------- External library/Utils ----------------------------------//
import * as moment from "moment";

// --------------------------Constants/Enums -----------------------------------------//
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { CommitmentType as CommitmentTypeCodeEnum, WeeklyMonthlyGroupingEnum } from "src/app/shared/constants/enumMaster";
import { Resource } from "src/app/shared/interfaces/resource.interface";
import { ValidationService } from "src/app/shared/validationService";
import { CommonService } from "src/app/shared/commonService";
import { CoreService } from "src/app/core/core.service";
import { PlaceholderAllocation } from "src/app/shared/interfaces/placeholderAllocation.interface";

@Component({
  selector: "app-gantt-commitment",
  templateUrl: "./gantt-commitment.component.html",
  styleUrls: ["./gantt-commitment.component.scss"]
})
export class GanttCommitmentComponent implements OnInit {
  // ------------------------Input Events---------------------------------------
  @Input() commitment: StaffingCommitment;
  @Input() dateRange: [Date, Date];
  @Input() bounds: HTMLElement;
  @Input() resource: Resource;
  @Input() isRowCollapsed: boolean = false;
  @Input() isTopbarCollapsed: boolean = false;
  @Input() selectedWeeklyMonthlyGroupingOption: string;
    
  // ------------------------Output Events---------------------------------------
  @Output() updateResourceAssignmentToProject =
    new EventEmitter<ResourceAllocation>();
  @Output() upsertPlaceholderAllocationsToProject = new EventEmitter<any>();
  @Output() upsertResourceAllocationsToProject = new EventEmitter<
    ResourceAllocation[]
  >();
  @Output() updateResourceCommitment = new EventEmitter<Commitment>();
  @Output() openQuickAddForm = new EventEmitter<any>();
  @Output() openSplitAllocationPopup = new EventEmitter<any>();
  @Output() openCaseDetailsDialog = new EventEmitter();
  @Output() openPopUpDialog = new EventEmitter();

  // ------------------------Local Variables---------------------------------------

  // @ViewChild(ContextMenuComponent) public basicMenu: ContextMenuComponent<any>;
  // gridSize = 45;
  gridSize = 32;
  gridSpace = 2;
  offsetLeft;
  gridCellMargin = 6;
  className = "";
  public investmentName: string;
  public commitmentDurationInDays: any;
  public commitmentDetails = "";
  toolTipText: string = "";
  commitmentDescription: string = "";
  clickTypeIndicator: any;
  accessibleFeatures = ConstantsMaster.appScreens.feature;
  public readonly weeklyMonthlyGroupingEnum = WeeklyMonthlyGroupingEnum;

  public numClicks = 0;
  public singleClickTimer: any;

  constructor(
    private _resourceAllocationService: ResourceAllocationService,
    private _localStorage: LocalStorageService,
    private notificationService: NotificationService,
    private _coreService: CoreService
  ) { }

  // ------------------------Life Cycle Hooks---------------------------------------

  ngOnInit() {
    this.setCommitmentClasses();
    this.getCommitmentDescriptionAndToolTipText();

    // console.log("Commitment", this.commitment);
  }

  ngOnChanges(changes: SimpleChanges){
    if(changes.isTopbarCollapsed || changes.isRowCollapsed || changes.selectedWeeklyMonthlyGroupingOption)
      this.setCommitmentClasses();
  }

  // ------------------------Component Event Handlers-----------------------------------

  public openCaseDetailsDialogHandler() {
    if(this.selectedWeeklyMonthlyGroupingOption != this.weeklyMonthlyGroupingEnum.MONTHLY) {
      setTimeout(() => {
        // 'clickTypeIndicator' prevent single click from firing twice in case of double click
        if (this.clickTypeIndicator) {
          return true;
        } else {
          this.openCaseDetailsDialog.emit({
            oldCaseCode: this.commitment.oldCaseCode,
            pipelineId: this.commitment.pipelineId,
            planningCardId: this.commitment.planningCardId
          });
        }
      }, 500);
      this.clickTypeIndicator = 0;
    }
  }

  public onResizeStart = ($event) => {
    if(this.selectedWeeklyMonthlyGroupingOption != this.weeklyMonthlyGroupingEnum.MONTHLY) {
      if (!this.commitment.isEditable || !this.isCommitmentAccessible(this.commitment)) {
        this.notificationService.showValidationMsg("Update not allowed!!");
        return;
      }
      const classArray: Array<string> = Array.from($event.host.classList);
      const classesToRemove = classArray.filter((c) =>
        c.match(/duration-[0-9]*/g)
      );
      if (classesToRemove.length) {
        $event.host.classList.remove(classesToRemove);
      }
    }
  };

  public onResizeStop = ($event) => {
    if(this.selectedWeeklyMonthlyGroupingOption != this.weeklyMonthlyGroupingEnum.MONTHLY) {
      if (!this.commitment.isEditable || !this.isCommitmentAccessible(this.commitment)) {
        return;
      }

      const duration = Math.ceil($event.size.width / this.gridSize);
      $event.host.classList.add(`duration-${duration}`);

      if ($event.direction.e) {
        let endDate = "";

        if (
          Date.parse(this.commitment.startDate) <
          Date.parse(this.dateRange[0].toDateString())
        ) {
          endDate = DateService.addDays(this.dateRange[0], duration - 1);
        } else {
          endDate = DateService.addDays(
            this.commitment.startDate,
            duration - 1
          );
        }

        if (Date.parse(endDate) === Date.parse(this.commitment.endDate)) {
          return;
        }else {
          if (
            this.commitment.commitmentTypeName ===
            ConstantsMaster.CommitmentType.Allocation
          ) {
            const existingAllocation = Object.assign(
              {},
              this.commitment
            );
            const updatedAllocation = Object.assign(
              {},
              this.commitment
            );
            updatedAllocation.endDate = endDate;

            const [isValidAllocation, errorMessage] =
              this._resourceAllocationService.validateMonthCloseForUpdates(
                updatedAllocation,
                existingAllocation
              );

            if (!isValidAllocation) {
              //$event.host.classList.remove(`duration-${duration}`);;
              //this.setCommitmentClasses(true, $event);
              this.notificationService.showValidationMsg(
                errorMessage
              );
              return;
            } else {
              this.commitment = updatedAllocation;

              this.checkForPrePostAndUpsertResourceAllocation(
                this.convertStaffingCommitmentToResourceAllocation(
                  this.commitment
                )
              );
            }
          }
          else if( this.commitment.commitmentTypeName ===
            ConstantsMaster.CommitmentType.PlanningCard){
              this.commitment.endDate = endDate;
              const placeholderAllocations: PlaceholderAllocation[] = [
                this.convertStaffingCommitmentToPlaceholderAllocation(
                this.commitment
                )
              ];
            this.upsertPlaceholderAllocationsToProject.emit({ placeholderAllocations });

          }
             else {
            this.commitment.endDate = endDate;
            const commitmentToBeUpdated =
              this.convertStaffingCommitmentToCommitment(
                this.commitment
              );
            this.updateResourceCommitment.emit(commitmentToBeUpdated);
          }
        }
      } else {
        let startDate = "";

        if (
          Date.parse(this.commitment.endDate) >
          Date.parse(this.dateRange[1].toDateString())
        ) {
          startDate = DateService.addDays(
            this.dateRange[1],
            -(duration - 1)
          );
        } else {
          startDate = DateService.addDays(
            this.commitment.endDate,
            -(duration - 1)
          );
        }

        if (moment(this.resource.startDate).isAfter(startDate, "day")) {
          $event.host.classList.remove(`duration-${duration}`);
          this.setCommitmentClasses(true, $event);
          this.notificationService.showValidationMsg(
            ValidationService.employeeJoiningDateGreaterThanStartDate.replace(
              "[joiningDate]",
              DateService.convertDateInBainFormat(
                this.resource.startDate
              )
            )
          );
          return;
        }

        if (
          Date.parse(startDate) === Date.parse(this.commitment.startDate)
        ) {
          return;
        } else {
          if (
            this.commitment.commitmentTypeName ===
            ConstantsMaster.CommitmentType.Allocation
          ) {
            const existingAllocation = Object.assign(
              {},
              this.commitment
            );
            const updatedAllocation = Object.assign(
              {},
              this.commitment
            );
            updatedAllocation.startDate = startDate;

            const [isValidAllocation, errorMessage] =
              this._resourceAllocationService.validateMonthCloseForUpdates(
                updatedAllocation,
                existingAllocation
              );

            if (!isValidAllocation) {
              //$event.host.classList.remove(`duration-${duration}`);;
              //this.setCommitmentClasses(true, $event);
              this.notificationService.showValidationMsg(
                errorMessage
              );
              return;
            } else {
              this.commitment = updatedAllocation;

              this.checkForPrePostAndUpsertResourceAllocation(
                this.convertStaffingCommitmentToResourceAllocation(
                  this.commitment
                )
              );
            }
          }
          else if( this.commitment.commitmentTypeName ===
            ConstantsMaster.CommitmentType.PlanningCard){
              this.commitment.startDate = startDate;
            const placeholderAllocations: PlaceholderAllocation[] = [
              this.convertStaffingCommitmentToPlaceholderAllocation(
              this.commitment
              )
            ];
            this.upsertPlaceholderAllocationsToProject.emit({ placeholderAllocations });

          }
           else if (
            this.commitment.commitmentTypeName !==
            ConstantsMaster.CommitmentType.Allocation
          ) {
            this.commitment.startDate = startDate;
            const commitmentToBeUpdated =
              this.convertStaffingCommitmentToCommitment(
                this.commitment
              );
            this.updateResourceCommitment.emit(commitmentToBeUpdated);
          }
        }
      }
    }
  };

  checkForPrePostAndUpsertResourceAllocation(
    resourceAllocation: ResourceAllocation
  ) {
    const projectStartDate = DateService.convertDateInBainFormat(
      resourceAllocation.oldCaseCode
        ? resourceAllocation.caseStartDate
        : resourceAllocation.opportunityStartDate
    );
    const projectEndDate = DateService.convertDateInBainFormat(
      resourceAllocation.oldCaseCode
        ? resourceAllocation.caseEndDate
        : resourceAllocation.opportunityEndDate
    );
    let allocationsData: ResourceAllocation[] = [];

    if (projectStartDate && projectEndDate) {
      allocationsData =
        this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocation);
    } else {
      allocationsData.push(resourceAllocation);
    }

    this.upsertResourceAllocationsToProject.emit(allocationsData);
  }

  public handleCommitmentClick(event) {
    if(this.selectedWeeklyMonthlyGroupingOption != this.weeklyMonthlyGroupingEnum.MONTHLY) {
      this.numClicks++;

       // Detecting Single vs Double Click
       if (this.numClicks === 1) {
         // If Single Click, open Commitment Detail Popup
         this.singleClickTimer = setTimeout(() => {
           this.numClicks = 0;

           // Avoid double interaction with opening popup and case detail overlay
           if (!event.target.classList.contains("allocation-title")) {
             this.openCommitmentDetailPopup(event, this.commitment);
           }
         }, 200);
       } else if (this.numClicks === 2) {
        // If double click, open Quick Add Form Popup
        clearTimeout(this.singleClickTimer);
        this.numClicks = 0;
        if (this.isCommitmentAccessible(this.commitment)) {
          this.openQuickAddFormPopup(event);
        }
      }    
      event.stopPropagation();
    }
  }

  public openQuickAddFormPopup(event) {
    this.clickTypeIndicator = 1;
    document.getSelection().empty();
    if (!this.commitment.isEditable || !this.isCommitmentAccessible(this.commitment)) {
      this.notificationService.showValidationMsg(
        "Cannot edit this " + this.commitment.commitmentTypeName
      );
    } else {
      let resourceAllocationData: ResourceAllocation;
      let resourcePlaceholderAllocationData: PlaceholderAllocation
      if( this.commitment.commitmentTypeName ===
        ConstantsMaster.CommitmentType.PlanningCard){
        resourcePlaceholderAllocationData =
        this.convertStaffingCommitmentToPlaceholderAllocation(
          this.commitment
        );
      }
      else{
        resourceAllocationData =
          this.convertStaffingCommitmentToResourceAllocation(
            this.commitment
          );
        }

      this.openQuickAddForm.emit({
        commitmentTypeCode:
          this.commitment.commitmentTypeCode ??
          CommitmentTypeCodeEnum.CASE_OPP,
        resourceAllocationData: resourceAllocationData || resourcePlaceholderAllocationData,
        isUpdateModal: true
      });
    }
    event.stopPropagation();
  }

  // ------------------------Helper Functions-----------------------------------

  private setCommitmentClasses(reAdjustClass = false, $event = null) {
    let startCount = 0;
    let duration;
    let border = "";
    if (
      this.commitment.commitmentTypeCode ===
      CommitmentTypeCodeEnum.PLANNING_CARD
    ) {
      border = "border-dotted";
    }

    if(this.selectedWeeklyMonthlyGroupingOption == this.weeklyMonthlyGroupingEnum.MONTHLY) {
      this.setCommitmentClassesForMonthlyView(border, startCount, duration);
    } else {
      this.setCommitmentClassesForWeeklyView(border, startCount, duration);
    }

    if (reAdjustClass) {
      this.offsetLeft =
        (startCount - 1) * this.gridSize + this.gridCellMargin;
      $event.host.classList.add(`duration-${duration}`);
    }
  }

  private setCommitmentClassesForWeeklyView(border, startCount, duration) {
    let dateRangeStartDate;
    let dateRangeEndDate;
    let commitmentStartDate;
    const commitmentEndDate = moment(this.commitment.endDate);

      dateRangeStartDate = moment(this.dateRange[0]).startOf('day');
      dateRangeEndDate = moment(this.dateRange[1]).startOf('day');
      commitmentStartDate = moment(this.commitment.startDate).startOf('day');    
      this.commitmentDurationInDays = commitmentEndDate.diff(
        commitmentStartDate,
        'days'
      );
      
      if (commitmentStartDate.isAfter(dateRangeStartDate)) {
        startCount =
          commitmentStartDate.diff(dateRangeStartDate, 'days') + 1;
      }
    
      const end = commitmentEndDate.isAfter(dateRangeEndDate)
        ? dateRangeEndDate
        : commitmentEndDate;
      const start = commitmentStartDate.isAfter(dateRangeStartDate)
        ? commitmentStartDate
        : dateRangeStartDate;
      duration = end.diff(start, 'days') + 1;
      this.commitment["duration"] = duration;
    
      this.className =
      "start-" +
      startCount +
      " duration-" +
      duration +
      " commitment-" +
      this.getCommitmentColorClass() +
      " " +
      border
  }

  private setCommitmentClassesForMonthlyView(border, startCount, duration) {
    let dateRangeStartDate;
    let dateRangeEndDate;
    let commitmentStartDate;
    const commitmentEndDate = moment(this.commitment.endDate);  

      dateRangeStartDate = moment(this.dateRange[0]);
      dateRangeEndDate = moment(this.dateRange[1]);
      commitmentStartDate = moment(this.commitment.startDate);
      const startOfWeek = commitmentStartDate.clone().startOf('isoWeek');
      const endOfWeek = commitmentEndDate.clone().startOf('isoWeek'); // Monday as the start of the week
      this.commitmentDurationInDays = endOfWeek.diff(startOfWeek, 'weeks') + 1;

      if (commitmentStartDate.isAfter(dateRangeStartDate)) {
        startCount =
          startOfWeek.diff(dateRangeStartDate, 'weeks') + 1;
      }
  
      const end = commitmentEndDate.isAfter(dateRangeEndDate)
        ? dateRangeEndDate
        : commitmentEndDate;
      const start = commitmentStartDate.isAfter(dateRangeStartDate)
        ? commitmentStartDate
        : dateRangeStartDate;
      const startOfWeek1 = start.clone().startOf('isoWeek');
      const endOfWeek1 = end.clone().startOf('isoWeek'); // Monday as the start of the week
      duration = endOfWeek1.diff(startOfWeek1, 'weeks') + 1;
      this.commitment["duration"] = duration;

      this.className =
      "start-" +
      startCount +"-" + this.gridSpace +
      " duration-" +
      duration +"-" + this.gridSpace +
      " commitment-" +
      this.getCommitmentColorClass() +
      " " +
      border
  }

  private getCommitmentColorClass(): string {
    let colorClass = "";  

      if(this.isTopbarCollapsed == false || this.isRowCollapsed == false){
        colorClass = this.getCommitmentColorClassExpanded(); 
      }
      else if(this.isTopbarCollapsed || this.isRowCollapsed){
        colorClass = this.getCommitmentColorClassCollapsed();
      }
      else{ 
        colorClass = this.getCommitmentColorClassCollapsed();
      }
    return colorClass;
  }

  private getCommitmentColorClassExpanded(): string {   
    return CommonService.getCommitmentColorClass(this.commitment.commitmentTypeCode, this.commitment.investmentCode,this.commitment.pipelineId,this.commitment.oldCaseCode,this.commitment.includeInCapacityReporting,this.commitment.status ?? null);
  }

  private getCommitmentColorClassCollapsed(): string {
    let colorClass = "";
    switch (this.commitment.commitmentTypeCode) {
      case CommitmentTypeCodeEnum.CASE_OPP:
      case CommitmentTypeCodeEnum.PLANNING_CARD:
      case CommitmentTypeCodeEnum.NAMED_PLACEHOLDER:
      {
          colorClass = "blue";
          break;
      }
      case CommitmentTypeCodeEnum.SHORT_TERM_AVAILABLE:
      case CommitmentTypeCodeEnum.NOT_AVAILABLE:
      case CommitmentTypeCodeEnum.LIMITED_AVAILABILITY:
      case CommitmentTypeCodeEnum.AAG:
      case CommitmentTypeCodeEnum.ADAPT:
      case CommitmentTypeCodeEnum.FRWD:
      case CommitmentTypeCodeEnum.VACATION:
      case CommitmentTypeCodeEnum.LOA:
      case CommitmentTypeCodeEnum.TRAINING:
      case CommitmentTypeCodeEnum.RECRUITING:
      case CommitmentTypeCodeEnum.HOLIDAY:
      case CommitmentTypeCodeEnum.PEG:
      case CommitmentTypeCodeEnum.PEG_Surge:
      case CommitmentTypeCodeEnum.DOWN_DAY: {
          colorClass = "purple";
          break;
      }
      default: {
          colorClass = "purple";
          break;
      }
    }
  return colorClass;
  }

  private getCommitmentDescriptionAndToolTipText() {
    if (
      this.commitment.commitmentTypeCode ==
        CommitmentTypeCodeEnum.CASE_OPP ||
      this.commitment.commitmentTypeCode ===
        CommitmentTypeCodeEnum.NAMED_PLACEHOLDER ||
      this.commitment.commitmentTypeCode ===
        CommitmentTypeCodeEnum.PLANNING_CARD
    ) {
      this.getCommitmentDescriptionAndToolTipTextForAllocations();
    } else {
      this.getCommitmentDescriptionAndToolTipTextForAllOtherCommitments();
    }
  }

  private getCommitmentDescriptionAndToolTipTextForAllocations() {
    const investmentName = this.commitment.investmentName
      ? " - " + this.commitment.investmentName
      : "";
    const caseRoleName = this.commitment.caseRoleName
      ? " - " + this.commitment.caseRoleName
      : "";
    const formattedStartDate = moment(this.commitment.startDate).format(
      "DD MMM YYYY"
    );
    const formattedEndDate = moment(this.commitment.endDate).format(
      "DD MMM YYYY"
    );

    const namedPlaceholderText = this.commitment.commitmentTypeCode === CommitmentTypeCodeEnum.NAMED_PLACEHOLDER
    ? "PLACEHOLDER - "
    : "";

    switch(this.commitment.commitmentTypeCode){
      case CommitmentTypeCodeEnum.CASE_OPP:
      case CommitmentTypeCodeEnum.NAMED_PLACEHOLDER : {
        
        if (this.commitment.oldCaseCode) {
          this.commitmentDescription = `${namedPlaceholderText}Case - ${this.commitment.oldCaseCode
            } - ${this.commitment.caseName + investmentName + caseRoleName} (${this.commitment.allocation
            }%) (${formattedStartDate} - ${formattedEndDate})`;
          this.toolTipText = this.commitmentDescription;
        } else if (this.commitment.pipelineId) {
          this.commitmentDescription = `${namedPlaceholderText}Opportunity - ${this.commitment.opportunityName + investmentName + caseRoleName
            } (${this.commitment.allocation}%) (${formattedStartDate} - ${formattedEndDate})`;
          this.toolTipText = this.commitmentDescription;
        }

        break;
      }
      case CommitmentTypeCodeEnum.PLANNING_CARD:{
        this.commitmentDescription = `Planning Card - ${this.commitment.planningCardName} (${this.commitment.allocation}%) (${formattedStartDate} - ${formattedEndDate})`;
        this.toolTipText = `${this.commitment.planningCardName} (${this.commitment.allocation}%) (${formattedStartDate} - ${formattedEndDate}) `;
        
        break;
      }
    }
    
  }

  private getCommitmentDescriptionAndToolTipTextForAllOtherCommitments() {
    const description = this.commitment.description
      ? " - " + this.commitment.description
      : "";
    const status = this.commitment.status
      ? " - " + this.commitment.status
      : "";

    let vacationStatus;

    if(this.commitment.status == 'Submitted'){
     vacationStatus = ", " + "Pending Approval"
    }

    else if(this.commitment.status == 'Approved'){
      vacationStatus = ", " + this.commitment.status
    }

    else{
      vacationStatus = "";
    }


    const formattedStartDate = moment(this.commitment.startDate).format(
      "DD MMM YYYY"
    );
    const formattedEndDate = moment(this.commitment.endDate).format(
      "DD MMM YYYY"
    );

    this.commitmentDescription = `${this.commitment.commitmentTypeName + description
      }`;
      if (this.commitment.commitmentTypeCode == 'V') {
        this.toolTipText = this.commitment.approvedBy && this.commitment.status == 'Approved'
        ? `${this.commitmentDescription} - (${formattedStartDate} - ${formattedEndDate}), Approved By: ${this.commitment.approvedBy}`
        : `${this.commitmentDescription} - (${formattedStartDate} - ${formattedEndDate}) ${vacationStatus}`;
      }
    else{
    this.toolTipText = `${this.commitmentDescription + status
      } (${formattedStartDate} - ${formattedEndDate})`;
    }
  }

  // Open Case Detail Popup
  openCommitmentDetailPopup(event, commitment) {
    const positionObj = {
      top: event.clientY,
      left: event.clientX,
      right: 0
    };

    this.openPopUpDialog.emit({
      commitment: commitment,
      positionObj: positionObj
    });
  }

  contextMenuHandler(event, commitment) {
    if (this.commitmentDurationInDays > 0) {
      commitment.startDate = DateService.convertDateInBainFormat(
        commitment.startDate
      );
      commitment.endDate = DateService.convertDateInBainFormat(
        commitment.endDate
      );
      if(commitment.commitmentTypeName ===
        ConstantsMaster.CommitmentType.PlanningCard){
        const resourcePlaceholderAllocationData =
        this.convertStaffingCommitmentToPlaceholderAllocation(
          this.commitment
        );
        this.openSplitAllocationPopup.emit({ allocationData: resourcePlaceholderAllocationData });
      }
      else{
        this.openSplitAllocationPopup.emit({ allocationData: commitment });
      }
      
      
    } else {
      this.notificationService.showValidationMsg(
        "Cannot split one day allocation"
      );
      return false;
    }
  }

  convertStaffingCommitmentToResourceAllocation(
    commitment: StaffingCommitment
  ) {
    const resourceAllocationData: ResourceAllocation = {
      id: commitment.id,
      allocation: commitment.allocation
        ? parseInt(commitment.allocation.toString(), 10)
        : commitment.allocation,
      caseName: commitment.caseName,
      caseRoleCode: commitment.caseRoleCode,
      clientName: commitment.clientName,
      currentLevelGrade: commitment.currentLevelGrade,
      employeeCode: commitment.employeeCode,
      employeeName: commitment.employeeName,
      endDate: commitment.endDate,
      investmentCode: commitment.investmentCode,
      investmentName: commitment.investmentName,
      commitmentTypeReasonCode: commitment.commitmentTypeReasonCode,
      commitmentTypeReasonName: commitment.commitmentTypeReasonName,
      lastUpdatedBy: commitment.lastUpdatedBy,
      oldCaseCode: commitment.oldCaseCode,
      operatingOfficeAbbreviation: commitment.operatingOfficeAbbreviation,
      operatingOfficeCode: commitment.operatingOfficeCode,
      opportunityName: commitment.opportunityName,
      pipelineId: commitment.pipelineId,
      serviceLineCode: commitment.serviceLineCode,
      serviceLineName: commitment.serviceLineName,
      startDate: commitment.startDate,
      caseEndDate: commitment.caseEndDate,
      caseStartDate: commitment.caseStartDate,
      caseTypeCode: commitment.caseTypeCode,
      internetAddress: commitment.internetAddress,
      notes: !commitment.notes
        ? commitment.description
        : commitment.notes,
      opportunityEndDate: commitment.opportunityEndDate,
      opportunityStartDate: commitment.opportunityStartDate
    };

    return resourceAllocationData;
  }


  convertStaffingCommitmentToPlaceholderAllocation(
    commitment: StaffingCommitment
  ) {
    const resourceAllocationData: PlaceholderAllocation = {
      id: commitment.id || commitment.placeholderId,
      allocation: commitment.allocation
        ? parseInt(commitment.allocation.toString(), 10)
        : commitment.allocation,
      planningCardId: commitment.planningCardId,
      planningCardTitle: commitment.planningCardName,
      caseName: commitment.caseName,
      caseRoleCode: commitment.caseRoleCode,
      clientName: commitment.clientName,
      currentLevelGrade: commitment.currentLevelGrade,
      employeeCode: commitment.employeeCode,
      employeeName: commitment.employeeName,
      endDate: commitment.endDate,
      investmentCode: commitment.investmentCode,
      investmentName: commitment.investmentName,
      lastUpdatedBy: commitment.lastUpdatedBy,
      oldCaseCode: commitment.oldCaseCode,
      operatingOfficeAbbreviation: commitment.operatingOfficeAbbreviation,
      operatingOfficeCode: commitment.operatingOfficeCode,
      opportunityName: commitment.opportunityName,
      pipelineId: commitment.pipelineId,
      serviceLineCode: commitment.serviceLineCode,
      serviceLineName: commitment.serviceLineName,
      startDate: commitment.startDate,
      caseEndDate: commitment.caseEndDate,
      caseStartDate: commitment.caseStartDate,
      caseTypeCode: commitment.caseTypeCode,
      internetAddress: commitment.internetAddress,
      notes: !commitment.notes
        ? commitment.description
        : commitment.notes,
      opportunityEndDate: commitment.opportunityEndDate,
      opportunityStartDate: commitment.opportunityStartDate
    };

    return resourceAllocationData;
  }
  convertStaffingCommitmentToCommitment(
    staffingCommitment: StaffingCommitment
  ) {
    const commitment: Commitment = {
      id: staffingCommitment.id,
      commitmentType: {
        commitmentTypeCode: staffingCommitment.commitmentTypeCode,
        commitmentTypeName: this._localStorage
          .get(ConstantsMaster.localStorageKeys.commitmentTypes)
          .find(
            (x) =>
              x.commitmentTypeCode ===
              this.commitment.commitmentTypeCode
          ).commitmentTypeName,
        precedence: this._localStorage
          .get(ConstantsMaster.localStorageKeys.commitmentTypes)
          .find(
            (x) =>
              x.commitmentTypeCode ===
              this.commitment.commitmentTypeCode
        ).precedence
      },
      commitmentTypeReasonCode: staffingCommitment.commitmentTypeReasonCode,
      employeeCode: staffingCommitment.employeeCode,
      endDate: staffingCommitment.endDate,
      lastUpdatedBy: staffingCommitment.lastUpdatedBy,
      notes: staffingCommitment.description,
      startDate: staffingCommitment.startDate
    };
    return commitment;
  }

  getFeatureName(commitment) {
    let feature = `commitmentType/${commitment.commitmentTypeCode}`;
    return feature;
  }

  isCommitmentAccessible(commitment) {
    let editableCommitments: string[] = CommonService.getEditableCommitmentTypesCodesList(this._coreService.loggedInUserClaims);
    return editableCommitments.some(x => x === commitment.commitmentTypeCode);
  }

  isContextMenuAccessible(commitment) {
    const featureName = this.getFeatureName(commitment);
    const accessibleFeatures = this._coreService.loggedInUserClaims.FeatureAccess;
    const isReadOnly = CommonService.isReadOnlyAccessToFeature(featureName, accessibleFeatures);
    const isLinkDisabled = CommonService.isLinkDisabledForFeature(featureName, accessibleFeatures);
    let isContextMenuAccessible = true;

    if (isReadOnly || isLinkDisabled) {
      isContextMenuAccessible = false;
    }
    return isContextMenuAccessible;
  }
}
