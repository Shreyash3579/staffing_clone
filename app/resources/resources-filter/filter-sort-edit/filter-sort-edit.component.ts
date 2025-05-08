import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { SortByComponent } from '../sort-by/sort-by.component';
import { FilterByComponent } from '../filter-by/filter-by.component';
import { NotificationService } from 'src/app/shared/notification.service';
import { SortRow } from 'src/app/shared/interfaces/sort-row.interface';
import { ValidationService } from 'src/app/shared/validationService';
import { FilterRow } from 'src/app/shared/interfaces/filter-row.interface';
import { ResourceViewCD } from 'src/app/shared/interfaces/resource-view-cd.interface';
import { ResourceViewCommercialModel } from 'src/app/shared/interfaces/resource-view-commercial-model.interface';

@Component({
  selector: 'app-filter-sort-edit',
  templateUrl: './filter-sort-edit.component.html',
  styleUrls: ['./filter-sort-edit.component.scss']
})
export class FilterSortEditComponent implements OnInit, OnChanges {

  @ViewChild("editSortBy", { static: false }) sortByComponent: SortByComponent;
  @ViewChild("editFilterBy", { static: false }) filterByComponent: FilterByComponent;

  @Input() type: string;
  @Input() sortRows : SortRow[] = [];
  @Input() filterBy: any[] = [];
  @Input() resourcesRecentCDList:ResourceViewCD[] = [];
  @Input() resourcesCommercialModelList:ResourceViewCommercialModel[] = [];

  @Output() applyChangesEmitter = new EventEmitter();
  @Output() cancelChangesEmmitter = new EventEmitter();

  _sortRows: SortRow[];
  showHeader: boolean = false;
  isDropdownOpen: boolean = false;
  label: string = "";
  lengthOfArray: number = 0;
  validationMessages: string[] = [];

  constructor(private notifyService: NotificationService) { }

  ngOnInit(): void {
  }

  ngOnChanges(change: SimpleChanges) {

    if (change.sortRows) {
      this.getOptionsData();
      this.isValidSortByArray(this.sortRows);
    }
    if (change.filterBy ) {
      this.getOptionsData();
      this.isValidFilterByArray(this.filterBy)
    }
    if(change.resourcesCommercialModelList && this.resourcesCommercialModelList){
      console.log();
    }

  }
  

  getOptionsData() {
    switch (this.type) {
      case "sort":
        this.label = "Sort";
        this.lengthOfArray = !this.sortRows ? 0 : this.sortRows.length;
        break;
      case "filter":
        this.label = "Filter";
        this.lengthOfArray = !this.filterBy ? 0 : this.filterBy.length;
        break;
    }
  }

  toggleDropdownMenu() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  // Delete all
  deleteAll() {
    if (this.type == 'sort') {
      this.sortRows = [];
      this.applyChangesEmitter.emit({ type: this.type, data: this.sortRows });
    } else if (this.type == 'filter') {
      this.filterBy = [];
      this.applyChangesEmitter.emit({ type: this.type, data: this.filterBy });
    } else {
      return;
    }

    this.getOptionsData();
    this.notifyService.showSuccess(`Your ${this.type} options have been deleted.`);
    this.toggleDropdownMenu();
  }

  // Apply changes
  apply() {

    if (this.type == 'sort') {
      if(!this.isValidSortByArray(this.sortByComponent.sortRows))
        return;

      this.sortRows = this.sortByComponent.sortRows;
      this.applyChangesEmitter.emit({ type: this.type, data: this.sortRows });
    } else if (this.type == 'filter') {
      if(!this.isValidFilterByArray(this.filterByComponent.filterBy))
        return;

      this.filterBy = this.filterByComponent.filterBy;
      this.applyChangesEmitter.emit({ type: this.type, data: this.filterBy });
    } else {
      return;
    }

    this.getOptionsData();
    this.notifyService.showSuccess(`Your ${this.type} changes have been applied.`);
    this.toggleDropdownMenu();
  }

  cancelChanges() {
    this.cancelChangesEmmitter.emit(this.type)
    this.isDropdownOpen = false;
    this.validationMessages = [];
  }

  isValidSortByArray(sortRows: SortRow[]){
    let isValid = true;
    if(sortRows.some( x=> !x.field)){
      isValid = false;
      this.addToValidationMessages(ValidationService.sortByValidationMessage);
    }else{
      isValid = true;
      this.removeFromValidationMessages(ValidationService.sortByValidationMessage);
    }

    return isValid;
  }

  isValidFilterByArray(filterRows : FilterRow[]){
    let isValid = true;
    if(filterRows.some( x=> !x.filterField) || filterRows.some( x=> ValidationService.isNullEmptyOrUndefined(x.filterValue)) ||  filterRows.some( x=> !x.filterOperator)){
      isValid = false;
      this.addToValidationMessages(ValidationService.filterByValidationMessage);
    }else{
      isValid = true;
      this.removeFromValidationMessages(ValidationService.filterByValidationMessage);
    }

    return isValid;
  }

  addToValidationMessages(validationMsg: string){
    if(!this.validationMessages.some( x => x == validationMsg))
      this.validationMessages.push(validationMsg);
  }

  removeFromValidationMessages(validationMsg: string){
    this.validationMessages = this.validationMessages.filter( x => x != validationMsg);
  }
}
