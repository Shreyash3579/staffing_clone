import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { StaffingReportComponent } from './staffing-report/staffing-report.component';
import { AnalyticsComponent } from './analytics.component';
import { ResourceAvailabilityComponent } from './resource-availability/resource-availability.component';
import { IndividualResourceDetailsComponent } from './individual-resource-details/individual-resource-details.component';
import { WeeklyDeploymentSummaryComponent } from './weekly-deployment-summary/weekly-deployment-summary.component';
import { RingfenceStaffingComponent } from './ringfence-staffing/ringfence-staffing.component';
import { OverAllocatedAuditComponent } from './over-allocated-audit/over-allocated-audit.component';
import { AllocatedOnLoaAuditComponent } from './allocated-on-loa-audit/allocated-on-loa-audit.component';
import { PriceRealizationComponent } from './price-realization/price-realization.component';
import { CaseUpdatesAuditComponent } from './case-updates-audit/case-updates-audit.component';
import { ReportAuthGuardService as AuthGuardService } from '../core/authentication/report-auth-guard.service';
import { StaffingAllocationsMonthlyComponent } from './staffing-allocations-monthly/staffing-allocations-monthly.component';
import { MonthlyDeploymentComponent } from './monthly-deployment/monthly-deployment.component';
import { MonthlyPracticeAreaStaffingComponent } from './monthly-practice-area-staffing/monthly-practice-area-staffing.component';
import { TimeInLaneComponent } from './time-in-lane/time-in-lane.component';
import { PracticeStaffingCaseServedComponent } from './practice-staffing-case-served/practice-staffing-case-served.component';
import { CommitmentDetailsComponent } from './commitment-details/commitment-details.component';
import { CaseEconomicsComponent } from './case-economics/case-economics.component';
import { TestResponsiveReportComponent } from './test-responsive-report/test-responsive-report.component';
import { HistoricalAllocationsForPromotionsComponent } from './historical-allocations-for-promotions/historical-allocations-for-promotions.component';
import { WhoWorkedWithWhomComponent } from './who-worked-with-whom/who-worked-with-whom.component';
import { AffiliateTimeInPracticeComponent } from './affiliate-time-in-practice/affiliate-time-in-practice.component';
import { SmapAllocationsComponent } from './smap-allocations/smap-allocations.component';
import { MonthlyFteUtilizationComponent } from './monthly-fte-utilization-container/monthly-fte-utilization/monthly-fte-utilization.component';
import { MonthlyFteUtilizationIndividualComponent } from './monthly-fte-utilization-container/monthly-fte-utilization-individual/monthly-fte-utilization-individual.component';
import { CaseExperienceComponent } from './case-experience/case-experience.component';
import { DailyDeploymentDetailsComponent } from './daily-deployment-details/daily-deployment-details.component';
import { GlobalCapacitySummary } from './global-capacity-summary/global-capacity-summary.component';
import { GlobalClientFacingHeadcountComponent } from './global-client-facing-headcount/global-client-facing-headcount.component';
import { StaffingInsightsComponent } from './staffing-insights/staffing-insights.component';

type PathMatch = "full" | "prefix" | undefined; //support Angular 14 upgrade 

const routes: Routes = [
  { path: '', component: AnalyticsComponent, children: [
    {path: '',  redirectTo: 'weeklyDeploymentSummaryView', pathMatch: 'full' as PathMatch},
    {path: 'staffingAllocation', component: StaffingReportComponent, canActivate: [AuthGuardService] },
    {path: 'staffingAllocationsMonthly', component: StaffingAllocationsMonthlyComponent, canActivate: [AuthGuardService] },
    {path: 'historicalAllocationsForPromotions', component: HistoricalAllocationsForPromotionsComponent, canActivate: [AuthGuardService] },
    {path: 'availability', component: ResourceAvailabilityComponent, canActivate: [AuthGuardService] },
    {path: 'dailyDeploymentDetails', component: DailyDeploymentDetailsComponent, canActivate: [AuthGuardService] },
    {path: 'weeklyDeploymentIndividualResourceDetails', component: IndividualResourceDetailsComponent, canActivate: [AuthGuardService] },
    {path: 'weeklyDeploymentSummaryView', component: WeeklyDeploymentSummaryComponent, canActivate: [AuthGuardService] },
    {path: 'monthlyDeployment', component: MonthlyDeploymentComponent, canActivate: [AuthGuardService] },
    {path: 'monthlyFTEUtilization', component: MonthlyFteUtilizationComponent, canActivate: [AuthGuardService] },
    {path: 'monthlyFTEUtilizationIndividual', component: MonthlyFteUtilizationIndividualComponent, canActivate: [AuthGuardService] },
    {path: 'practiceStaffing', component: MonthlyPracticeAreaStaffingComponent, canActivate: [AuthGuardService] },
    {path: 'commitmentDetails', component: CommitmentDetailsComponent, canActivate: [AuthGuardService] },
    {path: 'timeInLane', component: TimeInLaneComponent, canActivate: [AuthGuardService] },
    {path: 'practiceStaffingCaseServed', component: PracticeStaffingCaseServedComponent, canActivate: [AuthGuardService] },
    {path: 'ringfenceStaffing', component: RingfenceStaffingComponent, canActivate: [AuthGuardService] },
    {path: 'overAllocatedAudit', component: OverAllocatedAuditComponent, canActivate: [AuthGuardService] },
    {path: 'allocatedWhileOnLOAAudit', component: AllocatedOnLoaAuditComponent, canActivate: [AuthGuardService] },
    {path: 'caseUpdatesAudit', component: CaseUpdatesAuditComponent, canActivate: [AuthGuardService] },
    {path: 'priceRealization', component: PriceRealizationComponent, canActivate: [AuthGuardService] },
    {path: 'caseEconomics', component: CaseEconomicsComponent, canActivate: [AuthGuardService] },
    {path: 'testResponsiveReport', component: TestResponsiveReportComponent, canActivate: [AuthGuardService] },
    {path: 'whoWorkedWithWhom', component: WhoWorkedWithWhomComponent, canActivate: [AuthGuardService]},
    {path: 'affiliateTimeInPractice', component: AffiliateTimeInPracticeComponent, canActivate: [AuthGuardService]},
    {path: 'smapAllocations', component: SmapAllocationsComponent, canActivate: [AuthGuardService]},
    {path: 'caseExperience', component: CaseExperienceComponent, canActivate: [AuthGuardService]},
    {path:'globalCapacitySummary', component: GlobalCapacitySummary,canActivate: [AuthGuardService]},
    {path:'globalClientFacingHeadcount', component: GlobalClientFacingHeadcountComponent,canActivate: [AuthGuardService]},
    {path: 'staffingInsights', component: StaffingInsightsComponent, canActivate: [AuthGuardService]}
  ] }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class AnalyticsRoutingModule { }
