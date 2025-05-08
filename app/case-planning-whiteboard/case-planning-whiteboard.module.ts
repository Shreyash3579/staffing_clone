import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CaseCardComponent } from './case-card/case-card.component';
import { MaterialModule } from '../shared/material.module';
import { CasePlanningWhiteboardService } from './case-planning-whiteboard.service';
import { PlanningBoardModalComponent } from './planning-board-modal/planning-board-modal.component';
import { PlanningBoardStageComponent } from './planning-board-stage/planning-board-stage.component';
import { ContextMenuModule } from '@perfectmemory/ngx-contextmenu';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { PlanningBoardFilterComponent } from './planning-board-filter/planning-board-filter.component';
import { SharedModule } from '../shared/shared.module';
import { UpdateCaseCardComponent } from './update-case-card/update-case-card.component';
import { AgGridModule } from 'ag-grid-angular';
import { MetricsTableComponent } from "./metrics-table/metrics-table.component";
import { QuickFiltersComponent } from './quick-filters/quick-filters.component';
import { JoinPlaygroundPopUpComponent } from './join-playground-pop-up/join-playground-pop-up.component';
import { StaffableTeamsModalComponent } from './staffable-teams-modal/staffable-teams-modal.component';
import { StaffableTeamComponent } from './staffable-teams-modal/staffable-team/staffable-team.component';
import { DemandNotesModalComponent } from './demand-notes-modal/demand-notes-modal.component';
import { AgGridNoteCellRendererComponent } from './ag-grid-note-cell-renderer/ag-grid-note-cell-renderer.component';
import { NgbPopoverModule } from "@ng-bootstrap/ng-bootstrap";
import { FormsModule} from "@angular/forms";

@NgModule({
  declarations: [
    PlanningBoardModalComponent,
    CaseCardComponent,
    PlanningBoardStageComponent,
    PlanningBoardFilterComponent,
    UpdateCaseCardComponent,
    MetricsTableComponent,
    QuickFiltersComponent,
    JoinPlaygroundPopUpComponent,
    StaffableTeamsModalComponent,
    StaffableTeamComponent,
    DemandNotesModalComponent,
    AgGridNoteCellRendererComponent,
  ],
  imports: [
    BsDatepickerModule.forRoot(),
    CommonModule,
    MaterialModule,
    ContextMenuModule,
    FormsModule,
    NgbPopoverModule,
    SharedModule,
    AgGridModule
  ],
  providers: [
    CasePlanningWhiteboardService
  ]
})
export class CasePlanningWhiteboardModule { }
