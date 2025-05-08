import { Component, OnInit, OnChanges, Input, EventEmitter, Output, SimpleChanges, ViewChild } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TreeviewItem, TreeviewHelper, DropdownTreeviewComponent, TreeviewI18n, TreeviewConfig } from '@soy-andrey-semyonov/ngx-treeview';
import { DropdownTreeviewSingleSelectI18n } from './DropdownTreeViewSingleSelectI18n';

@Component({
  selector: 'app-single-select-dropdown',
  templateUrl: './single-select-dropdown.component.html',
  styleUrls: ['./single-select-dropdown.component.scss'],
  providers: [{ provide: TreeviewI18n, useClass: DropdownTreeviewSingleSelectI18n }]
})
export class SingleSelectDropdownComponent implements OnInit, OnChanges {

  @ViewChild(DropdownTreeviewComponent, { static: true }) dropdownTreeViewComponent: DropdownTreeviewComponent;
  // -----------------------Local variables--------------------------------------------//
  items: TreeviewItem[];
  dropdownSettings: {};
  buttonClasses = [
    'btn-outline-primary',
    'btn-outline-secondary',
    'btn-outline-success',
    'btn-outline-danger',
    'btn-outline-warning',
    'btn-outline-info',
    'btn-outline-light',
    'btn-outline-dark'
  ];
  buttonClass = this.buttonClasses[3];
  private _dropdownList = new BehaviorSubject<any>({});
  private _selectedItem = new BehaviorSubject<any>({});
  private _title = new BehaviorSubject<any>({});
  filterText: string;
  private dropdownTreeviewSingleSelectI18n: DropdownTreeviewSingleSelectI18n;

  // -----------------------Input Events--------------------------------------------//
  @Input()
  set dropdownList(value) {
    this._dropdownList.next(value);
  }
  get dropdownList() {
    return this._dropdownList.getValue();
  }
  @Input()
  set selectedItem(value) {
    this._selectedItem.next(value);
  }
  get selectedItem() {
    return this._selectedItem.getValue();
  }
  @Input()
  set title(value) {
    this._title.next(value);
  }
  get title() {
    return this._title.getValue();
  }

  @Input() hasFilter = false;

  // -----------------------Output Events--------------------------------------------//
  @Output() valueChange = new EventEmitter<any>();

  constructor(public i18n: TreeviewI18n) {
    this.dropdownTreeviewSingleSelectI18n = i18n as DropdownTreeviewSingleSelectI18n;
  }

  // -----------------------Component Lifecycle Hooks --------------------------------------------//
  ngOnInit() {
    this.items = [];
    this.dropdownSettings = {
      hasAllCheckBox: false,
      hasFilter: this.hasFilter,
      hasCollapseExpand: false,
      decoupleChildFromParent: false,
      maxHeight: 500
    };
    this._dropdownList.subscribe(dropdownItems => {
      if (dropdownItems) {
        this.items = [];
        this.dropdownList.forEach(element => {
          this.items.push(new TreeviewItem(element));
        });
      }
    });
    this._selectedItem.subscribe(selectedItem => {
      if (selectedItem && this._dropdownList) {
        const item = TreeviewHelper.findItemInList(this.items, this.selectedItem);
        this.select(item);
      }
    });

    this._title.subscribe(title => {
      if (title) {
        this.dropdownTreeViewComponent.i18n['title'] = this.title;
      }
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (this.selectedItem) {
      this.selectAll();
    } else {
      this.updateSelectedItem();
    }
  }

  // -----------------------Event Handlers--------------------------------------------//
  select(item: TreeviewItem) {
    // if (item.children === undefined) {
    //     this.selectItem(item);
    // }
    if (item) {
      this.selectItem(item);
    }
  }

  private updateSelectedItem() {
    if (this.items) {
      const selectedItem = TreeviewHelper.findItemInList(this.items, this.selectedItem);
      if (selectedItem) {
        this.selectItem(selectedItem);
      } else {
        this.selectAll();
      }
    }
  }
  private selectItem(item: TreeviewItem) {
    if (!item.value) {
      return false;
    }
    if (this.dropdownTreeviewSingleSelectI18n.selectedItem !== item) {
      this.dropdownTreeviewSingleSelectI18n.selectedItem = item;
      if (this.selectedItem !== item.value) {
        this.selectedItem = item.value;
        this.dropdownTreeViewComponent.buttonLabel = item.text; //added since placeholder label was not updating after selection changed
        this.valueChange.emit(item);
      }
    }
  }

  private selectAll() {
    const allItem = this.dropdownTreeViewComponent.treeviewComponent?.allItem;
    if (allItem) this.selectItem(allItem);
  }
}

