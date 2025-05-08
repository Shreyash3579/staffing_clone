import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DropdownLoaderHelperService {

  constructor() { }

  /*
    dropdownData - data to be shown in dropdown
    textKeyName - name of the property that will be show as Text field in dropdown
    valueKeyName - name of the property that will be used as Value field in dropdown 
  */
  getFormattedHierarchyDropDownItems(dropdownData, textKeyName: string, valueKeyName: string){

    const childrenList = dropdownData.map(item => {
      return {
          text: item[textKeyName],
          value: item[valueKeyName],
          collapsed: true,
          children: item.children?.map(child => {
              return {
                  text: child[textKeyName],
                  value: child[valueKeyName],
                  checked: false
              };
          }),
          checked: false
      };
    });

    const dropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: childrenList
    };

    return dropdownList;
  }
}
