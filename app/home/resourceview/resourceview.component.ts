import { Component, OnInit, Input, Output, EventEmitter, OnDestroy, SimpleChanges } from '@angular/core';
import { Resource } from '../../shared/interfaces/resource.interface';

@Component({
  selector: 'app-resourceview',
  templateUrl: './resourceview.component.html',
  styleUrls: ['./resourceview.component.scss']
})
export class ResourceviewComponent implements OnInit, OnDestroy {
  showAlertDetails: boolean = false;
  upcomingCommitmentsForAlerts:any[] = [];
  //-----------------------Input Variables--------------------------------------------//

  @Input() resource: Resource;

  //-----------------------Output Events--------------------------------------------//

  @Output() openResourceDetailsDialog = new EventEmitter();
  @Output() resourceSelectedEmitter = new EventEmitter();
  constructor() { }

  //-----------------------Component LifeCycle Events and Functions--------------------------------------------//

  ngOnInit() { }

  
// Currently, in the retired staffing tab, alerts are shown in a comma-separated format within the tooltip so we are concatenating the allocations and commitments alerts to display in the alert message.
// While we could update the alert display to match the format used in our new staffing tab, as this page is going to be retired soon , we've opted not to modify the existing logic.
  ngOnChanges(changes:SimpleChanges ){
    this.upcomingCommitmentsForAlerts = [];    
    if (this.resource.upcomingCommitmentsForAlerts && this.resource.upcomingCommitmentsForAlerts.allocations) {
      this.upcomingCommitmentsForAlerts = this.upcomingCommitmentsForAlerts.concat(this.resource.upcomingCommitmentsForAlerts.allocations);
    }
    if (this.resource.upcomingCommitmentsForAlerts && this.resource.upcomingCommitmentsForAlerts.commitments) {
      this.upcomingCommitmentsForAlerts = this.upcomingCommitmentsForAlerts.concat(this.resource.upcomingCommitmentsForAlerts.commitments);
    }
    }

  resourceClickHandler(event) {
    if (event.ctrlKey) {
      this.resource.isSelected = !this.resource.isSelected;
      this.resourceSelectedEmitter.emit(this.resource);
    }
  }

  isAlertForStaffableAs(alert) {
    return alert.indexOf('Staffable as') > -1;
  }

  //-------------------Component Event Handlers-------------------------------------//

  openResourceDetailsDialogHandler(employeeCode) {
    this.openResourceDetailsDialog.emit(employeeCode);
  }

  toggleAlertDetails() {
    this.showAlertDetails = !this.showAlertDetails;
  }

  hideAlertDetails() {
    this.showAlertDetails = false;
  }

  ngOnDestroy(){
  }

}
