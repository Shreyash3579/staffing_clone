import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { HomeComponent } from "./home.component";
import { StageComponent } from "./stage/stage.component";
// import { ProjectviewComponent } from "./projectview/projectview.component";
import { HomeService } from "./home.service";
import { HomeRoutingModule } from "./home-routing.module";
import { ResourceviewComponent } from "./stage/supply/resourceview/resourceview.component";
import { InfiniteScrollModule } from "ngx-infinite-scroll";
import { SharedModule } from "../shared/shared.module";
import { OverlayModule } from "../overlay/overlay.module";
import { ResourcesComponent } from "./stage/supply/resources/resources.component";
import { MenubarComponent } from "./quick-filter/menubar/menubar.component";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { NgbPopoverModule } from "@ng-bootstrap/ng-bootstrap";
import { DemandComponent } from "./stage/demand/demand.component";
import { ProjectviewComponent } from "./stage/demand/projectview/projectview.component";
import { StaffingSupplyEffects } from "./state/effects/staffing-supply.effect";
import { StaffingDemandEffects } from "./state/effects/staffing-demand.effect";
import { EffectsModule } from "@ngrx/effects";
import { ProjectResourceComponent } from "./stage/demand/project-resource/project-resource.component";
import { PlanningCardComponent } from "./stage/demand/planning-card/planning-card.component";
import { SupplyWeekBucketComponent } from "./stage/supply/supply-week-bucket/supply-week-bucket.component";
import { AngularSplitModule } from "angular-split";
import { ProjectGuessedAllocationComponent } from "./stage/demand/project-guessed-allocation/project-guessed-allocation.component";
import { QuickFilterComponent } from "./quick-filter/quick-filter.component";
import { StoreModule } from "@ngrx/store";
import { demandReducer } from "./state/reducers/staffing-demand.reducer";
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { SearchResultsComponent } from "./search-results/search-results.component";
import { supplyReducer } from "./state/reducers/staffing-supply.reducer";
import { ResourcesAdvancedFilterComponent } from "./quick-filter/menubar/resources-advanced-filter/resources-advanced-filter.component";
import { ProjectsAdvancedFilterComponent } from "./quick-filter/menubar/projects-advanced-filter/projects-advanced-filter.component";
import { HistoricalDemandComponent } from "./historical-demand/historical-demand.component";
import { FeatureAccessModule } from '../feature-access/feature-access.module';
import { HomeHelperService } from "./homeHelper.service";
import { AzureSearchService } from "./azureSearch.service";

//--------------------Material Imports-------------------------------------------------
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DragDropModule } from "@angular/cdk/drag-drop";
import { ProfileImageComponent } from "../core/profile-image/profile-image.component";
import { NotificationBannerComponent } from "../shared/notification-banner/notification-banner.component";

@NgModule({
  declarations: [
    HomeComponent,
    StageComponent,
    ResourceviewComponent,
    ResourcesComponent,
    MenubarComponent,
    DemandComponent,
    PlanningCardComponent,
    ProjectResourceComponent,
    ProjectviewComponent,
    SupplyWeekBucketComponent,
    ProjectGuessedAllocationComponent,
    QuickFilterComponent,
    SearchResultsComponent,
    ResourcesAdvancedFilterComponent,
    ProjectsAdvancedFilterComponent,
    HistoricalDemandComponent
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [
    CommonModule,
    HomeRoutingModule,
    AngularSplitModule,
    InfiniteScrollModule,
    SharedModule,
    OverlayModule,
    MatProgressSpinnerModule,
    NgbPopoverModule,
    StoreModule.forFeature('projects', demandReducer),
    StoreModule.forFeature('staffingResources', supplyReducer),
    NgbDropdownModule,
    FeatureAccessModule,
    EffectsModule.forFeature([ StaffingSupplyEffects, StaffingDemandEffects]),
    FeatureAccessModule,
    ProfileImageComponent,
    MatProgressBarModule,
    DragDropModule,
    NotificationBannerComponent
  ],
  exports: [HomeComponent],
  providers: [
    HomeService,
    HomeHelperService, 
    AzureSearchService
  ]
})
export class HomeModule {}
