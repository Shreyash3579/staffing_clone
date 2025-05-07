import { Component, Input, OnInit, Output, EventEmitter } from "@angular/core";
import { BsDatepickerConfig } from "ngx-bootstrap/datepicker";

// Interfaces
import { Project } from "src/app/shared/interfaces/project.interface";
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";
import { ProjectDetails } from "src/app/shared/interfaces/projectDetails.interface";
import { OfficeHierarchy } from "src/app/shared/interfaces/officeHierarchy.interface";

// Constants
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";

// Services
import { DateService } from "src/app/shared/dateService";
import { CoreService } from "src/app/core/core.service";
import { ResourceOrCasePlanningViewNote } from "src/app/shared/interfaces/resource-or-case-planning-view-note.interface";
import { CommonService } from "src/app/shared/commonService";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { BS_DEFAULT_CONFIG } from "src/app/shared/constants/bsDatePickerConfig";
import { ValidationService } from "src/app/shared/validationService";
import * as moment from "moment";
import { CaseOppChanges } from "src/app/shared/interfaces/caseOppChanges.interface";
import { Store } from "@ngrx/store";
import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as fromPlanningCardOverlayStore from 'src/app/state/reducers/planning-card-overlay.reducer';
import * as ProjectOverlayActions from 'src/app/state/actions/project-overlay.action';
import * as PlanningCardActions from 'src/app/state/actions/planning-card-overlay.action';
import { IncludeInDemandProjectPreference } from "src/app/shared/interfaces/includeInDemandProjectPreference";

import * as fromProjects from "../State/case-planning.reducer";
import * as casePlanningActions from "../State/case-planning.actions";

@Component({
  selector: "app-gantt-project",
  templateUrl: "./gantt-project.component.html",
  styleUrls: ["./gantt-project.component.scss"]
})
export class GanttProjectComponent implements OnInit {
  // inputs
  @Input() casesGanttData: any;
  @Input() planningCards: PlanningCard;
  @Input() project: any;
  @Input() rowIndex: number;

  // outputs
  @Output() openAddTeamEmitter = new EventEmitter();
  @Output() skuTermClickForProject = new EventEmitter();
  @Output() upsertCasePlanningNote = new EventEmitter();
  @Output() deleteCasePlanningNotes = new EventEmitter();

  @Output() updateProbabilityEmitter = new EventEmitter();
  @Output() updateStartDateEmitter = new EventEmitter();
  @Output() updateEndDateEmitter = new EventEmitter();
  @Output() updateOfficeEmitter = new EventEmitter();

  

  // local var
  isRowCollapsed: boolean = false;
  isLoading = false;
  projectDetails = {
    oldCaseCode: "",
    planningCardId: "",
    pipelineId: "",
    clientName: "",
    startDate: "",
    overrideStartDate: "",
    overrideEndDate: "",
    overrideProbabilityPercent: 0,
    endDate: "",
    sku: "",
    manager: "",
    caseName: "",
    probabilityPercent: 0,
    managingOfficeAbbreviation: "",
    managingOfficeCode: "",
    placeholderAllocations: [],
    allocations: [],
    includeInDemand: false,
    isFlagged: false,
    isPegPlanningCard: false,
    estimatedTeamSize: "",
    pricingTeamSize: ""
  };

  loggedInUser: string;
  userNote = "";
  isFlagged: boolean = false;

  casePlanningNotes: ResourceOrCasePlanningViewNote[] = [];
  accessibleFeatures = ConstantsMaster.appScreens.feature;
  isNotesReadonly: boolean = false;
  bsConfig: Partial<BsDatepickerConfig>;

  oppStartDateValidationObj = { isValid: true, showMessage: false, errorMessage: '' };
  oppEndDateValidationObj = { isValid: true, showMessage: false, errorMessage: '' };
  oppPercentValidationObj = { isValid: true, showMessage: false, errorMessage: '' };
  officeValidationObj = { isValid: true, showMessage: false, errorMessage: '' };
  changedStartDate: string = null;
  changedEndDate: string = null;
  changedProbabilityPercentage: number = null;
  changedSharedOffices: string = null;
  updatedProjectData: CaseOppChanges = {} as CaseOppChanges;
  updatedPlanningCard: PlanningCard = {} as PlanningCard;

  // office picker
  officeHierarchy: OfficeHierarchy;
  selectedOfficeList = [];
  editableCol = '';

  constructor(
    private store: Store<fromProjects.State>,
    private coreService: CoreService, 
    private localStorageService: LocalStorageService,
    private projectAllocationsStore: Store<fromProjectAllocationsStore.State>,
    private planningCardStore: Store<fromPlanningCardOverlayStore.State>,
  ) { }

  ngOnInit(): void {
    // Getting logged in user
    // Used for setting new note authors and comparing with previous note authors
    this.loggedInUser = this.coreService.loggedInUser.firstName + " " + this.coreService.loggedInUser.lastName;

    this.isNotesReadonly = !this.isNotesAccessible();

    // set date picker config
    this.bsConfig = BS_DEFAULT_CONFIG;
    this.bsConfig.containerClass = 'theme-red';  

    // set up office hierarchy
    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.setOffices();
  }

  ngOnChanges(changes) {
    if (changes.casesGanttData || changes.planningCards) {
      this.setProjectDetails();
      this.setOffices();
    }
    if (changes.project && this.project) {

      this.projectDetails.clientName = this.project.clientName;
      this.projectDetails.caseName = this.project.caseName;
      this.projectDetails.manager = this.project.manager;
      this.projectDetails.managingOfficeCode = this.project.managingOfficeCode;
      this.projectDetails.oldCaseCode = this.project.caseCode;
      this.projectDetails.pipelineId = this.project.pipelineId;
      this.projectDetails.probabilityPercent = this.project.probabilityPercent;
      this.projectDetails.startDate = DateService.convertDateInBainFormat(this.project.startDate);
      this.projectDetails.endDate = DateService.convertDateInBainFormat(this.project.endDate);
      this.projectDetails.managingOfficeAbbreviation = this.project.office.managingOfficeAbbreviation;
      this.projectDetails.sku = this.project.combinedSkuTerm;
      this.projectDetails.placeholderAllocations = this.project.placeholderAllocations;
      this.projectDetails.includeInDemand = this.project.includeInDemand;
      this.projectDetails.isFlagged = this.project.isFlagged;
      this.projectDetails.estimatedTeamSize = this.project.estimatedTeamSize;
      this.projectDetails.pricingTeamSize = this.project.pricingTeamSize;
      this.casePlanningNotes = this.project.casePlanningViewNotes;
    }
  }

  setProjectDetails() {
    if (this.casesGanttData) {
      this.projectDetails.clientName = this.casesGanttData.clientName || "";
      this.projectDetails.caseName = this.casesGanttData.caseName ?? this.casesGanttData.opportunityName ?? "";

      this.projectDetails.manager =
        this.casesGanttData.caseManagerFullName ?? "";
      this.projectDetails.managingOfficeCode =
        this.casesGanttData?.managingOfficeCode?.toString() ??
        this.casesGanttData?.billingOfficeCode?.toString() ??
        "0";

      this.projectDetails.oldCaseCode = this.casesGanttData.oldCaseCode || "";
      this.projectDetails.pipelineId = this.casesGanttData.pipelineId || "";
      this.projectDetails.probabilityPercent = this.casesGanttData.overrideProbabilityPercent || this.casesGanttData.probabilityPercent;
      this.projectDetails.overrideProbabilityPercent = this.casesGanttData.overrideProbabilityPercent;

      this.projectDetails.startDate = DateService.convertDateInBainFormat(this.casesGanttData.overrideStartDate) || DateService.convertDateInBainFormat(this.casesGanttData.startDate) || "";
      this.projectDetails.overrideStartDate = DateService.convertDateInBainFormat(this.casesGanttData.overrideStartDate) || "";
      this.projectDetails.endDate = DateService.convertDateInBainFormat(this.casesGanttData.overrideEndDate) || DateService.convertDateInBainFormat(this.casesGanttData.endDate) || "";
      this.projectDetails.overrideEndDate = DateService.convertDateInBainFormat(this.casesGanttData.overrideEndDate) || "";

      this.projectDetails.managingOfficeAbbreviation = this.casesGanttData.managingOfficeAbbreviation ?? this.casesGanttData.billingOfficeAbbreviation ?? "";
      this.projectDetails.sku = this.casesGanttData?.combinedSkuTerm;

      this.casePlanningNotes = this.casesGanttData.casePlanningViewNotes; 
      this.projectDetails.placeholderAllocations = this.casesGanttData?.placeholderAllocations;
      this.projectDetails.includeInDemand = this.casesGanttData?.includeInDemand;
      this.projectDetails.isFlagged = this.casesGanttData?.isFlagged;
      this.projectDetails.estimatedTeamSize = this.casesGanttData?.estimatedTeamSize;
      this.projectDetails.pricingTeamSize = this.casesGanttData?.pricingTeamSize;

    } else if (this.planningCards) {
      this.projectDetails.planningCardId = this.planningCards.id || "";
      this.projectDetails.clientName = "";
      this.projectDetails.caseName = this.planningCards.name || "";

      this.projectDetails.manager = "";
      this.projectDetails.managingOfficeCode = this.planningCards.sharedOfficeCodes ?? "";

      this.projectDetails.oldCaseCode = "";
      this.projectDetails.probabilityPercent = this.planningCards.probabilityPercent;

      this.projectDetails.startDate = DateService.convertDateInBainFormat(this.planningCards.startDate) || "";
      this.projectDetails.endDate = DateService.convertDateInBainFormat(this.planningCards.endDate) || "";

      this.projectDetails.managingOfficeAbbreviation = this.planningCards.sharedOfficeAbbreviations || "";
      this.projectDetails.sku = this.planningCards?.combinedSkuTerm;
      this.projectDetails.placeholderAllocations = this.planningCards?.allocations.filter((allocation) => 
        allocation.isPlaceholderAllocation || allocation.isConfirmed);
      this.projectDetails.includeInDemand = this.planningCards?.includeInDemand;
      this.projectDetails.isFlagged = this.planningCards?.isFlagged;

      this.casePlanningNotes = this.planningCards.casePlanningViewNotes;
      this.projectDetails.isPegPlanningCard = !!this.planningCards.pegOpportunityId;
    }
  }

  setOffices() {
    this.selectedOfficeList = [];
    this.selectedOfficeList = this.projectDetails?.managingOfficeCode?.split(",");

    if (this.officeHierarchy) {
      this.officeHierarchy = JSON.parse(JSON.stringify(this.officeHierarchy));
    }
  }

  includeInDemand() {
    this.projectDetails.includeInDemand = !this.projectDetails.includeInDemand;
    const details = {
        oldCaseCode: this.projectDetails.oldCaseCode,
        pipelineId: this.projectDetails.pipelineId,
        planningCardId: this.projectDetails.planningCardId,
        includeInDemand: this.projectDetails.includeInDemand,
        isFlagged: this.projectDetails.isFlagged
      };

    let projDetails = [].concat(details);
    this.upsertCasePlanningProjectDetails(projDetails);
  }

  onFlagClick() {
    this.projectDetails.isFlagged = !this.projectDetails.isFlagged;
    const details = {
      oldCaseCode: this.projectDetails.oldCaseCode,
      pipelineId: this.projectDetails.pipelineId,
      planningCardId: this.projectDetails.planningCardId,
      includeInDemand: this.projectDetails.includeInDemand,
      isFlagged: this.projectDetails.isFlagged
    };

    let projDetails = [].concat(details);
    this.upsertCasePlanningProjectDetails(projDetails);
  }

  upsertCasePlanningProjectDetails(projDetails) {   
    this.store.dispatch(
      new casePlanningActions.UpsertCasePlanningProjectDetails({
        projDetails
      }));
  }

  openAddTeamSkuFormHandler(projectToOpen) {
    this.openAddTeamEmitter.emit(projectToOpen);
  }

  skuTermClickForProjectHandler() {
    if (this.planningCards) {
      return;
    }

    const projectId = this.casesGanttData.oldCaseCode || this.casesGanttData.pipelineId;
    this.skuTermClickForProject.emit(projectId);
  }

  upsertCasePlanningNoteHandler(event: ResourceOrCasePlanningViewNote) {
    let noteToBeUpserted: ResourceOrCasePlanningViewNote = {
      id: event.id,
      note: event.note,
      oldCaseCode: !!this.casesGanttData ? this.casesGanttData.oldCaseCode : null,
      pipelineId: !!this.casesGanttData ? this.casesGanttData.pipelineId : null,
      planningCardId: !!this.planningCards ? this.planningCards.id : null,
      lastUpdated: event.lastUpdated,
      createdBy: event.createdBy,
      createdByName: event.createdByName,
      sharedWith: event.sharedWith,
      sharedWithDetails: event.sharedWithDetails,
      isPrivate: event.isPrivate,
      lastUpdatedBy: event.lastUpdatedBy
    };
    this.upsertCasePlanningNote.emit(noteToBeUpserted);
  }

  deleteCasePlanningNotesHandler(event) {
    this.deleteCasePlanningNotes.emit(event);
  }

  isNotesAccessible() {
    const featureName = this.accessibleFeatures.addCasePlanningViewNotes;
    const accessibleFeaturesForUser = this.coreService.loggedInUserClaims.FeatureAccess;
    const isAccessable = CommonService.isAccessToFeatureReadOnlyOrNone(featureName, accessibleFeaturesForUser);

    return isAccessable;
  }

  // update probability
  updateProbability(event) {
    const validationObj = ValidationService.validateProbablePercentage(event);
    if (!validationObj.isValid) {
      this.oppPercentValidationObj = { showMessage: true, ...validationObj };
    } else {
      this.oppPercentValidationObj = { showMessage: false, ...validationObj };
      this.changedProbabilityPercentage = event;
      if(this.projectDetails.planningCardId) {
        this.updatePlanningCard();
      } else {
        this.updatedProjectData.pipelineId = this.casesGanttData?.pipelineId;
        this.updatedProjectData.oldCaseCode = this.casesGanttData?.oldCaseCode;
        this.updatedProjectData.probabilityPercent = this.changedProbabilityPercentage ?? this.projectDetails.probabilityPercent;
        this.updateProjectChanges(this.updatedProjectData);
      }
    }
  }

  updateStartDate(changedDate) {
    const validationObj = ValidationService.validateDate(changedDate);
    if (!validationObj.isValid) {
      this.projectDetails.startDate = '';
      this.oppStartDateValidationObj = { showMessage: true, ...validationObj };
    } else {
      this.projectDetails.startDate = DateService.convertDateInBainFormat(changedDate);
      this.changedStartDate = DateService.convertDateInBainFormat(changedDate);
      this.oppStartDateValidationObj = { showMessage: false, ...validationObj };
      if (this.validateStartDate() && this.validateEndDate()) {
        this.changedEndDate = DateService.convertDateInBainFormat(this.projectDetails.endDate);
        if(this.projectDetails.planningCardId) {
          this.updatePlanningCard();
        } else {
          this.updatedProjectData.pipelineId = this.casesGanttData?.pipelineId;
          this.updatedProjectData.oldCaseCode = this.casesGanttData?.oldCaseCode;
          this.updatedProjectData.startDate = this.changedStartDate ?? this.projectDetails.startDate;
          this.updateProjectChanges(this.updatedProjectData);
        }
      }
      // This is done to close editing of input as it remained open when you changed date afer correcting an error in input
      //this.disableOppStartDateEdit('');

    }
  }

  updateEndDate(changedDate) {
    const validationObj = ValidationService.validateDate(changedDate);
    if (!validationObj.isValid) {
      this.projectDetails.endDate = '';
      this.oppEndDateValidationObj = { showMessage: true, ...validationObj };
    } else {
      this.projectDetails.endDate = DateService.convertDateInBainFormat(changedDate);
      this.changedEndDate = DateService.convertDateInBainFormat(changedDate);
      this.oppEndDateValidationObj = { showMessage: false, ...validationObj };
      if (this.validateEndDate() && this.validateStartDate()) {
        this.changedStartDate = DateService.convertDateInBainFormat(this.projectDetails.startDate);
        if(this.projectDetails.planningCardId) {
          this.updatePlanningCard();
        } else {
          this.updatedProjectData.pipelineId = this.casesGanttData?.pipelineId;
          this.updatedProjectData.oldCaseCode = this.casesGanttData?.oldCaseCode;
          this.updatedProjectData.endDate = this.changedEndDate ?? this.projectDetails.endDate;
          this.updateProjectChanges(this.updatedProjectData);
        }
      }
      // This is done to close editing of input as it remained open when you changed date afer correcting an error in input
      //this.disableOppEndDateEdit('');
    }
  }

  updateOffices() {
    this.validateOffice();
    this.updatePlanningCard();
  }

  updatePlanningCard() {
    if (this.validatePlanningCardInput()) {
      this.updatedPlanningCard = this.planningCards;
      this.updatedPlanningCard.startDate = DateService.convertDateInBainFormat(this.changedStartDate) ?? 
                                            DateService.convertDateInBainFormat(this.projectDetails.startDate);
      this.updatedPlanningCard.endDate = DateService.convertDateInBainFormat(this.changedEndDate) ?? 
                                          DateService.convertDateInBainFormat(this.projectDetails.endDate);
      this.updatedPlanningCard.probabilityPercent = this.changedProbabilityPercentage ?? this.projectDetails.probabilityPercent;
      this.updatedPlanningCard.sharedOfficeCodes = this.selectedOfficeList.toString() ?? this.projectDetails.managingOfficeCode;
      this.updateProjectChanges(this.updatedPlanningCard);
    } else {
      this.oppStartDateValidationObj.showMessage = !!this.oppStartDateValidationObj.errorMessage;
      this.oppEndDateValidationObj.showMessage = !!this.oppEndDateValidationObj.errorMessage;
      this.oppPercentValidationObj.showMessage = !!this.oppPercentValidationObj.errorMessage;
    }
  }

  updateProject() {
    if (this.validateInput()) {
      this.updatedProjectData.pipelineId = this.casesGanttData?.pipelineId;
      this.updatedProjectData.oldCaseCode = this.casesGanttData?.oldCaseCode;
      this.updatedProjectData.startDate = this.changedStartDate ?? this.projectDetails.startDate;
      this.updatedProjectData.endDate = this.changedEndDate ?? this.projectDetails.endDate;
      this.updatedProjectData.probabilityPercent = this.changedProbabilityPercentage ?? this.projectDetails.probabilityPercent;
      this.updatedProjectData.notes = this.casesGanttData?.notes;
      this.updatedProjectData.caseServedByRingfence = this.casesGanttData?.caseServedByRingfence;
      this.updateProjectChanges(this.updatedProjectData);
    } else {
      this.oppStartDateValidationObj.showMessage = !!this.oppStartDateValidationObj.errorMessage;
      this.oppEndDateValidationObj.showMessage = !!this.oppEndDateValidationObj.errorMessage;
      this.oppPercentValidationObj.showMessage = !!this.oppPercentValidationObj.errorMessage;
      this.officeValidationObj.showMessage = !!this.officeValidationObj.errorMessage;
    }
  }

  hideValidationMessage(target, event) {
    if (target === 'probabilityPercent') {
      this.oppPercentValidationObj.showMessage = false;
    } else if (target === 'probabilityStartDate') {
      this.oppStartDateValidationObj.showMessage = false;
    } else if (target === 'probabilityEndDate') {
      this.oppEndDateValidationObj.showMessage = false;
    } else if (target === 'office') {
      this.officeValidationObj.showMessage = false;
    }
    this.editableCol = '';
    event.stopPropagation();
  }


  updateProjectChanges(event) {
    if(event.pipelineId) {
      this.projectAllocationsStore.dispatch(
        new ProjectOverlayActions.UpdateOpportunity({
            event : event
        })
      )}
    if(this.projectDetails.planningCardId) {
      this.planningCardStore.dispatch(
        new PlanningCardActions.UpsertPlanningCard({
          planningCard: event
        })
      )
    }
  }

  validatePlanningCardInput() {
    if (this.oppStartDateValidationObj.isValid && this.oppEndDateValidationObj.isValid && this.oppPercentValidationObj.isValid
      && this.officeValidationObj.isValid
    ) {
      return true;
    } else {
      return false;
    }
  }

  validateInput() {
    if (this.oppStartDateValidationObj.isValid && this.oppEndDateValidationObj.isValid && this.oppPercentValidationObj.isValid
    ) {
      return true;
    } else {
      return false;
    }
  }

  onOfficeChange(officeCodes) {
    if (officeCodes && this.isArrayEqual(this.selectedOfficeList.map(String), officeCodes.split(","))) {
      return false;
    }
    this.selectedOfficeList = officeCodes.split(",");
  }

  private isArrayEqual(array1, array2) {
    return JSON.stringify(array1) === JSON.stringify(array2);
  }

  private validateOffice(): boolean {
    if (this.selectedOfficeList[0] === '') {
      this.officeValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.officeReqMsg
      };
      return false;
    }
    this.officeValidationObj = {
      isValid: true, showMessage: false, errorMessage: ''
    };
    return true;
  }

  private validateEndDate(): boolean {
    if (this.projectDetails.endDate === null || !(this.projectDetails.endDate.length > 0)) {
      this.oppEndDateValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateReqMsg
      };
      return false;
    }
    if (moment(this.projectDetails.endDate).isBefore(moment(this.projectDetails.startDate))) {
      this.oppEndDateValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateCompMsg
      };
      return false;
    }
    this.oppEndDateValidationObj = {
      isValid: true, showMessage: false, errorMessage: ''
    };
    return true;
  }

  private validateStartDate(): boolean {
    if (this.projectDetails.startDate === null || !(this.projectDetails.startDate.length > 0)) {
      this.oppStartDateValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateReqMsg
      };
      return false;
    }
    if (moment(this.projectDetails.startDate).isAfter(moment(this.projectDetails.endDate))) {
      this.oppStartDateValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.startDateCompMsg
      };
      return false;
    }
    this.oppStartDateValidationObj = {
      isValid: true, showMessage: false, errorMessage: ''
    };
    return true;
  }
}