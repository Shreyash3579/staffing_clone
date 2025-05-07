import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ResourcesSupplyFilterGroupEnum } from 'src/app/shared/constants/enumMaster';

@Component({
  selector: 'app-viewing-dropdown-menu',
  templateUrl: './viewing-dropdown-menu.component.html',
  styleUrls: ['./viewing-dropdown-menu.component.scss']
})
export class ViewingDropdownMenuComponent implements OnInit {
  // Inputs
  @Input()
  set viewingOptions(value: any[]) {
    this._viewingOptions = value;
    this.loadDataOnLoad();
  }
  get viewingOptions() {
    return this._viewingOptions;
  }

  // Outputs
  @Output() selectViewEmitter = new EventEmitter();
  @Output() createNewEmitter = new EventEmitter();
  @Output() editGroupEmitter = new EventEmitter();

  public _viewingOptions: any[];
  isDropdownOpen: boolean = false;
  groupNameQuery: string = "";

  selectedGroup: any;

  STAFFING_SETTINGS_FILTER_ENUM = ResourcesSupplyFilterGroupEnum.STAFFING_SETTINGS;
  SAVED_GROUP_FILTER_ENUM = ResourcesSupplyFilterGroupEnum.SAVED_GROUP;
  CUSTOM_GROUP_FILTER_ENUM = ResourcesSupplyFilterGroupEnum.CUSTOM_GROUP;

  constructor() { }

  ngOnInit(): void {
    this.loadDataOnLoad();
  }

  loadDataOnLoad() {
    this.setSelectedGroup();
    this.toggleEditButtonOnLoad();
  }
  
  setSelectedGroup() {
    this.viewingOptions.forEach((option) => {
      option.items.forEach((group) => {
        if (group.selected) {
          this.selectedGroup = group;
        }
      });
    });
  }

  handleViewSelection(option, groupType: string, viewingGroup) {
    this.selectedGroup = viewingGroup;
    this.toggleDropdownMenu();
    this.selectViewEmitterHandler(viewingGroup);
    
  }

  selectViewEmitterHandler(viewingGroup) {
    this.selectViewEmitter.emit(viewingGroup);
  }

  handleCreateNewSelection(groupType: string) {
    this.createNewEmitter.emit(groupType);
    this.closeDropdownMenu();
  }

  handleGroupEditSelected(viewingGroup) {
    this.editGroupEmitter.emit(viewingGroup);
    this.closeDropdownMenu();
  }

  //--------------------------------------------------------------------------------
  toggleEditButtonOnLoad() {
    //this.selectViewEmitterHandler(this.selectedGroup);
  }

  toggleDropdownMenu() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdownMenu() {
    this.isDropdownOpen = false;
  }
}
