// ----------------------- Angular Package References ----------------------------------//
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

// -----------------------Angular  Module References ----------------------------------//
import { OverlayRoutingModule } from './overlay-routing.module';
import { SharedModule } from '../shared/shared.module';

// -----------------------External  Module References ----------------------------------//
import { AgGridModule } from 'ag-grid-angular';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MaterialModule } from '../shared/material.module';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { InfiniteScrollModule } from 'ngx-infinite-scroll';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { FeatureAccessModule } from '../feature-access/feature-access.module';

// ----------------------- Component References ----------------------------------//
import { AgGridNotesComponent } from './ag-grid-notes/ag-grid-notes.component';
import { AgGridSplitAllocationPopUpComponent } from './ag-grid-split-allocation-pop-up/ag-grid-split-allocation-pop-up.component';
import { GanttCaseComponent } from './gantt/gantt-case/gantt-case.component';
import { GanttResourceComponent } from './gantt/gantt-resource/gantt-resource.component';
import { ResourceOverlayComponent } from './resource-overlay/resource-overlay.component';
import { OverlayComponent } from './overlay.component';
import { ProjectOverlayComponent } from './project-overlay/project-overlay.component';
import { PlanningCardOverlayComponent } from './planning-card-overlay/planning-card-overlay.component';
import { ProjectHeaderComponent } from './project-overlay/project-header/project-header.component';
import { ResourceOverlayHeaderComponent } from './resource-overlay/resource-overlay-header/resource-overlay-header.component';
import { SkuTabListComponent } from './sku-tab-list/sku-tab-list.component';
import { SkuTabComponent } from './sku-tab/sku-tab.component';
import { AboutComponent } from './about/about.component';
import { EmployeeCertificateComponent } from './about/employee-certificate/employee-certificate.component';
import { EmployeeLanguageComponent } from './about/employee-language/employee-language.component';
import { EmployeeLevelGradeChangesComponent } from './about/employee-level-grade-changes/employee-level-grade-changes.component';
import { EmployeeGlobalTrainingsComponent } from './about/employee-global-trainings/employee-global-trainings.component';
import { EmployeeSchoolHistoryComponent } from './about/employee-school-history/employee-school-history.component';
import { EmployeeWorkHistoryComponent } from './about/employee-work-history/employee-work-history.component';
import { PreferencesComponent } from './preferences/preferences.component';
import { PreferenceListComponent } from './preferences/preference-list/preference-list.component';
import { EmployeeStaffableAsListComponent } from './about/employee-staffable-as-list/employee-staffable-as-list.component';
import { EmployeeStaffableAsComponent } from './about/employee-staffable-as-list/employee-staffable-as/employee-staffable-as.component';
import { CaseEconomicsComponent } from './case-economics/case-economics.component';
import { ResourceRatingsComponent } from './resource-ratings/resource-ratings.component';

// ----------------------- Service References ----------------------------------//
import { BackfillDialogService } from './dialogHelperService/backFillDialog.service';
import { CaseRollService } from './behavioralSubjectService/caseRoll.service';
import { CaseRollDialogService } from './dialogHelperService/caseRollDialog.service';
import { staCommitmentDialogService } from './dialogHelperService/staCommitmentCaseOppDialog.service';
import { CommitmentsService } from './../shared/commitments.service';
import { NotesDialogService } from './dialogHelperService/notesDialog.service';
import { OpportunityService } from './behavioralSubjectService/opportunity.service';
import { OverlayMessageService } from './behavioralSubjectService/overlayMessage.service';
import { OverlayDialogService } from './dialogHelperService/overlayDialog.service';
import { OverlayService } from './overlay.service';
import { PlaceholderAssignmentService } from './behavioralSubjectService/placeholderAssignment.service';
import { QuickAddDialogService } from './dialogHelperService/quickAddDialog.service';
import { PlaceholderDialogService } from './dialogHelperService/placeholderDialog.service';
import { ResourceAssignmentService } from './behavioralSubjectService/resourceAssignment.service';
import { ResourceCommitmentService } from './behavioralSubjectService/resourceCommitment.service';
import { SkuCaseTermService } from './behavioralSubjectService/skuCaseTerm.service';
import { SplitAllocationDialogService } from './dialogHelperService/splitAllocationDialog.service';
import { UserPreferenceService } from './behavioralSubjectService/userPreference.service';
import { ResourcesCommitmentsDialogService } from './dialogHelperService/resourcesCommitmentsDialog.service';
import { ResourceStaffableAsService } from './behavioralSubjectService/resourceStaffableAs.service';
import { ShowQuickPeekDialogService } from './dialogHelperService/show-quick-peek-dialog.service';
import { OverlappedTeamDialogService } from './dialogHelperService/overlapped-team-dialog.service';
import { EmployeeTransferComponent } from './about/employee-transfer/employee-transfer.component';
import { PegOpportunityDialogService } from './dialogHelperService/peg-opportunity-dialog.service';
import { CasePlanningWhiteboardService } from '../case-planning-whiteboard/case-planning-whiteboard.service';
import { EffectsModule } from '@ngrx/effects';
import { ResourceOverlayEffects } from './State/effects/resource-overlay.effects';
import { StoreModule } from '@ngrx/store';
import { resourceOverlayReducer } from './State/reducer/resource-overlay.reducer';
import { PlanningCardHeaderComponent } from './planning-card-overlay/planning-card-header/planning-card-header.component';
import { GanttPlanningCardComponent } from './gantt/gantt-planning-card/gantt-planning-card.component';
import { SharePlanningCardDialogService } from './dialogHelperService/share-planning-card-dialog.service';
import { ProfileImageComponent } from '../core/profile-image/profile-image.component';
import { StaffingInsightsToolContainerComponent } from '../staffing-insights-tool/staffing-insights-tool-container/staffing-insights-tool-container.component';
import { addPlanningCardDialogService } from './dialogHelperService/addPlanningCardDialog.service';
import { StaffingIntakeFormModule } from '../staffing-intake-form/staffing-intake-form.module';

@NgModule({
    declarations: [
        ResourceOverlayComponent,
        ProjectOverlayComponent,
        PlanningCardOverlayComponent,
        PlanningCardHeaderComponent,
        GanttResourceComponent,
        GanttCaseComponent,
        GanttPlanningCardComponent,
        ProjectHeaderComponent,
        SkuTabListComponent,
        SkuTabComponent,
        OverlayComponent,
        AgGridNotesComponent,
        AgGridSplitAllocationPopUpComponent,
        ResourceOverlayHeaderComponent,
        AboutComponent,
        EmployeeCertificateComponent,
        EmployeeLanguageComponent,
        EmployeeLevelGradeChangesComponent,
        EmployeeGlobalTrainingsComponent,
        EmployeeSchoolHistoryComponent,
        EmployeeWorkHistoryComponent,
        PreferencesComponent,
        PreferenceListComponent,
        EmployeeStaffableAsListComponent,
        EmployeeStaffableAsComponent,
        CaseEconomicsComponent,
        ResourceRatingsComponent,
        EmployeeTransferComponent,
        ResourceOverlayComponent,
        ProjectOverlayComponent,
        AgGridNotesComponent,
        AgGridSplitAllocationPopUpComponent
    ],
    imports: [
        CommonModule,
        OverlayRoutingModule,
        MatSlideToggleModule,
        MaterialModule,
        SharedModule,
        InfiniteScrollModule,
        ScrollingModule,
        AgGridModule,
        TabsModule.forRoot(),
        FeatureAccessModule,
        EffectsModule.forFeature([ResourceOverlayEffects]),
        StoreModule.forFeature('overlay', resourceOverlayReducer),
        ProfileImageComponent,
        StaffingInsightsToolContainerComponent,
        StaffingIntakeFormModule
    ],
    exports: [
        ResourceOverlayComponent,
        ProjectOverlayComponent,
    ],
    providers: [
        OverlayService,
        ResourceAssignmentService,
        ResourceCommitmentService,
        SkuCaseTermService,
        UserPreferenceService,
        OverlayMessageService,
        OpportunityService,
        CaseRollService,
        CommitmentsService,
        OverlayDialogService,
        QuickAddDialogService,
        PlaceholderDialogService,
        NotesDialogService,
        CaseRollDialogService,
        staCommitmentDialogService,
        BackfillDialogService,
        OverlappedTeamDialogService,
        SplitAllocationDialogService,
        PlaceholderAssignmentService,
        ResourcesCommitmentsDialogService,
        ResourceStaffableAsService,
        ShowQuickPeekDialogService,
        PegOpportunityDialogService,
        CasePlanningWhiteboardService,
        SharePlanningCardDialogService,
        addPlanningCardDialogService
    ],
    // schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA]
})
export class OverlayModule { }
