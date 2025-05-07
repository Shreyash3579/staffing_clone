import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { CommonService } from 'src/app/shared/commonService';

@Component({
  selector: 'app-search-planning-card',
  templateUrl: './search-planning-card.component.html',
  styleUrls: ['./search-planning-card.component.scss']
})
export class SearchPlanningCardComponent implements OnInit {

  @Input() searchedPlanningCard: any;
  @Output() openProjectDetailsDialog = new EventEmitter<string>();
  officeFlatList: any;
  
  
  constructor( private localStorageService: LocalStorageService
  ) { }

  ngOnInit(): void {
    this.intializeDataFromLocalStorage();
  }

intializeDataFromLocalStorage() {
    this.officeFlatList = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
}
  openProjectDetailsDialogHandler() {
    this.openProjectDetailsDialog.emit(this.searchedPlanningCard.id);
  }

  getFirstSharedOffice(sharedOfficeCodes: string) {
     const offices = sharedOfficeCodes.split(',');
     const officeAbbList = CommonService.GetOfficeNames(sharedOfficeCodes, this.officeFlatList).split(', ');
     return offices.length > 1 ? `${officeAbbList[0]} (+${offices.length - 1})` : officeAbbList[0];
  }

  getSharedOfficeAbbNames(sharedOfficeCodes: string): string {
    return CommonService.GetOfficeNames(sharedOfficeCodes, this.officeFlatList);
  }

}
