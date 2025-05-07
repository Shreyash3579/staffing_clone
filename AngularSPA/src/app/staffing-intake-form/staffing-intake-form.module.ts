import { CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA, NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { StoreModule } from "@ngrx/store";

//--------------------Material Imports-------------------------------------------------
import { DragDropModule } from "@angular/cdk/drag-drop";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatOptionModule } from "@angular/material/core";
import { MatIconModule } from "@angular/material/icon";
import { StaffDetailsFormComponent } from "./staff-details-form/staff-details-form.component";
import { RoleFormComponent } from "./staff-details-form/role-form/role-form.component";
import { LeadershipComponentComponent } from "./leadership-component/leadership-component.component";
import { EarlyInputFormComponent } from "./early-input-form/early-input-form.component";
import { StaffingIntakeFormComponent } from "./staffing-intake-form.component";
import { StaffingIntakeFormService } from "./staffing-intake-form.service";
import { MatExpansionModule } from "@angular/material/expansion";
import { NgSelectModule } from "@ng-select/ng-select";
import { staffingIntakeReducer } from "./State/staffing-intake.reducer";
import { EffectsModule } from "@ngrx/effects";
import { StaffingIntakeEffects } from "./State/staffing-intake.effects";
import { SingleSelectDropdownComponent } from "../standalone-components/single-select-dropdown/single-select-dropdown.component";
import { MultiSelectDropdownComponent } from "../standalone-components/multi-select-dropdown/multi-select-dropdown.component";
import { StaffingIntakeFormRoutingModule } from "./staffing-intake-form-routing.module";
import { LeadershipRoleComponent } from "./leadership-component/leadership-role/leadership-role.component";
import { ResourcesTypeahedComponent } from "../standalone-components/resources-typeahed/resources-typeahed.component";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { WorkstreamComponent } from "./staff-details-form/workstream/workstream.component";
import { PageNotFoundComponent } from "./page-not-found/page-not-found.component";
import { PlacesTypeahedComponent } from "../standalone-components/places-typeahead/places-typeahed.component";

@NgModule({
  declarations: [
    StaffDetailsFormComponent,
    RoleFormComponent,
    WorkstreamComponent,
    LeadershipComponentComponent,
    LeadershipRoleComponent,
    EarlyInputFormComponent,
    StaffingIntakeFormComponent,
    PageNotFoundComponent
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
  imports: [
    StaffingIntakeFormRoutingModule,
    CommonModule,
    ReactiveFormsModule,
    MatOptionModule,
    MatIconModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatExpansionModule,
    DragDropModule,
    NgSelectModule,
    MatOptionModule,
    MatIconModule,
    MatProgressBarModule,
    StoreModule.forFeature('staffingIntake', staffingIntakeReducer),
    EffectsModule.forFeature([ StaffingIntakeEffects]),
    SingleSelectDropdownComponent,
    MultiSelectDropdownComponent,
    ResourcesTypeahedComponent,
    PlacesTypeahedComponent
  ],
  exports: [StaffingIntakeFormComponent],
  providers: [
    StaffingIntakeFormService
  ]
})
export class StaffingIntakeFormModule {}
