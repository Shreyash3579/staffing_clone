import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from "@angular/core";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";

// services
import { DateService } from "src/app/shared/dateService";
import { LocalStorageService } from "src/app/shared/local-storage.service";

// interfaces
import { Office } from "src/app/shared/interfaces/office.interface";
import { ProjectDetails } from "src/app/shared/interfaces/projectDetails.interface";
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";

import { Store } from "@ngrx/store";
import * as fromProjects from "../State/case-planning.reducer";
import * as casePlanningActions from "../State/case-planning.actions";
import { ProjectType } from "src/app/shared/constants/enumMaster";
import { Project } from "src/app/shared/interfaces/project.interface";

export interface ProjectData {
  planningCardId: string;
  type: string;
  pipelineId: string;
  clientName: string;
  caseName: string;
  manager: string;
  caseCode: string;
  probabilityPercent: string;
  startDate: string;
  endDate: string;
  includeInDemand: boolean;
  includeInCapacityReporting: boolean;
  allocatedResources?: any;
  office: {
    managingOfficeAbbreviation: string;
    managingOfficeCode: any;
  }
};

export interface HeaderColumn {
  field: string;
  sort: number;
  filter: any;
  selectedFilterList: any;
};

@Component({
  selector: "app-gantt-header",
  templateUrl: "./gantt-header.component.html",
  styleUrls: ["./gantt-header.component.scss"]
})
export class GanttHeaderComponent implements OnInit {
  // inputs
  @Input() dateRange: [Date, Date];
  @Input() show: boolean = true;

  @Input() cases: Project[] = [];
  @Input() planningCards: PlanningCard[] = [];

  // outputs
  @Output() expandCollapseSidebarEmitter = new EventEmitter<boolean>();
  @Output() sortEmitter = new EventEmitter<any>();
  @Output() filterProjectsEmitter = new EventEmitter<any>();

  // local vars
  headerColumns: HeaderColumn[] = [];
  isIncludedInDemand: boolean = true;
  sortOptions = ["asc", "desc"];
  perDayDates = [];
  areFiltersLoaded: boolean = false;

  // filters
  projects: ProjectData[] = [];
  combinedData: ProjectData[] = [];
  selectedMockFilterList = [];
  projectDetails = [];

  clients = [];
  caseNames = [];
  managers = [];
  caseCodes = [];
  probabilities = [];
  startDates = [];
  endDates = [];
  includeInDemand = [];
  offices: Office[] = [];
  officeList = [];

  constructor(private localStorageService: LocalStorageService,
    
    private store: Store<fromProjects.State>,
  ) { }

  ngOnInit() {
    this.offices = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
    this.getColumnValues();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.dateRange && this.dateRange) {
      this.perDayDates = [];
      this.setPerDayInfo();
    }

    if ((changes.cases && this.cases) || (changes.planningCards && this.planningCards)) {
      this.areFiltersLoaded = false;
      this.getProjects();    
      this.getIncludeInDemand();
    }
  }

  // days & weeks
  setPerDayInfo() {
    const projectStartDate = DateService.getFormattedDate(this.dateRange[0]);
    const projectEndDate = DateService.getFormattedDate(this.dateRange[1]);
    let datesBetweenRange = DateService.getDates(projectStartDate, projectEndDate);

    datesBetweenRange.forEach((date, index, selectedDates) => {
      let day = date.format('ddd').toLowerCase();
      let className = day == "sat" ? "weekend" : day == "sun" ? "weekend sunday" : "weekdays";
      let datePart = date.date();
      let monthNumber = date.month() + 1; //+ 1 is done as JavaScript getMonth() index is 0 based not 1 based;
      let monthName = "";
      let weekName = "";
      let weekLength = 0;

      if (
        (index == 0 && selectedDates.length > 1 && selectedDates[index + 1].date() != 1) ||
        datePart == 1
      ) {
        monthName = DateService.getMonthName(monthNumber);
      }

      if ((index == 0 && selectedDates.length > 1) || day == "mon") {
        weekName = DateService.convertDateInBainFormat(date.toDate());
      }
      weekLength = 7 - date.day() + 1; //If week starts on Monday, then week length should be 7 i.e (7 -1 + 1)

      day = day.toUpperCase() + day.slice(1, 2);
      var dateDetails = {
        date: datePart,
        day: day,
        className: "week-length-" + weekLength + " " + className,
        monthName: monthName,
        weekName: weekName,
        weekLength: weekName.length ? weekLength : "",
        fullDate: DateService.convertDateInBainFormat(date.toDate())
      };
      this.perDayDates.push(dateDetails);
    });
  }

  getIncludeInDemand() {
    this.projects.forEach((project) => {
      if (!project.includeInDemand) {
        this.isIncludedInDemand = false;
      }
    });
    this.planningCards.forEach((card) => {
      if (!card.includeInDemand) {
        this.isIncludedInDemand = false;
      }
    });
  }

  // sorting
  toggleSort(columnIndex) {
    let column = this.headerColumns[columnIndex];
    let currentSortOrder = column.sort;

    let newSortOrder = 0;
    if (currentSortOrder === 0) {
      newSortOrder = 1;
    } else if(currentSortOrder === 1){
        newSortOrder = -1;
    }

    column.sort = newSortOrder;
    this.headerColumns.forEach((column, index) => {
      if (index !== columnIndex) {
        column.sort = 0;
      }
    });
    this.sortDataBasedOnSelection(column, newSortOrder);
  }

  filterProjects(value, columnIndex) {
    const column = this.headerColumns[columnIndex];
    const filterItem = column.filter.children.find(item => item.value === value);

    if (filterItem) {
      filterItem.checked = !filterItem.checked;
    }

    column.selectedFilterList = value != '' ? value.split(",") : [];
    const filterData = {
      field: column.field,
      value: column.selectedFilterList
    }
    this.filterProjectsEmitter.emit(filterData);
  }


 sortDataBasedOnSelection(column, newSortOrder) {
  const sortData = {
    column: column,
    sort: newSortOrder,
    projects: this.combinedData
  }
    this.sortEmitter.emit(sortData);
  }

  includeAllInDemand() {
    this.projectDetails = [];
    this.isIncludedInDemand = !this.isIncludedInDemand;
    let projDetails;
    this.cases.forEach((item) => {
      item.includeInDemand = this.isIncludedInDemand;
      projDetails = {
        oldCaseCode: item.oldCaseCode,
        pipelineId: item.pipelineId,
        planningCardId: null,
        includeInDemand: this.isIncludedInDemand,
        isFlagged: item.isFlagged
      };
      this.projectDetails.push(projDetails);
    });
    
    this.planningCards.forEach((item) => {
      item.includeInDemand = this.isIncludedInDemand;
      projDetails = {
        oldCaseCode: null,
        pipelineId: null,
        planningCardId: item.id,
        includeInDemand: this.isIncludedInDemand,
        isFlagged: item.isFlagged
      };
      this.projectDetails.push(projDetails);
    });
    this.upsertCasePlanningProjectDetails();   
  }

  upsertCasePlanningProjectDetails() {
    this.store.dispatch(
      new casePlanningActions.UpsertCasePlanningProjectDetails({
        projDetails: this.projectDetails
      })
    );
  }

  getProjects() {
    this.projects = [];

    if (this.cases.length) {
      this.cases.forEach((item) => {
        let project: ProjectData = {
          pipelineId: item.pipelineId || '',
          planningCardId: '',
          type: item?.type,
          clientName: item?.clientName || '',
          caseName: item?.caseName || item.opportunityName || '',
          manager: item.caseManagerFullName || '',
          caseCode: item?.oldCaseCode || '',
          probabilityPercent: item?.probabilityPercent ? String(item?.probabilityPercent) : null,
          startDate: DateService.convertDateInBainFormat(item?.startDate) || null,
          endDate: DateService.convertDateInBainFormat(item?.endDate) || null,
          includeInDemand: item?.includeInDemand || false,
          includeInCapacityReporting: false,
          allocatedResources: item?.allocatedResources || null,
          office: {
            managingOfficeAbbreviation: item?.managingOfficeAbbreviation || null,
            managingOfficeCode: item?.managingOfficeCode || null
          }
        };

        this.projects.push(project);
        this.combinedData.push(project);
      });
    } else if (this.planningCards.length) {
      this.planningCards.forEach((card) => {
        let project: ProjectData = {
          pipelineId: '',
          planningCardId: card.id || '',
          type: ProjectType.PlanningCard,
          clientName: '',
          caseName: card?.name || '',
          manager: '',
          caseCode: '',
          probabilityPercent: card?.probabilityPercent ? String(card?.probabilityPercent) : null,
          startDate: DateService.convertDateInBainFormat(card?.startDate) || null,
          endDate: DateService.convertDateInBainFormat(card?.endDate) || null,
          includeInDemand: card?.includeInDemand || false,
          includeInCapacityReporting: card?.includeInCapacityReporting || false,
          allocatedResources: card?.regularAllocations || null,
          office: {
            managingOfficeAbbreviation: card?.sharedOfficeAbbreviations || null,
            managingOfficeCode: card?.sharedOfficeCodes || null
          }
        };

        this.projects.push(project);
        this.combinedData.push(project);
      });
    }

    if (this.projects.length) {
      this.areFiltersLoaded = true;

      if (this.areFiltersLoaded) {
        this.getColumnValues();
      } else {
        return;
      }
    }
  }

  getColumnValues() {

    this.projects.forEach((project) => {
      if (project.clientName) {
        this.clients.push(project.clientName);
      }

      if (project.caseName) {
        this.caseNames.push(project.caseName);
      }

      if (project.manager) {
        this.managers.push(project.manager);
      }

      if (project.caseCode) {
        this.caseCodes.push(project.caseCode);
      }

      if (project.probabilityPercent) {
        this.probabilities.push(project.probabilityPercent);
      }

      if (project.startDate) {
        this.startDates.push(project.startDate);
      }

      if (project.endDate) {
        this.endDates.push(project.endDate);
      }

      if (project.includeInDemand) {
        this.includeInDemand.push(project.includeInDemand);
      }
    });

    // set columns
    this.setColumns(0, "client", this.clients);
    this.setColumns(1, "case", this.caseNames);
    this.setColumns(2, "manager", this.managers);
    this.setColumns(3, "code", this.caseCodes);
    this.setColumns(4, "%", this.probabilities);
    this.setColumns(5, "start", this.startDates);
    this.setColumns(6, "end", this.endDates);
    this.setOffice(7, "office", this.offices);

    this.setColumns(8, "include in demand", ["0", "1"]);
  }

  setOffice(index: number, field: string, filtersList) {
    let finalFiltersList = filtersList.map(data => {
      return {
        "text": data.officeName,
        "value": data.officeCode,
        "checked": false
      }
    });

    // get distinct values in finalFiltersList
    finalFiltersList = finalFiltersList.filter((item, index, self) =>
      index === self.findIndex((t) => (
        t.text === item.text
      ))
    );
    finalFiltersList = finalFiltersList.sort((a, b) => a.text.localeCompare(b.text));
    this.headerColumns[index] = { field: field, sort: 0, filter: this.setFilters(finalFiltersList), selectedFilterList: [] };
  }

  // filtering
  setColumns(index: number, field: string, filtersList) {
    let finalFiltersList = filtersList.map(data => {
      return {
        "text": data,
        "value": data,
        "checked": false
      }
    });

    // get distinct values in finalFiltersList
    finalFiltersList = finalFiltersList.filter((item, index, self) =>
      index === self.findIndex((t) => (
        t.text === item.text
      ))
    );
    finalFiltersList = finalFiltersList.sort((a, b) => a.text.localeCompare(b.text));
    this.headerColumns[index] = { field: field, sort: 0, filter: this.setFilters(finalFiltersList), selectedFilterList: [] };
  }

  setFilters(filtersList) {
    return {
      text: "all",
      value: 1,
      collapsed: false,
      checked: false,
      children: filtersList
    }
  }
}