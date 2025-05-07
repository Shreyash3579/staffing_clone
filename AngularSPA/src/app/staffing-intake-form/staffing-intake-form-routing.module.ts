import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StaffingIntakeFormComponent } from './staffing-intake-form.component';

const routes: Routes = [
    {
        path: '',
        component: StaffingIntakeFormComponent
    }
  ];
  @NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
  })
export class StaffingIntakeFormRoutingModule { }
