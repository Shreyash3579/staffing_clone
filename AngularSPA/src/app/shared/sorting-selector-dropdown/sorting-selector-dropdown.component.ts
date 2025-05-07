import { Component, EventEmitter, OnInit, Output, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-sorting-selector-dropdown',
  templateUrl: './sorting-selector-dropdown.component.html',
  styleUrls: ['./sorting-selector-dropdown.component.scss']
})
export class SortingSelectorDropdownComponent implements OnInit {

  public isDropdownOpen: boolean = false;

    public sortBy = { label: "", direction: "", id:"", icon: "" };

    // Sorting Options
    public sortOptions = [
        { label: "Employee", id:"fullName", active: false },
        { label: "Position", id:"position", active: false },
        { label: "Level", id:"levelGrade", active: false },
        { label: "Office", id:"office", active: false },
        { label: "Availability Percentage", id:"percentAvailable", active: false },
        { label: "Date", id:"dateFirstAvailable", active: false },
        { label: "Hire Date", id:"startDate", active: false },
        { label: "Last Staffed on Billable", id:'lastBillableDate', active: false}
    ];

    // Sorting Directions
    public sortDirections = [
        { label: "Ascending", value: "asc", active: true, icon: "arrow-up"},
        { label: "Descending", value: "desc", active: false, icon: "arrow-down"}
    ];

  @Output() getResourcesSortBySelectedValues = new EventEmitter();

  constructor() { }

  ngOnInit(): void {
    var _this = this;
    window.onclick = function (event) {
        if (!event.target.matches(".toggle-sort-cta, .sort-dropdown-wrapper, .option-cta, .option")) {
            _this.isDropdownOpen = false;
        }
    };
  this.setDefaultSortBy("asc","arrow-up");
}

ngOnChanges(changes: SimpleChanges): void {
}

setDefaultSortBy(selectedDirection, selectedIcon){
  this.sortBy={label: "", direction : selectedDirection, id:"", icon : selectedIcon}
}

// Open & Close Menu
toggleDropdownMenu(event: Event) {
  event.stopPropagation();
  this.isDropdownOpen = !this.isDropdownOpen;
}

// Selecting Sort Option
handleOptionClick(option, event) {
  this.sortOptions.forEach((item) => {
      if (item.label === option.label) {
          item.active = true;
          this.sortBy.label = item.label;
          this.sortBy.id = item.id;
      } else {
          item.active = false;
      }
  });

  this.sortDataOnSelectedOptionAndDirection();
}

 // Selecting Sort Direction
 handleDirectionClick(option,event) {
  this.sortDirections.forEach((item) => {
      if (item.label === option.label) {
          item.active = true;
          this.sortBy.direction = item.value;
          this.sortBy.icon = item.icon;
      } else {
          item.active = false;
      }
  });

  this.isDropdownOpen = !this.isDropdownOpen;
  this.sortDataOnSelectedOptionAndDirection();
}

sortDataOnSelectedOptionAndDirection(){
  const sortBy = this.sortOptions.find(x => x.active)?.id ?? 'fullName' ;
  const sortDirection = this.sortDirections.find(x => x.active)?.value ?? 'asc' ;
  this.getResourcesSortBySelectedValues.emit({sortBy: sortBy, direction: sortDirection});
}

 // Unselecting Sort Option & Direction
 handleOptionRemoval(event: Event, option) {
  event.stopPropagation();

  this.sortOptions.forEach((item) => {
      if (item.label === option.label) {
          item.active = false;
      } else {
          return;
      }
  });
  
  this.setDefaultSortBy(this.sortBy.direction, this.sortBy.icon);
  
  this.getResourcesSortBySelectedValues.emit({sortBy: this.sortBy.id});
}

}
