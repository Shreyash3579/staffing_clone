import { Component, Input, OnInit, Output, EventEmitter } from "@angular/core";

// Interfaces
import { Project } from "src/app/shared/interfaces/project.interface";
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";

// Constants
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";

// Services
import { DateService } from "src/app/shared/dateService";
import { CoreService } from "src/app/core/core.service";
import { ResourceOrCasePlanningViewNote } from "src/app/shared/interfaces/resource-or-case-planning-view-note.interface";
import { CommonService } from "src/app/shared/commonService";

@Component({
  selector: "app-project-gantt-resources",
  templateUrl: "./project-gantt-resources.component.html",
  styleUrls: ["./project-gantt-resources.component.scss"]
})
export class ProjectGanttResourcesComponent implements OnInit {
  // ---------------- Input Variables -------------------- //
  @Input() casesGanttData: Project;
  @Input() planningCards: PlanningCard;
  @Input() rowIndex: number;

  // -----------------Output Events ------------------- //
  @Output() openAddTeamEmitter = new EventEmitter();
  @Output() skuTermClickForProject = new EventEmitter();
  @Output() upsertCasePlanningNote = new EventEmitter();
  @Output() deleteCasePlanningNotes = new EventEmitter();

  public isRowCollapsed: boolean = false;
  public isLoading = false;
  public projectDetails = {
    caseCode: "",
    percent: "",
    clientName: "",
    projectName: "",
    officeAbbreviation: "",
    startDate: "",
    endDate: "",
    projectType: "",
    sku: ""
  };

  public loggedInUser: string;
  public userNote = "";


  casePlanningNotes: ResourceOrCasePlanningViewNote[] = [];
  accessibleFeatures = ConstantsMaster.appScreens.feature;
  isNotesReadonly: boolean = false;

  constructor(private coreService: CoreService) { }

  ngOnInit(): void {
    // Getting logged in user
    // Used for setting new note authors and comparing with previous note authors
    this.loggedInUser = this.coreService.loggedInUser.firstName + " " + this.coreService.loggedInUser.lastName;

    this.setProjectDetails();
    this.isNotesReadonly = !this.isNotesAccessible();
  }

  setProjectDetails() {
    if (this.casesGanttData) {
      //this.casesGanttData.allNotes = [];

      this.projectDetails.caseCode = this.casesGanttData.oldCaseCode;
      this.projectDetails.projectType = this.casesGanttData.type;
      this.projectDetails.percent = this.casesGanttData.probabilityPercent + "%";
      this.projectDetails.clientName = this.casesGanttData.clientName;
      this.projectDetails.projectName = this.casesGanttData.caseName ?? this.casesGanttData.opportunityName;
      this.projectDetails.startDate = this.casesGanttData.startDate;
      this.projectDetails.endDate = this.casesGanttData.endDate;
      this.projectDetails.officeAbbreviation = this.casesGanttData.managingOfficeAbbreviation;
      this.projectDetails.sku = this.casesGanttData.skuCaseTerms?.skuTerms?.map(x => x.name).join(",") || "";
      this.casePlanningNotes = this.casesGanttData.casePlanningViewNotes;
    } else if (this.planningCards) {
      //this.planningCards.allNotes = [];

      this.projectDetails.clientName = "N/A";
      this.projectDetails.projectName = this.planningCards.name;
      this.projectDetails.officeAbbreviation = this.planningCards.sharedOfficeAbbreviations;
      this.projectDetails.startDate = DateService.convertDateInBainFormat(this.planningCards.startDate);
      this.projectDetails.endDate = DateService.convertDateInBainFormat(this.planningCards.endDate);
      this.casePlanningNotes = this.planningCards.casePlanningViewNotes;
    }
  }

  openAddTeamSkuFormHandler(projectToOpen) {
    // if (projectToOpen.oldCaseCode || projectToOpen.pipelineId) {
    //     this.openAddTeamEmitter.emit(projectToOpen);
    // }

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
    }
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
}
