import { Component, Input, OnInit } from '@angular/core';
import { StaffingInsightsToolDisclaimerComponent } from '../staffing-insights-tool-disclaimer/staffing-insights-tool-disclaimer.component';
import { StaffingInsightsToolComponent } from '../staffing-insights-tool.component';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { EmployeeBasic } from 'src/app/shared/interfaces/employee.interface';

@Component({
  selector: 'app-staffing-insights-tool-container',
  standalone: true,
  imports: [StaffingInsightsToolDisclaimerComponent, StaffingInsightsToolComponent],
  templateUrl: './staffing-insights-tool-container.component.html',
  styleUrl: './staffing-insights-tool-container.component.scss'
})
export class StaffingInsightsToolContainerComponent implements OnInit{
  @Input() employee: EmployeeBasic;
  
  isDisclaimerRead: boolean = false;
  constructor(private localStorageService: LocalStorageService){}
  
  ngOnInit(): void {
    this.isDisclaimerRead = this.localStorageService.get("isStaffingInsightsToolDisclaimerRead");
  }

  setDisclaimerRead(){
    this.isDisclaimerRead = true;
    this.localStorageService.set("isStaffingInsightsToolDisclaimerRead", true);
  }

}
