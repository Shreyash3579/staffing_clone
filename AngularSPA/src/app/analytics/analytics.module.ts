import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { AnalyticsComponent } from "./analytics.component";
import { AnalyticsRoutingModule } from "./analytics-routing.module";
import { StaffingReportComponent } from "./staffing-report/staffing-report.component";
import { ResourceAvailabilityComponent } from "./resource-availability/resource-availability.component";
import { IndividualResourceDetailsComponent } from "./individual-resource-details/individual-resource-details.component";
import { PopoverModule } from "ngx-bootstrap/popover";
import { WeeklyDeploymentSummaryComponent } from "./weekly-deployment-summary/weekly-deployment-summary.component";
import { RingfenceStaffingComponent } from "./ringfence-staffing/ringfence-staffing.component";
import { OverAllocatedAuditComponent } from "./over-allocated-audit/over-allocated-audit.component";
import { AllocatedOnLoaAuditComponent } from "./allocated-on-loa-audit/allocated-on-loa-audit.component";
import { PriceRealizationComponent } from "./price-realization/price-realization.component";
import { CaseUpdatesAuditComponent } from "./case-updates-audit/case-updates-audit.component";
import { StaffingAllocationsMonthlyComponent } from "./staffing-allocations-monthly/staffing-allocations-monthly.component";
import { MonthlyDeploymentComponent } from "./monthly-deployment/monthly-deployment.component";
import { MonthlyPracticeAreaStaffingComponent } from "./monthly-practice-area-staffing/monthly-practice-area-staffing.component";
import { PracticeStaffingCaseServedComponent } from "./practice-staffing-case-served/practice-staffing-case-served.component";
import { CommitmentDetailsComponent } from "./commitment-details/commitment-details.component";
import { CaseEconomicsComponent } from "./case-economics/case-economics.component";
import { TimeInLaneComponent } from "./time-in-lane/time-in-lane.component";
import { ShareUrlComponent } from "./common/share-url/share-url.component";
import { FeatureAccessModule } from "../feature-access/feature-access.module";
import { HistoricalAllocationsForPromotionsComponent } from "./historical-allocations-for-promotions/historical-allocations-for-promotions.component";
import { WhoWorkedWithWhomComponent } from "./who-worked-with-whom/who-worked-with-whom.component";
import { AffiliateTimeInPracticeComponent } from "./affiliate-time-in-practice/affiliate-time-in-practice.component";
import { SmapAllocationsComponent } from "./smap-allocations/smap-allocations.component";
import { MonthlyFteUtilizationComponent } from "./monthly-fte-utilization-container/monthly-fte-utilization/monthly-fte-utilization.component";
import { MonthlyFteUtilizationIndividualComponent } from "./monthly-fte-utilization-container/monthly-fte-utilization-individual/monthly-fte-utilization-individual.component";
import { CaseExperienceComponent } from "./case-experience/case-experience.component";
import { NgbPopoverModule } from "@ng-bootstrap/ng-bootstrap";
import { DailyDeploymentDetailsComponent } from "./daily-deployment-details/daily-deployment-details.component";
import { GlobalCapacitySummary } from "./global-capacity-summary/global-capacity-summary.component";
import { GlobalClientFacingHeadcountComponent } from "./global-client-facing-headcount/global-client-facing-headcount.component";
import { StaffingInsightsComponent } from "./staffing-insights/staffing-insights.component";
// import { TestResponsiveReportComponent } from './test-responsive-report/test-responsive-report.component';

//--------------------Material Imports-------------------------------------------------
import { ClipboardModule } from '@angular/cdk/clipboard';
import { SharedModule } from "../shared/shared.module";
import { NotificationBannerComponent } from "../shared/notification-banner/notification-banner.component";

@NgModule({
  declarations: [
    AnalyticsComponent,
    StaffingReportComponent,
    ResourceAvailabilityComponent,
    IndividualResourceDetailsComponent,
    WeeklyDeploymentSummaryComponent,
    RingfenceStaffingComponent,
    OverAllocatedAuditComponent,
    AllocatedOnLoaAuditComponent,
    PriceRealizationComponent,
    CaseUpdatesAuditComponent,
    StaffingAllocationsMonthlyComponent,
    MonthlyDeploymentComponent,
    MonthlyPracticeAreaStaffingComponent,
    PracticeStaffingCaseServedComponent,
    CommitmentDetailsComponent,
    CaseEconomicsComponent,
    TimeInLaneComponent,
    ShareUrlComponent,
    HistoricalAllocationsForPromotionsComponent,
    WhoWorkedWithWhomComponent,
    AffiliateTimeInPracticeComponent,
    SmapAllocationsComponent,
    MonthlyFteUtilizationComponent,
    MonthlyFteUtilizationIndividualComponent,
    CaseExperienceComponent,
    DailyDeploymentDetailsComponent,
    GlobalCapacitySummary,
    GlobalClientFacingHeadcountComponent,
    StaffingInsightsComponent
    // TestResponsiveReportComponent
  ],
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    AnalyticsRoutingModule,
    PopoverModule.forRoot(),
    FeatureAccessModule,
    NgbPopoverModule,
    ClipboardModule,
    NotificationBannerComponent
  ]
})
export class AnalyticsModule { }
