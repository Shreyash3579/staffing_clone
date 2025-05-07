import { Component, OnInit, Input, OnChanges, OnDestroy, SimpleChanges, Output, EventEmitter } from "@angular/core";

// third-party references
// services

// interfaces
import { ResourceGroup } from "src/app/shared/interfaces/resourceGroup.interface";
import { Resource } from "src/app/shared/interfaces/resource.interface";
import { ViewingGroup } from "src/app/shared/interfaces/viewingGroup.interface";
import { CdkDragDrop } from "@angular/cdk/drag-drop";
import { PlaceholderAllocation } from "src/app/shared/interfaces/placeholderAllocation.interface";
import { ResourceAllocation } from "src/app/shared/interfaces/resourceAllocation.interface";
import { ResourceAssignmentService } from "src/app/overlay/behavioralSubjectService/resourceAssignment.service";
import { PlaceholderAssignmentService } from "src/app/overlay/behavioralSubjectService/placeholderAssignment.service";
import { ResourceAllocationService } from "src/app/shared/services/resourceAllocation.service";
import { NotificationService } from "src/app/shared/notification.service";
import { Store } from "@ngrx/store";
import * as StaffingDemandActions from 'src/app/home-copy/state/actions/staffing-demand.action';
import * as fromStaffingDemand from 'src/app/home-copy/state/reducers/staffing-demand.reducer';

@Component({
  selector: "home-resources",
  templateUrl: "./resources.component.html",
  styleUrls: ["./resources.component.scss"]
})
export class ResourcesComponent implements OnInit, OnChanges, OnDestroy {
  // Inputs
  @Input() filteredResourceGroups: ResourceGroup[]; // Store resources received from Database
  @Input() isAvailableToday: boolean;
  // @Input() selectedViewingGroup: ViewingGroup;

  // Outputs
  @Output() resourceSelectedEmitter = new EventEmitter();

  // variables
  resourceGroups: ResourceGroup[];
  viewingGroup: ViewingGroup;
  // -----------------------Constructor--------------------------------------------//
  constructor(
    private resourceAssignmentService: ResourceAssignmentService,
    private placeholderAssignmentService: PlaceholderAssignmentService,
    private resourceAllocationHelperService: ResourceAllocationService,
    private notifyService: NotificationService,
    private demandStore: Store<fromStaffingDemand.State>
    ) { }

  // -----------------------Component LifeCycle Events and Functions-------------------//
  ngOnInit() { }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.filteredResourceGroups && this.filteredResourceGroups) {
      this.resourceGroups = this.filteredResourceGroups;
    }

    // if (changes.selectedViewingGroup && this.selectedViewingGroup) {
    //   this.viewingGroup = this.selectedViewingGroup;
    // }
  }

  ngOnDestroy() { }

  trackByResourceGroup(index, item) {
    return item.groupTitle;
  }

  trackByResource(index, item) {
    return item.employeeCode;
  }

  resourceSelectedEmitterHandler(event) {
    this.resourceSelectedEmitter.emit(event)
  }

  // -----------------------Helpers-------------------//

  // usingViewingGroup(groupTitle: string): boolean {
  //   let groupName = groupTitle.split("(")[0].trim();
  //   return this.viewingGroup && groupName == this.viewingGroup.name ? true : false;
  // }

  // getGroupName(groupTitle: string): string {
  //   return groupTitle.split("(")[0].trim();
  // }
  checkIfDroppedBetweenSupplyLists(event) {
    // if element is dragged and dropped between supply lists, then those elements would not have any allocation id
    return !event.previousContainer.data[event.previousIndex].id;
  }

  onResourceDrop(event: CdkDragDrop<any>) {
    /*
      1) if element is dragged and dropped from and to the same list, then do nothing
      2) if element is dragged and dropped betwwen supply lists, then do nothing
    */
    if (event.container.id === event.previousContainer.id || this.checkIfDroppedBetweenSupplyLists(event)) {
      return;
    }

    if (event.previousContainer.data[event.previousIndex].planningCardId) {
      const staffableEmployee: PlaceholderAllocation = event.previousContainer.data[event.previousIndex];
      this.demandStore.dispatch(
        new StaffingDemandActions.DeletePlaceholderAllocations({
          placeholderIds : staffableEmployee.id,
          placeholderAllocation: [].concat(staffableEmployee),
          notifyMessage: 'Placeholder Deleted', 
        })
    )
    } else {
      const staffableEmployee: ResourceAllocation = event.previousContainer.data[event.previousIndex];

      const [isValidaAllocation, errorMessage] = this.resourceAllocationHelperService.validateMonthCloseForInsertAndDelete(staffableEmployee);

      if (!isValidaAllocation) {
        this.notifyService.showValidationMsg(errorMessage);
        return;
      } else {
        this.demandStore.dispatch(
          new StaffingDemandActions.DeleteResourceAllocations({
            allocationIds: staffableEmployee.id,
            commitmentIds: "",
            allocation: [].concat(staffableEmployee)
          })
        );
      }
    }

    event.previousContainer.data.splice(event.previousIndex, 1);
  }

  // -----------------------Local Functions-----------------------------------//
}
