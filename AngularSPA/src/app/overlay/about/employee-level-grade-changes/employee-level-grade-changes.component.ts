import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { DateService } from 'src/app/shared/dateService';
import { LevelGradeTransactionModel } from 'src/app/shared/interfaces/level-grade-transaction.interface';

@Component({
  selector: 'app-employee-level-grade-changes',
  templateUrl: './employee-level-grade-changes.component.html',
  styleUrls: ['./employee-level-grade-changes.component.scss']
})
export class EmployeeLevelGradeChangesComponent implements OnInit, OnDestroy {

  @Input() levelGradeHistoryData: LevelGradeTransactionModel[];
  constructor() { }

  ngOnInit(): void {
  }

  ngOnDestroy() {
    this.levelGradeHistoryData = null;
  }

  getFormattedDate(date: string) {
    if (date == null) {
      return null;
    }
    return DateService.convertDateInBainFormat(date);
  }

}
