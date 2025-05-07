import { DefaultTreeviewI18n, TreeviewItem, TreeviewSelection } from '@soy-andrey-semyonov/ngx-treeview';
import { Injectable } from "@angular/core";

@Injectable()
export class DropdownTreeviewSingleSelectI18n extends DefaultTreeviewI18n {
  private internalSelectedItem: TreeviewItem;
  title = 'Options';

  set selectedItem(value: TreeviewItem) {
      if (value && value.children === undefined) {
          this.internalSelectedItem = value;
      }
  }

  get selectedItem(): TreeviewItem {
      return this.internalSelectedItem;
  }

  getText(selection: TreeviewSelection): string {
      return this.internalSelectedItem ? this.internalSelectedItem.text : `select ${this.title}`;
  }

}
