import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AffiliationRole } from '../../interfaces/affiliationRole.interface';
import { CommitmentType } from '../../interfaces/commitmentType.interface';
import { Office } from '../../interfaces/office.interface';
import { PositionHierarchy } from '../../interfaces/positionHierarchy.interface';
import { PracticeArea } from '../../interfaces/practiceArea.interface';
import { ResourceFilter } from '../../interfaces/resource-filter.interface';
import { ServiceLine } from '../../interfaces/serviceLine.interface';
import { StaffableAsType } from '../../interfaces/staffableAsType.interface';

@Component({
  selector: 'app-saved-resource-filters',
  templateUrl: './saved-resource-filters.component.html',
  styleUrls: ['./saved-resource-filters.component.scss']
})
export class SavedResourceFiltersComponent implements OnInit {
  _savedResourceFilters: ResourceFilter[];
  searchTerm: string = '';
  filteredItems: ResourceFilter[];
  emptyLogMessage = 'No Saved Filters Found!';

  // -----------------------Input Events--------------------------------------------//
  @Input()
  set savedResourceFilters(value: ResourceFilter[]) {
    this._savedResourceFilters = this.filteredItems = value;
  }
  get savedResourceFilters() {
    return this._savedResourceFilters;
  }
  @Input() officeFlatList: Office[];
  @Input() staffingTags: ServiceLine[];
  @Input() commitmentTypes: CommitmentType[];
  @Input() practiceAreas: PracticeArea[];
  @Input() employeeStatus: any;
  @Input() sortsBy: any;
  @Input() filterConfig;
  @Input() staffableAsTypes: StaffableAsType[];
  @Input() positionsHierarchy: PositionHierarchy[];
  @Input() affiliationRoles: AffiliationRole[];

  // -----------------------Output Events--------------------------------------------//
  @Output() refreshView = new EventEmitter<ResourceFilter>();
  @Output() deleteSavedFilter = new EventEmitter<string>();
  @Output() setAsDefaultFilter = new EventEmitter<ResourceFilter[]>();

  constructor() { }

  ngOnInit(): void { }

  assignCopy() {
    this.filteredItems = Object.assign([], this.savedResourceFilters);
  }

  filterItem(value) {
    if (!value) {
      this.assignCopy();
    } // when nothing has typed
    this.filteredItems = Object.assign([], this.savedResourceFilters).filter(
      item => item.title?.toLowerCase().indexOf(value.toLowerCase()) > -1
    )
  }

  refreshViewHandler(event) {
    this.refreshView.emit(event);
  }

  deleteSavedFilterHandler(event) {
    this.deleteSavedFilter.emit(event);
    this.searchTerm = '';
  }

  setAsDefaultFilterHandler(event) {
    this.setAsDefaultFilter.emit(event);
  }

}
