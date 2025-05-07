// ----------------------- Angular Package References ----------------------------------//
import {
  Component,
  ElementRef,
  OnInit,
  ViewChild,
  ViewEncapsulation,
  Input,
  Output,
  EventEmitter,
  AfterViewInit,
  OnDestroy,
  HostListener
} from '@angular/core';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

// --------------------------Interfaces -----------------------------------------//
import { Commitment } from 'src/app/shared/interfaces/commitment.interface';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { Employee } from 'src/app/shared/interfaces/employee.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { InvestmentCategory } from 'src/app/shared/interfaces/investmentCateogry.interface';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';

// ----------------------- Component/Service References ----------------------------------//
import { GanttService } from '../gantt.service';
import { CaseRoleType } from 'src/app/shared/interfaces/caseRoleType.interface';
import { NotificationService } from '../../../shared/notification.service';
import { GanttResourceService } from './gantt-resource.service';
import { DateService } from 'src/app/shared/dateService';
import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';
import { ValidationService } from 'src/app/shared/validationService';

/// --------------------------------Third Party References--------------------------------------------
import 'dhtmlx-gantt';
import 'dhtmlx-gantt/gantt-api.js';
import * as moment from 'moment';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { Gantt } from 'dhtmlx-gantt';
import { BS_DEFAULT_CONFIG } from 'src/app/shared/constants/bsDatePickerConfig';
import { CoreService } from 'src/app/core/core.service';

@Component({
  encapsulation: ViewEncapsulation.None,
  selector: 'app-gantt-resource',
  templateUrl: './gantt-resource.component.html',
  styleUrls: ['./gantt-resource.component.scss']
})
export class GanttResourceComponent
  implements OnInit, AfterViewInit, OnDestroy {
  // ----------------------- Local Variables --------------------------------------------//
  destroy$: Subject<boolean> = new Subject<boolean>();
  public selectedAllocations = [];
  public ganttResourceData = [];
  public resourceCommitments;
  public scaleConfig = 'month';
  private dataProcessor;
  private gantt$;
  public downloadFileTypes = {
    pdf: ConstantsMaster.exportFileSettings.fileType.pdf,
    png: ConstantsMaster.exportFileSettings.fileType.png,
    exl: ConstantsMaster.exportFileSettings.fileType.exl,
  };
  readonly customCssForExportedFiles = ConstantsMaster.exportFileSettings.gantttCssStyles;
  public calendarOptions = ConstantsMaster.ganttCalendarScaleOptions.options;
  bsConfig: Partial<BsDatepickerConfig>;
  minDate = new Date(ConstantsMaster.datePickerSettings.minDate);
  oldCaseCode: any;
  pipelineId: any;
  clickTypeIndicator: any;
  isTaskCaseType: boolean;
  private ganttResourceService: GanttResourceService;

  // -------------------------------- Events --------------------------------------------//
  @Input() commitmentTypes: CommitmentType[];
  @Input() investmentCategories: InvestmentCategory[];
  @Input() caseRoleTypes: CaseRoleType[];
  @Input() resource: Employee;
  @Input() commitmentStartDate: Date = new Date();
  @Input() calendarRadioSelected: string;

  @Output() updateResourceCommitment = new EventEmitter();
  @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();
  @Output() deleteResourceCommitment = new EventEmitter();
  @Output() deleteResourceAllocationFromCase = new EventEmitter();
  @Output() openQuickAddForm = new EventEmitter<any>();
  @Output() getCommitmentForEmployeeOnOrAfterSelectedDate = new EventEmitter<any>();
  @Output() oldCaseCodeToOpen = new EventEmitter<any>();
  @Output() changeCalendarOption = new EventEmitter();
  @Output() openSplitAllocationDialog = new EventEmitter();
  @Output() deleteSelectedProjectsConfirmationDialog = new EventEmitter();
  @Output() openUpdateAllocationsDatesDialog = new EventEmitter();
  // --------------------------------View Child References --------------------------------------------//
  @ViewChild('gantt_here', { static: true }) ganttContainer: ElementRef;
  
  // --------------------------------Host Listner --------------------------------------------//
  // Hack: This will hide gantt tootip if mouse moves too fast.
  @HostListener('document:mousemove', ['$event'])
  onMousemove(event: MouseEvent) {
    const borders = {
      left: this.gantt$.$container.parentNode.offsetLeft,
      right: this.gantt$.$container.parentNode.offsetLeft + this.gantt$.$container.clientWidth,
      top: this.gantt$.$container.parentNode.offsetTop,
      bottom: this.gantt$.$container.parentNode.offsetTop + this.gantt$.$container.clientHeight
    };

    if (event.clientX <= borders.left ||
      event.clientX >= borders.right ||
      event.clientY <= borders.top ||
      event.clientY >= borders.bottom
    ) {
      this.gantt$.ext.tooltips.tooltip.hide();
    }
  }

  // --------------------------------Constructor --------------------------------------------//
  constructor(
    private _ganttService: GanttService,
    private _resourceAllocationService: ResourceAllocationService,
    private elemRef: ElementRef,
    private notifyService: NotificationService,
    private _coreService: CoreService) { }

  // --------------------------------Life Cycle Events --------------------------------------------//
  ngOnInit() {

    this.initialiseDatePicker();

    this.subscribeToChangesInResourceCommitments();
    this.getGanttInstance();
    //  this.gantt$.init(this.ganttContainer.nativeElement);
    this.dataProcessor = this.gantt$.createDataProcessor({
      task: {
        update: (data: any) => this.updateCommitment(data),
        create: (data: any) => null,
        delete: id => null,
      },
      link: null
    });

    this.bindAngularEventsToGantt();
    this.onCalendarChange();
    window.addEventListener('scroll', this.hideTooltipOnScroll, true);
  }

  hideTooltipOnScroll = (): void => {
    this.gantt$.ext.tooltips.tooltip.hide();
  }

  onCalendarChange() {
    const selectedCalendarOption = this.calendarOptions.find(Item => Item.value === this.calendarRadioSelected);
    this.setScaleConfig(selectedCalendarOption);
    this.changeCalendarOption.emit(this.calendarRadioSelected);
  }

  ngAfterViewInit() {
    const ganttSortIconElement = document.querySelector('.gantt_sort');
    if (ganttSortIconElement) {
      ganttSortIconElement.classList.remove('gantt_sort');
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next(true);
    this.destroy$.complete();
    this.gantt$.clearAll();
    this.dataProcessor.destructor();
    this.gantt$.ext.inlineEditors.detachEvent('onBeforeSave');
    this.gantt$.ext.inlineEditors.detachEvent('onEditStart');
    this.gantt$.detachEvent('onTaskDblClick');
    this.gantt$.detachEvent('onTaskClick');
    this.gantt$.destructor();
    this.gantt$ = null;
    window.removeEventListener('scroll', this.hideTooltipOnScroll, true);
  }

  exportToHandler(fileExtension) {
    switch (fileExtension) {
      case this.downloadFileTypes.pdf:
        this.exportGanttToPdf(); break;
      case this.downloadFileTypes.png:
        this.exportGanttToPng(); break;
      case this.downloadFileTypes.exl:
        this.exportGanttToExcel(); break;
    }
  }

  exportGanttToPdf() {
    let exportedFileName = 'Resource' + ConstantsMaster.exportFileSettings.fileExtensions.pdf;
    if (this.resource.fullName !== undefined && this.resource.employeeCode !== undefined) {
      exportedFileName = this.resource.fullName + '-' + this.resource.employeeCode +
        ConstantsMaster.exportFileSettings.fileExtensions.pdf;
    }

    // resetting gantt scale for exported file
    if (this.gantt$.getScale().full_width > 1000) {
      this.gantt$.config.scale_unit = 'month';
      this.gantt$.config.subscales = [];
    }
    this.gantt$.exportToPDF({
      name: exportedFileName,
      header: this.customCssForExportedFiles + '<h1>' + ConstantsMaster.exportFileSettings.headerText + '</h1>',
      footer: '<h4>' + ConstantsMaster.exportFileSettings.footerText + '</h4>',
      locale: navigator.language ? navigator.language.split('-')[0] : ConstantsMaster.exportFileSettings.defaultLanguage,
      skin: this.gantt$.skin,
    });
  }

  exportGanttToPng() {
    let exportedFileName = 'Resource' + ConstantsMaster.exportFileSettings.fileExtensions.png;
    if (this.resource.fullName !== undefined && this.resource.employeeCode !== undefined) {
      exportedFileName = this.resource.fullName + '-' +
        this.resource.employeeCode + ConstantsMaster.exportFileSettings.fileExtensions.png;
    }

    // resetting gantt scale for exported file
    if (this.gantt$.getScale().full_width > 1000) {
      this.gantt$.config.scale_unit = 'month';
      this.gantt$.config.subscales = [];
    }
    this.gantt$.exportToPNG({
      name: exportedFileName,
      header: this.customCssForExportedFiles + '<h1>' + ConstantsMaster.exportFileSettings.headerText + '</h1>',
      footer: '<h4>' + ConstantsMaster.exportFileSettings.footerText + '</h4>',
      locale: navigator.language ? navigator.language.split('-')[0] : ConstantsMaster.exportFileSettings.defaultLanguage,
      skin: this.gantt$.skin,
    });
  }

  exportGanttToExcel() {
    // to remove hyperlinks
    this.gantt$.config.columns.map(column => {
      delete column.template;
    });
    this.gantt$.exportToExcel({
      name: this.gantt$.resource.fullName + '-' + this.gantt$.resource.employeeCode + ConstantsMaster.exportFileSettings.fileExtensions.exl,
      cellColors: true
    });
  }

  // -------------------------------- Helper Functions --------------------------------------------//

  subscribeToChangesInResourceCommitments() {

    this._ganttService.resourceCommitments.pipe(takeUntil(this.destroy$)).subscribe(commitments => {
      this.getGanttInstance();
      this.resourceCommitments = commitments;
      this.renderGanttChart();
      // this.gantt$.init(this.ganttContainer.nativeElement);
      if (commitments !== null && !(commitments.length > 0)) {
        this.showEmptyGanttMsg();
      }
      this.gantt$.selectedAllocations = [];
    });

  }

  initialiseDatePicker() {

    this.bsConfig = BS_DEFAULT_CONFIG;

  }

  private showEmptyGanttMsg() {
    const htmlElem = this.elemRef.nativeElement.querySelector('.gantt_data_area');
    if (htmlElem !== null) {
      htmlElem.innerHTML = '';
      htmlElem.innerHTML = '<div class="gantt-empty-grid">' + ConstantsMaster.ganttConstants.emptyTemplateText + '</div>';
    }
  }

  private bindAngularEventsToGantt() {
    this.gantt$.editResourceCommitment = this.editResourceCommitment;
    this.gantt$.openQuickAddForm = this.openQuickAddForm;
    this.gantt$.getInvestmentCategory = this.getInvestmentCategory;
    this.gantt$.resource = this.resource;
    this.gantt$.investmentCategories = this.investmentCategories;
    this.gantt$.openCaseOverlayPopup = this.openCaseOverlayPopup;
    this.gantt$.oldCaseCodeToOpen = this.oldCaseCodeToOpen;
    this.gantt$.splitAllocation = this.splitAllocation;
    this.gantt$.selectedAllocations = this.selectedAllocations;
    this.gantt$.openSplitAllocationDialog = this.openSplitAllocationDialog;
    this.gantt$.updateDeleteAllocations = this.updateDeleteAllocations;
    this.gantt$.openUpdateAllocationsDatesDialog = this.openUpdateAllocationsDatesDialog;
    this.gantt$.getAllocationDataFromGanttTask = this.getAllocationDataFromGanttTask;
    this.gantt$.deleteSelectedProjectsConfirmationDialog = this.deleteSelectedProjectsConfirmationDialog;
    this.gantt$.addContextMenuItem = this.addContextMenuItem;
  }

  private getGanttInstance() {
    if (!this.gantt$) {
      this.gantt$ = Gantt.getGanttInstance();
      this.gantt$.plugins({
        tooltip: true,
        marker: true
      });
      this.ganttResourceService = new GanttResourceService(this.gantt$, this.investmentCategories, this._coreService);
    }
  }

  private renderGanttChart() {
    this.ganttResourceService.overrideGanttDefaultSettings();
    this.gantt$.init(this.ganttContainer.nativeElement);
    if (this.resourceCommitments !== null) {
      this.getCommitmentsForGantt();
    }

    this.gantt$.clearAll();

    this.ganttResourceService.changeColorOrShapeOfTaskByCommitmentType();

    this.gantt$.parse({ data: this.ganttResourceData });

    this.gantt$.showDate(moment(this.commitmentStartDate).subtract(5, 'd'));

    this.setTodayLineInGanttChart();

    this.bindInlineEditorEvents();

  }

  private bindInlineEditorEvents() {
    this.gantt$.ext.inlineEditors.attachEvent('onBeforeEditStart', function (state) {
      if (state.columnName === 'allocation' &&
        document.getElementById(state.id)?.getAttribute('nonEditable')?.toLocaleLowerCase() === 'true') {
        return false;
      }
      return true;
    });
  }

  private setTodayLineInGanttChart() {
    const date_to_str = this.gantt$.date.date_to_str(this.gantt$.config.task_date);
    const markerId = this.gantt$.addMarker({
      start_date: new Date(),
      css: 'today',
      text: 'Now',
      title: date_to_str(new Date())
    });
    this.gantt$.getMarker(markerId);
  }


  getInvestmentCategory(investmentCode) {
    return this.investmentCategories.find(investmentCategory => investmentCategory.investmentCode === investmentCode).investmentName;
  }
  // Functions that will take input as scale name and output gantt chart in corresponding scale. Example  day, month, year, week
  setScaleConfig(selectedCalendarOption) {
    switch (selectedCalendarOption.value) {
      case 'day':
        this.gantt$.config.scale_unit = 'day';
        this.gantt$.config.step = 1;
        this.gantt$.config.date_scale = '%d %M';
        this.gantt$.templates.date_scale = null;

        this.gantt$.config.scale_height = 50;

        this.gantt$.config.subscales = [];
        break;
      case 'week':
        const weekScaleTemplate = function (date) {
          const dateToStr = this.$gantt.date.date_to_str('%d %M');
          const endDate = this.$gantt.date.add(
            this.$gantt.date.add(date, 1, 'week'),
            -1,
            'day'
          );
          return `${dateToStr(date)} - ${dateToStr(endDate)}`;
        };

        this.gantt$.config.scale_unit = 'week';
        this.gantt$.config.step = 1;
        this.gantt$.templates.date_scale = weekScaleTemplate;

        this.gantt$.config.scale_height = 50;

        this.gantt$.config.subscales = [{ unit: 'day', step: 1, date: '%D' }];
        break;
      case 'month':
        this.gantt$.config.scale_unit = 'month';
        this.gantt$.config.date_scale = '%F, %Y';
        this.gantt$.templates.date_scale = null;

        this.gantt$.config.scale_height = 50;

        this.gantt$.config.subscales = [{ unit: 'week', step: 1, date: '%j, %D' }];

        break;
      case 'year':
        this.gantt$.config.scale_unit = 'year';
        this.gantt$.config.step = 1;
        this.gantt$.config.date_scale = '%Y';
        this.gantt$.templates.date_scale = null;

        this.gantt$.config.min_column_width = 50;
        this.gantt$.config.scale_height = 90;

        this.gantt$.config.subscales = [{ unit: 'month', step: 1, date: '%M' }];
        break;
    }

    this.gantt$.render();
    this.updateHeaderCheckboxWhenScaleChanged();
  }

  updateHeaderCheckboxWhenScaleChanged() {
    if (this.gantt$.getTaskCount() > 0) {
      let isAllChecked = true;
      this.gantt$.eachTask(function (task) {
        if (task.parent === 0 && task.isChecked === false && task.editable === true) {
          isAllChecked = false;
        }
      });
      (document.getElementsByClassName('resource-gantt-select-all')[0] as HTMLInputElement).checked = isAllChecked;
    }
  }

  openCaseOverlayPopup(oldCaseCode, pipelineId) {
    this.oldCaseCodeToOpen.emit({ oldCaseCode, pipelineId });
  }

  // -------------------------------- API Calls --------------------------------------------//
  getCommitmentsForGantt() {
    this.ganttResourceData = this.ganttResourceService.getCommitmentsForGantt(this.resourceCommitments, this.resource);
  }

  addResourceCommitment() {

    this.openQuickAddForm.emit({
      commitmentTypeCode: '',
      resourceAllocationData: {} as ResourceAllocation,
      isUpdateModal: false
    });

  }

  editResourceCommitment(commitmentToEdit) {

    const resourceAllocationData: ResourceAllocation = {
      id: commitmentToEdit.id,
      caseName: commitmentToEdit.caseName,
      caseTypeCode: commitmentToEdit.caseTypeCode,
      clientName: commitmentToEdit.clientName,
      oldCaseCode: commitmentToEdit.oldCaseCode,
      employeeCode: commitmentToEdit.employeeCode,
      employeeName: commitmentToEdit.employeeName,
      currentLevelGrade: commitmentToEdit.currentLevelGrade,
      operatingOfficeCode: commitmentToEdit.operatingOfficeCode,
      operatingOfficeAbbreviation: commitmentToEdit.officeAbbreviation,
      serviceLineCode: commitmentToEdit.serviceLineCode,
      serviceLineName: commitmentToEdit.serviceLineName,
      pipelineId: commitmentToEdit.pipelineId,
      opportunityName: commitmentToEdit.opportunityName,
      // empty strings raise foreign key constraint error in Db. Hence sending null if there's no value
      investmentCode: commitmentToEdit.investmentCode || null,
      investmentName: commitmentToEdit.investmentName,
      commitmentTypeReasonCode: commitmentToEdit.commitmentTypeReasonCode || null,
      caseRoleCode: commitmentToEdit.caseRoleCode || null,
      allocation: commitmentToEdit.allocation,
      startDate: DateService.convertDateInBainFormat(commitmentToEdit.start_date),
      endDate: DateService.convertDateInBainFormat(commitmentToEdit.end_date),
      notes: commitmentToEdit.notes,
      caseStartDate: commitmentToEdit.caseStartDate,
      caseEndDate: commitmentToEdit.caseEndDate,
      opportunityStartDate: commitmentToEdit.opportunityStartDate,
      opportunityEndDate: commitmentToEdit.opportunityEndDate,
      lastUpdatedBy: null
    };

    this.openQuickAddForm.emit({
      commitmentTypeCode: commitmentToEdit.commitmentTypeCode,
      resourceAllocationData: resourceAllocationData,
      isUpdateModal: true
    });

  }

  isDirtyGantt(startDate, endDate, allocation, item) {
    for (let i = 0; i < this.ganttResourceData.length; i++) {
      if (this.ganttResourceData[i].id === item.id) {
        if (
          moment(this.ganttResourceData[i].end_date).startOf('day').isSame(endDate)
          && moment(this.ganttResourceData[i].start_date).startOf('day').isSame(startDate)
          && parseInt(this.ganttResourceData[i].allocation, 10) === parseInt(allocation, 10)
        ) {
          return false;
        } else {
          // to prevent multiple calls for same allocaiton details
          this.ganttResourceData[i].end_date = new Date(endDate);
          this.ganttResourceData[i].start_date = new Date(startDate);
          this.ganttResourceData[i].allocation = allocation;
          return true;
        }
      }
    }
  }
  updateCommitment(item) {

    // below conversion needed as gantt gives date in dd-mm-yyyy hh:mm:ss format which on conversion gives invalid date.
    // So when gantt returs 25-05-2020 (25th May) converting it using moment or new date("25-05-2020") returns invalid date
    const startDateParts = item.start_date.split(' ')[0].split('-');
    const startDate = `${startDateParts[2]}-${startDateParts[1]}-${startDateParts[0]}`;
    const endDateParts = item.end_date.split(' ')[0].split('-');
    const endDate = `${endDateParts[2]}-${endDateParts[1]}-${endDateParts[0]}`;
    const allocation = parseInt(item.allocation, 10);

    // Validations
    if (!ValidationService.isAllocationValid(item.allocation)) {
      document.getElementById(item.id).style.color = 'red';
      this.notifyService.showWarning(ValidationService.invalidAllocation);
      return false;
    }

    if (!this.isDirtyGantt(startDate, endDate, allocation, item)) {
      return false;
    }

    if (new Date(endDate) < new Date(startDate)) {
      this.notifyService.showWarning(ValidationService.startDateGreaterThanEndDate);
      return false;
    }

    if (moment(this.resource.startDate).isAfter(startDate, 'day')) {
      this.notifyService.showWarning(ValidationService.employeeJoiningDateGreaterThanStartDate.replace("[joiningDate]", DateService.convertDateInBainFormat(this.resource.startDate)));
      return false;
    }

    const commitmentType = this.commitmentTypes.find(
      ct => ct.commitmentTypeCode === item.commitmentTypeCode
    );
    if (item.commitmentTypeCode !== 'C') {
      const updatedUserCommitment: Commitment = {
        id: item.id,
        employeeCode: this.resource.employeeCode,
        startDate: startDate,
        endDate: endDate,
        notes: item.notes,
        commitmentType: {
          commitmentTypeCode: commitmentType.commitmentTypeCode,
          commitmentTypeName: commitmentType.commitmentTypeName,
          precedence: commitmentType.precedence
        },
        commitmentTypeReasonCode: item.commitmentTypeReasonCode || null,
        lastUpdatedBy: null,
        allocation: allocation
      };

      this.updateResourceCommitment.emit({
        resourceAllocation: updatedUserCommitment,
        event: 'ganttResource'
      });

    } else {

      
      const allExistingGanttAllocations = this._ganttService.resourceCommitments.getValue();
      const existingAllocation = allExistingGanttAllocations.find(x => x.id === item.id);

      const updatedUserAllocationOnCase: ResourceAllocation = {
        id: item.id,
        oldCaseCode: item.oldCaseCode || null,
        caseName: item.caseName,
        clientName: item.clientName,
        caseTypeCode: item.caseTypeCode,
        opportunityName: item.opportunityName,
        pipelineId: item.pipelineId || null,
        employeeCode: this.resource.employeeCode,
        employeeName: this.resource.fullName,
        operatingOfficeCode: this.resource.schedulingOffice.officeCode,
        operatingOfficeAbbreviation: this.resource.schedulingOffice.officeAbbreviation,
        currentLevelGrade: item.levelGrade,
        serviceLineCode: item.serviceLineCode,
        serviceLineName: item.serviceLineName,
        allocation: allocation,
        startDate: DateService.convertDateInBainFormat(startDate),
        endDate: DateService.convertDateInBainFormat(endDate),
        previousStartDate: DateService.convertDateInBainFormat(existingAllocation.startDate),
        previousEndDate: DateService.convertDateInBainFormat(existingAllocation.endDate),
        previousAllocation: existingAllocation.allocation,
        // empty strings raise foreign key constraint error in Db. Hence sending null if there's no value
        investmentCode: item.investmentCode || null,
        investmentName: item.investmentName,
        caseRoleCode: item.caseRoleCode || null,
        notes: item.notes || null,
        caseStartDate: item.caseStartDate,
        caseEndDate: item.caseEndDate,
        opportunityStartDate: item.opportunityStartDate,
        opportunityEndDate: item.opportunityEndDate,
        lastUpdatedBy: null
      };

      const allocationBeforeUpdate: ResourceAllocation = {
        id: existingAllocation.id,
        oldCaseCode: existingAllocation.oldCaseCode || null,
        caseName: existingAllocation.caseName,
        caseTypeCode: existingAllocation.caseTypeCode,
        clientName: existingAllocation.clientName,
        opportunityName: existingAllocation.opportunityName,
        pipelineId: existingAllocation.pipelineId || null,
        employeeCode: this.resource.employeeCode,
        employeeName: this.resource.fullName,
        operatingOfficeCode: this.resource.schedulingOffice.officeCode,
        operatingOfficeAbbreviation: this.resource.schedulingOffice.officeAbbreviation,
        currentLevelGrade: existingAllocation.levelGrade,
        serviceLineCode: existingAllocation.serviceLineCode,
        serviceLineName: existingAllocation.serviceLineName,
        allocation: existingAllocation.allocation,
        startDate: DateService.convertDateInBainFormat(existingAllocation.startDate),
        endDate: DateService.convertDateInBainFormat(existingAllocation.endDate),
        // empty strings raise foreign key constraint error in Db. Hence sending null if there's no value
        investmentCode: existingAllocation.investmentCode || null,
        investmentName: existingAllocation.investmentName,
        caseRoleCode: existingAllocation.caseRoleCode || null,
        notes: existingAllocation.notes || null,
        caseStartDate: existingAllocation.caseStartDate,
        caseEndDate: existingAllocation.caseEndDate,
        opportunityStartDate: existingAllocation.opportunityStartDate,
        opportunityEndDate: existingAllocation.opportunityEndDate,
        lastUpdatedBy: null
      };
      let allocationsToUpdate: ResourceAllocation[] = [];
      let successMessage = null;
      let isValidAllocation = true;
      const currentAllocationDecidingParamsForSplit = {
        allocation: updatedUserAllocationOnCase.allocation,
        investmentCode: updatedUserAllocationOnCase.investmentCode,
        caseRoleCode: updatedUserAllocationOnCase.caseRoleCode,
        startDate: updatedUserAllocationOnCase.startDate,
        endDate: updatedUserAllocationOnCase.endDate
      };

      let [canSplitForMonthClose, errorMessage] = this._resourceAllocationService.canSplitForMonthClose(allocationBeforeUpdate, currentAllocationDecidingParamsForSplit);


      if (canSplitForMonthClose) {
        [allocationsToUpdate, successMessage] = this._resourceAllocationService.splitAlloctionForMonthClose(allocationBeforeUpdate, updatedUserAllocationOnCase)
      } else {
        [isValidAllocation, errorMessage] = this._resourceAllocationService.validateMonthCloseForUpdates(updatedUserAllocationOnCase, allocationBeforeUpdate);
        allocationsToUpdate.push(updatedUserAllocationOnCase);
      }

      if (!isValidAllocation) {
        this.notifyService.showWarning(errorMessage);
        //revert gantt to old state when validation fails as Gantt client does not automatically revrt back the incorrect data
        setTimeout(() => {
          this._ganttService.resourceCommitments.next(allExistingGanttAllocations);
        }, 0);
        return false;
      } else {
        this.checkForPrePostAndUpsertResourceAllocation(allocationsToUpdate, successMessage);
      }

    }
  }

  checkForPrePostAndUpsertResourceAllocation(resourceAllocations: ResourceAllocation[], successMessage?) {

    const projectStartDate = DateService.convertDateInBainFormat(resourceAllocations[0].oldCaseCode
      ? resourceAllocations[0].caseStartDate
      : resourceAllocations[0].opportunityStartDate);

    const projectEndDate = DateService.convertDateInBainFormat(resourceAllocations[0].oldCaseCode
      ? resourceAllocations[0].caseEndDate
      : resourceAllocations[0].opportunityEndDate);
    let allocationsData: ResourceAllocation[] = [];

    if (projectStartDate && projectEndDate) {

      allocationsData = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocations);

    } else {

      allocationsData = allocationsData.concat(resourceAllocations);

    }

    this.upsertResourceAllocationsToProject.emit({
      resourceAllocation: allocationsData,
      event: 'ganttResource',
      splitSuccessMessage: successMessage
    });

  }

  /**DatePicker Methods */
  getCommitmentForSelectedDate(selectedDate) {
    if (!selectedDate || !ValidationService.validateDate(selectedDate).isValid) {
      selectedDate = new Date();
    } else if (selectedDate < this.minDate) {
      selectedDate = this.minDate;
    }
    this.commitmentStartDate = selectedDate;
    this.getCommitmentForEmployeeOnOrAfterSelectedDate.emit({ selectedDate: selectedDate });
  }

  splitAllocation(data) {
    this.openSplitAllocationDialog.emit({
      allocation: data.allocation,
      caseEndDate: data.caseEndDate,
      caseName: data.caseName,
      caseRoleCode: data.caseRoleCode,
      caseStartDate: data.caseStartDate,
      caseTypeCode: data.caseTypeCode,
      clientName: data.clientName,
      currentLevelGrade: data.levelGrade,
      employeeCode: data.employeeCode,
      employeeName: data.employeeName,
      endDate: moment(data.end_date).format('DD-MMM-YYYY'),
      id: data.id,
      internetAddress: data.internetAddress,
      investmentCode: data.investmentCode,
      investmentName: data.investmentName,
      lastUpdatedBy: data.lastUpdatedBy,
      notes: data.notes,
      oldCaseCode: data.oldCaseCode,
      operatingOfficeAbbreviation: data.operatingOfficeAbbreviation,
      operatingOfficeCode: data.operatingOfficeCode,
      opportunityEndDate: data.opportunityEndDate,
      opportunityName: data.opportunityName,
      opportunityStartDate: data.opportunityStartDate,
      pipelineId: data.pipelineId,
      primaryCapability: data.primaryCapability,
      primaryIndustry: data.primaryIndustry,
      probabilityPercent: data.probabilityPercent,
      serviceLineCode: data.serviceLineCode,
      serviceLineName: data.serviceLineName,
      startDate: moment(data.start_date).format('DD-MMM-YYYY'),
      terminationDate: data.terminationDate,
      joiningDate: data.joiningDate
    });
  }

  updateDeleteAllocations(selectedAllocations, selectedOption) {
    const selectedAllocationsFromGanttTask = this.getAllocationDataFromGanttTask(selectedAllocations);
    if (selectedOption === 'Delete') {
      this.deleteSelectedProjectsConfirmationDialog.emit(selectedAllocationsFromGanttTask);
    } else {
      this.openUpdateAllocationsDatesDialog.emit({ selectedOption: selectedOption, selectedAllocations: selectedAllocationsFromGanttTask });
    }
  }

  private getAllocationDataFromGanttTask(data) {
    return data.map(r => {
      return {
        allocation: r.allocation,
        caseEndDate: r.caseEndDate,
        caseName: r.caseName,
        caseRoleCode: r.caseRoleCode,
        caseStartDate: r.caseStartDate,
        caseTypeCode: r.caseTypeCode,
        clientName: r.clientName,
        currentLevelGrade: r.levelGrade,
        employeeCode: r.employeeCode,
        employeeName: r.employeeName,
        endDate: moment(r.end_date).format('DD-MMM-YYYY'),
        id: r.id,
        internetAddress: r.internetAddress,
        investmentCode: r.investmentCode,
        investmentName: r.investmentName,
        lastUpdatedBy: r.lastUpdatedBy,
        notes: r.notes,
        oldCaseCode: r.oldCaseCode,
        operatingOfficeAbbreviation: r.operatingOfficeAbbreviation,
        operatingOfficeCode: r.operatingOfficeCode,
        opportunityEndDate: r.opportunityEndDate,
        opportunityName: r.opportunityName,
        opportunityStartDate: r.opportunityStartDate,
        pipelineId: r.pipelineId,
        primaryCapability: r.primaryCapability,
        primaryIndustry: r.primaryIndustry,
        probabilityPercent: r.probabilityPercent,
        serviceLineCode: r.serviceLineCode,
        serviceLineName: r.serviceLineName,
        startDate: moment(r.start_date).format('DD-MMM-YYYY'),
        terminationDate: r.terminationDate,
        type: r.type,
        joiningDate: r.joiningDate
      };
    });
  }

  addContextMenuItem(title, selectedAllocations, value) {
    const startDateLi = document.createElement('li');
    startDateLi.setAttribute('value', value);
    if (value === 'Split') {
      if (selectedAllocations.length > 1 ||
        moment(selectedAllocations[0].start_date).format('DD-MMM-YYYY') === moment(selectedAllocations[0].end_date).format('DD-MMM-YYYY')) {
        startDateLi.style.color = '#bababa';
        startDateLi.title =
          selectedAllocations.length > 1 ? 'Cannot split multiple allocations at once' : 'Cannot split one day allocation';
      } else if (selectedAllocations.every(r => r.type.toLowerCase() !== 'case' && r.type.toLowerCase() !== 'opportunity')) {
        startDateLi.style.color = '#bababa';
        startDateLi.title = 'Commitments are not allowed to be Split.';
      } else {
        startDateLi.addEventListener('click', () => {
          this.splitAllocation(selectedAllocations[0]);
          document.getElementById('ganttResourceContextMenuId').innerHTML = '';
        });
      }
    } else {
      if (value === 'Delete' || selectedAllocations.every(r => r.type.toLowerCase() === 'case' || r.type.toLowerCase() === 'opportunity')) {
        startDateLi.addEventListener('click', (e) => {
          const clickedOption = e.target as HTMLElement;
          this.updateDeleteAllocations(selectedAllocations, clickedOption.getAttribute('value'));
          document.getElementById('ganttResourceContextMenuId').innerHTML = '';
        });
      } else {
        startDateLi.style.color = '#bababa';
        startDateLi.title = 'Commitments cannot be Updated';
      }
    }
    const startDateTextNode = document.createTextNode(title);
    startDateLi.appendChild(startDateTextNode);
    document.getElementById('gantt-resource-context-menu-ul').appendChild(startDateLi);
  }
}
