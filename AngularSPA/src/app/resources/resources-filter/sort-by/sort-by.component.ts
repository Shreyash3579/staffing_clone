import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, Input, OnInit } from '@angular/core';
import { ResourceFiltersBasicMenu } from 'src/app/shared/interfaces/resource-filters-basic-menu.interface';
import { SortRow } from 'src/app/shared/interfaces/sort-row.interface';
import { NotificationService } from 'src/app/shared/notification.service';

@Component({
  selector: 'app-sort-by',
  templateUrl: './sort-by.component.html',
  styleUrls: ['./sort-by.component.scss']
})
export class SortByComponent implements OnInit {

  @Input() menuPosition: string = "down";
  @Input() showHeader: boolean = true;
  @Input()
  set rowsToEdit(value: SortRow[]) {
    this._rowsToEdit = value;
    this.sortRows = this._rowsToEdit;
    this.setFilteredSortFieldOptions();
  }
  get rowsToEdit() {
    return this._rowsToEdit;
  }

  public _rowsToEdit: SortRow[];

  sortRows: SortRow[] = [];

  sortFieldOptions: ResourceFiltersBasicMenu[] = [
    { label: "Employee", value: "fullName", selected: false, isHidden: false },
    { label: "Position", value: "position", selected: false, isHidden: false },
    { label: "Level", value: "levelGrade", selected: false, isHidden: false },
    { label: "Office", value: "office", selected: false, isHidden: false },
    { label: "Availability Percentage", value: "percentAvailable", selected: false, isHidden: false },
    { label: "Availability Date", value: "dateFirstAvailable", selected: false, isHidden: false },
    { label: "Hire Date", value: "startDate", selected: false, isHidden: false },
    { label: "Last Staffed on Billable", value: 'lastBillableDate', selected: false, isHidden: false },
    { label: "Office Cluster", value: "officeCluster", selected: false, isHidden: false },
    {label:"Commitment Start Date", value:"commitmentStartDate", selected:false, isHidden:false},
  ];

  sortDirectionOptions: ResourceFiltersBasicMenu[] = [
    { label: "Ascending", value: "asc", selected: true },
    { label: "Descending", value: "desc", selected: false }
  ]

  constructor(private notifyService: NotificationService) { }

  ngOnInit(): void {
    if (this.rowsToEdit) {
      this.sortRows = this.rowsToEdit;
    } else {
      this.sortRows = [];
    }
  }

  handleSortingFieldSelection(field: ResourceFiltersBasicMenu, sortRowIndex: number) {
    this.sortRows[sortRowIndex].field = field.value;
    this.setFilteredSortFieldOptions();
  }

  setFilteredSortFieldOptions() {
    if (!!this.sortRows) {
      this.sortFieldOptions.forEach(option => {
        option.isHidden = this.sortRows.some(y => y.field == option.value);
        return option;
      });
    }
  }

  handleSortingDirectionelection(direction: ResourceFiltersBasicMenu, sortRowIndex: number) {
    this.sortRows[sortRowIndex].direction = direction.value;
  }

  // Add new sort row
  addSortRow() {
    this.sortRows.push(
      { field: "", direction: "asc" }
    );
  }

  // Delete single or all sort row(s)
  deleteRow(index) {
    this.sortRows.splice(index, 1);
    this.setFilteredSortFieldOptions();
  }
  deleteAll() {
    this.sortRows = [];
    this.setFilteredSortFieldOptions();
    this.showSuccessNotification("All sort conditions have been deleted.");
  }

  // Drop sort row
  drop(event: CdkDragDrop<string[]>) {
    try {
      moveItemInArray(this.sortRows, event.previousIndex, event.currentIndex);
    } catch (e) {
      console.error("Error moving sort row", e);
    }
  }

  showSuccessNotification(message: string) {
    this.notifyService.showSuccess(message);
  }
}
