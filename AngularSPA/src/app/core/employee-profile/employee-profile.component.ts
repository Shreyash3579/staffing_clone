import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Employee } from '../../shared/interfaces/employee.interface';
import { CoreService } from '../core.service';
import { CommonService } from 'src/app/shared/commonService';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-employee-profile',
  templateUrl: './employee-profile.component.html',
  styleUrls: ['./employee-profile.component.scss']
})
export class EmployeeProfileComponent implements OnInit, OnDestroy {
  @Input() employee: Employee;
  @Input() roles: string[];
  @Output() openSupplySettingsPopover = new EventEmitter<any>();
  @Output() openOfficeClosurePopover = new EventEmitter<any>();
  disableUserProfile = false;
  appScreens: any;
  isHideBulkUpdate = environment.settings.hideBulkUpdate;
  
  constructor(private coreService: CoreService) { }

  ngOnInit() {
    this.appScreens = ConstantsMaster.appScreens;
    this.disableUserProfile = !CommonService.hasAccessToFeature(this.appScreens.feature.staffingSettings, this.coreService.loggedInUserClaims?.FeatureAccess.map(x => x.FeatureName));
  }

  openSupplySettingsForm() {
    this.openSupplySettingsPopover.emit();
  }

  openOfficeClosureForm(){
    this.openOfficeClosurePopover.emit();
  }

  ngOnDestroy(): void {
  }

}
