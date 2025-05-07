import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CasePlanningComponent } from './case-planning.component';

const routes: Routes = [
  {
    path: '', component: CasePlanningComponent
  }
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CasePlanningRoutingModule { }
