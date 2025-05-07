import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

// components
import { StaffingResourcePopupComponent } from "./staffing-resource-popup.component";

const routes: Routes = [
  {
    path: "",
    component: StaffingResourcePopupComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StaffingResourcePopupRoutingModule {}
