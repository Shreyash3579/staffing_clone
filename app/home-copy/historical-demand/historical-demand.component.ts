// -------------------Angular Operators---------------------------------------//
import { Component, OnInit, Input, Output, EventEmitter, SimpleChanges } from '@angular/core';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { BsDatepickerActions } from 'ngx-bootstrap/datepicker/reducer/bs-datepicker.actions';
import { BsDatepickerAbstractComponent } from 'ngx-bootstrap/datepicker/base/bs-datepicker-container';

// -------------------Interfaces---------------------------------------//
import { ResourceGroup } from '../../../app/shared/interfaces/resourceGroup.interface';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';

// Services
import { CoreService } from 'src/app/core/core.service';
import { DateService } from 'src/app/shared/dateService';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { NotificationService } from 'src/app/shared/notification.service';
import { ValidationService } from 'src/app/shared/validationService';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';
import { CommonService } from 'src/app/shared/commonService';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { LoaderComponent } from 'src/app/shared/loader/loader.component';

@Component({
    selector: 'app-historical-demand',
    templateUrl: './historical-demand.component.html',
    styleUrls: ['./historical-demand.component.scss']
  })

  export class HistoricalDemandComponent implements OnInit {
  // inputs
  @Input() highlightedResourcesInPlanningCards: string[] = [];
  @Input() planningCards: PlanningCard[];
  @Input() projects: Project[];
  @Input() hideLoading: boolean;
  @Input() expandPanelComplete: boolean;
  @Input() isPdfExport: boolean;
  // outputs
  @Output() dateChangedEmitter = new EventEmitter();
  @Output() upsertResourceAllocationsToProjectEmitter = new EventEmitter<any>();
  @Output() removePlaceHolderEmitter = new EventEmitter();
  @Output() openPegRFPopUpEmitter = new EventEmitter();
  @Output() showQuickPeekDialog = new EventEmitter<any>();
  @Output() openCaseRollForm = new EventEmitter<any>();
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() upsertPlaceholderEmitter = new EventEmitter<any>();
  @Output() updatePlanningCardEmitter = new EventEmitter<any>();
  @Output() addProjectToUserExceptionHideListEmitter = new EventEmitter<any>();
  @Output() addProjectToUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() removeProjectFromUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() removePlanningCardEmitter = new EventEmitter();
  @Output() sharePlanningCardEmitter = new EventEmitter();
  @Output() openOverlappedTeamsForm = new EventEmitter();
  @Output() mergePlanningCardAndAllocations = new EventEmitter<any>();
  @Output() expandPanel = new EventEmitter<boolean>();


  // other
  collapseAll: boolean = true;
  showPlanningCards: boolean = true; 
  bsModalRef: BsModalRef;
  // showLoading: boolean = true;
  disableNextDateButton: boolean = true;  
  showMoreThanYearWarning = false;

  bsConfig: Partial<BsDatepickerConfig>;
  userPreferences: UserPreferences;
  selectedDate: [Date, Date];
  expandCompletePanel = false;
  maxEnabledEndDateForDatePicker: Date;
  demandTabs = [
    { label: "Planning Cards", active: true },
    { label: "Ongoing Cases", active: false },
  ];

  constructor(private _resourceAllocationService: ResourceAllocationService,
    private _notifyService: NotificationService,
    private localStorageService: LocalStorageService,
    private modalService: BsModalService) { }

  ngOnInit() {
    if(this.isPdfExport) {
      this.showExportPdfDownloadScreen();
      this.getDataForExport();
      this.demandTabs.forEach((tab, index) => {
        if (tab.active) {
          this.showPlanningCards = index === 0 ? true : false;
        }
      });
    } else {
      this.setDatePicker();
    }
    this.maxEnabledEndDateForDatePicker = new Date(DateService.subtractDays(DateService.getStartOfWeek(), 1));

    this.bsConfig = {
      containerClass: 'theme-red calendar-supply calendar-align-left',
      customTodayClass: 'custom-today-class',
      dateInputFormat: 'DD-MMM-YYYY',
      rangeInputFormat: 'DD-MMM-YYYY',
      isAnimated: true,
      showWeekNumbers: false,
      selectFromOtherMonth: true,
      adaptivePosition: true,
      // daysDisabled: [0, 6],
      maxDate: this.maxEnabledEndDateForDatePicker
    };
  }

  getDataForExport() {
    this.demandTabs = this.localStorageService.get(ConstantsMaster.localStorageKeys.demandTabs);
    this.collapseAll = this.localStorageService.get(ConstantsMaster.localStorageKeys.collapseAll);
    let date = this.localStorageService.get(ConstantsMaster.localStorageKeys.selectedDate);
    date = date.split(',');
    this.selectedDate = [new Date(date[0]), new Date(date[1])];
  }

  showExportPdfDownloadScreen() {
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        loaderMessage: 'Your file is being downloaded'
      }
    };
    this.bsModalRef = this.modalService.show(LoaderComponent, config);
  }

  ngAfterViewInit() {
    setTimeout(() => {
      if(this.isPdfExport)
      this.downloadOngoingReport();
    }, 5000);
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.expandPanelComplete) {
      this.expandCompletePanel = this.expandPanelComplete
    }
  }

  setDatePicker() {
    const endDate = DateService.getStartOfWeek();
    this.selectedDate = [new Date(DateService.subtractDays(endDate, 14)), new Date(DateService.subtractDays(endDate, 1))];
  }

  dateChangeEmitterHandler(event) {
    this.dateChangedEmitter.emit(event);
  }

  // tabs
  handleTabChange(tabIndex) {
    this.demandTabs.forEach((tab, index) => {
      if (index === tabIndex) {
        tab.active = true;
      } else {
        tab.active = false;
      }
    });

    this.showPlanningCards = tabIndex === 0 ? true : false;
  }

  // dates
  handleDateChange(selectedDate) {
    // To avoid API call during initialization we check for non nullable start and end dates
    if (!selectedDate || this.selectedDate.toString() === selectedDate.toString()) {
      return;
    }

    this.selectedDate = selectedDate;
    this.dateChangedEmitter.emit(this.selectedDate);

    this.enableDisableNextWeekButton(new Date(this.selectedDate[1]));
  }

  shiftDateRange(shiftDate) {
    if (shiftDate === "left") {
      const startDate = this.selectedDate[0];
      const endDate = this.selectedDate[1];

      startDate.setDate(startDate.getDate() - 7);
      endDate.setDate(endDate.getDate() - 7);

      this.selectedDate = [startDate, endDate];
      this.dateChangedEmitter.emit(this.selectedDate);
    } else {
      const startDate = this.selectedDate[0];
      const endDate = this.selectedDate[1];

        startDate.setDate(startDate.getDate() + 7);
        endDate.setDate(endDate.getDate() + 7);

        this.selectedDate = [startDate, endDate];
        this.dateChangedEmitter.emit(this.selectedDate);
      }

    this.enableDisableNextWeekButton(new Date(this.selectedDate[1]));
  }

  enableDisableNextWeekButton(selectedEndDate) {
    const probableEndDateOnNextWeekClick = selectedEndDate;
    probableEndDateOnNextWeekClick.setDate(probableEndDateOnNextWeekClick.getDate() + 7);

    if (probableEndDateOnNextWeekClick > this.maxEnabledEndDateForDatePicker) {
      this.disableNextDateButton = true;
    } else {
      this.disableNextDateButton = false;
    }
  }

  // output handlers

  mergePlanningcardToCaseOppEmitterHandler(event) {
    const project = event.project;
    const planningCard = event.planningCard;

    if (planningCard.placeholderAllocations?.length === 0 && planningCard.regularAllocations?.length === 0) {
      this._notifyService.showWarning(`Allocate resources in order to merge planning card`);
      return true;
    }
    const today = new Date().toLocaleDateString('en-US');
    const startDate = Date.parse(project.startDate) > Date.parse(today)
      ? project.startDate
      : Date.parse(project.endDate) < Date.parse(today)
        ? project.startDate
        : today;

    /*
      * NOTE: We are calculating opportunityEndDate if a resource is allocated to an opportunity that does not any end date or a duration.
      * For an opportunity that is going to start in future,
      * we have set the end date for the allocation as opportunity start date + 30 days.
      *
      * For an opportunuty that has already started, we have set the end date for the allocation as today + 30 days.
      *
      * TODO: Change the logic once Brittany comes up with the solution
    */

    let proposedEndDate = new Date(startDate);
    const defaultAllocationStartDate = new Date(startDate);
    const defaultAllocationEndDate = new Date(project.endDate);
    proposedEndDate.setDate(proposedEndDate.getDate() + 30);

    proposedEndDate = project.endDate !== null
      ? proposedEndDate
      : (!!planningCard.endDate ? new Date(planningCard.endDate) : proposedEndDate);

    const opportunityEndDate = proposedEndDate.toLocaleDateString('en-US');
    const maxEndDate = DateService.getMaxEndDateForAllocation(defaultAllocationStartDate, defaultAllocationEndDate);
    this.showMoreThanYearWarning = ValidationService.checkIfAllocationIsOfOneYear(defaultAllocationStartDate, defaultAllocationEndDate);

    const allocationEndDate = maxEndDate.toLocaleDateString('en-US');

    // if resource being dropped does not have an id that means resource is being dropped from resources list,
    // else its being dropped from one of the cards

    const resourceAllocations: ResourceAllocation[] = planningCard.regularAllocations
      .map(item => {
        return {
          oldCaseCode: project.oldCaseCode,
          caseName: project.caseName,
          clientName: project.clientName,
          pipelineId: project.pipelineId,
          caseTypeCode: project.caseTypeCode,
          opportunityName: project.opportunityName,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          operatingOfficeCode: item.operatingOfficeCode,
          operatingOfficeAbbreviation: item.operatingOfficeAbbreviation,
          currentLevelGrade: item.currentLevelGrade,
          serviceLineCode: item.serviceLineCode,
          serviceLineName: item.serviceLineName,
          allocation: item.allocation,
          startDate: this.getAllocationStartDate(item.startDate, project.startDate),
          endDate: this.getAllocationEndDate(allocationEndDate, opportunityEndDate),
          previousStartDate: null,
          previousEndDate: null,
          previousAllocation: null,
          investmentCode: item.investmentCode,
          investmentName: item.investmentName,
          caseRoleCode: item.caseRoleCode,
          caseStartDate: project.oldCaseCode ? project.startDate : null,
          caseEndDate: project.oldCaseCode ? project.endDate : null,
          opportunityStartDate: !project.oldCaseCode ? project.startDate : null,
          opportunityEndDate: !project.oldCaseCode ? project.endDate : null,
          lastUpdatedBy: null
        };
      });

    let allocationsToUpsert : ResourceAllocation[] ;

    if (resourceAllocations.length > 0) {
      resourceAllocations.forEach(alloc => {
        if (Date.parse(alloc.endDate) < Date.parse(alloc.startDate)) {
          alloc.endDate = alloc.startDate;
        }
      });

      const [isValidAllocation, monthCloseErrorMessage] = this._resourceAllocationService.validateMonthCloseForInsertAndDelete(resourceAllocations);

      if (!isValidAllocation) {
        this._notifyService.showValidationMsg(monthCloseErrorMessage);
        return;
      }

      allocationsToUpsert = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocations)

    }

    if (event.action === ConstantsMaster.PlanningCardMergeActions.CopyAndMerge) {
      //TODO: update this logic and make it better
      if(allocationsToUpsert?.length > 0){
        this.upsertResourceAllocationsToProjectEmitter.emit({
          resourceAllocation: allocationsToUpsert,
          showMoreThanYearWarning: this.showMoreThanYearWarning,
          allocationDataBeforeSplitting: resourceAllocations
        });
      }
      this.copyAndMergePlanningCard(planningCard, project, allocationEndDate, opportunityEndDate);
    } else {
      this.mergeAndUpdatePlanningCard(planningCard, allocationsToUpsert, project, allocationEndDate, opportunityEndDate, resourceAllocations);
    }
  }

  private copyAndMergePlanningCard(planningCard, project, allocationEndDate, opportunityEndDate) {
    const placeholderAllocations: PlaceholderAllocation[] = Object.assign([], planningCard).placeholderAllocations
      .map(item => {
        return {
          id: null,
          planningCardId: null,
          oldCaseCode: project.oldCaseCode,
          caseName: project.caseName,
          clientName: project.clientName,
          caseTypeCode: project.caseTypeCode,
          pipelineId: project.pipelineId,
          opportunityName: project.opportunityName,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          operatingOfficeCode: item.operatingOfficeCode,
          operatingOfficeAbbreviation: item.operatingOfficeAbbreviation,
          currentLevelGrade: item.currentLevelGrade,
          serviceLineCode: item.serviceLineCode,
          serviceLineName: item.serviceLineName,
          allocation: item.allocation,
          startDate: this.getAllocationStartDate(item.startDate, project.startDate),
          endDate: this.getAllocationEndDate(allocationEndDate, opportunityEndDate),
          investmentCode: item.investmentCode,
          investmentName: item.investmentName,
          caseRoleCode: item.caseRoleCode,
          caseStartDate: project.oldCaseCode ? project.startDate : null,
          caseEndDate: project.oldCaseCode ? project.endDate : null,
          opportunityStartDate: !project.oldCaseCode ? project.startDate : null,
          opportunityEndDate: !project.oldCaseCode ? project.endDate : null,
          isPlaceholderAllocation: item.isPlaceholderAllocation,
          positionGroupCode: item.positionGroupCode,
          lastUpdatedBy: null
        };
      });

    placeholderAllocations?.forEach(alloc => {
      if (Date.parse(alloc.endDate) < Date.parse(alloc.startDate)) {
        alloc.endDate = alloc.startDate;
      }
    });
    this.upsertPlaceholderHandler(placeholderAllocations, true, true);
  }

  private mergeAndUpdatePlanningCard(planningCard : PlanningCard, regularAllocationsToUpsert: ResourceAllocation[], project, allocationEndDate, opportunityEndDate, resourceAllocations) {
    const placeholderAllocations: PlaceholderAllocation[] = planningCard.placeholderAllocations
      .map(item => {
        return {
          id: item.id,
          planningCardId: null,
          oldCaseCode: project.oldCaseCode,
          caseTypeCode: project.caseTypeCode,
          caseName: project.caseName,
          clientName: project.clientName,
          pipelineId: project.pipelineId,
          opportunityName: project.opportunityName,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          operatingOfficeCode: item.operatingOfficeCode,
          operatingOfficeAbbreviation: item.operatingOfficeAbbreviation,
          currentLevelGrade: item.currentLevelGrade,
          serviceLineCode: item.serviceLineCode,
          serviceLineName: item.serviceLineName,
          allocation: item.allocation,
          startDate: this.getAllocationStartDate(item.startDate, project.startDate),
          endDate: this.getAllocationEndDate(allocationEndDate, opportunityEndDate),
          investmentCode: item.investmentCode,
          investmentName: item.investmentName,
          caseRoleCode: item.caseRoleCode,
          caseStartDate: project.oldCaseCode ? project.startDate : null,
          caseEndDate: project.oldCaseCode ? project.endDate : null,
          opportunityStartDate: !project.oldCaseCode ? project.startDate : null,
          opportunityEndDate: !project.oldCaseCode ? project.endDate : null,
          isPlaceholderAllocation: item.isPlaceholderAllocation,
          positionGroupCode: item.positionGroupCode,
          lastUpdatedBy: null
        };
      });

    placeholderAllocations?.forEach(alloc => {
      if (Date.parse(alloc.endDate) < Date.parse(alloc.startDate)) {
        alloc.endDate = alloc.startDate;
      }
    });

    //Update  planning card as merged and save case
    planningCard.mergedCaseCode = project.oldCaseCode;
    planningCard.isMerged = true;

    var payload = {
      resourceAllocations: regularAllocationsToUpsert ?? [],
      placeholderAllocations : placeholderAllocations ?? [],
      planningCard: planningCard,
      allocationDataBeforeSplitting: resourceAllocations
    }
    this.mergePlanningCardAndAllocations.emit(payload);
  }

  private getAllocationStartDate(allocationStartDate, caseOppStartDate) {
    const today = new Date().toLocaleDateString('en-US');
    const date7DaysAgo = new Date(new Date().setDate(new Date().getDate() - 7)).toLocaleDateString('en-US');
    if (caseOppStartDate === null && allocationStartDate === null) {
      return today;
    }
    if (caseOppStartDate === null && allocationStartDate !== null) {
      return DateService.convertDateInBainFormat(allocationStartDate);
    }
    if (Date.parse(caseOppStartDate) < Date.parse(date7DaysAgo)) {
      return today;
    }
    if (Date.parse(caseOppStartDate) >= Date.parse(date7DaysAgo) && Date.parse(caseOppStartDate) <= Date.parse(today)) {
      return DateService.convertDateInBainFormat(caseOppStartDate);
    }
    if (Date.parse(caseOppStartDate) > Date.parse(today) && allocationStartDate) {
      return DateService.convertDateInBainFormat(allocationStartDate);
    }
    return today;
  }
  private getAllocationEndDate(allocationEndDate, opportunityEndDate) {
    return allocationEndDate !== null
      ? DateService.convertDateInBainFormat(allocationEndDate)
      : DateService.convertDateInBainFormat(opportunityEndDate);
  }

  upsertResourceAllocationsToProjectHandler(upsertedAllocationsData) {
    this.upsertResourceAllocationsToProjectEmitter.emit(upsertedAllocationsData);
  }

  removePlaceHolderHandler(event) {
    this.removePlaceHolderEmitter.emit(event);
  }

  openPegRFPopUpHandler(pegOpportunityId) {
    this.openPegRFPopUpEmitter.emit(pegOpportunityId);
  }

  showQuickPeekDialogHandler(event) {
    this.showQuickPeekDialog.emit(event);
  }

  openCaseRollPopUpHandler(event) {
    this.openCaseRollForm.emit(event);
  }

  openPlaceholderFormHandler(event) {
    this.openPlaceholderForm.emit(event);
  }

  upsertPlaceholderHandler(payload, isMergeFromPlanningCard = false, isCopyAndMerge = false) {      
    payload = payload.placeholderAllocation ? payload.placeholderAllocation : payload;
    
    this.upsertPlaceholderEmitter.emit({ 
      allocations: payload, 
      isMergeFromPlanningCard: 
      isMergeFromPlanningCard, 
      isCopyAndMerge: isCopyAndMerge });
  }

  updatePlanningCardEmitterHandler(event) {
    this.updatePlanningCardEmitter.emit(event);
  }

  addProjectToUserExceptionHideListHandler(event) {
    this.addProjectToUserExceptionHideListEmitter.emit(event);
  }

  addProjectToUserExceptionShowListHandler(event) {
    this.addProjectToUserExceptionShowListEmitter.emit(event);
  }

  removeProjectFromUserExceptionShowListHandler(event) {
    this.removeProjectFromUserExceptionShowListEmitter.emit(event);
  }

  removePlanningCardEmitterHandler(event) {
    this.removePlanningCardEmitter.emit({ id: event.id });
  }

  sharePlanningCardEmitterHandler(event) {
    this.sharePlanningCardEmitter.emit(event);
  }

  openOverlappedTeamsPopupHandler(event) {
    this.openOverlappedTeamsForm.emit(event)
  }

  expandAndCollapsePanel() {
    this.expandCompletePanel = !this.expandCompletePanel;
    this.expandPanel.emit(this.expandCompletePanel);
  }

  printPdfHandler() {
    this.localStorageService.set(ConstantsMaster.localStorageKeys.historicalPlanningCards, this.planningCards.slice(0,100));
    this.localStorageService.set(ConstantsMaster.localStorageKeys.historicalProjects, this.projects.slice(0,100));
    this.localStorageService.set(ConstantsMaster.localStorageKeys.highlightedResourcesInPlanningCards, this.highlightedResourcesInPlanningCards);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.expandPanelComplete, this.expandPanelComplete);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.hideLoading, this.hideLoading);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.collapseAll, this.collapseAll);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.demandTabs, this.demandTabs);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.selectedDate, this.selectedDate.toString());


    const queryParam = window.location.href.indexOf('?') > 0 ? '&' : '?';
    const pdfExportUrl = window.location.href + queryParam + 'export=true';
    window.open(pdfExportUrl);
  }

  //parellel thread and glitch for 4 seconds handled in this
  async downloadOngoingReport() {
    const elementId = 'historical-demand-wrapper';
    const pdfFilename = 'Historical-Demand.pdf';
    const element = document.getElementById(elementId); 
    CommonService.generatePdf(elementId, pdfFilename);
  }
  
}
