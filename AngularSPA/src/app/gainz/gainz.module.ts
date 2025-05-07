import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { GainzRoutingModule } from "./gainz-routing.module";
import {ChatAssistantComponent} from "./chat-assistant/chat-assistant.component";
import {SidebarProjectListComponent} from "../standalone-components/sidebar-project-list/sidebar-project-list.component";
import {MatTab, MatTabGroup, MatTabLabel} from "@angular/material/tabs";
import {FormsModule} from "@angular/forms";
import {StaffingPlanComponent} from "./staffing-plan/staffing-plan.component";
import {ResourceDetailsTooltipComponent} from "../standalone-components/resource-details-tooltip/resource-details-tooltip.component";

@NgModule({
  declarations: [ChatAssistantComponent, StaffingPlanComponent],
  imports: [
    CommonModule,
    GainzRoutingModule,
    SidebarProjectListComponent,
    MatTabGroup,
    MatTab,
    FormsModule,
    MatTabLabel,
    ResourceDetailsTooltipComponent,
  ]
})
export class GainzModule { }
