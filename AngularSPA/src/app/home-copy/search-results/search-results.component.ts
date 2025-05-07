import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { EmployeeSearchResult } from "src/app/shared/interfaces/employeeSearchResult";

@Component({
  selector: "app-search-results",
  templateUrl: "./search-results.component.html",
  styleUrls: ["./search-results.component.scss"]
})
export class SearchResultsComponent implements OnInit {
  @Input() searchDocument: EmployeeSearchResult;
  @Output() openResourceDetailsDialog = new EventEmitter<string>();
  
  constructor() { }

  ngOnInit(): void {
  }

  openResourceDetailsDialogHandler(){
    this.openResourceDetailsDialog.emit(this.searchDocument.employeeCode);
  }

}
