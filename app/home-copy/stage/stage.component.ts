// -------------------Angular References---------------------------------------//
import { Component, EventEmitter, HostListener, Input, OnInit, Output, SimpleChanges, ViewChild } from "@angular/core";

// interfaces
import { Project } from "../../shared/interfaces/project.interface";
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";
import { ResourceGroup } from "src/app/shared/interfaces/resourceGroup.interface";
import { WeekData } from "src/app/shared/interfaces/weekData.interface";
import { ViewingGroup } from "src/app/shared/interfaces/viewingGroup.interface";
import { ResourcesCommitmentsDialogService } from "src/app/overlay/dialogHelperService/resourcesCommitmentsDialog.service";
import { Subscription } from "rxjs";
import { SupplyFilterCriteria } from "src/app/shared/interfaces/supplyFilterCriteria.interface";
import * as moment from "moment";
import { DateService } from "src/app/shared/dateService";
import { Office } from "src/app/shared/interfaces/office.interface";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { SupplyWeekBucketComponent } from "./supply/supply-week-bucket/supply-week-bucket.component";
import { CommonService } from "src/app/shared/commonService";

@Component({
  selector: "app-stage",
  templateUrl: "./stage.component.html",
  styleUrls: ["./stage.component.scss"]
})
export class StageComponent implements OnInit {
  // Inputs
  @Input() planningCards: PlanningCard[];
  @Input() projects: Project[];
  @Input() groupingArray: string[] = [];
  @Input() highlightedResourcesInPlanningCards: [];
  @Input() userPreferences: any;
  @Input() selectedGroupingOption: string;
  @Input() availableResources: any;
  @Input() supplyFilterCriteriaObj: SupplyFilterCriteria ;

  // Outputs
  // @Output() viewingGroupEmitter = new EventEmitter();
  @Output() openPegRFPopUpEmitter = new EventEmitter();
  @Output() upsertResourceAllocationsToProjectEmitter = new EventEmitter<any>();
  @Output() removePlaceHolderEmitter = new EventEmitter();
  @Output() showQuickPeekDialog = new EventEmitter<any>();
  @Output() mergePlanningCardAndAllocations = new EventEmitter<any>();
  @Output() openCaseRollForm = new EventEmitter<any>();
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() upsertPlaceholderEmitter = new EventEmitter<any>();
  @Output() updatePlanningCardEmitter = new EventEmitter<any>();
  @Output() addProjectToUserExceptionHideListEmitter = new EventEmitter<any>();
  @Output() addProjectToUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() removeProjectFromUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() addPlanningCardEmitter = new EventEmitter<any>();
  @Output() removePlanningCardEmitter = new EventEmitter<any>();
  @Output() sharePlanningCardEmitter = new EventEmitter<any>();
  @Output() openOverlappedTeamsForm = new EventEmitter<any>();
  @Output() selectAllResourcesForWeekEmitter = new EventEmitter();


  @ViewChild(SupplyWeekBucketComponent) supplyWeekBucket: SupplyWeekBucketComponent;

  // variables
  maxVisibleGroups: number = 5;
  weeksOrDaysToShow: string[] = [];
  currentPage: number = 0;
  showNextWeekOrDayButton: boolean = false;
  showPreviousWeekOrDayButton: boolean = false;
  resourceGroups: any;
  groupIndexToSearch: number = -1; // default viewing group to display
  resourceGroupsData: ViewingGroup[] = [];
  selectedViewingGroup: ViewingGroup;
  distinctResourceGroupObj: ViewingGroup[] = [];
  distinctResourceGroupTitles : string[] =[]
  officeList: Office[];
  availabilityyWeeksRange: any;
  selectedResources = [];
  xCordinateForContextMenu: Number;
  yCordinateForContextMenu: Number;
  contextMenuOptions = [
    { text: 'View Resource Data', value: 'viewResourceData', action: 'viewResourceData' },
    { text: 'Select Week', value: 'selectAll', action: 'selectAllResources' },
    { text: 'Deselect All', value: 'deselectAll', action: 'deselectAllResources' },
    { text: 'Email Resources', value: 'emailResources', action: 'emailResources' },
  ];
  public showContextMenu: Boolean = false;
  private resourceDeselectionSubscription: Subscription;

  constructor(private resourcesCommitmentsDialogService: ResourcesCommitmentsDialogService, private localStorageService: LocalStorageService
    
  ) { }

  ngOnInit() {
    this.getWeeksOrDaysToShow();
    this.deselectResourceOnDeleteFromResourceViewPopup();
    this.getLookupListFromLocalStorage();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.userPreferences && this.userPreferences) {
      this.getSelectedViewingGroup(null);
      this.getWeeksOrDaysToShow();
    }
    if (changes.groupingArray && this.groupingArray) {
      this.currentPage = 0;
      this.getSelectedViewingGroup(null);
      this.getWeeksOrDaysToShow();
    }

    if(changes.selectedGroupingOption && this.selectedGroupingOption){
      this.updateContextMenuLabel();
    }

    if (changes.availableResources && this.availableResources) {
      if (this.availableResources.length > 0) {
        this.getViewingGroupToBeDisplayed();
        this.getSelectedViewingGroup(null);
      } else {
        this.distinctResourceGroupObj = [];
        this.resourceGroups = [];
      }
      this.getWeeksOrDaysToShow();
    }
  }

  updateContextMenuLabel(): void {
    const selectText = this.selectedGroupingOption === 'Weekly' ? 'Select Week' : 'Select Day';
    const selectOption = this.contextMenuOptions.find(option => option.value === 'selectAll');
    if (selectOption) {
      selectOption.text = selectText;
    }
  }

  ngOnDestory() {
    this.resourceDeselectionSubscription.unsubscribe();
  }

  getLookupListFromLocalStorage() {
    this.officeList = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
  }

  deselectResourceOnDeleteFromResourceViewPopup() {
    this.resourceDeselectionSubscription = this.resourcesCommitmentsDialogService.resourceToBeDeselected.subscribe(
      (employeeCode) => {
        this.resourceGroups?.forEach((resGroup) => {
          resGroup.resources.forEach((res) => {
            if (res.employeeCode == employeeCode) {
              res.isSelected = false;
            }
          });
        });
      }
    );
  }

  showNextPreviousButton(noOfcolumnToBeShifted) {
    this.showNextWeekOrDayButton =
      this.groupingArray.length - (this.maxVisibleGroups + this.currentPage * noOfcolumnToBeShifted) > 0 ? true : false;
    this.showPreviousWeekOrDayButton = this.currentPage > 0 ? true : false;
  }

  getWeeksOrDaysToShow() {
    this.maxVisibleGroups = this.selectedGroupingOption === "Weekly" ? 5 : 7;
    let noOfcolumnToBeShifted = this.selectedGroupingOption === "Weekly" ? 1 : 7;

    let endOfWeekOrDayView = this.maxVisibleGroups + this.currentPage * noOfcolumnToBeShifted;
    this.weeksOrDaysToShow = this.groupingArray.slice(this.currentPage * noOfcolumnToBeShifted, endOfWeekOrDayView);

    this.showNextPreviousButton(noOfcolumnToBeShifted);
  }

  arrangeResourcesByGroup() {
    this.resourceGroups = this.getResourcesSortAndGroupBySelectedValues(
      this.availableResources,
      this.supplyFilterCriteriaObj.groupBy,
      this.supplyFilterCriteriaObj.sortBy
    );
  }

  //viewing group master list
  getViewingGroupToBeDisplayed() {
    this.arrangeResourcesByGroup();
    this.distinctResourceGroupObj = [];
    this.resourceGroups.forEach((resourceGroup) => {
      this.distinctResourceGroupObj.push({
        name: resourceGroup.groupTitle.replace(/\(\d+\)/, "").trim(),
        count: 0,
        active: false
      });
    });
    this.distinctResourceGroupObj = this.sortResourceGroupsByAvailability(this.distinctResourceGroupObj);
    let viewAllIndex = this.distinctResourceGroupObj.findIndex((obj) => obj.name === "View All");
    if (viewAllIndex !== -1) {
      let viewAllObj = this.distinctResourceGroupObj.splice(viewAllIndex, 1)[0];
      if (this.distinctResourceGroupObj.length > 1) {
        this.distinctResourceGroupObj.push(viewAllObj);
      }
    }
  }

  sortResourceGroupsByAvailability(resourceGroups) {
    return resourceGroups.sort((a, b) => {
      const aName = a.name.toLowerCase();
      const bName = b.name.toLowerCase();
  
      const aIsPrioritized = aName.includes("available") && !aName.includes("short term");
      const bIsPrioritized = bName.includes("available") && !bName.includes("short term");
  
      if (aIsPrioritized && !bIsPrioritized) {
        return -1;
      }
      if (!aIsPrioritized && bIsPrioritized) {
        return 1;
      }
  
      return 0; // Maintain the original order if both are equal in priority
    });
  }

  getResourcesSortAndGroupBySelectedValues(resources, groupBy, sortBy): ResourceGroup[] {
    if (sortBy === undefined || sortBy == null || sortBy === "") {
      sortBy = "levelGradeDesc";
    } else {
      this.supplyFilterCriteriaObj.sortBy = sortBy;
    }
    if (groupBy === undefined || groupBy == null || groupBy === "") {
      groupBy = "serviceLine";
    } else {
      this.supplyFilterCriteriaObj.groupBy = groupBy;
    }

    const sortByList = (sortBy && sortBy.length) > 1 ? sortBy.split(",") : null;
    const groupByList = (groupBy && groupBy.length) > 0 ? groupBy : null;

    const returnedGroupedArray = this.groupResourceBySelectedKey(resources, groupByList);

    this.sortResourceGroups(returnedGroupedArray, sortByList);
    return returnedGroupedArray;
  }

  private sortResourceGroups(resourceGroups, sortByList) {
    if (sortByList && resourceGroups && resourceGroups.length > 0) {
      //done to always apply sort By Name at last to get consistent results
      if (!sortByList.includes("fullName")) {
        sortByList.push("fullName");
      }
      resourceGroups.forEach((group) => {
        group.resources.sort((previousElement, nextElement) => {
          for (let index = 0; index < sortByList.length; index++) {
            //level grade descending order
            if (sortByList[index] === "levelGradeDesc") {
              const comparer = this.sortAlphanumeric(previousElement.levelGrade, nextElement.levelGrade);
              if (comparer === 1) {
                return -1; // Reverse: Place larger value first for descending order
              }
              if (comparer === -1) {
                return 1; // Reverse: Place smaller value later for descending order
              }
            }
            
            if (sortByList[index] === "levelGradeAsc") {
              const comparer = this.sortAlphanumeric(previousElement.levelGrade, nextElement.levelGrade);
              if (comparer === 1 || comparer === -1) {
                return comparer;
              }
            }

            if (
              sortByList[index] === "office" &&
              previousElement[sortByList[index]].officeName > nextElement[sortByList[index]].officeName
            ) {
              return 1;
            }
            if (
              sortByList[index] === "office" &&
              previousElement[sortByList[index]].officeName < nextElement[sortByList[index]].officeName
            ) {
              return -1;
            }
            if (
              sortByList[index] === "dateFirstAvailable" &&
              new Date(previousElement[sortByList[index]]) > new Date(nextElement[sortByList[index]])
            ) {
              return 1;
            }
            if (
              sortByList[index] === "dateFirstAvailable" &&
              new Date(previousElement[sortByList[index]]) < new Date(nextElement[sortByList[index]])
            ) {
              return -1;
            }
            if (previousElement[sortByList[index]] > nextElement[sortByList[index]]) {
              return 1;
            }
            if (previousElement[sortByList[index]] < nextElement[sortByList[index]]) {
              return -1;
            }
          }
        });
      });
    }
  }

  groupResourceBySelectedKey(resources, groupByList) {
    let groupedResources: ResourceGroup[];
    if (!groupByList?.length) {
      groupByList = [];
      groupByList.push("serviceLine");
    }

    if (!resources?.length) {
      return groupedResources;
    }
    // sort resources before grouping
    if (groupByList.indexOf("dateFirstAvailable") > -1) {
      resources.sort((previousElement, nextElement) => {
        return <any>new Date(previousElement.dateFirstAvailable) - <any>new Date(nextElement.dateFirstAvailable);
      });
    } else if (groupByList.indexOf("office") > -1) {
      resources.sort((previousElement, nextElement) => {
        return previousElement.schedulingOffice.officeName - nextElement.schedulingOffice.officeName;
      });
    } else {
      resources
        .sort((previousElement, nextElement) => {
          return previousElement.levelGrade.toString().localeCompare(nextElement.levelGrade, "en", { numeric: true });
        })
        .reverse();
    }

    if (groupByList.indexOf("weeks") > -1) {
      resources.sort((previousElement, nextElement) => {
        return <any>new Date(previousElement.dateFirstAvailable) - <any>new Date(nextElement.dateFirstAvailable);
      });
      this.availabilityyWeeksRange = this.getWeeksRange(resources);
    }

    let groupingArray = [];

    // Loop through each resource
    const reducedArray = this.groupBy(resources, (resource) => {
      const baseGroupingArray = [];

      // Add common grouping criteria
      if (groupByList.indexOf("office") > -1) {
        baseGroupingArray.push(resource.schedulingOffice.officeName);
      }
      if (groupByList.indexOf("cluster") > -1) {
        let resourceOfficeCluster = this.officeList.find(x=>x.officeCode == resource.schedulingOffice.officeCode)?.officeCluster;
        baseGroupingArray.push(resourceOfficeCluster);
      }
      if (groupByList.indexOf("serviceLine") > -1) {
        baseGroupingArray.push(resource.serviceLine.serviceLineName);
      }
      if (groupByList.indexOf("position") > -1) {
        baseGroupingArray.push(resource.position.positionGroupName);
      }
      if (groupByList.indexOf("levelGrade") > -1) {
        baseGroupingArray.push(resource.levelGrade);
      }
      if (groupByList.indexOf("dateFirstAvailable") > -1) {
        const availableDate = resource.prospectiveDateFirstAvailable || resource.dateFirstAvailable;
        if (availableDate) {
          baseGroupingArray.push(moment(availableDate).format("DD-MMM-YYYY"));
        }
      }
      if (groupByList.indexOf("weeks") > -1) {
        const weekStartDate = this.getWeekStartDate(this.availabilityyWeeksRange, resource);
        baseGroupingArray.push(weekStartDate);
      }

      // Array of availabilityStatus
      if (groupByList.indexOf("availability") > -1) 
      {
        groupingArray = [];
        const statuses = Array.isArray(resource.availabilityStatus)
          ? resource.availabilityStatus
          : [resource.availabilityStatus];
          if (Array.isArray(resource.availabilityStatus)) {
            statuses.forEach((status) => {
              groupingArray.push(...baseGroupingArray, status); // Push to reducedArray
            });
          } 
          else {
            groupingArray.push(...baseGroupingArray, resource.availabilityStatus); // Push to reducedArray
        }
      } 
      else 
      {
        groupingArray = baseGroupingArray; 
      }

      return groupingArray;

    });

    let viewAllResources = []

    Object.entries(reducedArray).forEach(([key, value]) => {
      const keyWithoutDoubleQuotes = key.replace(/['"]+/g, "");

      const groupingKey = keyWithoutDoubleQuotes.substring(1, keyWithoutDoubleQuotes.lastIndexOf("]")).split(",");
      const group: ResourceGroup = {
        groupTitle: groupingKey.length > 1 ? groupingKey.join(" - ") : groupingKey[0],
        resources: JSON.parse(JSON.stringify(value))
      };

      group.groupTitle += ` (${group.resources.length})`;
      (groupedResources = groupedResources || []).push(group);

      viewAllResources = viewAllResources.concat(group.resources);
    });

    groupedResources.push({ groupTitle: "View All", resources: viewAllResources });

    return groupedResources;
  }

  getWeekStartDate(availabilityyWeeksRange, resource) {
    let effectiveDate = "";
    for (let index = 0; index < availabilityyWeeksRange.length; index++) {
      const startDate = availabilityyWeeksRange[index];
      const endDate = availabilityyWeeksRange[index + 1];
      if (
        (moment(resource.dateFirstAvailable).isSameOrAfter(startDate) &&
          moment(resource.dateFirstAvailable).isBefore(endDate)) ||
        (endDate === undefined && moment(resource.dateFirstAvailable).isSameOrAfter(startDate))
      ) {
        effectiveDate = DateService.convertDateInBainFormat(startDate);
      }
    }
    return effectiveDate;
  }

  private groupBy(array, func) {
    const groups = {};
    array.forEach((obj) => {
      if (Array.isArray(obj.availabilityStatus) && this.supplyFilterCriteriaObj.groupBy?.includes("availability") ) {
        obj.availabilityStatus.forEach((status) => {
          // Create a new object with the updated availabilityStatus
          const newObj = { ...obj, availabilityStatus: status };
          const group = JSON.stringify(func(newObj));
          groups[group] = groups[group] || [];
          groups[group].push(newObj); // Push the new object
        });
      } else {
        const group = JSON.stringify(func(obj));
        groups[group] = groups[group] || [];
        groups[group].push(obj);
      }
    });
    return groups;
  }

  getWeeksRange(resources: any) {
    const day = 1; // monday
    let availabilityByWeeks = [];
    const firstResourceAvailableDate = moment(resources[0].dateFirstAvailable).clone();

    // get all mondays in the given date range
    if (
      moment(resources[0].dateFirstAvailable)
        .day(7 + day)
        .isAfter(moment(resources[resources.length - 1].dateFirstAvailable))
    ) {
      availabilityByWeeks.push(firstResourceAvailableDate.day(7 + day).clone());
    } else {
      while (
        firstResourceAvailableDate
          .day(7 + day)
          .isSameOrBefore(moment(resources[resources.length - 1].dateFirstAvailable))
      ) {
        availabilityByWeeks.push(firstResourceAvailableDate.clone());
      }
    }
    availabilityByWeeks = [moment(availabilityByWeeks[0]).subtract(7, "days")].concat(availabilityByWeeks);
    availabilityByWeeks.push(moment(availabilityByWeeks[availabilityByWeeks.length - 1]).add(7, "days"));
    return availabilityByWeeks;
  }

  sortAlphanumeric(previous, next) {
    const regexAlpha = /[^a-zA-Z]/g;
    const regexNumeric = /[^0-9]/g;
    const previousAlphaPart = previous.replace(regexAlpha, "");
    const nextAlphaPart = next.replace(regexAlpha, "");
    if (previousAlphaPart === nextAlphaPart) {
      const previousNumericPart = parseInt(previous.replace(regexNumeric, ""), 10);
      const nextNumericPart = parseInt(next.replace(regexNumeric, ""), 10);
      if (previousNumericPart > nextNumericPart) {
        return 1;
      }
      if (previousNumericPart < nextNumericPart) {
        return -1;
      }
    } else {
      if (previousAlphaPart > nextAlphaPart) {
        return 1;
      }
      if (previousAlphaPart < nextAlphaPart) {
        return -1;
      }
    }
  }

  getSelectedViewingGroup(group) {
    let selectedViewingGroup: any;
    // if the group is not null, then select the group otherwise check if the selectedViewingGroup is not null or undefined, then select it otherwise select the first group
    if (group != null) {
      selectedViewingGroup = this.distinctResourceGroupObj.find((groupObj) => group.name === groupObj.name);
      this.selectedViewingGroup = selectedViewingGroup;
    } else {
      if (
        !this.selectedViewingGroup ||
        !this.distinctResourceGroupObj.find((groupObj) => this.selectedViewingGroup.name === groupObj.name)
      ) {
        selectedViewingGroup = this.distinctResourceGroupObj[0];
        this.selectedViewingGroup = selectedViewingGroup;
      }
    }
  }

  resourceSelectedEmitterHandler(event) {
    if (event.isSelected) {
      this.selectedResources.push(event);
    } else {
      this.selectedResources.splice(this.selectedResources.indexOf(event), 1);
    }
  }


  resourcesMultipleSelectionDeselectionEmitterHandler(event) {

    this.selectedResources = event;

  }

  deselectAllResources() {
   this.supplyWeekBucket.toggleResourceSelectionOnTheSupplySide(false);
  }

  selectAllResources(){
    this.supplyWeekBucket.toggleResourceSelectionOnTheSupplySide(true);
  }

  contextMenuOptionClickHandler(event) {
    this.showContextMenu = false;
    this[event.option.action].call(this);
  }

  @HostListener("document:click")
  documentClick(): void {
    this.showContextMenu = false;
  }

  openContextMenu(event) {
    if (this.selectedResources.length > 0) {
      event.stopPropagation();
      this.xCordinateForContextMenu = event.clientX > 1300 ? 1300 : event.clientX;
      this.yCordinateForContextMenu = event.clientY > 422 ? 422 : event.clientY;
      this.showContextMenu = true;
    }
    return false;
  }

  viewResourceData() {
    let employees = [];
    this.selectedResources.forEach((resource) => {
      if (resource.isSelected) {
        employees.push({
          employeeCode: resource.employeeCode,
          employeeName: resource.fullName,
          levelGrade: resource.levelGrade,
          dateFirstAvailable: resource.dateFirstAvailable,
          percentAvailable: resource.percentAvailable,
          internetAddress: resource.internetAddress,
          isNotAvailable:
            resource.isTerminated ||
            resource.onTransitionOrTerminationAndNotAvailable ||
            !resource.dateFirstAvailable ||
            !resource.percentAvailable
        });
      }
    });
    this.resourcesCommitmentsDialogService.showResourcesCommitmentsDialogHandler(employees);
  }

  emailResources(){
    let employees = [];
    this.selectedResources.forEach((resource) => {
      if (resource.isSelected) {
        employees.push({
          internetAddress: resource.internetAddress
        });
      }
    });
    CommonService.sendEmailToSelectedResources(employees);
  }

  onPreviousWeekClick() {
    this.currentPage--;
    this.getWeeksOrDaysToShow();
  }

  onNextWeekClick() {
    this.currentPage++;
    this.getWeeksOrDaysToShow();
  }

  toggleWeeksExpandCollapse(columnIndex: number) {
    const columns = document.querySelectorAll(".column-" + columnIndex);

    if (columns && columns.length) {
      columns.forEach((col) => {
        col.classList.toggle("expanded");
      });
    }
  }

  upsertResourceAllocationsToProjectHandler(upsertedAllocationsData) {
    this.upsertResourceAllocationsToProjectEmitter.emit(upsertedAllocationsData);
  }

  addProjectToUserExceptionHideListHandler(event) {
    this.addProjectToUserExceptionHideListEmitter.emit(event);
  }

  addProjectToUserExceptionShowListHandler(event) {
    this.addProjectToUserExceptionShowListEmitter.emit(event);
  }

  removeProjectFromUserExceptionShowListHandler(event) {
    this.removeProjectFromUserExceptionShowListEmitter.emit(event);
  }

  mergePlanningCardAndAllocationsHandler(event) {
    this.mergePlanningCardAndAllocations.emit(event);
  }

  removePlaceHolderHandler(event) {
    this.removePlaceHolderEmitter.emit(event);
  }

  openPegRFPopUpHandler(pegOpportunityId) {
    this.openPegRFPopUpEmitter.emit(pegOpportunityId);
  }
  showQuickPeekDialogHandler(event) {
    this.showQuickPeekDialog.emit(event);
  }
  openCaseRollPopUpHandler(event) {
    this.openCaseRollForm.emit(event);
  }
  openPlaceholderFormHandler(event) {
    this.openPlaceholderForm.emit(event);
  }
  openOverlappedTeamsPopupHandler(event) {
    this.openOverlappedTeamsForm.emit(event);
  } 
  
  upsertPlaceholderHandler(event) {
    this.upsertPlaceholderEmitter.emit(event);
  }

  updatePlanningCardEmitterHandler(event) {
    this.updatePlanningCardEmitter.emit(event);
  }
  addPlanningCardHandler(event) {
    this.addPlanningCardEmitter.emit(event);
  }
  removePlanningCardEmitterHandler(event) {
    this.removePlanningCardEmitter.emit({ id: event.id });
  }
  sharePlanningCardEmitterHandler(event) {
    this.sharePlanningCardEmitter.emit(event);
  }


}
