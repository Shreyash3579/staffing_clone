// --------------------------------Angular References--------------------------------------------
import {
    Component, ElementRef, OnInit, ViewChild, ViewEncapsulation, Input, EventEmitter,
    Output, AfterViewInit, OnDestroy, HostListener
  } from '@angular/core';
  import { takeUntil } from 'rxjs/operators';
  import { Subject } from 'rxjs';
  
  // --------------------------------Gantt/Third Party References--------------------------------------------
  import 'dhtmlx-gantt';
  import 'dhtmlx-gantt/gantt-api.js';
  import * as moment from 'moment';
  
  // --------------------------------Interfaces--------------------------------------------
  import { CaseRoleType } from 'src/app/shared/interfaces/caseRoleType.interface';
  import { InvestmentCategory } from 'src/app/shared/interfaces/investmentCateogry.interface';
  import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
  import { ResourceCommitment } from 'src/app/shared/interfaces/resourceCommitment';
  
  // --------------------------------Services--------------------------------------------
  import { GanttService } from '../gantt.service';
  
  /// --------------------------------Third Party References--------------------------------------------
  import { DateService } from 'src/app/shared/dateService';
  import { ValidationService } from 'src/app/shared/validationService';
  
  /// --------------------------------Constants----------------------------------------//
  import { BS_DEFAULT_CONFIG } from 'src/app/shared/constants/bsDatePickerConfig';
  import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
  import { CommitmentsService } from './../../../shared/commitments.service';
  
  /// --------------------------------Shared References--------------------------------------------
  import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';
  import { Gantt } from 'dhtmlx-gantt';
  import { NotificationService } from '../../../shared/notification.service';
  import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
  import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
  import { GanttPlanningCardService } from './gantt-planning-card.service';
  
  
  @Component({
    encapsulation: ViewEncapsulation.None,
    selector: 'app-gantt-planning-card',
    templateUrl: './gantt-planning-card.component.html',
    styleUrls: ['./gantt-planning-card.component.scss']
  })
  export class GanttPlanningCardComponent implements OnInit, AfterViewInit, OnDestroy {
    // ----------------------- Local Variables --------------------------------------------//
    destroy$: Subject<boolean> = new Subject<boolean>();
    public selectedAllocations = [];
    public ganttAssignedResources = [];
    public scaleConfig = 'month';
    public userAllocation: ResourceAllocation[];
    private dataProcessor;
    private ganttCase$;
    private initialSelectedDate: Date = new Date();
    public resourcesAllCommitments: ResourceCommitment;
    public downloadFileTypes = {
      pdf: ConstantsMaster.exportFileSettings.fileType.pdf,
      png: ConstantsMaster.exportFileSettings.fileType.png,
      exl: ConstantsMaster.exportFileSettings.fileType.exl,
    };
    public calendarOptions = ConstantsMaster.ganttCalendarScaleOptions.options;
    readonly customCssForExportedFiles = ConstantsMaster.exportFileSettings.gantttCssStyles;
    bsConfig: Partial<BsDatepickerConfig>;
    minDate = new Date(ConstantsMaster.datePickerSettings.minDate);
    clickTypeIndicator: any;
    employeeCode: any;
    showCommitments: boolean;
    private ganttPlanningCardService: GanttPlanningCardService;
    accessibleFeatures = ConstantsMaster.appScreens.feature;

    // --------------------------------Input Events --------------------------------------------//
    @Input() investmentCategories: InvestmentCategory[];
    @Input() caseRoleTypes: CaseRoleType[];
    @Input() activeResourcesStartDate: Date = new Date();
    @Input() calendarRadioSelected: string;
    @Input() isGanttCalendarReadOnly: boolean = false;

    // --------------------------------Output Events --------------------------------------------//
    @Output() updateResourceToProject = new EventEmitter();
    @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();
    @Output() upsertPlaceholderAllocationsToProject = new EventEmitter<any>();
    @Output() getActiveResourcesForProjectOnOrAfterSelectedDate = new EventEmitter();
    @Output() resourceCodeToOpen = new EventEmitter();
    @Output() changeCalendarOption = new EventEmitter();
    @Output() openQuickAddForm = new EventEmitter<any>();
    @Output() openPlaceholderForm = new EventEmitter<any>();
    @Output() openSplitAllocationDialog = new EventEmitter();
    @Output() deleteSelectedProjectsConfirmationDialog = new EventEmitter();
    @Output() deleteSelectedPlaceholdersConfirmationDialog = new EventEmitter();
    @Output() openUpdateAllocationsDatesDialog = new EventEmitter();
    @Output() openUpdatePlaceholdersDatesDialog = new EventEmitter();
  
    // --------------------------------View Child References --------------------------------------------//
    @ViewChild('gantt_here', { static: true }) ganttContainer: ElementRef;
  
    // --------------------------------Host Listner --------------------------------------------//
    // Hack: This will hide gantt tootip if mouse moves too fast.
    @HostListener('document:mousemove', ['$event'])
    onMousemove(event: MouseEvent) {
      const borders = {
        left: this.ganttCase$.$container.parentNode.offsetLeft,
        right: this.ganttCase$.$container.parentNode.offsetLeft + this.ganttCase$.$container.clientWidth,
        top: this.ganttCase$.$container.parentNode.offsetTop,
        bottom: this.ganttCase$.$container.parentNode.offsetTop + this.ganttCase$.$container.clientHeight
      };
  
      if (event.clientX <= borders.left ||
        event.clientX >= borders.right ||
        event.clientY <= borders.top ||
        event.clientY >= borders.bottom
      ) {
        this.ganttCase$.ext.tooltips.tooltip.hide();
      }
    }
  
    // --------------------------------Constructor --------------------------------------------//
  
    constructor(
      private _ganttService: GanttService,
      private _resourceAllocationService: ResourceAllocationService,
      private _commitmentsService: CommitmentsService,
      private elemRef: ElementRef,
      private notifyService: NotificationService) { }
  
    // --------------------------------Life Cycle Events --------------------------------------------//
  
    ngOnInit() {
  
      this.initialiseDatePicker();
  
      this.subscribeToChangesInAllocationData();
      this.getGanttInstance();
      // this.ganttCase$.init(this.ganttContainer.nativeElement);
      this.dataProcessor = this.ganttCase$.createDataProcessor({
        task: {
          update: (data: any) => this.updateResource(data),
          create: (data: any) => null,
          delete: (id) => null
        },
        link: null
      });
      this.bindAngularEventsToGantt();
  
      // new property added to prevent making unnecessary vacation api call when date is unchanged
      this.initialSelectedDate = this.activeResourcesStartDate;
      this.onCalendarChange();
      window.addEventListener('scroll', this.hideTooltipOnScroll, true);
    }
  
    ngAfterViewInit() {
      const ganttSortIconElement = document.querySelector('.gantt_sort');
      if (ganttSortIconElement) {
        ganttSortIconElement.classList.remove('gantt_sort');
      }
    }
  
    ngOnDestroy(): void {
      this.destroy$.next(true);
      this.destroy$.unsubscribe();
      this.ganttCase$.clearAll();
      this.dataProcessor.destructor();
      this.ganttCase$.ext.inlineEditors.detachEvent('onBeforeSave');
      this.ganttCase$.ext.inlineEditors.detachEvent('onEditStart');
      this.ganttCase$.detachEvent('onTaskDblClick');
      this.ganttCase$.detachEvent('onTaskClick');
      this.ganttCase$.destructor();
      this.ganttCase$ = null;
      this.showCommitments = false;
      window.removeEventListener('scroll', this.hideTooltipOnScroll, true);
    }
  
    hideTooltipOnScroll = (): void => {
      this.ganttCase$.ext.tooltips.tooltip.hide();
    }
  
    // --------------------------Helper Functions--------------------------------//
  
    subscribeToChangesInAllocationData() {
  
      this._ganttService.resourceAndPlaceholderAllocations.pipe(takeUntil(this.destroy$)).subscribe(allocations => {
        this.getGanttInstance();
        var resourceAllocations = allocations;
  
        if (resourceAllocations?.length > 0) {
          resourceAllocations = this.ganttPlanningCardService.sortGridColumns(resourceAllocations);
        }

        
        this.userAllocation = resourceAllocations;
  
        this.renderGanttChart();
        if (resourceAllocations !== null && !(resourceAllocations?.length > 0)) {
          this.showEmptyGanttMsg();
        }
        this.ganttCase$.selectedAllocations = [];
      });
  
    }
  
    initialiseDatePicker() {
  
      this.bsConfig = BS_DEFAULT_CONFIG;
  
    }
  
    onCalendarChange() {
      const selectedCalendarOption = this.calendarOptions.find(Item => Item.value === this.calendarRadioSelected);
      this.setScaleConfig(selectedCalendarOption);
      this.changeCalendarOption.emit(this.calendarRadioSelected);
    }
  
    showCommitmentsForResources() {
      if (this.showCommitments && this.userAllocation !== null &&
        (!this.resourcesAllCommitments || this.initialSelectedDate !== this.activeResourcesStartDate)) {
  
        const distinctEmployeeCodes = this.gettUniqueEmployeeCodesAsString(this.userAllocation);
        const startDate = this.activeResourcesStartDate;
        const endDate = null; //get all data active on or after start date
        const commitmentTypesToShowInGantt = (new Array(ConstantsMaster.CommitmentType.Loa.toLowerCase(),
          ConstantsMaster.CommitmentType.Vacation.toLowerCase(),
          ConstantsMaster.CommitmentType.Training.toLowerCase())).join(',');
  
        this.getResourcesCommitmentsWithinDateRangeByCommitmentTypes(distinctEmployeeCodes, startDate, endDate, commitmentTypesToShowInGantt);
  
      } else {
        //remove commitments from Gantt by rendering it again
        this.renderGanttChart();
      }
    }
  
    getResourcesCommitmentsWithinDateRangeByCommitmentTypes(employeeCodes: string, startDate: Date, endDate: Date, commitmentTypeCodes: string) {
  
      this._commitmentsService.getResourcesCommitmentsWithinDateRangeByCommitmentTypes(employeeCodes, startDate, endDate, commitmentTypeCodes).subscribe(employeeAllCommitments => {
        this.resourcesAllCommitments = employeeAllCommitments;
        this.initialSelectedDate = this.activeResourcesStartDate;
  
        this.renderGanttChart();
      });
  
    }
  
    openResourceOverlayPopup(resourceCode) {
      this.resourceCodeToOpen.emit({ resourceCode: resourceCode });
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
      let exportedFileName = 'Case' + ConstantsMaster.exportFileSettings.fileExtensions.pdf;
      if (this.userAllocation[0].oldCaseCode) {
        exportedFileName = (this.userAllocation[0]).caseName + '-' + (this.userAllocation[0]).oldCaseCode +
          ConstantsMaster.exportFileSettings.fileExtensions.pdf;
      } else if(this.userAllocation[0].pipelineId) {
        exportedFileName = (this.userAllocation[0]).opportunityName + ConstantsMaster.exportFileSettings.fileExtensions.pdf;
      }
      else{
        exportedFileName = (this.userAllocation[0]).planningCardTitle + ConstantsMaster.exportFileSettings.fileExtensions.pdf;
      }
  
      // resetting gantt scale for exported file
      if (this.ganttCase$.getScale().full_width > 1000) {
        this.ganttCase$.config.scale_unit = 'month';
        this.ganttCase$.config.subscales = [];
      }
  
      this.ganttCase$.exportToPDF({
        name: exportedFileName,
        header: this.customCssForExportedFiles + '<h1>' + ConstantsMaster.exportFileSettings.headerText + '</h1>',
        footer: '<h4>' + ConstantsMaster.exportFileSettings.footerText + '</h4>',
        locale: navigator.language ? navigator.language.split('-')[0] : ConstantsMaster.exportFileSettings.defaultLanguage,
        skin: this.ganttCase$.skin,
      });
    }
  
    exportGanttToPng() {
      let exportedFileName = 'Case' + ConstantsMaster.exportFileSettings.fileExtensions.png;
      if (this.userAllocation[0].oldCaseCode) {
        exportedFileName = (this.userAllocation[0]).caseName + '-' + (this.userAllocation[0]).oldCaseCode +
          ConstantsMaster.exportFileSettings.fileExtensions.png;
      } else if(this.userAllocation[0].pipelineId) {
        exportedFileName = (this.userAllocation[0]).opportunityName + ConstantsMaster.exportFileSettings.fileExtensions.png;
      }
      else{
        exportedFileName = (this.userAllocation[0]).planningCardTitle + ConstantsMaster.exportFileSettings.fileExtensions.png;
      }
  
      // resetting gantt scale for exported file
      if (this.ganttCase$.getScale().full_width > 1000) {
        this.ganttCase$.config.scale_unit = 'month';
        this.ganttCase$.config.subscales = [];
      }
  
      this.ganttCase$.exportToPNG({
        name: exportedFileName,
        header: this.customCssForExportedFiles + '<h1>' + ConstantsMaster.exportFileSettings.headerText + '</h1>',
        footer: '<h4>' + ConstantsMaster.exportFileSettings.footerText + '</h4>',
        locale: navigator.language ? navigator.language.split('-')[0] : ConstantsMaster.exportFileSettings.defaultLanguage,
        skin: this.ganttCase$.skin,
      });
    }
  
    exportGanttToExcel() {
      // to remove hyperlinks
      this.ganttCase$.config.columns.map(column => {
        delete column.template;
      });
      this.ganttCase$.exportToExcel({
        name: (this.userAllocation[0]).caseName + '-' + (this.userAllocation[0]).oldCaseCode +
          ConstantsMaster.exportFileSettings.fileExtensions.exl,
        cellColors: true
      });
    }
  
    // -------------------------------- Helper Functions --------------------------------------------//
  
    private gettUniqueEmployeeCodesAsString(resourceArray): string {
      const distinctEmployeeCodes = resourceArray.map(e => e.employeeCode);
      const employeeCodes = distinctEmployeeCodes?.filter((item, i, ar) => ar.indexOf(item) === i).join(',');
      return employeeCodes;
    }
  
    private bindAngularEventsToGantt() {
      this.ganttCase$.editResource = this.editResource;
      this.ganttCase$.openQuickAddForm = this.openQuickAddForm;
      this.ganttCase$.openPlaceholderForm = this.openPlaceholderForm;
      this.ganttCase$.openResourceOverlayPopup = this.openResourceOverlayPopup;
      this.ganttCase$.resourceCodeToOpen = this.resourceCodeToOpen;
      this.ganttCase$.getInvestmentCategory = this.ganttPlanningCardService.getInvestmentCategory;
      this.ganttCase$.splitAllocation = this.splitAllocation;
      this.ganttCase$.openSplitAllocationDialog = this.openSplitAllocationDialog;
      this.ganttCase$.updateDeleteAllocations = this.updateDeleteAllocations;
      this.ganttCase$.addContextMenuItem = this.addContextMenuItem;
      this.ganttCase$.selectedAllocations = this.selectedAllocations;
      this.ganttCase$.getAllocationDataFromGanttTask = this.getAllocationDataFromGanttTask;
      this.ganttCase$.openUpdateAllocationsDatesDialog = this.openUpdateAllocationsDatesDialog;
      this.ganttCase$.openUpdatePlaceholdersDatesDialog = this.openUpdatePlaceholdersDatesDialog;
      this.ganttCase$.deleteSelectedProjectsConfirmationDialog = this.deleteSelectedProjectsConfirmationDialog;
      this.ganttCase$.deleteSelectedPlaceholdersConfirmationDialog = this.deleteSelectedPlaceholdersConfirmationDialog;
    }
  
    private showEmptyGanttMsg() {
      const htmlElem = this.elemRef.nativeElement.querySelector('.gantt_data_area');
      if (htmlElem !== null) {
        htmlElem.innerHTML = '';
        htmlElem.innerHTML = '<div class="gantt-empty-grid">' + ConstantsMaster.ganttConstants.emptyTemplateText + '</div>';
      }
    }
  
    private getGanttInstance() {
      if (!this.ganttCase$) {
        this.ganttCase$ = Gantt.getGanttInstance();
        this.ganttCase$.plugins({
          tooltip: true,
          marker: true,
          grouping: true
        });
        this.ganttPlanningCardService = new GanttPlanningCardService(this.investmentCategories, this.ganttCase$, this.isGanttCalendarReadOnly);
      }
    }
  
    private renderGanttChart() {
      this.ganttPlanningCardService.overrideGanttDefaultSettings();
  
      this.ganttCase$.init(this.ganttContainer.nativeElement);
      this.getResourceAllocationsAndCommitmentsForGantt();
  
      this.ganttCase$.clearAll();
  
      this.ganttPlanningCardService.configureTimeLineColor();
  
      this.ganttCase$.parse({ data: this.ganttAssignedResources });
      this.ganttCase$.showDate(moment(new Date(this.activeResourcesStartDate)).subtract(5, 'd'));
  
      this.ganttPlanningCardService.setTodayLineInGanttChart();
  
      // Split Task and Grouping can not be applied simultaneously
      // Show grouping when show commitments is unchecekd and have placeholder (unnamed) allocations
      if (!this.showCommitments &&
        this.ganttAssignedResources.some(x => x.isPlaceholderAllocation) &&
        this.ganttAssignedResources.some(x => !x.isPlaceholderAllocation)) {
        this.ganttPlanningCardService.configureTaskGrouping(this.ganttAssignedResources.some(x => x.isPlaceholderAllocation));
      }
    }
  
    // Functions that will take input as scale name and output gantt chart in corresponding scale. Example  day, month, year, week
    setScaleConfig(selectedCalendarOption) {
      switch (selectedCalendarOption.value) {
        case 'day':
          this.ganttCase$.config.scale_unit = 'day';
          this.ganttCase$.config.step = 1;
          this.ganttCase$.config.date_scale = '%d %M';
          this.ganttCase$.templates.date_scale = null;
  
          this.ganttCase$.config.scale_height = 50;
  
          this.ganttCase$.config.subscales = [];
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
  
          this.ganttCase$.config.scale_unit = 'week';
          this.ganttCase$.config.step = 1;
          this.ganttCase$.templates.date_scale = weekScaleTemplate;
  
          this.ganttCase$.config.scale_height = 50;
  
          this.ganttCase$.config.subscales = [{ unit: 'day', step: 1, date: '%D' }];
          break;
        case 'month':
          this.ganttCase$.config.scale_unit = 'month';
          this.ganttCase$.config.date_scale = '%F, %Y';
          this.ganttCase$.templates.date_scale = null;
  
          this.ganttCase$.config.scale_height = 50;
  
          this.ganttCase$.config.subscales = [{ unit: 'week', step: 1, date: '%j, %D' }];
  
          break;
        case 'year':
          this.ganttCase$.config.scale_unit = 'year';
          this.ganttCase$.config.step = 1;
          this.ganttCase$.config.date_scale = '%Y';
          this.ganttCase$.templates.date_scale = null;
  
          this.ganttCase$.config.min_column_width = 50;
          this.ganttCase$.config.scale_height = 90;
  
          this.ganttCase$.config.subscales = [{ unit: 'month', step: 1, date: '%M' }];
          break;
      }
      this.ganttCase$.render();
      this.ganttPlanningCardService.updateHeaderCheckboxWhenScaleChanged();
    }
  
    // -------------------------------- API Calls --------------------------------------------//
  
    getResourceAllocationsAndCommitmentsForGantt() {
      this.ganttAssignedResources = this.ganttPlanningCardService.convertResourceAllocationsInGanttFormat(this.userAllocation);
  
      if (this.showCommitments && this.resourcesAllCommitments) {
        const convertedAllCommitmentsForResources = this.ganttPlanningCardService.convertResourcesCommitmentsInGanttFormat(this.resourcesAllCommitments, this.userAllocation?.filter(x => !x.isPlaceholderAllocation));
        this.ganttAssignedResources = this.ganttAssignedResources?.filter(x => !x.isPlaceholderAllocation);
        this.ganttAssignedResources = this.ganttAssignedResources?.concat(convertedAllCommitmentsForResources);
      }
  
    }
  
    addResource() {
  
      this.openQuickAddForm.emit({
        commitmentTypeCode: 'C',
        resourceAllocationData: {} as ResourceAllocation,
        isUpdateModal: false
      });
  
    }
  
    editResource(resourceToEdit) {
      const resourceAllocationData: ResourceAllocation = {
        id: resourceToEdit.id,
        caseName: resourceToEdit.caseName,
        clientName: resourceToEdit.clientName,
        oldCaseCode: resourceToEdit.oldCaseCode,
        caseTypeCode: resourceToEdit.caseTypeCode,
        employeeCode: resourceToEdit.employeeCode,
        employeeName: resourceToEdit.employeeName,
        currentLevelGrade: resourceToEdit.currentLevelGrade,
        operatingOfficeCode: resourceToEdit.operatingOfficeCode,
        operatingOfficeAbbreviation: resourceToEdit.operatingOfficeAbbreviation,
        pipelineId: resourceToEdit.pipelineId,
        planningCardId: resourceToEdit.planningCardId,
        planningCardTitle: resourceToEdit.planningCardTitle,
        opportunityName: resourceToEdit.opportunityName,
        investmentCode: resourceToEdit.investmentCode || null, // empty strings raise foreign key constraint error in Db. Hence sending null if there's no value
        investmentName: resourceToEdit.investmentName,
        caseRoleCode: resourceToEdit.caseRoleCode || null,
        allocation: resourceToEdit.allocation,
        serviceLineName: resourceToEdit.serviceLineName,
        serviceLineCode: resourceToEdit.serviceLineCode,
        startDate: DateService.convertDateInBainFormat(resourceToEdit.start_date),
        endDate: DateService.convertDateInBainFormat(resourceToEdit.end_date),
        notes: resourceToEdit.notes,
        caseStartDate: resourceToEdit.caseStartDate,
        caseEndDate: resourceToEdit.caseEndDate,
        opportunityStartDate: resourceToEdit.opportunityStartDate,
        opportunityEndDate: resourceToEdit.opportunityEndDate,
        lastUpdatedBy: null,
        joiningDate: resourceToEdit.joiningDate,
        commitmentTypeCode: resourceToEdit.commitmentTypeCode,
        commitmentTypeName: resourceToEdit.commitmentTypeName,
        isPlaceholderAllocation: resourceToEdit.isPlaceholderAllocation,
        positionGroupCode: resourceToEdit.positionGroupCode
      };

    if (resourceAllocationData.isPlaceholderAllocation) {
      this.openPlaceholderForm.emit({
        placeholderAllocationData: resourceAllocationData,
        isUpdateModal: true
      });
    } 
    else 
    {
      this.openQuickAddForm.emit({
        commitmentTypeCode: 'PC',
        resourceAllocationData: resourceAllocationData,
        isUpdateModal: true
      });

    }
  };

  
    isDirtyGantt(startDate, endDate, item) {
      for (let i = 0; i < this.ganttAssignedResources.length; i++) {
        if (this.ganttAssignedResources[i].id === item.id) {
          if (
            moment(this.ganttAssignedResources[i].end_date).startOf('day').isSame(endDate)
            && moment(this.ganttAssignedResources[i].start_date).startOf('day').isSame(startDate)
          ) {
            return false;
          } else {
            // to prevent multiple calls for same allocaiton details
            this.ganttAssignedResources[i].end_date = new Date(endDate);
            this.ganttAssignedResources[i].start_date = new Date(startDate);
            return true;
          }
        }
      }
    }
  
    updateResource(item) {
      // below conversion needed as gantt gives date in dd-mm-yyyy hh:mm:ss format which on conversion gives invalid date.
      // So when gantt returs 25-05-2020 (25th May) converting it using moment or new date("25-05-2020") returns invalid date
      const startDateParts = item.start_date.split(' ')[0].split('-');
      const startDate = `${startDateParts[2]}-${startDateParts[1]}-${startDateParts[0]}`;
      const endDateParts = item.end_date.split(' ')[0].split('-');
      const endDate = `${endDateParts[2]}-${endDateParts[1]}-${endDateParts[0]}`;
  
      if (!this.isDirtyGantt(startDate, endDate, item)) {
        return false;
      }
      // Validation
      if (new Date(endDate) < new Date(startDate)) {
        this.notifyService.showWarning(ValidationService.startDateGreaterThanEndDate);
        return false;
      } else {
         this.updatePlaceholderAllocation(item, startDate, endDate);
      }
    }
  
    updatePlaceholderAllocation(item, startDate, endDate) {
      if (moment(item.joiningDate).isAfter(startDate, 'day')) {
        this.notifyService.showWarning(ValidationService.employeeJoiningDateGreaterThanStartDate.replace("[joiningDate]", DateService.convertDateInBainFormat(item.joiningDate)));
        return false;
      }
      const updatedPlaceholderAssignment: PlaceholderAllocation = {
        id: item.id,
        oldCaseCode: item.oldCaseCode || null,
        caseName: item.caseName,
        clientName: item.clientName,
        caseTypeCode:item.caseTypeCode,
        opportunityName: item.opportunityName,
        pipelineId: item.pipelineId || null,
        planningCardId: item.planningCardId,
        planningCardTitle: item.planningCardTitle,
        employeeCode: item.employeeCode.trim() || null,
        employeeName: item.text,
        operatingOfficeCode: item.operatingOfficeCode,
        operatingOfficeAbbreviation: item.operatingOfficeAbbreviation,
        currentLevelGrade: item.currentLevelGrade.trim(),
        serviceLineCode: item.serviceLineCode,
        serviceLineName: item.serviceLineName,
        allocation: item.allocation,
        startDate: DateService.convertDateInBainFormat(startDate),
        endDate: DateService.convertDateInBainFormat(endDate),
        // empty strings raise foreign key constraint error in Db. Hence sending null if there's no value
        investmentCode: item.investmentCode || null,
        investmentName: item.investmentName,
        caseRoleCode: item.caseRoleCode || null,
        notes: item.notes || null,
        caseStartDate: item.caseStartDate,
        caseEndDate: item.caseEndDate,
        opportunityStartDate: item.opportunityStartDate,
        opportunityEndDate: item.opportunityEndDate,
        isPlaceholderAllocation: item.isPlaceholderAllocation,
        commitmentTypeCode: item.commitmentTypeCode,
        commitmentTypeName: item.commitmentTypeName,
        positionGroupCode: item.positionGroupCode,
        lastUpdatedBy: null,
        joiningDate: item.joiningDate
      };
  
      this.upsertPlaceholderAllocationsToProject.emit({
        placeholderAllocations: [].concat(updatedPlaceholderAssignment),
        event: 'ganttCase'
      });
    }
  
    checkForPrePostAndUpsertResourceAllocation(resourceAllocation: ResourceAllocation) {
  
      const projectStartDate = DateService.convertDateInBainFormat(
        resourceAllocation.oldCaseCode ? resourceAllocation.caseStartDate : resourceAllocation.opportunityStartDate);
      const projectEndDate = DateService.convertDateInBainFormat(
        resourceAllocation.oldCaseCode ? resourceAllocation.caseEndDate : resourceAllocation.opportunityEndDate);
      let allocationsData: ResourceAllocation[] = [];
  
      if (projectStartDate && projectEndDate) {
  
        allocationsData = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocation);
  
      } else {
  
        allocationsData.push(resourceAllocation);
  
      }
  
      this.upsertResourceAllocationsToProject.emit({
        resourceAllocation: allocationsData,
        event: 'ganttCase'
      });
  
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
          currentLevelGrade: r.currentLevelGrade,
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
          planningCardId: r.planningCardId,
          planningCardTitle: r.planningCardTitle,
          primaryCapability: r.primaryCapability,
          primaryIndustry: r.primaryIndustry,
          probabilityPercent: r.probabilityPercent,
          serviceLineCode: r.serviceLineCode,
          serviceLineName: r.serviceLineName,
          startDate: moment(r.start_date).format('DD-MMM-YYYY'),
          terminationDate: r.terminationDate,
          joiningDate: r.joiningDate,
          isPlaceholderAllocation: r.isPlaceholderAllocation,
          commitmentTypeCode: r.commitmentTypeCode,
          positionGroupCode: r.positionGroupCode
        };
      });
    }
  
    /**DatePicker Methods */
    getActiveResourcesForSelectedDate(selectedDate) {
      if (!selectedDate || !ValidationService.validateDate(selectedDate).isValid) {
        selectedDate = new Date();
      } else if (selectedDate < this.minDate) {
        selectedDate = this.minDate;
      }
      this.getActiveResourcesForProjectOnOrAfterSelectedDate.emit({ selectedDate: selectedDate });
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
        currentLevelGrade: data.currentLevelGrade,
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
        planningCardId: data.planningCardId,
        planningCardTitle: data.planningCardTitle,
        isPlaceholderAllocation: data.isPlaceholderAllocation,
        primaryCapability: data.primaryCapability,
        primaryIndustry: data.primaryIndustry,
        probabilityPercent: data.probabilityPercent,
        serviceLineCode: data.serviceLineCode,
        serviceLineName: data.serviceLineName,
        startDate: moment(data.start_date).format('DD-MMM-YYYY'),
        terminationDate: data.terminationDate
      });
    }
  
    updateDeleteAllocations(selectedAllocations, selectedOption) {
      const selectedAllocationsFromGanttTask = this.getAllocationDataFromGanttTask(selectedAllocations);

        if (selectedOption === 'Delete') {
          this.deleteSelectedPlaceholdersConfirmationDialog.emit(selectedAllocationsFromGanttTask);
        } else {
          this.openUpdatePlaceholdersDatesDialog.emit({ title: selectedOption, placeholderAllocations: selectedAllocationsFromGanttTask });
        }
    }
  
    addContextMenuItem(title, selectedAllocations, value) {
      const startDateLi = document.createElement('li');
      startDateLi.setAttribute('value', value);
      // Disabled context menu when confirmed and unnamed alocation are selected to make updates
      if (selectedAllocations.some(x => x.isPlaceholderAllocation) && selectedAllocations.some(x => !x.isPlaceholderAllocation)) {
        startDateLi.style.color = '#bababa';
        startDateLi.title = 'Cannot update Unnamed + Confirmed allocation simultaneously';
      } else if (value === 'Split' && selectedAllocations.every(x => x.isPlaceholderAllocation)) {
        startDateLi.title = 'Cannot split selected placeholder(unnamed) allocation';
      } else if (value === 'Split') {

        if (selectedAllocations.length > 1 ||
          moment(selectedAllocations[0].start_date).format('DD-MMM-YYYY') === moment(selectedAllocations[0].end_date).format('DD-MMM-YYYY')) {
          startDateLi.style.color = '#bababa';
          startDateLi.title =
            selectedAllocations.length > 1 ? 'Cannot split multiple allocations at once' : 'Cannot split one day allocation';
        } else {
          startDateLi.addEventListener('click', () => {
            this.splitAllocation(selectedAllocations[0]);
            document.getElementById('ganttCaseContextMenuId').innerHTML = '';
          });
        }
      } else {
        startDateLi.addEventListener('click', (e) => {
          const clickedOption = e.target as HTMLElement;
          this.updateDeleteAllocations(selectedAllocations, clickedOption.getAttribute('value'));
          document.getElementById('ganttCaseContextMenuId').innerHTML = '';
        });
      }
      const startDateTextNode = document.createTextNode(title);
      startDateLi.appendChild(startDateTextNode);
      document.getElementById('gantt-case-context-menu-ul').appendChild(startDateLi);
    }
  }
  