import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-search-resource',
  templateUrl: './search-resource.component.html',
  styleUrls: ['./search-resource.component.scss']
})
export class SearchResourceComponent implements OnInit {

  @Input() searchedResource;
  @Input() isCurrentSelection: boolean = false;
  @Output() openResourceDetailsDialog = new EventEmitter<string>();
  @Output() addResourceToSelectedListEmitter = new EventEmitter<any>();
  @Output() removeResourceFromSelectedListEmitter = new EventEmitter<string>();

  

  constructor() { }

  ngOnInit(): void {
  }

  getImageUrl() {
    this.searchedResource.profileImageUrl = "assets/img/user-icon.jpg";
  }

  openResourceDetailsDialogHandler(){
    this.openResourceDetailsDialog.emit(this.searchedResource.id);
  }

  onCheckboxChange(event: any) {
    if(event.target.checked){
      this.addResourceToSelectedListEmitter.emit(this.searchedResource);
    }
    else{
      this.removeResourceFromSelectedListEmitter.emit(this.searchedResource.employeeCode);
    }

  }
}