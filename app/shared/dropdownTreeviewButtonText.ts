import { Injectable } from '@angular/core';
import {
  TreeviewI18n,
  TreeviewSelection,
  TreeviewHelper,
  TreeviewItem
} from '@soy-andrey-semyonov/ngx-treeview';

/**
 * DropdownTreeViewButtonText extends TreeviewI18n of treeview library
 * This shows the text on multiselect dropdown button
 */
@Injectable()
export class DropdownTreeViewButtonText extends TreeviewI18n {
  // title property holds the dropdown type that identifies dropdown type, when no option is selected
  title = 'Options';
  items: TreeviewItem[]; // this populated only for Office hierarchy dropdown
  officeClusterSelected;
  getText(selection: TreeviewSelection): any {
    const selectedItems = this.extractSelecteditems(selection);
    switch (selection.checkedItems.length) {
      case 0:
        return `Select ${this.title}`;
      case 1:
        if (this.items) {
          const dropdownText = selection.checkedItems[0].text;
          const parentOffice = TreeviewHelper.findParent(
            this.items[0],
            selection.checkedItems[0]
          );
          return parentOffice && parentOffice.children.length > 1
            ? dropdownText
            : parentOffice.text;
        } else {
          return selection.checkedItems[0].text;
        }
      default:
        if (this.items) {
          return this.getButtonTextForOfficeHierarchy(selection, selectedItems);
        } else {
          return `${selection.checkedItems[0].text} + ${selection.checkedItems
            .length - 1}`;
        }
    }
  }

  getAllCheckboxText(): string {
    return 'All';
  }

  getFilterPlaceholder(): string {
    return 'Filter';
  }

  getFilterNoItemsFoundText(): string {
    return 'No items found';
  }

  getTooltipCollapseExpandText(isCollapse: boolean): string {
    return isCollapse ? 'Expand' : 'Collapse';
  }

  private getButtonTextForOfficeHierarchy(selection: TreeviewSelection, selectedItems) {
    this.officeClusterSelected = [];
    if (selection.uncheckedItems.length === 0) {
      return 'All Bain Offices';
    }
    for (const item of selection.checkedItems) {
      const parentOffice = TreeviewHelper.findParent(this.items[0], item);
      const isAllChildItemsSelected = parentOffice.children.every(x => x.checked);
      if (isAllChildItemsSelected) {
        this.officeClusterSelected.push(parentOffice);
        selectedItems = this.removeSelectedofficeIfparentofficeSelected(parentOffice, selectedItems);
        this.checkParentOfficeForAllChildOfficeSelected(parentOffice, selectedItems);
      }
    }
    if (this.officeClusterSelected.length > 0) {
      return this.getDropdownTextForOfficeClusterSelected(selectedItems);
    } else {
      return `${selection.checkedItems[0].text} + ${selection.checkedItems.length - 1}`;
    }
  }

  private checkParentOfficeForAllChildOfficeSelected(office, selectedItems) {
    const parentOffice = TreeviewHelper.findParent(this.items[0], office);
    if (!parentOffice) {
      return selectedItems;
    }
    if (parentOffice.children.every(x => x.checked)) {
      this.officeClusterSelected.push(parentOffice);
      selectedItems = this.removeSelectedofficeIfparentofficeSelected(parentOffice, selectedItems);
      this.checkParentOfficeForAllChildOfficeSelected(parentOffice, selectedItems);
    }
  }

  private removeSelectedofficeIfparentofficeSelected(parentOffice, selectedItems) {
    const childItems = [];
    parentOffice.children.forEach(x => childItems.push(x));
    return selectedItems.filter(x => !childItems.includes(x));
  }

  private extractSelecteditems(selection: TreeviewSelection) {
    const selectedItems = [];
    for (const item of selection.checkedItems) {
      selectedItems.push(item);
    }

    return selectedItems;
  }

  private getDropdownTextForOfficeClusterSelected(selectedItems) {
    let selectedofficeCluster = this.officeClusterSelected;
    selectedofficeCluster = selectedofficeCluster.filter((item, index) => selectedofficeCluster.indexOf(item) === index);
    const childOfficeClusterSelectedList = [];
    for (const item of this.items[0].children) {
      this.getChildOfficeClusterForParentOfficeCusterSelected(item, selectedofficeCluster, selectedItems, childOfficeClusterSelectedList);
    }
    selectedofficeCluster = selectedofficeCluster.filter(x => !childOfficeClusterSelectedList.some(y => x.value === y.value));
    const remainingOfficeSelected = selectedofficeCluster.length - 1 + selectedItems.length;
    const dropdownText = remainingOfficeSelected > 0
      ? `${selectedofficeCluster[0].text} + ${remainingOfficeSelected}`
      : selectedofficeCluster[0].text;
    return dropdownText;
  }

  private getChildOfficeClusterForParentOfficeCusterSelected(itemCLuster, selectedofficeCluster, selectedItems, children) {
    if (itemCLuster && itemCLuster.checked && itemCLuster.children) {
      itemCLuster.children.forEach(x => children.push(x));
    }
    if (itemCLuster && itemCLuster.children) {
      for (const item of itemCLuster.children) {
        this.getChildOfficeClusterForParentOfficeCusterSelected(item, selectedofficeCluster, selectedItems, children);
      }
    }
    return true;
  }
}
