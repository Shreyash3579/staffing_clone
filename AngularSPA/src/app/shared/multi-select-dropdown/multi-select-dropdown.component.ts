import { Component, OnInit, Input, Output, EventEmitter, ViewChild, ViewEncapsulation, OnDestroy, AfterViewInit } from '@angular/core';
import { TreeviewItem, TreeviewHelper, TreeviewI18n, DropdownTreeviewComponent, TreeviewComponent, TreeviewConfig } from '@soy-andrey-semyonov/ngx-treeview';
import { DropdownTreeViewButtonText } from '../dropdownTreeviewButtonText';
import { debounceTime } from 'rxjs/operators';
import { TreeviewDropdownType } from '../constants/enumMaster';
import { BehaviorSubject, Subject, Subscription } from 'rxjs';

@Component({
  selector: 'app-multi-select-dropdown',
  templateUrl: './multi-select-dropdown.component.html',
  styleUrls: ['./multi-select-dropdown.component.scss'],
  encapsulation: ViewEncapsulation.None,
  providers: [{ provide: TreeviewI18n, useClass: DropdownTreeViewButtonText }]
})
export class MultiSelectDropdownComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild("treeViewDropDown", { static: false }) dropdownTreeViewComponent: TreeviewComponent | DropdownTreeviewComponent ;
  // -----------------------Local variables--------------------------------------------//
  items: TreeviewItem[] = [];
  dropDownListTreeViewFormat;
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
  buttonClass = this.buttonClasses[1];
  private _dropdownList = new BehaviorSubject<any>({});
  private _selectedItems = new BehaviorSubject<any>({});
  private _title = new BehaviorSubject<any>({});
  private _checkBoxChange = new Subject();
  private subscription: Subscription;

  public get treeviewDropdownType(): typeof TreeviewDropdownType {
    return TreeviewDropdownType; 
  }
  
  // -----------------------Input Events--------------------------------------------//
  @Input() treeViewType = this.treeviewDropdownType.DROPDOWN_TREE_VIEW;

  @Input() disabled: boolean = false;

  @Input() appendParentCodesOnSelect: boolean = false;

  @Input()
  set dropdownList(value) {
    this._dropdownList.next(value);
  }
  get dropdownList() {
    return this._dropdownList.getValue();
  }
  @Input()
  set selectedItems(value) {
    this._selectedItems.next(value);
  }
  get selectedItems() {
    return this._selectedItems.getValue();
  }
  @Input()
  set title(value) {
    this._title.next(value);
  }
  get title() {
    return this._title.getValue();
  }
  @Input() maxHeight = 350;
  // -----------------------Output Events--------------------------------------------//
  @Output() refreshView = new EventEmitter<any>();

  constructor() {
    this.treeViewType = this.treeviewDropdownType.DROPDOWN_TREE_VIEW;
  }

  // -----------------------Component Lifecycle Hooks --------------------------------------------//
  ngOnInit() {
    this.addDebounceToCheckBoxClickEvent();
    this.initializeTreeViewDropDownSettings();

  }

  ngAfterViewInit(){

    this._dropdownList.subscribe(dropdownItems => {
      if (dropdownItems) {
        this.items = [new TreeviewItem(dropdownItems)];
      }
    });

    this._selectedItems.subscribe(selectedItems => {
      if (selectedItems?.length && this._dropdownList.value) {
        this.checkSelectedItems();
      }
    });

    this._title.subscribe(title => {
      if (title) {
        this.dropdownTreeViewComponent.i18n['title'] = this.title;
      }
    });

  }

  initializeTreeViewDropDownSettings(){
    this.dropdownSettings = TreeviewConfig.create({
      hasAllCheckBox: false,
      hasFilter: true,
      hasCollapseExpand: false,
      decoupleChildFromParent: false,
      maxHeight: this.maxHeight
    });
  }

  deSelectAllItems(){
    let treeViewComponent = this.getTreeViewComponent(this.dropdownTreeViewComponent);
    treeViewComponent.allItem.checked = false;
    treeViewComponent.onAllCheckedChange();
  }

  // unCheckAllItems(items) {
  //   if (items && items.length > 0 && items[0].children) {
  //     items.forEach(currentItem => {
  //       currentItem.checked = false;
  //       // currentItem.collapsed = true;
  //       this.unCheckAllItems(currentItem['internalChildren']);
  //     });
  //   }
  // }

  getTreeViewComponent(component): TreeviewComponent{
    if(component.hasOwnProperty('treeviewComponent')){
      return component["treeviewComponent"];
    }else{
      return component;
    }
  }

  // -----------------------Event Handlers--------------------------------------------//

  //This will prevent multiple API calls when checkboxes are checked/unchecked in quick successions
  addDebounceToCheckBoxClickEvent() {

    this.subscription = this._checkBoxChange.pipe(
      debounceTime(1000)
    ).subscribe(selectedValues => this.refreshView.emit(selectedValues));

  }

  checkSelectedItems() {
    if (Array.isArray(this.selectedItems)) {
      this.selectedItems.forEach(element => {
        const foundItem: TreeviewItem = TreeviewHelper.findItem(this.items[0], element);
        if (foundItem) {
          foundItem.checked = true;
          this.expandParentItem(foundItem);
        }
      });
    }
  }

  expandParentItem(currentItem: TreeviewItem) {
    const parentItem = TreeviewHelper.findParent(this.items[0], currentItem);
    if (parentItem) {
      // to prevent dropdown hierarchy from expanding when user selects the parent node
      // parentItem.collapsed = false;
      parentItem.correctChecked();
      this.expandParentItem(parentItem);
    }
    return false;
  }

  onSelectChange(items: any) {
    if (JSON.stringify(this.selectedItems) === JSON.stringify(items)) {
      return false;
    }

    //add parent offices codes (cluster codes, region codes etc) to the selected list of offices
    if(this.appendParentCodesOnSelect){
      const selectedItems = new Set<string>();
  
      // Process each selected item
      for (const item of items) {
        selectedItems.add(item.toString());
        
        // Find the item in the tree structure
        const treeItem = TreeviewHelper.findItem(this.items[0], item.toString());
        
        if (treeItem) {
          this.addParentItems(treeItem, selectedItems);
          
        }
      }
    
      const updatedItems = Array.from(selectedItems);
    
      if (JSON.stringify(this.selectedItems) === JSON.stringify(updatedItems)) {
        return false;
      }
  
      this.selectedItems = updatedItems;
    }else{

      var selecteditemList = this.selectedItems.filter(element => items.includes(element)).map(val => val);
      var newlySelectedItem = items.filter(element => !this.selectedItems.includes(element)).map(val => val);
     
      if (newlySelectedItem.length > 0) {
        selecteditemList = selecteditemList.concat(newlySelectedItem);
      }
      this.selectedItems = selecteditemList;
    }
  
    this._checkBoxChange.next(this.selectedItems.toString());
  }

  
  addParentItems(item: any, selectedItems: Set<string>) {
    const parentItem = TreeviewHelper.findParent(this.items[0], item);
    
    // Stop recursion if no parent or parent isn't checked
    if (!parentItem || parentItem.checked !== true) {
      return;
    }
  
    // Add parent to selected items
    selectedItems.add(parentItem.value.toString());
  
    // Recursively process parent's parent
    this.addParentItems(parentItem, selectedItems);
  }

  
  
  // This function is required to filter office from tree view
  onFilterChange(item: any) { }

  //---------------------------Destroy Event --------------------------

  ngOnDestroy() {
    this._checkBoxChange.complete();
    this._dropdownList?.complete();
    this._selectedItems?.complete();
    this._title?.complete();
    this.subscription.unsubscribe();
  }

}
