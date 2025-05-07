import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";

// modules
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { StaffingResourcePopupRoutingModule } from "./staffing-resource-popup-routing.module";

// services
import { StaffingResourcePopupService } from "./staffing-resource-popup.service";
import { DateService } from "../shared/dateService";
import { CommonService } from "../shared/commonService";

// components
import { StaffingResourcePopupComponent } from "./staffing-resource-popup.component";
import { ResourceHeaderComponent } from "./resource-header/resource-header.component";
import { WeeklyHeaderComponent } from "./weekly-header/weekly-header.component";
import { ResourceBodyComponent } from "./resource-body/resource-body.component";
import { CommitmentsBodyComponent } from "./commitments-body/commitments-body.component";
import { CommitmentComponent } from "./commitment/commitment.component";
import { ProfileImageComponent } from "../core/profile-image/profile-image.component";

@NgModule({
  declarations: [
    StaffingResourcePopupComponent,
    ResourceHeaderComponent,
    WeeklyHeaderComponent,
    ResourceBodyComponent,
    CommitmentsBodyComponent,
    CommitmentComponent
  ],
  imports: [CommonModule, MatProgressSpinnerModule, StaffingResourcePopupRoutingModule, ProfileImageComponent],
  exports: [StaffingResourcePopupComponent],
  providers: [StaffingResourcePopupService, DateService, CommonService]
})
export class StaffingResourcePopupModule {}
