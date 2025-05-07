import { CUSTOM_ELEMENTS_SCHEMA, NgModule, NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { OfficeDropdownComponent } from './office-dropdown/office-dropdown.component';
import { TreeviewModule } from '@soy-andrey-semyonov/ngx-treeview';
import { MultiSelectDropdownComponent } from './multi-select-dropdown/multi-select-dropdown.component';
import { SingleSelectDropdownComponent } from './single-select-dropdown/single-select-dropdown.component';
import { PaginatorComponent } from './paginator/paginator.component';
import { ProjectActionPanelComponent } from './project-action-panel/project-action-panel.component';
import { AppBootstrapModule } from './app-bootstrap.module';
import { AgGridDatepickerComponent } from './ag-grid-datepicker/ag-grid-datepicker.component';
import { QuickAddFormComponent } from './quick-add-form/quick-add-form.component';
import { BackfillFormComponent } from './backfill-form/backfill-form.component';
import { SharedService } from './shared.service';
import { SystemconfirmationFormComponent } from './systemconfirmation-form/systemconfirmation-form.component';
import { CaseRollFormComponent } from './case-roll-form/case-roll-form.component';
import { STACommitmentFormComponent } from './sta-commitment-form/sta-commitment.form.component';
import { SlideBarComponent } from './slide-bar/slide-bar.component';
import { IncrementDecrementButtonsComponent } from './increment-decrement-buttons/increment-decrement-buttons.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { PopupDragService } from './services/popupDrag.service';
import { UpdateDateFormComponent } from './update-date-form/update-date-form.component';
import { DecimalRoundingPipe } from './pipes/decimal-rounding.pipe';
import { SearchCaseOppComponent } from './search-case-opp/search-case-opp.component';
import { AddSecurityUserFormComponent } from './add-security-user-form/add-security-user-form.component';
import { SharePlanningCardComponent } from './share-planning-card/share-planning-card.component';
import { LoaderComponent } from './loader/loader.component';
import { StaffingContextMenuComponent } from './context-menu/context-menu.component';
import { ResourcesCommitmentsComponent } from './resources-commitments/resources-commitments.component';
import { CommitmentsService } from './commitments.service';
import { HtmlGridComponent } from './html-table-grid/html-table-grid.component';
import { QuickPeekComponent } from './quick-peek/quick-peek.component';
import { SidebarFiltersComponent } from './side-bar-filters/side-bar-filters.component';
import { MatExpansionModule } from '@angular/material/expansion';
import { ExpansionPanelComponent } from './expansion-panel/expansion-panel.component';
import { RangeSliderComponent } from './range-slider/range-slider.component';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { SavedResourceFiltersComponent } from './side-bar-filters/saved-resource-filters/saved-resource-filters.component';
import { SavedResourceFilterDetailComponent } from './side-bar-filters/saved-resource-filters/saved-resource-filter-detail/saved-resource-filter-detail.component';
import { InlineEditableDateComponent } from './inline-editable-date/inline-editable-date-component';
import { InlineEditableNotesComponent } from './inline-editable-notes/inline-editable-notes.component';
import { PlaceholderFormComponent } from './placeholder-form/placeholder-form.component';
import { DemandFiltersComponent } from './demand-filters/demand-filters.component';
import { PlanningCardTypeAheadComponent } from './planning-card-typeahead/planning-card-typeahead.component';
import { OverlappedTeamsFormComponent } from './overlapped-teams-form/overlapped-teams-form.component';
import { RestrictSpecialCharDirective } from './increment-decrement-buttons/restrict-special-char.directive';
import { FeatureAccessModule } from '../feature-access/feature-access.module';
import { CasesTypeaheadComponent } from './cases-typeahead/cases-typeahead.component';
import { AgGridMultiSelectComponent } from './ag-grid-multi-select/ag-grid-multi-select.component';
import { AgGridOfficeDropdownComponent } from './ag-grid-office-dropdown/ag-grid-office-dropdown.component';
import { ResourcesTypeaheadComponent } from './resources-typeahead/resources-typeahead.component';
import { CustomDropdownComponent } from "./custom-dropdown/custom-dropdown.component";
import { TooltipComponent } from "./tooltip/tooltip.component";
import { PegOpportunityComponent } from './peg-opportunity/peg-opportunity.component';
import { SortingSelectorDropdownComponent } from "./sorting-selector-dropdown/sorting-selector-dropdown.component";
import { HomeService } from '../home/home.service';
import { GanttNotesComponent } from './gantt-notes/gantt-notes.component';
import { GanttRecentCdComponent } from './gantt-recent-cd/gantt-recent-cd.component';
import { GanttCommercialModelComponent } from './gantt-commercial-model/gantt-commercial-model.component';
import { ResourcesSharePopupComponent } from './gantt-notes/resources-share-popup/resources-share-popup.component';
import { CustomCheckboxComponent } from "./custom-checkbox/custom-checkbox.component";
import { CreateGroupComponent } from './staffing-settings/create-group/create-group.component';
import { GroupMemberComponent } from './staffing-settings/group-member/group-member.component';
import { GroupSettingsComponent } from './staffing-settings/group-settings/group-settings.component';
import { ShareGroupComponent } from './staffing-settings/share-group/share-group.component';
import { GroupActionPanelComponent } from './staffing-settings/group-action-panel/group-action-panel.component';
import { DemandSettingsComponent } from './staffing-settings/demand-settings/demand-settings.component';
import { StaffingSettingsComponent } from './staffing-settings/staffing-settings.component';
import { SupplySettingsComponent } from './staffing-settings/supply-settings/supply-settings.component';
import { MatTabsModule } from '@angular/material/tabs';
import { ValidationMessageComponent } from './validation-message/validation-message.component';
import { NotesModalComponent } from './notes-modal/notes-modal.component';
import { PlanningCardService } from "../shared/services/planningCard.service";
import { SharedNotesModalComponent } from './shared-notes-modal/shared-notes-modal.component';
import { AddNewPlanningCardFormComponent } from './add-new-planning-card-form/add-new-planning-card-form.component';
import { OverlayModule } from '../overlay/overlay.module';
import { NotificationBannerComponent } from './notification-banner/notification-banner.component';

@NgModule({
    declarations: [
        OfficeDropdownComponent,
        MultiSelectDropdownComponent,
        SingleSelectDropdownComponent,
        PaginatorComponent,
        ProjectActionPanelComponent,
        AgGridDatepickerComponent,
        QuickAddFormComponent,
        BackfillFormComponent,
        SystemconfirmationFormComponent,
        CaseRollFormComponent,
        STACommitmentFormComponent,
        SlideBarComponent,
        IncrementDecrementButtonsComponent,
        UpdateDateFormComponent,
        DecimalRoundingPipe,
        SearchCaseOppComponent,
        SharePlanningCardComponent,
        LoaderComponent,
        StaffingContextMenuComponent,
        ResourcesCommitmentsComponent,
        QuickPeekComponent,
        HtmlGridComponent,
        SidebarFiltersComponent,
        ExpansionPanelComponent,
        RangeSliderComponent,
        SavedResourceFiltersComponent,
        SavedResourceFilterDetailComponent,
        AddSecurityUserFormComponent,
        InlineEditableDateComponent,
        InlineEditableNotesComponent,
        PlaceholderFormComponent,
        DemandFiltersComponent,
        OverlappedTeamsFormComponent,
        RestrictSpecialCharDirective,
        CasesTypeaheadComponent,
        AgGridMultiSelectComponent,
        AgGridOfficeDropdownComponent,
        ResourcesTypeaheadComponent,
        CustomDropdownComponent,
        PegOpportunityComponent,
        TooltipComponent,
        PlanningCardTypeAheadComponent,
        SortingSelectorDropdownComponent,
        GanttNotesComponent,
        GanttRecentCdComponent,
        GanttCommercialModelComponent,
        ResourcesSharePopupComponent,
        CustomCheckboxComponent,
        StaffingSettingsComponent,
        SupplySettingsComponent,
        DemandSettingsComponent,
        GroupSettingsComponent,
        CreateGroupComponent,
        ShareGroupComponent,
        GroupMemberComponent,
        GroupActionPanelComponent,
        ValidationMessageComponent,
        NotesModalComponent,
        SharedNotesModalComponent,
        AgGridDatepickerComponent,
        StaffingSettingsComponent,
        AddNewPlanningCardFormComponent
    ],
    exports: [
        OfficeDropdownComponent,
        MultiSelectDropdownComponent,
        SingleSelectDropdownComponent,
        PaginatorComponent,
        ProjectActionPanelComponent,
        QuickAddFormComponent,
        BackfillFormComponent,
        FormsModule,
        AppBootstrapModule,
        AgGridDatepickerComponent,
        SystemconfirmationFormComponent,
        CaseRollFormComponent,
         STACommitmentFormComponent,
        SlideBarComponent,
        IncrementDecrementButtonsComponent,
        UpdateDateFormComponent,
        DecimalRoundingPipe,
        AddSecurityUserFormComponent,
        LoaderComponent,
        StaffingContextMenuComponent,
        ResourcesCommitmentsComponent,
        QuickPeekComponent,
        HtmlGridComponent,
        SidebarFiltersComponent,
        RangeSliderComponent,
        NgSelectModule,
        InlineEditableDateComponent,
        InlineEditableNotesComponent,
        DemandFiltersComponent,
        RestrictSpecialCharDirective,
        CasesTypeaheadComponent,
        AgGridMultiSelectComponent,
        AgGridOfficeDropdownComponent,
        ResourcesTypeaheadComponent,
        CustomDropdownComponent,
        TreeviewModule,
        SortingSelectorDropdownComponent,
        GanttNotesComponent,
        GanttRecentCdComponent,
        GanttCommercialModelComponent,
        ResourcesSharePopupComponent,
        CustomCheckboxComponent,
        GroupSettingsComponent,
        CreateGroupComponent,
        ShareGroupComponent,
        GroupMemberComponent,
        GroupActionPanelComponent,
        ValidationMessageComponent,
        NotesModalComponent,
        AddNewPlanningCardFormComponent
    ],
    imports: [
        CommonModule,
        FormsModule,
        AppBootstrapModule,
        TreeviewModule.forRoot(),
        NgSelectModule,
        MatExpansionModule,
        TabsModule.forRoot(),
        FeatureAccessModule,
        MatTabsModule
    ],
    providers: [
        SharedService,
        PopupDragService,
        CommitmentsService,
        HomeService,
        PlanningCardService
    ],
    // schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA]
})
export class SharedModule { }
