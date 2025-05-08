import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ResourcesComponent } from './resources.component';
import { ResourcesRoutingModule } from './resources-routing.module';
import { ResourcesService } from './resources.service';
import { FilterComponent } from './filter/filter.component';
import { ResourceComponent } from './resource/resource.component';
import { GanttComponent } from './gantt/gantt.component';
import { GanttHeaderComponent } from './gantt-header/gantt-header.component';
import { GanttBodyComponent } from './gantt-body/gantt-body.component';
import { GanttTaskComponent } from './gantt-task/gantt-task.component';
import { ResourceInfoTabsComponent } from './resource-info-tabs/resource-info-tabs.component';
import { StoreModule } from '@ngrx/store';
import { reducer } from './State/resources.reducer';
import { EffectsModule } from '@ngrx/effects';
import { ResourcesEffects } from './State/resources.effects';
import { SharedModule } from '../shared/shared.module';
import { AngularDraggableModule } from 'angular2-draggable';
import { GanttCommitmentComponent } from './gantt-commitment/gantt-commitment.component';
import { OverlayModule } from '../overlay/overlay.module';
import { ContextMenuModule } from '@perfectmemory/ngx-contextmenu';
import { InfiniteScrollModule } from 'ngx-infinite-scroll';
import { GanttThresholdComponent } from './gantt-threshold/gantt-threshold.component';
import { FeatureAccessModule } from '../feature-access/feature-access.module';
import { PopupModalComponent } from './popup-modal/popup-modal.component';
import { AllocationNotesContextMenuComponent } from './allocation-notes-context-menu/allocation-notes-context-menu.component';
import { CasePlanningWhiteboardService } from 'src/app/case-planning-whiteboard/case-planning-whiteboard.service';
import { GroupingComponent } from './grouping/grouping.component';
import { CaseComponent } from './case/case.component';
import { GanttCaseBodyComponent } from './gantt-case-body/gantt-case-body.component';
import { ViewingDropdownMenuComponent } from './resources-filter/viewing-dropdown-menu/viewing-dropdown-menu.component';
import { MoreOptionsMenuComponent } from './resources-filter/more-options-menu/more-options-menu.component';
import { CustomGroupModalComponent } from './resources-filter/custom-group-modal/custom-group-modal.component';
import { ResourceBasicMenuComponent } from './resources-filter/resource-basic-menu/resource-basic-menu.component';
import { FilterByComponent } from './resources-filter/filter-by/filter-by.component';
import { SortByComponent } from './resources-filter/sort-by/sort-by.component';
import { SavedGroupModalComponent } from './resources-filter/saved-group-modal/saved-group-modal.component';
import { SavedResourceFiltersComponent } from './resources-filter/saved-resource-filters/saved-resource-filters.component';
import { FilterSortEditComponent } from './resources-filter/filter-sort-edit/filter-sort-edit.component';
import {NgbTooltip} from "@ng-bootstrap/ng-bootstrap";

//--------------------Material Imports-------------------------------------------------
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ProfileImageComponent } from '../core/profile-image/profile-image.component';
import { NotificationBannerComponent } from '../shared/notification-banner/notification-banner.component';
import { ScrollingModule } from '@angular/cdk/scrolling';

@NgModule({
    declarations: [
        ResourcesComponent,
        FilterComponent,
        GanttTaskComponent,
        ResourceInfoTabsComponent,
        GanttComponent,
        ResourceComponent,
        GanttHeaderComponent,
        GanttBodyComponent,
        GanttCommitmentComponent,
        GanttThresholdComponent,
        PopupModalComponent,
        AllocationNotesContextMenuComponent,
        GroupingComponent,
        CaseComponent,
        GanttCaseBodyComponent,
        ViewingDropdownMenuComponent,
        MoreOptionsMenuComponent,
        CustomGroupModalComponent,
        ResourceBasicMenuComponent,
        FilterByComponent,
        SortByComponent,
        SavedGroupModalComponent,
        SavedResourceFiltersComponent,
        FilterSortEditComponent
    ],
    imports: [
        CommonModule,
        AngularDraggableModule,
        ResourcesRoutingModule,
        StoreModule.forFeature('resources', reducer),
        EffectsModule.forFeature([ResourcesEffects]),
        SharedModule,
        OverlayModule,
        ContextMenuModule,
        InfiniteScrollModule,
        FeatureAccessModule,
        MatProgressBarModule,
        ProfileImageComponent,
        NgbTooltip,
      NotificationBannerComponent,
      ScrollingModule
    ],
    providers: [
        ResourcesService,
        CasePlanningWhiteboardService
    ]
})
export class ResourcesModule { }
