import { Component, EventEmitter, OnInit, Output, Input } from "@angular/core";
import { BsModalRef } from "ngx-bootstrap/modal";
import { CoreService } from "src/app/core/core.service";
import { CommonService } from "src/app/shared/commonService";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { ResourceOrCasePlanningViewNote } from "src/app/shared/interfaces/resource-or-case-planning-view-note.interface";
import { NotificationService } from "src/app/shared/notification.service";
import { CasePlanningWhiteboardService } from "../case-planning-whiteboard.service";
import * as fromUserSpecificDetailsStore from 'src/app/state/reducers/user-specific-details.reducer';
import * as UserSpecificDetailsAction from 'src/app/state/actions/user-specific-details.action';
import { Store } from "@ngrx/store";

@Component({
  selector: "app-demand-notes-modal",
  templateUrl: "./demand-notes-modal.component.html",
  styleUrls: ["./demand-notes-modal.component.scss"]
})
export class DemandNotesModalComponent implements OnInit {
  @Output() deleteCaseViewNotes = new EventEmitter<any>();
  @Output() upsertCaseViewNotes = new EventEmitter<any>();
  @Output() setCaseViewNotes = new EventEmitter<any>();

  @Input() rowIndex: number;

  cardData: any = {};
  // New Note
  casePlanningNotes: ResourceOrCasePlanningViewNote[] = [];
  isNotesReadonly: boolean = false;
  accessibleFeatures = ConstantsMaster.appScreens.feature;

  constructor(
    public modalRef: BsModalRef,
    private coreService: CoreService,
    private planningBoardService: CasePlanningWhiteboardService,
    private notifyService: NotificationService,
    private userSpecificDetailsStore: Store<fromUserSpecificDetailsStore.State>

  ) { }

  ngOnInit(): void {
    // Get user logged in
    this.isNotesReadonly = !this.isNotesAccessible();
    this.getDemandNotes(this.cardData.data);
  }

  getDemandNotes(projectToOpen) {
    this.planningBoardService.getCasePlanningViewNotes(projectToOpen.oldCaseCode, projectToOpen.pipelineId, projectToOpen.planningCardId).subscribe(data => {
      this.casePlanningNotes = data;
      this.setCaseViewNotes.emit(this.casePlanningNotes)
    });
  }

  deleteCaseNotesHandler(caseNoteId) {
    this.deleteCaseNote(caseNoteId);
  }

  private deleteCaseNote(deletedCaseNoteId) {
    this.planningBoardService.deleteCasePlanningNotes(deletedCaseNoteId).subscribe(
      (success) => {
        this.casePlanningNotes = this.casePlanningNotes.filter((note) => note.id !== deletedCaseNoteId);
        this.notifyService.showSuccess("Note deleted");
        this.deleteCaseViewNotes.emit(deletedCaseNoteId);
        this.userSpecificDetailsStore.dispatch(new UserSpecificDetailsAction.GetMostRecentSharedWithEmployeeGroups());
      },
      (error) => {
        this.notifyService.showError("Error while deleting note");
      }
    );
  }

  upsertCaseNotesHandler(event) {

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

    }

    this.planningBoardService.upsertCasePlanningViewNote(noteToBeUpserted).subscribe((insertedNote) => {
      if (this.casePlanningNotes.some(x => x.id === insertedNote.id)) {
        this.casePlanningNotes = this.casePlanningNotes.filter(x => x.id !== insertedNote.id);
      }
      else {
        this.casePlanningNotes = this.casePlanningNotes.filter(x => !!x.id);
      }
      this.casePlanningNotes.splice(0, 0, insertedNote);

      this.notifyService.showSuccess(
        `Note added ${insertedNote.note.length === 1000 ? "with 1000 char" : ""}`
      );

      this.upsertCaseViewNotes.emit(insertedNote);
      this.userSpecificDetailsStore.dispatch(new UserSpecificDetailsAction.GetMostRecentSharedWithEmployeeGroups());
    });
  }

  isNotesAccessible() {
    const featureName = this.accessibleFeatures.addCasePlanningViewNotes;

    const accessibleFeaturesForUser = this.coreService.loggedInUserClaims.FeatureAccess;
    const isAccessable = CommonService.isAccessToFeatureReadOnlyOrNone(featureName, accessibleFeaturesForUser);
    return isAccessable;
  }
}
