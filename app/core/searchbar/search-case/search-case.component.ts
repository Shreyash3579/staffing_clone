import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-search-case',
  templateUrl: './search-case.component.html',
  styleUrls: ['./search-case.component.scss']
})
export class SearchCaseComponent implements OnInit {

  @Input() searchedCase;
  @Output() openProjectDetailsDialog = new EventEmitter<string>();
  
  constructor() { }

  ngOnInit(): void {
  }

  openProjectDetailsDialogHandler(){
    this.openProjectDetailsDialog.emit(this.searchedCase.caseCode);
  }

}
