import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomeComponent } from './home.component';
import { SidebarComponent } from './supply/sidebar.component';
import { StageComponent } from './demand/stage.component';
import { ProjectviewComponent } from './projectview/projectview.component';
import { HomeService } from './home.service';
import { HomeRoutingModule } from './home-routing.module';
import { ResourceviewComponent } from './resourceview/resourceview.component';
import { NgMultiSelectDropDownModule } from 'ng-multiselect-dropdown';
import { MaterialModule } from '../shared/material.module';
import { AdvancedFiltersComponent } from './demand/advanced-filters/advanced-filters.component';
import { FilterComponent } from './demand/filter/filter.component';
import { SidebarFilterComponent } from './supply/sidebar-filter/sidebar-filter.component';
import { ProjectResourceComponent } from './project-resource/project-resource.component';
import { InfiniteScrollModule } from 'ngx-infinite-scroll';
import { NotificationService } from '../shared/notification.service';
import { LocalStorageService } from '../shared/local-storage.service';
import { SharedModule } from '../shared/shared.module';
import { OverlayModule } from '../overlay/overlay.module';
import { ResourceAssignmentService } from '../overlay/behavioralSubjectService/resourceAssignment.service';
import { CasePlanningWhiteboardService } from 'src/app/case-planning-whiteboard/case-planning-whiteboard.service';
import { ResourceCommitmentService } from '../overlay/behavioralSubjectService/resourceCommitment.service';
import { OverlayMessageService } from '../overlay/behavioralSubjectService/overlayMessage.service';
import { SkuCaseTermService } from '../overlay/behavioralSubjectService/skuCaseTerm.service';
import { UserPreferenceService } from '../overlay/behavioralSubjectService/userPreference.service';
import { OpportunityService } from '../overlay/behavioralSubjectService/opportunity.service';
import { CaseRollService } from '../overlay/behavioralSubjectService/caseRoll.service';
import { TeamsComponent } from './supply/teams/teams.component';
import { ResourcesComponent } from './supply/resources/resources.component';
import { PlanningCardComponent } from './demand/planning-card/planning-card.component';
import { ShowQuickPeekDialogService } from '../overlay/dialogHelperService/show-quick-peek-dialog.service';
import { ProjectGuessedAllocationComponent } from './project-guessed-allocation/project-guessed-allocation.component';
import { ProfileImageComponent } from '../core/profile-image/profile-image.component';

@NgModule({
  declarations: [
    HomeComponent,
    SidebarComponent,
    StageComponent,
    ProjectviewComponent,
    ResourceviewComponent,
    FilterComponent,
    SidebarFilterComponent,
    ProjectResourceComponent,
    AdvancedFiltersComponent,
    TeamsComponent,
    ResourcesComponent,
    PlanningCardComponent,
    ProjectGuessedAllocationComponent
  ],
  imports: [
    CommonModule,
    HomeRoutingModule,
    MaterialModule,
    NgMultiSelectDropDownModule.forRoot(),
    InfiniteScrollModule,
    SharedModule,
    OverlayModule,
    ProfileImageComponent
  ],
  exports: [
    HomeComponent
  ],
  providers: [
    HomeService,
    NotificationService,
    LocalStorageService,
    ResourceAssignmentService,
    ResourceCommitmentService,
    OverlayMessageService,
    SkuCaseTermService,
    UserPreferenceService,
    OpportunityService,
    CaseRollService,
    ShowQuickPeekDialogService,
    CasePlanningWhiteboardService
  ],
})
export class HomeModule { }
