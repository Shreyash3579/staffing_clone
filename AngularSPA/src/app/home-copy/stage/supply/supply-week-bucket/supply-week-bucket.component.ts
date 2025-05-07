import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { DateService } from 'src/app/shared/dateService';
import { ResourceGroup } from 'src/app/shared/interfaces/resourceGroup.interface';
import { ViewingGroup } from 'src/app/shared/interfaces/viewingGroup.interface';

// interfaces
import { WeekData } from 'src/app/shared/interfaces/weekData.interface';
// import { ViewingGroup } from 'src/app/shared/interfaces/viewingGroup.interface';

@Component({
  selector: 'app-supply-week-bucket',
  templateUrl: './supply-week-bucket.component.html',
  styleUrls: ['./supply-week-bucket.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SupplyWeekBucketComponent implements OnInit {


  @Input() groupingArray: string[] = [];
  @Input() selectedViewingGroup: ViewingGroup;
  @Input() availableResourcesGroup: any;
  @Input() selectedGroupingOption: string;
  @Input() distinctResourceGroupObj: ViewingGroup[] = [];

  @Output() resourceSelectedEmitter = new EventEmitter();

  @Output() resourcesMultipleSelectionDeselectionEmitter = new EventEmitter();
 

  resourcesGroup: WeekData[] = [];
  groupIndexToSearch: number = -1;
  filteredResourceGroupsToShowInBuckets: WeekData[] = [];

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges) {

    if(changes.availableResourcesGroup && changes.availableResourcesGroup.currentValue) {
      this.convertResourceDataToBucket();
    }

    if(changes.groupingArray  && this.availableResourcesGroup) {
      this.convertResourceDataToBucket();
    }

    if(changes.selectedViewingGroup && changes.selectedViewingGroup.currentValue) {
      this.getViewingGroup();
    }
  
  }

  convertResourceDataToBucket() {
    this.resourcesGroup=[];
    this.groupingArray.forEach(week => {
     const startDate = DateService.convertDateInBainFormat(week);
     const endDate = DateService.addDays(DateService.convertDateInBainFormat(week), 7);
      const weekData: WeekData =  {
        date: week,
        resourceGroups: this.availableResourcesGroup.map((resourceGroup: ResourceGroup) => { 
          var  filteredResources = this.getResourcesForWeekOrDay(resourceGroup, startDate, endDate);
          if (filteredResources.length > 0) {
            return { groupTitle: resourceGroup.groupTitle, resources: filteredResources };
          }
        }).filter(Boolean) // Filter out undefined values
      };
      this.resourcesGroup.push(weekData);
    });
    this.getViewingGroup();
  }


  getResourcesForWeekOrDay(resourceGroup, startDate, endDate){
  var filteredResources
  if (this.selectedGroupingOption === 'Weekly') {
    filteredResources = resourceGroup.resources.filter((resource: any) => {
      
      var dateFirstAvailable = this.updateDateFirstAvailable(resourceGroup, resource);
      
      return new Date(dateFirstAvailable) >= new Date(startDate) && new Date(dateFirstAvailable) < new Date(endDate)
    });
  }
  else{
     filteredResources = resourceGroup.resources.filter((resource: any) => {
      
      var dateFirstAvailable = this.updateDateFirstAvailable(resourceGroup, resource);
      
      return new Date(dateFirstAvailable).getTime() == new Date(startDate).getTime();
    });
  }
    return filteredResources;
  }

  updateDateFirstAvailable(resourceGroup, resource){  
    var dateFirstAvailable;

    if(resourceGroup.groupTitle.includes(ConstantsMaster.availabilityBuckets.IncludeInCapacity) && resource.prospectiveDateFirstAvailable)
      {
        dateFirstAvailable = DateService.convertDateInBainFormat(resource.prospectiveDateFirstAvailable); 

        resource.dateFirstAvailable = DateService.convertDateInBainFormat(resource.prospectiveDateFirstAvailable); 
        resource.percentAvailable = resource.prospectivePercentAvailable;
      }
      else{
        dateFirstAvailable = DateService.convertDateInBainFormat(resource.dateFirstAvailable); 
      }
      return dateFirstAvailable;
    }

    toggleResourceSelectionOnTheSupplySide(isSelectionMode: boolean) {
     
      if(this.selectedViewingGroup.name === 'View All'){
        this.toggleResourceSelectionOnTheSupplySideBasedOnSelectedViewingGroup(isSelectionMode,this.filteredResourceGroupsToShowInBuckets);
      }
 
      else{
        
        this.toggleResourceSelectionOnTheSupplySideBasedOnSelectedViewingGroup(isSelectionMode,this.resourcesGroup);      
      }
 
    }
 
    toggleResourceSelectionOnTheSupplySideBasedOnSelectedViewingGroup(isSelectionMode: boolean, resourcesGroup: WeekData[]) {
      let selectedResources = [];
        resourcesGroup.forEach(weekData => {
        weekData.resourceGroups.forEach(resourceGroup => {
         
          if(isSelectionMode){
             // Check if any resource is selected within the week/day column
             const isAnyResourceSelected = resourceGroup.resources.some(resource => resource.isSelected && !selectedResources.includes(resource));

            // If true, update isSelected to true for all resources in the group
            if (isAnyResourceSelected) {
              resourceGroup.resources.forEach(resource => {
                resource.isSelected = true;
                selectedResources.push(resource);
              });
            }
        }
          else {
            selectedResources = [];
            resourceGroup.resources.forEach(resource => {
              resource.isSelected = false;
            });
          }
        });
      });

      this.resourcesMultipleSelectionDeselectionEmitter.emit(selectedResources);

    }
    

getViewingGroup() {

    if (this.selectedViewingGroup) {
      this.distinctResourceGroupObj.forEach(groupObj => groupObj.active = false);
      this.selectedViewingGroup.active = true;
      this.groupIndexToSearch = this.distinctResourceGroupObj.indexOf(this.selectedViewingGroup);

      this.filteredResourceGroupsToShowInBuckets = this.resourcesGroup.map(weekData => {
        const filteredResourceGroups: ResourceGroup[] = weekData.resourceGroups.filter(resourceGroup =>
          this.isGroupMatchUsingRegex(this.selectedViewingGroup.name, resourceGroup.groupTitle)
        );
        return { date: weekData.date, resourceGroups: filteredResourceGroups };
      });
    }
  }

  isGroupMatchUsingRegex(groupName, groupTitle) {
    const regexPattern = `^${groupName}$`;
    const regex = new RegExp(regexPattern, 'i');
    return regex.test(groupTitle.replace(/\([^)]*\)/g, '').trim());
  }

  trackByFn(index: number, item: any): any {
    return item.date;
  }

  resourceSelectedEmitterHandler(event) {
    this.resourceSelectedEmitter.emit(event)
  }

}
