import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { AdminRoutingModule } from './admin-routing.module';
import { AdminComponent } from './admin.component';
import { UserListComponent } from './users-list/users-list.component';
import { AdminService } from './admin.service';
import { SharedModule } from '../shared/shared.module';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { reducer } from './State/admin.reducer';
import { AdminEffects } from './State/admin.effects';
import { AgGridModule } from 'ag-grid-angular';
import { PracticeRingfencesComponent } from './practice-ringfences/practice-ringfences.component';

//---------------------------Material Imports-------------------------------------------------
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { GroupingComponent } from '../standalone-components/grouping/grouping.component';
import { GroupListComponent } from './groups-list/groups-list.component';
import { NotificationBannerComponent } from '../shared/notification-banner/notification-banner.component';

@NgModule({
  declarations: [
    AdminComponent,
    UserListComponent,
    GroupListComponent,
    PracticeRingfencesComponent
  ],
  imports: [
    AdminRoutingModule,
    CommonModule,
    SharedModule,
    MatProgressBarModule,
    StoreModule.forFeature('admin', reducer),
    EffectsModule.forFeature([AdminEffects]),
    AgGridModule,
    GroupingComponent,
    NotificationBannerComponent
  ],
  providers: [AdminService]
})
export class AdminModule { }
