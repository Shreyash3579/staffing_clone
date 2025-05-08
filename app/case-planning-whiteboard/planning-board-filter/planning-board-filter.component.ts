import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { DateService } from 'src/app/shared/dateService';

@Component({
  selector: 'app-planning-board-filter',
  templateUrl: './planning-board-filter.component.html',
  styleUrls: ['./planning-board-filter.component.scss']
})
export class PlanningBoardFilterComponent implements OnInit {
  minStartDate: Date;
  @Output() onSearchItemSelect = new EventEmitter();

  constructor() { }

  ngOnInit(): void {
   this.minStartDate = DateService.getStartOfWeek();
  }

  onSearchItemSelectHandler(selectedProject){
    this.onSearchItemSelect.emit(selectedProject);
  }

}
