import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { CasePlanningComponent } from './case-planning.component';
import { CasePlanningRoutingModule } from './case-planning-routing.module';
import { CasePlanningService } from './case-planning.service';
import { OverlayModule } from '../overlay/overlay.module';
import { GanttComponent } from './gantt/gantt.component';
import { GanttHeaderComponent } from './gantt-header/gantt-header.component';
import { GanttBodyComponent } from './gantt-body/gantt-body.component';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { GanttTaskComponent } from './gantt-task/gantt-task.component';
import { GanttProjectDetailsComponent } from './gantt-project-details/gantt-project-details.component';
import { CasePlanningFilterComponent } from './case-planning-filter/case-planning-filter.component';
import { AddTeamSkuComponent } from './add-team-sku/add-team-sku.component';
import { GanttProjectComponent } from './gantt-project/gantt-project.component';
import { PlaceholderFormComponent } from './placeholder-form/placeholder-form.component';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { SharedModule } from '../shared/shared.module';
import { InfiniteScrollModule } from 'ngx-infinite-scroll';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { FeatureAccessModule } from '../feature-access/feature-access.module';
import { ReactiveFormsModule } from '@angular/forms';
import { CasePlanningWhiteboardModule } from '../case-planning-whiteboard/case-planning-whiteboard.module';
import { CasePlanningEffects } from './State/case-planning.effects';
import { casePlanningTabReducer } from './State/case-planning.reducer';
import { PopoverModule } from 'ngx-bootstrap/popover';
import { CasePlanningMetricsComponent } from './case-planning-metrics/case-planning-metrics.component';
import { CasePlanningMetricsTableComponent } from './case-planning-metrics-table/case-planning-metrics-table.component';
import { CasePlanningGroupingComponent } from './case-planning-filter/case-planning-grouping/case-planning-grouping.component';

@NgModule({
  declarations: [
    CasePlanningComponent,
    GanttComponent,
    GanttHeaderComponent,
    GanttBodyComponent,
    GanttProjectDetailsComponent,
    GanttTaskComponent,
    GanttProjectComponent,
    CasePlanningFilterComponent,
    CasePlanningMetricsComponent,
    CasePlanningMetricsTableComponent,
    AddTeamSkuComponent,
    PlaceholderFormComponent,
    CasePlanningGroupingComponent
  ],
  imports: [
    CommonModule,
    CasePlanningRoutingModule,
    StoreModule.forFeature('casePlanningCopy', casePlanningTabReducer),
    EffectsModule.forFeature([CasePlanningEffects]),
    DragDropModule,
    MatProgressBarModule,
    SharedModule,
    InfiniteScrollModule,
    OverlayModule,
    BsDropdownModule.forRoot(),
    PopoverModule.forRoot(),
    FeatureAccessModule,
    ReactiveFormsModule,
    CasePlanningWhiteboardModule
  ],
  exports:[],
  providers: [
    CasePlanningService
  ]
})
export class CasePlanningModule { }
