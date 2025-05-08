import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CasePlanningComponent } from './case-planning.component';
import { CasePlanningRoutingModule } from './case-planning-routing.module';
import { ProjectsGanttComponent } from './projects-gantt/projects-gantt.component';
import { ProjectsGanttHeaderComponent } from './projects-gantt-header/projects-gantt-header.component';
import { ProjectsGanttBodyComponent } from './projects-gantt-body/projects-gantt-body.component';
import { ProjectsGanttTaskComponent } from './projects-gantt-task/projects-gantt-task.component';
import { ProjectsGanttProjectComponent } from './projects-gantt-project/projects-gantt-project.component';
import { StoreModule } from '@ngrx/store';
import { reducer } from './State/case-planning.reducer';
import { EffectsModule } from '@ngrx/effects';
import { CasePlanningEffects } from './State/case-planning.effects';
import { CasePlanningService } from './case-planning.service';
import { SharedModule } from '../shared/shared.module';
import { InfiniteScrollModule } from 'ngx-infinite-scroll';
import { OverlayModule } from '../overlay/overlay.module';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { CasePlanningFilterComponent } from './case-planning-filter/case-planning-filter.component';
import { FeatureAccessModule } from '../feature-access/feature-access.module';
import { AddTeamSkuComponent } from './add-team-sku/add-team-sku.component';
import { ReactiveFormsModule } from '@angular/forms';
import { PlaceholderFormComponent } from './placeholder-form/placeholder-form.component';
import { AllocatedPlaceholderComponent } from './allocated-placeholder/allocated-placeholder.component';
import { CasePlanningWhiteboardModule } from '../case-planning-whiteboard/case-planning-whiteboard.module';
import { ProjectGanttResourcesComponent } from './project-gantt-resources/project-gantt-resources.component';

//---------------------------Material Imports-------------------------------------------------
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { NotificationBannerComponent } from '../shared/notification-banner/notification-banner.component';

@NgModule({
  declarations: [
    CasePlanningComponent,
    ProjectsGanttComponent,
    ProjectsGanttHeaderComponent,
    ProjectsGanttBodyComponent,
    ProjectsGanttTaskComponent,
    ProjectsGanttProjectComponent,
    CasePlanningFilterComponent,
    AddTeamSkuComponent,
    PlaceholderFormComponent,
    AllocatedPlaceholderComponent,
    ProjectGanttResourcesComponent
  ],
  imports: [
    CommonModule,
    CasePlanningRoutingModule,
    StoreModule.forFeature('casePlanning', reducer),
    EffectsModule.forFeature([CasePlanningEffects]),
    DragDropModule,
    MatProgressBarModule,
    SharedModule,
    InfiniteScrollModule,
    OverlayModule,
    BsDropdownModule.forRoot(),
    FeatureAccessModule,
    ReactiveFormsModule,
    CasePlanningWhiteboardModule,
    NotificationBannerComponent
  ],
  exports:[ AddTeamSkuComponent],
  providers: [
     CasePlanningService
  ]
})
export class CasePlanningModule { }
