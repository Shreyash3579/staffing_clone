// ----------------------- Angular Package References ----------------------------------//
import {
  Component,
  OnInit,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges
} from "@angular/core";

// ----------------------- Service References ----------------------------------//
import { DateService } from "src/app/shared/dateService";
import { NotificationService } from "src/app/shared/notification.service";
import { ValidationService } from "src/app/shared/validationService";

// ----------------------- Helper Services References ----------------------------------//
import { ResourceCommitmentHelperService } from "src/app/shared/helperServices/resourceCommitmentHelper.service";

// --------------------------Interfaces -----------------------------------------//
import { ResourceAllocation } from "src/app/shared/interfaces/resourceAllocation.interface";
import { ResourceStaffing } from "src/app/shared/interfaces/resourceStaffing.interface";
import { StaffingCommitment } from "src/app/shared/interfaces/staffingCommitment.interface";
import { Commitment } from "src/app/shared/interfaces/commitment.interface";

// --------------------------Constants/Enums -----------------------------------------//
import * as moment from "moment";
import { CommitmentType as CommitmentTypeCodeEnum, EmployeeCaseGroupingEnum, WeeklyMonthlyGroupingEnum } from "src/app/shared/constants/enumMaster";
import { Resource } from "src/app/shared/interfaces/resource.interface";

// --------------------------Components -----------------------------------------//
import { CommonService } from "src/app/shared/commonService";
import { ProjectBasic } from "src/app/shared/interfaces/project.interface";
import { Dayjs } from "dayjs";
import { PlaceholderAllocation } from "src/app/shared/interfaces/placeholderAllocation.interface";

@Component({
  selector: "resources-gantt-task",
  templateUrl: "./gantt-task.component.html",
  styleUrls: ["./gantt-task.component.scss"],
  providers: [ResourceCommitmentHelperService]
})
export class GanttTaskComponent implements OnInit, OnChanges {
  // -----------------------Local Variables--------------------------------------------//
  public items: any;
  // public _highlightResourceTask = new BehaviorSubject<any>({});
  // public highlightResource = false;
  public allocationMatrix: ResourceAllocation[][];
  public commitmentMatrix: StaffingCommitment[][];
  public collapsedCasesCommitmentMatrix: StaffingCommitment[][];
  public perDayAllocation: number[];
  public perDayClass = [];
  public projectStartDate = "";
  public projectEndDate = "";
  public commitmentArray: StaffingCommitment[] = [];
  public isRowCollapsed = false;
  public numClicks = 0;
  public readonly weeklyMonthlyGroupingEnum = WeeklyMonthlyGroupingEnum;

  // -----------------------Input Events-----------------------------------------------//
  _staffing: ResourceStaffing;
  _selectedCommitmentTypes: string[];
  @Input() index: number;
  @Input() dateRange: [Date, Date];
  @Input() thresholdRangeValue: any;
  @Input() resource: Resource;
  @Input() case: ProjectBasic;
  @Input() objGanttCollapsedRows;
  @Input() selectedEmployeeCaseGroupingOption: string;
  @Input() selectedWeeklyMonthlyGroupingOption: string;
  @Input() rowIndex = "";
  @Input() isTopbarCollapsed : boolean;

  @Input()
  set selectedCommitmentTypes(value: string[]) {
      this._selectedCommitmentTypes = value;
      if (value && this.staffing) {
          this.commitmentMatrix = this.getCommitmentMatrix();
      }
  }
  get selectedCommitmentTypes() {
      return this._selectedCommitmentTypes;
  }

  @Input()
  set staffing(value: ResourceStaffing) {
      this._staffing = value;
      if (value) {
          this.perDayAllocation = this.getPerDayAllocation();
      }
  }
  get staffing() {
      return this._staffing;
  }

  // ------------------------Output Events---------------------------------------
  @Output() updateResourceAssignmentToProject = new EventEmitter<ResourceAllocation>();
  @Output() upsertPlaceholderAllocationsToProject = new EventEmitter<any>();
  @Output() upsertResourceAllocationsToProject = new EventEmitter<ResourceAllocation[]>();
  @Output() updateResourceCommitment = new EventEmitter<Commitment>();
  @Output() highlightResourceRow = new EventEmitter();
  @Output() openQuickAddForm = new EventEmitter<any>();
  @Output() openSplitAllocationPopup = new EventEmitter<any>();
  @Output() updateGanttResourceHeight = new EventEmitter<any>();
  @Output() openCaseDetailsDialog = new EventEmitter<any>();
  @Output() openPopUpDialog = new EventEmitter();

  // --------------------------Component LifeCycle Events----------------------------//
  constructor(
      private notifyService: NotificationService,
      private resourceCommitmentHelperService: ResourceCommitmentHelperService
  ) {}

  ngOnInit() {
    this.getPerDayAllocationData();
  }

  // because we're not invoking api call while switching different commitment types the view
  // is not getting refreshed for the resource details components (left hand side) and hence
  // the alignment issue started appearing again. so the below event fires everytime there is a
  // change in selectedCommitmentTypes input property
  ngOnChanges(changes: SimpleChanges) {
    if (changes.selectedCommitmentTypes) {
        this.updateGanttResourceHeight.emit({
            resource: this.staffing.resource,
            commitmentMatrix: this.commitmentMatrix
        });
    }

    if (changes.objGanttCollapsedRows && this.objGanttCollapsedRows) {
        if(this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES){
            this.isRowCollapsed = true;
            this.collapsedCasesCommitmentMatrix = this.getCollapsedCasesCommitmentMatrix();
        }else{
            this.isRowCollapsed = this.objGanttCollapsedRows.exceptionRowIndexes.includes(this.resource.employeeCode)
            ? !this.objGanttCollapsedRows.isAllCollapsed : this.objGanttCollapsedRows.isAllCollapsed;
        }
    }
    if ((changes.selectedWeeklyMonthlyGroupingOption && this.selectedWeeklyMonthlyGroupingOption) ||
        changes.selectedEmployeeCaseGroupingOption && this.selectedEmployeeCaseGroupingOption) {
        this.getPerDayAllocationData();
    }

    if (changes.staffing && this.staffing) {
        this.commitmentMatrix = this.getCommitmentMatrix(); // Update matrix on any relevant changes
        this.updateGanttResourceHeight.emit({
            resource: this.staffing.resource,
            commitmentMatrix: this.commitmentMatrix
        });
        this.getPerDayAllocationData();
    }

  }

  trackByCommitmentId(index: number, commitment: any): string {
    return commitment.id; // Ensure that this ID is unique for each commitment
  }

  getPerDayAllocationData() {
    this.projectStartDate = DateService.getFormattedDate(this.dateRange[0]);
        this.projectEndDate = DateService.getFormattedDate(this.dateRange[1]);
        this.perDayAllocation = this.getPerDayAllocation();
        if (!this.perDayAllocation.length) {
            const datesBetweenRange = DateService.getDates(
                this.projectStartDate,
                this.projectEndDate
            );
            datesBetweenRange.forEach((date) => {
                const day = DateService.getDayAbbreviationsInLowerCase(date);
                if(this.selectedWeeklyMonthlyGroupingOption === this.weeklyMonthlyGroupingEnum.MONTHLY) {
                    if(day === "mon") {
                        this.perDayAllocation.push(0);
                    }
                } else {    
                    this.perDayAllocation.push(0);
                }
            });
        }
        this.getClassNameForEachDay();
  }
  
  // --------------------------Helper Methods----------------------------//
  openCaseDetailsDialogHandler(event) {
      this.openCaseDetailsDialog.emit(event);
  }
  
  openPopUpDialogHandler(event) {
    const rowElement = document.querySelector<HTMLElement>(
      `#gantt-row-${this.rowIndex}`
    );
    let commitmentsData = [];

    if(!rowElement.className.includes('collapsed')) {
      commitmentsData.push(event.commitment);
    }
    else {
      commitmentsData = this.commitmentArray;
    }

    this.openPopUpDialog.emit({
        commitment: commitmentsData,
        positionObj: event.positionObj
    });

  }

  getCommitmentMatrix() { 
    let commitmentMatrix = new Array<Array<StaffingCommitment>>();

    if (this.isCommitmentDataEmpty()) {
        return commitmentMatrix;
    }

      this.commitmentArray =
          this.getAllCommitmentsConvertedInStaffingCommitmentModel();

      //show PEG separately
      this.commitmentArray= this.commitmentArray.filter( x => x.commitmentTypeCode !== CommitmentTypeCodeEnum.PEG);

      let allocations = this.commitmentArray.filter(x =>
        x.commitmentTypeCode === CommitmentTypeCodeEnum.CASE_OPP
        || x.commitmentTypeCode === CommitmentTypeCodeEnum.PLANNING_CARD
        || x.commitmentTypeCode === CommitmentTypeCodeEnum.NAMED_PLACEHOLDER);
        
      let commitments = this.commitmentArray.filter(f => !allocations.includes(f));

    commitmentMatrix = this.getCommitmentMatrixData(allocations, commitments, commitmentMatrix);
    return commitmentMatrix;
  }

  getCollapsedCasesCommitmentMatrix()
  {
    let commitmentMatrix = new Array<Array<StaffingCommitment>>();

    if (this.isCommitmentDataEmpty()) {
        return commitmentMatrix;
    }

    this.commitmentArray =
        this.getAllCommitmentsConvertedInStaffingCommitmentModel();
    
      //show PEG separately
      this.commitmentArray= this.commitmentArray.filter( x => x.commitmentTypeCode !== CommitmentTypeCodeEnum.PEG);

      let allocations = this.commitmentArray.filter(x =>
        x.commitmentTypeCode === CommitmentTypeCodeEnum.CASE_OPP
        || x.commitmentTypeCode === CommitmentTypeCodeEnum.PLANNING_CARD
        || x.commitmentTypeCode === CommitmentTypeCodeEnum.NAMED_PLACEHOLDER);

        allocations = allocations.filter(x => (this.case.oldCaseCode && x.oldCaseCode == this.case.oldCaseCode) ||
                                                (this.case.pipelineId && x.pipelineId == this.case.pipelineId) ||
                                                (this.case.planningCardId && x.planningCardId == this.case.planningCardId));

        let commitments = this.commitmentArray.filter(x =>
            !(x.commitmentTypeCode === CommitmentTypeCodeEnum.CASE_OPP)
            && !(x.commitmentTypeCode === CommitmentTypeCodeEnum.PLANNING_CARD)
            && !(x.commitmentTypeCode === CommitmentTypeCodeEnum.NAMED_PLACEHOLDER));

    commitmentMatrix = this.getCommitmentMatrixData(allocations, commitments, commitmentMatrix);
    return commitmentMatrix;

  }

  getCommitmentMatrixData(allocations, commitments, commitmentMatrix)
  {
    let allocationMatrixData = new Array<Array<StaffingCommitment>>();
    let commitmentMatrixData = new Array<Array<StaffingCommitment>>();

    allocations.forEach((allocation: StaffingCommitment) => {
        let flag = false;
        if (
            this.commitmentLiesInSelectedDateRange(
                allocation,
                this.dateRange
            )
        ) {
            allocationMatrixData.every((innerArray) => {
                if (
                    this.isCommitmentNonOverlappingInCommitmentMatrix(
                        allocation,
                        innerArray
                    )
                ) {
                    innerArray.push(allocation);
                    flag = true;
                    return false;
                }
                return true;
            });

            if (!flag) {
                const staffingMatrixNewList =
                    new Array<StaffingCommitment>();
                staffingMatrixNewList.push(allocation);

                allocationMatrixData.push(staffingMatrixNewList);
            }
        }
    });

    commitments.forEach((commitment: StaffingCommitment) => {
        let flag = false;
        if (
            this.commitmentLiesInSelectedDateRange(
                commitment,
                this.dateRange
            )
        ) {
            commitmentMatrixData.every((innerArray) => {
                if (
                    this.isCommitmentNonOverlappingInCommitmentMatrix(
                        commitment,
                        innerArray
                    )
                ) {
                    innerArray.push(commitment);
                    flag = true;
                    return false;
                }
                return true;
            });

            if (!flag) {
                const staffingMatrixNewList =
                    new Array<StaffingCommitment>();
                staffingMatrixNewList.push(commitment);

                commitmentMatrixData.push(staffingMatrixNewList);
            }
        }
    });

    commitmentMatrix = commitmentMatrix.concat(allocationMatrixData, commitmentMatrixData);

    return commitmentMatrix;
  }

  getPerDayAllocation() {
      const allocations = this.staffing.allocations;
      let daysOrWeeks;

      if(this.selectedWeeklyMonthlyGroupingOption === this.weeklyMonthlyGroupingEnum.MONTHLY) {
        daysOrWeeks = "weeks";
      } else {
            daysOrWeeks = "days";
      }

      if (
          allocations.length <= 0 ||
          !this.projectStartDate ||
          !this.projectEndDate
      ) {
          return new Array<number>(0);
      }

      const numberOfDays =
          moment(this.projectEndDate).diff(
              moment(this.projectStartDate),
              daysOrWeeks
          ) + 1;
      const numberOfAllocations = allocations.length;

      const allocationMatrix: number[][] =
          this.getMatrixForEveryDayAllocation(allocations, numberOfDays, daysOrWeeks);
      const perDayAllocation = this.calculatePerDayAllocation(
          numberOfDays,
          numberOfAllocations,
          allocationMatrix
      );

      return perDayAllocation;
  }

  getMatrixForEveryDayAllocation(
      allocations: ResourceAllocation[],
      numberOfDays: number,
      daysOrWeeks
  ) {
      let row = 0;
      const resourceAllocationMatrix = new Array<Array<number>>(
          allocations.length
      );
      for (let index = 0; index < allocations.length; index++) {
          resourceAllocationMatrix[index] = new Array<number>(
              numberOfDays
          ).fill(0);
      }

      allocations.forEach((resourceAllocation) => {
          const columnStartIndex =
              moment(resourceAllocation.startDate) <
              moment(this.projectStartDate)
                  ? 0
                  : moment(resourceAllocation.startDate).diff(
                        moment(this.projectStartDate),
                        daysOrWeeks
                    );

          const columnEndIndex =
              moment(resourceAllocation.endDate) < moment(this.projectEndDate)
                  ? moment(resourceAllocation.endDate).diff(
                        moment(this.projectStartDate),
                        daysOrWeeks
                    )
                  : numberOfDays;

          for (
              let column = columnStartIndex;
              column <= columnEndIndex;
              column++
          ) {
              resourceAllocationMatrix[row][column] =
                  resourceAllocation.allocation;
          }
          row++;
      });

      return resourceAllocationMatrix;
  }

  calculatePerDayAllocation(
      numberOfDays: number,
      numberOfAllocations: number,
      allocationMatrix: number[][]
  ) {
      const perDayAllocation = new Array<number>(numberOfDays).fill(0);
      for (let columnIndex = 0; columnIndex < numberOfDays; columnIndex++) {
          let sumOfAllocationsByDay = 0;
          for (let rowIndex = 0; rowIndex < numberOfAllocations; rowIndex++) {
              sumOfAllocationsByDay =
                  sumOfAllocationsByDay +
                  allocationMatrix[rowIndex][columnIndex];
          }

          perDayAllocation[columnIndex] = sumOfAllocationsByDay;
      }
      return perDayAllocation;
  }

  getClassNameForEachDay(): any {
    this.perDayClass = [];
      const datesBetweenRange: Dayjs[] = DateService.getDates(
          this.projectStartDate,
          this.projectEndDate
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
        datesBetweenRange.forEach((date, index) => {
            const day = DateService.getDayAbbreviationsInLowerCase(date); //gets weekday abbreviations like "mon", tues etc
            let className = day === "sat" || day === "sun" ? "weekend" : "";
            if (className !== "weekend" && this.perDayAllocation[index] < 100) {
                className = "underutilized";
            }
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

  updateResourceAssignmentToProjectHandler(resourceAllocation) {
      this.updateResourceAssignmentToProject.emit(resourceAllocation);
  }
  upsertPlaceholderAllocationsToProjectHandler(placeholderAllocations) {
    this.upsertPlaceholderAllocationsToProject.emit(placeholderAllocations);
  }

  upsertResourceAllocationsToProjectHandler(resourceAllocations) {
      this.upsertResourceAllocationsToProject.emit(resourceAllocations);
  }

  updateResourceCommitmentHandler(resourceCommitment) {
      this.updateResourceCommitment.emit(resourceCommitment);
  }

  setResourceDataInUpsertedCommitment(
      resourceAllocationObj: ResourceAllocation
  ): ResourceAllocation {
      resourceAllocationObj.employeeCode =
          this.staffing.resource.employeeCode;
      resourceAllocationObj.employeeName = this.staffing.resource.fullName;
      resourceAllocationObj.operatingOfficeCode =
          this.staffing.resource.schedulingOffice.officeCode;
      resourceAllocationObj.operatingOfficeAbbreviation =
          this.staffing.resource.schedulingOffice.officeAbbreviation;
      resourceAllocationObj.currentLevelGrade =
          this.staffing.resource.levelGrade;
      resourceAllocationObj.serviceLineCode =
          this.staffing.resource.serviceLine.serviceLineCode;
      resourceAllocationObj.serviceLineName =
          this.staffing.resource.serviceLine.serviceLineName;
      /** Note: Here individual checks of undefined and null are added as 0 is a valid value.
  Hence the traditional !resourceAllocationObj.allocation check will not work */
      resourceAllocationObj.allocation =
          resourceAllocationObj.allocation == undefined ||
          resourceAllocationObj.allocation == null
              ? this.staffing.resource.fte * 100
              : resourceAllocationObj.allocation;

      resourceAllocationObj.joiningDate = this.staffing.resource.startDate;

      return resourceAllocationObj;
  }

  openQuickAddFormHandler(event) {
      event.resourceAllocationData = this.setResourceDataInUpsertedCommitment(
          event.resourceAllocationData
      );

      this.openQuickAddForm.emit(event);
  }

  public openQuickAddFormPopup(event) {
    if(this.selectedWeeklyMonthlyGroupingOption != this.weeklyMonthlyGroupingEnum.MONTHLY) {
        this.numClicks++;
        if(this.numClicks == 2)
        {
            this.numClicks = 0;
        if (!!this.staffing && this.staffing.resource.isTerminated) {
            event.stopPropagation();
            this.notifyService.showValidationMsg(
                ValidationService.terminatedEmployeeAllocation
            );
            return;
        }

        var columnDate = this.getDate(event, "columnDate");
        var startDateOfWeekValue = this.getDate(event, "columnStartDate");
        var startDateOfWeek = new Date(startDateOfWeekValue?.textContent);
        var dateElement;
        if(Number(columnDate?.textContent) < startDateOfWeek.getDate())
        {
            dateElement = startDateOfWeek.setMonth(startDateOfWeek.getMonth() + 1);
            dateElement = startDateOfWeek.setDate(Number(columnDate?.textContent));
        }
        else {
            dateElement = startDateOfWeek.setDate(Number(columnDate?.textContent));
        }
        const startDate = new Date(dateElement).toString();

        const resourceAllocationData = this.setResourceDataInUpsertedCommitment(
            {} as ResourceAllocation
        );
        resourceAllocationData.startDate = startDate;

        this.openQuickAddForm.emit({
            commitmentTypeCode: "",
            resourceAllocationData: resourceAllocationData,
            isUpdateModal: false
        });
        }
        event.stopPropagation();
    }
  }

  private getDate(event, className) {
    document.getSelection().empty();
    const xAxis = event.clientX;
    const firstElementWithClassColumnDays = document
    .getElementsByClassName(className)[0];

    if(!firstElementWithClassColumnDays) return;

    const parentDatePosition = document
        .getElementsByClassName(className)[0]
        .getBoundingClientRect() as DOMRect;
    const yAxis = parentDatePosition.y;
    const dateElement = document.elementFromPoint(xAxis, yAxis)
        .lastElementChild as HTMLElement;
    return dateElement;
  }

  // --------------------------Prvate Helper Functin----------------------------//

  private isCommitmentDataEmpty() {
      return (
          !this.staffing.allocations.length &&
          !this.staffing.placeholderAllocations.length &&
          !this.staffing.commitments &&
          !this.staffing.loAs &&
          !this.staffing.trainings &&
          !this.staffing.transfers &&
          !this.staffing.transitions &&
          !this.staffing.vacations &&
          !this.staffing.timeOffs &&
          !this.staffing.holidays
      );
  }

  private isOtherCommitmentsSelected() {
      return (
          (this.selectedCommitmentTypes.length === 1 &&
              !this.selectedCommitmentTypes.includes("C")) ||
          this.selectedCommitmentTypes.length > 1
      );
  }

  private commitmentLiesInSelectedDateRange(
      commitment: StaffingCommitment,
      dateRange: [Date, Date]
  ) {
      if (dateRange) {
          return (
              moment(commitment.startDate).isSameOrBefore(
                  dateRange[1],
                  "day"
              ) &&
              moment(commitment.endDate).isSameOrAfter(dateRange[0], "day")
          );
      }
      return true;
  }

  private isCommitmentNonOverlappingInCommitmentMatrix(
      commitment: StaffingCommitment,
      commitmentmatrixInnerArray
  ) {
      const innerArrayFirstElementStartDate = moment(
          commitmentmatrixInnerArray[0].startDate
      ).startOf("day");
      const innerArrayLastElementEndDate = moment(
          commitmentmatrixInnerArray[commitmentmatrixInnerArray.length - 1]
              .endDate
      ).startOf("day");

      return (
          moment(commitment.startDate)
              .startOf("day")
              .isAfter(innerArrayLastElementEndDate) ||
          moment(commitment.endDate)
              .startOf("day")
              .isBefore(innerArrayFirstElementStartDate)
      );
  }

  public openSplitAllocationPopupHandler(event) {
      this.openSplitAllocationPopup.emit(event);
  }

  getAllCommitmentsConvertedInStaffingCommitmentModel() {
      let commitmentArray: StaffingCommitment[] = [];

      if (this.isOtherCommitmentsSelected()) {
          commitmentArray = commitmentArray.concat(
              this.resourceCommitmentHelperService.convertAllCommitmentsCreatedinBOSSToStaffingCommitmentModel(
                  this.staffing.commitments,
                  this.selectedCommitmentTypes
              )
          );
      }

      if (
          this.selectedCommitmentTypes.includes(
            CommitmentTypeCodeEnum.CASE_OPP
          )
      ) {
          commitmentArray = commitmentArray.concat(
              this.resourceCommitmentHelperService.convertResourceAllocationsToStaffingCommitmentModel(
                  this.staffing.allocations
              )
          );
      }

      if (
        this.selectedCommitmentTypes.includes(
              CommitmentTypeCodeEnum.NAMED_PLACEHOLDER
        )
    ) {
        commitmentArray = commitmentArray.concat(
            this.resourceCommitmentHelperService.convertPlaceholderAllocationsToStaffingCommitmentModel(
                this.staffing.placeholderAllocations
            )
        );
    }

      if (
          this.selectedCommitmentTypes.includes(
              CommitmentTypeCodeEnum.PLANNING_CARD) && !this.selectedCommitmentTypes.includes(
                CommitmentTypeCodeEnum.NAMED_PLACEHOLDER
          )
      ) {
          commitmentArray = commitmentArray.concat(
              this.resourceCommitmentHelperService.convertPlaceholderAllocationsToStaffingCommitmentModel(
                  this.staffing.placeholderAllocations.filter(x => x.planningCardId)
              )
          );
      }

      if (this.selectedCommitmentTypes.includes(CommitmentTypeCodeEnum.LOA)) {
          commitmentArray = commitmentArray.concat(
              this.resourceCommitmentHelperService.convertLOAsFromSourceToStaffingCommitmentModel(
                  this.staffing.loAs
              )
          );
      }

      if (
          this.selectedCommitmentTypes.includes(
              CommitmentTypeCodeEnum.VACATION
          )
      ) {
          commitmentArray = commitmentArray.concat(
              this.resourceCommitmentHelperService.convertVacationsFromSourceToStaffingCommitmentModel(
                  this.staffing.vacations,
                  this.staffing.timeOffs
              )
          );
      }

      if (
          this.selectedCommitmentTypes.includes(
              CommitmentTypeCodeEnum.TRAINING
          )
      ) {
          commitmentArray = commitmentArray.concat(
              this.resourceCommitmentHelperService.convertTrainingsFromSourceToStaffingCommitmentModel(
                  this.staffing.trainings
              )
          );
      }

      if (
          this.selectedCommitmentTypes.includes(
              CommitmentTypeCodeEnum.HOLIDAY
          )
      ) {
          commitmentArray = commitmentArray.concat(
              this.resourceCommitmentHelperService.convertHolidaysFromSourceToStaffingCommitmentModel(
                  this.staffing.holidays
              )
          );
      }

      return commitmentArray.sort((a, b) => {
        const isPreviousElementAllocation = a.commitmentTypeCode === CommitmentTypeCodeEnum.CASE_OPP
        || a.commitmentTypeCode === CommitmentTypeCodeEnum.PLANNING_CARD ? 1 : 0;
        const isNextElementAllocation = b.commitmentTypeCode === CommitmentTypeCodeEnum.CASE_OPP
        || b.commitmentTypeCode === CommitmentTypeCodeEnum.PLANNING_CARD ? 1 : 0;

        if(isPreviousElementAllocation && !isNextElementAllocation) {
          return -1;
        }
        else if(!isPreviousElementAllocation && isNextElementAllocation) {
          return 1
        }
        else if ((new Date(a.startDate).getTime() === new Date(b.startDate).getTime())
        || (moment(a.startDate).isSameOrBefore(this.dateRange[0], "day")
            && moment(b.startDate).isSameOrBefore(this.dateRange[0], "day"))) {
            if(a.clientName || b.clientName) {
              return CommonService.sortByString(a.clientName, b.clientName, 'des');
            }
            else {
              return CommonService.sortByString(a.commitmentTypeName, b.commitmentTypeName, 'des');
            }
        }
        else
          return (new Date(a.startDate).getTime() - new Date(b.startDate).getTime())
      });
  }
}
