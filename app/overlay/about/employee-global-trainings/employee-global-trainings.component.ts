import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { DateService } from 'src/app/shared/dateService';
import { Training } from "src/app/shared/interfaces/training";

@Component({
  selector: 'app-employee-global-trainings',
  templateUrl: './employee-global-trainings.component.html',
  styleUrls: ['./employee-global-trainings.component.scss']
})
export class EmployeeGlobalTrainingsComponent implements OnInit, OnDestroy {

  @Input() globalTrainingsData: Training[];

  constructor() { }

  ngOnInit(): void {
  }

  ngOnDestroy() {
    this.globalTrainingsData = null;
  }

  getFormattedDate(date: string) {
    if (date == null) {
      return null;
    }
    return DateService.convertDateInBainFormat(date);
  }

}
