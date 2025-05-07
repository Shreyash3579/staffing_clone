import { Component, EventEmitter, OnInit, Output, Input } from "@angular/core";
import { BsModalRef } from "ngx-bootstrap/modal";
import { CoreService } from "src/app/core/core.service";
import { CommonService } from "src/app/shared/commonService";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { ResourceOrCasePlanningViewNote } from "src/app/shared/interfaces/resource-or-case-planning-view-note.interface";
import { Subscription } from "rxjs";
import { Store, select } from "@ngrx/store";
import * as fromStaffingSupply from "src/app/home-copy/state/reducers/staffing-supply.reducer";
import * as fromStaffingDemand from "src/app/home-copy/state/reducers/staffing-demand.reducer";
import { ResourceCommitment } from "../interfaces/resourceCommitment";
import { Project } from "../interfaces/project.interface";
import { PlanningCard } from "../interfaces/planningCard.interface";

@Component({
  selector: "app-notes-modal",
  templateUrl: "./notes-modal.component.html",
  styleUrls: ["./notes-modal.component.scss"]
})

export class NotesModalComponent implements OnInit {
  @Output() deleteNotes = new EventEmitter<any>();
  @Output() upsertNotes = new EventEmitter<any>();
  @Output() setNotes = new EventEmitter<any>();

  @Input() rowIndex: number;

  cardData: any = {};
  // New Note
  notes: ResourceOrCasePlanningViewNote[] = [];
  isNotesReadonly: boolean = false;
  accessibleFeatures = ConstantsMaster.appScreens.feature;
  resourceNotes : ResourceOrCasePlanningViewNote[] = [];
  storeSub: Subscription = new Subscription();
  
  constructor(
    public modalRef: BsModalRef,
    private coreService: CoreService,
    private demandStore: Store<fromStaffingDemand.State>,
    private supplyStore: Store<fromStaffingSupply.State>,
  ) { }

  ngOnInit(): void {
    // Get user logged in
    this.isNotesReadonly = !this.isNotesAccessible();
    this.getDemandNotes(this.cardData.data);
    this.setStoreSuscriptions();
  }

  setStoreSuscriptions() {
    this.getResourcesFromStore();
    this.getProjectsFromStore();
    this.getPlanningCardsFromStore();
  }

  getResourcesFromStore() {
    this.storeSub.add(this.supplyStore
      .pipe(
        select(fromStaffingSupply.getStaffingResources))
      .subscribe((resourcesData: ResourceCommitment) => {
        if (resourcesData) {
          this.updateResourceNotesFromStore(resourcesData);
        }
      }))
  }

  getProjectsFromStore() {
    this.storeSub.add(this.demandStore
      .pipe(
        select(fromStaffingDemand.getStaffingProjects))
      .subscribe((projectsData: Project[]) => {
        if (projectsData) { 
          if(this.cardData.data.pipelineId || this.cardData.data.oldCaseCode)
          this.updateProjectNotesFromStore(projectsData);
        }
      }))

      this.storeSub.add(this.demandStore
        .pipe(
          select(fromStaffingDemand.getStaffingHistoricalProjects))
        .subscribe((projectsData: Project[]) => {
          if (projectsData) { 
            if(this.cardData.data.pipelineId || this.cardData.data.oldCaseCode)
            this.updateProjectNotesFromStore(projectsData);
          }
        }))
   
  }

  getPlanningCardsFromStore() {
    this.storeSub.add(this.demandStore
      .pipe(
        select(fromStaffingDemand.getStaffingPlanningCards))
      .subscribe((planningCardsData: PlanningCard[]) => {
        if (planningCardsData) {
          planningCardsData.forEach(planningCard => {
            planningCard.placeholderAllocations = planningCard.allocations.filter(x => x.isPlaceholderAllocation);
            planningCard.regularAllocations = planningCard.allocations.filter(x => !x.isPlaceholderAllocation);
          });
          if(this.cardData.data.planningCardId)
          this.updatePlanningCardNotesFromStore(planningCardsData);
          
        }
      }))
  }


  getDemandNotes(projectOrResourceToOpen) {
    // const employeeCode = projectOrResourceToOpen.employeeCode;
    // const noteTypeCode = NoteTypeCode.RESOURCE_ALLOCATION_NOTE;
    // this.sharedService.getResourceViewNotes(employeeCode,noteTypeCode).subscribe((resourceNotes) => {
    //   this.notes = resourceNotes;
    // })
    
    this.notes = this.cardData.data.notes;
    this.setNotes.emit(this.notes)
  }

  deleteNotesHandler(noteId) {
    this.deleteNote(noteId);
  }

  private deleteNote(deletedNoteId) {
    this.deleteNotes.emit(deletedNoteId);
  }

  upsertNotesHandler(event) {

    let noteToBeUpserted: ResourceOrCasePlanningViewNote = {
      id: event.id,
      note: event.note,
      lastUpdated: event.lastUpdated,
      createdBy: event.createdBy,
      createdByName: event.createdByName,
      sharedWith: event.sharedWith,
      sharedWithDetails: event.sharedWithDetails,
      isPrivate: event.isPrivate,
      lastUpdatedBy: event.lastUpdatedBy,
      planningCardId: this.cardData.data.planningCardId,
      oldCaseCode: this.cardData.data.oldCaseCode,
      pipelineId: this.cardData.data.pipelineId,
      employeeCode: this.cardData.data.employeeCode,
      noteTypeCode: this.cardData.data.noteTypeCode
    }
    this.upsertNotes.emit(noteToBeUpserted);  
  }

  isNotesAccessible() {
    const featureName = this.accessibleFeatures.addCasePlanningViewNotes;

    const accessibleFeaturesForUser = this.coreService.loggedInUserClaims.FeatureAccess;
    const isAccessable = CommonService.isAccessToFeatureReadOnlyOrNone(featureName, accessibleFeaturesForUser);
    return isAccessable;
  }

  updateResourceNotesFromStore(resourcesData) {
    if(this.cardData.data?.employeeCode) {
      this.notes = resourcesData.resources.find(x => x.employeeCode == this.cardData.data.employeeCode)?.resourceViewNotes;
    }

  }

  updateProjectNotesFromStore(projectsData) {
    if(this.cardData.data?.pipelineId ) {
      let project = projectsData.find(x => x.pipelineId == this.cardData.data.pipelineId);
      if(project) {
       this.notes = project.casePlanningViewNotes;
      }
    }

    if(this.cardData.data?.oldCaseCode) {
      let project = projectsData.find(x => x.oldCaseCode == this.cardData.data.oldCaseCode);
      if(project) {
        this.notes = project.casePlanningViewNotes;
       }
    }
  }

  updatePlanningCardNotesFromStore(planningCardsData) {
    if(this.cardData.data?.planningCardId) {
      this.notes = planningCardsData.find(x => x.id == this.cardData.data.planningCardId)?.casePlanningViewNotes;
    }
  }

  // ---------------------------Component Unload--------------------------------------------//

  ngOnDestroy() {
    this.storeSub.unsubscribe();
  }

}
