// -------------------Angular Operators---------------------------------------//
import { Component, OnInit, Output, EventEmitter, Input, OnChanges, HostListener, OnDestroy } from '@angular/core';
import { Subject, Subscription } from 'rxjs';

// -------------------Third Party References---------------------------------------//
import { CdkDragDrop } from '@angular/cdk/drag-drop';

// -------------------Service References---------------------------------------//
import { ResourceAssignmentService } from 'src/app/overlay/behavioralSubjectService/resourceAssignment.service';

// -------------------Interfaces---------------------------------------//
import { ResourceGroup } from 'src/app/shared/interfaces/resourceGroup.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
import { ServiceLineHierarchy } from 'src/app/shared/interfaces/serviceLineHierarchy';
import { LevelGrade } from 'src/app/shared/interfaces/levelGrade.interface';
import { PlaceholderAssignmentService } from 'src/app/overlay/behavioralSubjectService/placeholderAssignment.service';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { PracticeArea } from 'src/app/shared/interfaces/practiceArea.interface';
import { PositionHierarchy } from 'src/app/shared/interfaces/positionHierarchy.interface';
import { AffiliationRole } from 'src/app/shared/interfaces/affiliationRole.interface';
import { ResourcesCommitmentsDialogService } from 'src/app/overlay/dialogHelperService/resourcesCommitmentsDialog.service';

@Component({
  selector: 'home-resources',
  templateUrl: './resources.component.html',
  styleUrls: ['./resources.component.scss']
})
export class ResourcesComponent implements OnInit, OnChanges, OnDestroy {

  // -----------------------Input Variables--------------------------------------------//
  @Input() clearSearch: Subject<boolean>;
  @Input() filteredResourceGroups: ResourceGroup[]; // Store resources received from Database
  @Input() searchedResourceGroups: ResourceGroup[];
  @Input() officeHierarchy: OfficeHierarchy;
  @Input() staffingTagsHierarchy: ServiceLineHierarchy[];
  @Input() positionsHierarchy: PositionHierarchy[];
  @Input() levelGrades: LevelGrade[];
  @Input() resourceLength: number;
  @Input() practiceAreas: PracticeArea[];
  @Input() affiliationRoles: AffiliationRole[];

  // -----------------------Output Events--------------------------------------------//
  @Output() openResourceDetailsDialog = new EventEmitter();
  @Output() getResourcesIncludingTerminatedBySearchString = new EventEmitter();
  @Output() getResources = new EventEmitter();
  @Output() getResourcesSortAndGroupBySelectedValuesEmitter = new EventEmitter<any>();
  @Output() getResourcesAvailabilityBySelectedValuesEmitter = new EventEmitter<any>();
  @Output() openQuickAddForm = new EventEmitter();
  @Output() openResourcesCommitmentsDialog = new EventEmitter();

  // -----------------------Local Variables--------------------------------------------//
  selectedResources = [];
  resourceGroups: ResourceGroup[];
  xCordinateForContextMenu: Number;
  yCordinateForContextMenu: Number;
  contextMenuOptions = [
    { text: 'View Resource Data', value: 'viewResourceData', action: 'viewResourceData' },
    { text: 'Deselect All', value: 'deselectAll', action: 'deselectAllResources' }
  ];
  public showContextMenu: Boolean = false;
  private resourceDeselectionSubscription: Subscription;
  // -----------------------Constructor--------------------------------------------//
  constructor(
    private resourceAssignmentService: ResourceAssignmentService,
    private placeholderAssignmentService: PlaceholderAssignmentService,
    private resourceAllocationService: ResourceAllocationService,
    private notifyService: NotificationService,
    private resourcesCommitmentsDialogService: ResourcesCommitmentsDialogService
  ) { }

  // -----------------------Component LifeCycle Events and Functions-------------------//
  ngOnInit() {
    this.deselectResourceOnDeleteFromResourceViewPopup();
  }

  ngOnChanges() {
    if (this.searchedResourceGroups?.length > 0) {
      this.selectResourcesWhenResourcesListChanged(this.searchedResourceGroups);
      this.resourceGroups = this.searchedResourceGroups;
    } else {
      this.selectResourcesWhenResourcesListChanged(this.filteredResourceGroups);
      this.resourceGroups = this.filteredResourceGroups;
    }
    if (!(this.selectedResources?.length > 0)) {
      this.deselectAllResources();
    }
  }

  ngOnDestroy() {
    document.removeEventListener('click', this.documentClick, true);
    this.resourceDeselectionSubscription.unsubscribe();
  }

  //-----------------------Context Menu Events-----------------------------------//

  contextMenuOptionClickHandler(event) {
    this.showContextMenu = false;
    this[event.option.action].call(this);
  }

  deselectResourceOnDeleteFromResourceViewPopup() {
    this.resourceDeselectionSubscription = this.resourcesCommitmentsDialogService.resourceToBeDeselected.subscribe(employeeCode => {
      this.resourceGroups?.forEach(group => {
      group.resources.forEach(resource => {
        if(resource.employeeCode === employeeCode){
            resource.isSelected = false;
        }
      });
    });
  });
}

  openContextMenu(event) {
    if (this.selectedResources.length > 0) {
      event.stopPropagation();
      this.xCordinateForContextMenu = event.clientX;
      this.yCordinateForContextMenu = event.clientY > 620 ? 620 : event.clientY;
      this.showContextMenu = true;
    }
    return false;
  }

  @HostListener('document:click')
  documentClick(): void {
    this.showContextMenu = false;
  }

  // -----------------------Context Menu Events Handlers-----------------------------------//

  resourceSelectedEmitterHandler(event) {
    if (event.isSelected) {
      this.selectedResources.push(event);
    } else {
      this.selectedResources.splice(this.selectedResources.indexOf(event), 1);
    }
  }

  selectResourcesWhenResourcesListChanged(resourceGroup) {
    this.selectedResources.forEach(sr => {
      resourceGroup.forEach(group => {
        let resource = group.resources.find(resource => resource.employeeCode === sr.employeeCode);
        if (resource) {
          resource.isSelected = sr.isSelected;
        }
      })
    });
  }

  deselectAllResources() {
    this.resourceGroups?.forEach(group => {
      group.resources.forEach(resource => {
        resource.isSelected = false;
      });
    });
    this.selectedResources = [];
  }

  viewResourceData() {
    let employees = [];
    this.selectedResources.forEach(resource => {
      if (resource.isSelected) {
        employees.push({
          employeeCode: resource.employeeCode,
          employeeName: resource.fullName,
          levelGrade: resource.levelGrade,
          dateFirstAvailable: resource.dateFirstAvailable,
          percentAvailable: resource.percentAvailable,
          internetAddress: resource.internetAddress,
          isNotAvailable:  resource.isTerminated || resource.onTransitionOrTerminationAndNotAvailable || !resource.dateFirstAvailable || !resource.percentAvailable
        });
      }
    });
    this.openResourcesCommitmentsDialog.emit(employees);
  }

  // -----------------------Output Event Handlers-----------------------------------//

  openResourceDetailsDialogHandler(employeeCode: string) {
    this.openResourceDetailsDialog.emit(employeeCode);
  }

  getResourcesIncludingTerminatedBySearchStringHandler(event) {
    this.getResourcesIncludingTerminatedBySearchString.emit(event);
  }

  getResourcesHandler(event) {
    this.getResources.emit(event);
  }

  getResourcesSortAndGroupBySelectedValuesHandler(event) {
    this.getResourcesSortAndGroupBySelectedValuesEmitter.emit(event);
  }

  getResourcesAvailabilityBySelectedValuesHandler(event) {
    this.getResourcesAvailabilityBySelectedValuesEmitter.emit(event);
  }

  /*
   * Event triggered from quick add button in filters component
   * Propogates event to parent for opening bootstrap modal
  */
  openQuickAddFormHandler(event) {
    this.openQuickAddForm.emit(event);
  }

  onResourceDrop(event: CdkDragDrop<any>) {
    // if element is dragged and dropped from and to the same card, then do nothing

    if (event.container.id === event.previousContainer.id) {
      return;
    }

    if (event.previousContainer.data[event.previousIndex].planningCardId) {
      const staffableEmployee: PlaceholderAllocation = event.previousContainer.data[event.previousIndex];
      this.placeholderAssignmentService.deletePlaceHoldersByIds({ placeholderIds: staffableEmployee.id, notifyMessage: 'Resource Deleted' });

    } else {
      const staffableEmployee: ResourceAllocation = event.previousContainer.data[event.previousIndex];

      const [isValidaAllocation, errorMessage] = this.resourceAllocationService.validateMonthCloseForInsertAndDelete(staffableEmployee);

      if(!isValidaAllocation){
        this.notifyService.showValidationMsg(errorMessage);
        return;
      }else{
        this.resourceAssignmentService.deleteResourceAssignmentFromCase(staffableEmployee.id);
        this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics([].concat(staffableEmployee));
      }
      
    }

    event.previousContainer.data.splice(event.previousIndex, 1);

  }
}
