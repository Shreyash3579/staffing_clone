import { Component, Input, OnInit, SimpleChange, SimpleChanges } from '@angular/core';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { SharedNotesModalComponent } from '../shared-notes-modal/shared-notes-modal.component';
import { ResourceNotesAlert } from '../interfaces/resource-notes-alert';
import { SharedNotesService } from '../../core/services/shared-notes-info.service';
import { NotesAlertDialogService } from 'src/app/overlay/dialogHelperService/notesAlertDialog.service';
import { CaseIntakeAlert } from 'src/app/shared/interfaces/caseIntakeAlert.interface';
import { combineLatest,forkJoin, of, Subject, Subscription } from 'rxjs';
import { CoreService } from 'src/app/core/core.service';
import { PlanningCard } from '../interfaces/planningCard.interface';
import { Project } from '../interfaces/project.interface';


import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as fromPlanningCardOverlayStore from 'src/app/state/reducers/planning-card-overlay.reducer';

import { select, Store } from '@ngrx/store';
import { SignalrService } from '../signalR.service';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-notification-banner',
  standalone: true, 
  imports: [CommonModule],
  templateUrl: './notification-banner.component.html',
  styleUrls: ['./notification-banner.component.scss']
})
export class NotificationBannerComponent implements OnInit {

  bsModalRef: BsModalRef;
  showBanner: boolean = false;

  @Input() sharedNotes: ResourceNotesAlert[];
  @Input() caseIntakeAlerts: CaseIntakeAlert[];
  @Input() projectIdentifiers: string[] = [];
  @Input()  historicalProjectIdentifiers: string[] = [];
  @Input()  planningCardIds: string[] = [];

  storeSub: Subscription = new Subscription();

  private projectsLoaded$ = new Subject<void>();
  private planningCardsLoaded$ = new Subject<void>();
  private historicalProjectsLoaded$ = new Subject<void>();
  subscription: Subscription = new Subscription();

  constructor(private modalService: BsModalService, private _sharedNotesService:SharedNotesService, private _notesAlertDialogService:NotesAlertDialogService, 
    private sharedNotesService: SharedNotesService, private coreService: CoreService,
    private projectAllocationsStore: Store<fromProjectAllocationsStore.State>,
    private planningCardOverlayStore: Store<fromPlanningCardOverlayStore.State>,
    private signalrService: SignalrService
    ) { }

  ngOnInit(): void {  
   
    this.subscribeEvents();

  }

ngOnChanges(changes: SimpleChanges) {

  if (changes.sharedNotes && changes.sharedNotes?.currentValue && changes.sharedNotes?.currentValue.length > 0) {
    this.showBanner = true;
    }

  if(changes.caseIntakeAlerts && changes.caseIntakeAlerts?.currentValue && changes.caseIntakeAlerts?.currentValue.length > 0) {
      this.showBanner = true;
   }
}



subscribeEvents(){;

  this._notesAlertDialogService.showBanner.subscribe({
    next: (value) => { this.showBanner = value; }
  });

}


filterDemandPresentInStore(caseIntakeAlertsData: CaseIntakeAlert[]) {
    
  let filteredProjects = [];
  let filteredPlanningCards = [];

  if(this.projectIdentifiers || this.planningCardIds)
  {
  caseIntakeAlertsData.forEach(data => {
    if (data.oldCaseCode) {
      const project = this.projectIdentifiers.find(x => x === data.oldCaseCode);
      if (project) {
        filteredProjects.push(data);
      }
    } else if (data.opportunityId) {
      const project = this.projectIdentifiers.find(x => x === data.opportunityId);
      if (project) {
        filteredProjects.push(data);
      }
    } else if (data.planningCardId) {
      const planningCard = this.planningCardIds.find(x => x === data.planningCardId);
      if (planningCard) {
        filteredPlanningCards.push(data);
      }
    }
  });
  }

  if(this.historicalProjectIdentifiers )
  {
  caseIntakeAlertsData.forEach(data => {
    if (data.oldCaseCode || data.opportunityId) {
      const project = this.historicalProjectIdentifiers.find(x => x === data.oldCaseCode || x === data.opportunityId);
      if (project) {
        filteredProjects.push(data);
      }
    } 
  });
  }

  return filteredProjects.concat(filteredPlanningCards);
}


  view(): void {
    if (this.showBanner) {
      if (this.projectIdentifiers.length > 0 || this.historicalProjectIdentifiers.length > 0 || this.planningCardIds.length > 0) {
        const sharedNotes$ = this._sharedNotesService.getSharedNotes(null);
        const caseIntakeDetails$ = this._sharedNotesService.getCaseIntakeAlerts(null);

        // Combine both observables into one
        forkJoin([sharedNotes$, caseIntakeDetails$]).subscribe(([sharedNotesData, caseIntakeDetailsData]) => {
          this.sharedNotes = sharedNotesData;
          this.caseIntakeAlerts = this.filterDemandPresentInStore(caseIntakeDetailsData);
          this._notesAlertDialogService.openAlertsDialogHandler(sharedNotesData, caseIntakeDetailsData);
        });
      } else {
        this.caseIntakeAlerts = [];
        const sharedNotes$ = this._sharedNotesService.getSharedNotes(null);  // Only fetch shared notes
        // Make the API call for shared notes only
        sharedNotes$.subscribe(sharedNotesData => {
          this.sharedNotes = sharedNotesData;
          this._notesAlertDialogService.openAlertsDialogHandler(sharedNotesData, []);
        })
      }
    }
}


}
