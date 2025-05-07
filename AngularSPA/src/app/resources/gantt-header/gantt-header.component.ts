// ----------------------- Angular Package References ----------------------------------//
import {
  Component,
  OnInit,
  Input,
  OnChanges,
  SimpleChanges,
  Output,
  EventEmitter
} from "@angular/core";

// --------------------------Interfaces -----------------------------------------//
import { HeaderConfig } from "src/app/shared/interfaces/headerConfig.interface";

// ----------------------- Service References ----------------------------------//
import { DateService } from "src/app/shared/dateService";
import { EmployeeCaseGroupingEnum } from "src/app/shared/constants/enumMaster";
import { WeeklyMonthlyGroupingEnum } from "src/app/shared/constants/enumMaster";
import {Dayjs} from "dayjs";
import {ConstantsMaster} from "../../shared/constants/constantsMaster";

@Component({
  selector: "[resources-gantt-header]",
  templateUrl: "./gantt-header.component.html",
  styleUrls: ["./gantt-header.component.scss"]
})
export class GanttHeaderComponent implements OnInit, OnChanges {
  public perDayDate = [];
  public perWeekDate = [];
  public isLeftSideBarCollapsed = false;
  public isTopbarCollapsed = false;
  public monthRowsCount = {};
  public distinctMonths = [];
  public notesAlertText = ConstantsMaster.NotesAlert;
  public recentCdInfo = ConstantsMaster.RecentCd;
  public commercialModelInfo = ConstantsMaster.CommercialModel;
  public weeklyMonthlyGroupingEnum = WeeklyMonthlyGroupingEnum;
  public leftSideHeaders : HeaderConfig[] = [];

  public employeeHeaders: HeaderConfig[] =[];

    public caseCodeHeaders: HeaderConfig[] = [
        { label: "Client", id: "client", sortDirection : '',isHidden: false },
        { label: "Code", id: "caseCode", sortDirection : '' ,isHidden: false},
        { label: "Start", id: "startDate", sortDirection : '',isHidden: false },
        { label: " ", id: " ", sortDirection : '',isHidden: false },
        { label: "End", id: "endDate", sortDirection : '',isHidden: false },
        { label: " ", id: " ", sortDirection : '' ,isHidden: false}
    ];

// -----------------------Input Variables-----------------------------------------------//
  @Input() dateRange: [Date, Date];
  @Input() selectedEmployeeCaseGroupingOption: string;
  @Input() selectedWeeklyMonthlyGroupingOption: string;
  @Input() isSelectedPracticeView:boolean;

// ------------------------Output Events--------------------------------------------//
  @Output() expandCollapseSidebarEmitter = new EventEmitter();
  @Output() expandCollapseTopbarEmitter = new EventEmitter();

  constructor() {}

  // ------------------------Lief Cycle Events--------------------------------------------//
  ngOnInit() {
    this.setGanttHeaders();
  }

  ngOnChanges(changes: SimpleChanges) {

    if (changes.dateRange && this.dateRange) {
      this.perDayDate = [];
      this.setPerDayInfo();
    }

    if (changes.selectedEmployeeCaseGroupingOption && changes.selectedEmployeeCaseGroupingOption.currentValue) {
      this.setGanttHeaders();
    }

    if(changes.isSelectedPracticeView){
      this.updateEmployeeHeaders();
    }
  }

  // ------------------------Private Helper Methods--------------------------------------------//
  updateEmployeeHeaders() {
    this.employeeHeaders = [
      { label: "Employee", id: "fullName", sortDirection: '', isHidden: false },
      { label: "Position", id: "position", sortDirection: '', isHidden: this.isSelectedPracticeView },
      { label: "Lvl", id: "levelGrade", sortDirection: '', isHidden: false },
      { label: "Off", id: "office", sortDirection: '', isHidden: false },
      { label: "%", id: "percentAvailable", sortDirection: '', isHidden: false },
      { label: "Date", id: "dateFirstAvailable", sortDirection: '', isHidden: false },
      { label: "Note", id: "note", sortDirection: '', isHidden: !this.isSelectedPracticeView },
      { label: "CD", id: "recentcd", sortDirection: '', isHidden: !this.isSelectedPracticeView, hasInfoIcon :true, infoText: this.recentCdInfo  },
      { label: "Commercial Model", id: "Commercial Model", sortDirection: '', isHidden: !this.isSelectedPracticeView, hasInfoIcon: true, infoText: this.commercialModelInfo }
    ];
    this.setGanttHeaders();
  }

  setGanttHeaders(){
    if(this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES){
        this.leftSideHeaders =  this.caseCodeHeaders;
        this.isTopbarCollapsed = true;
    }else{
        this.leftSideHeaders =  this.employeeHeaders;
        this.isTopbarCollapsed = false;
    }
  }

  // Collapse & Expand All Rows
  expandCollapseAllRows() {
    this.isTopbarCollapsed = !this.isTopbarCollapsed;
    this.expandCollapseTopbarEmitter.emit(this.isTopbarCollapsed);
  }

  // Collapse Left Sidebar
  expandCollapseSidebar(event) {
    this.isLeftSideBarCollapsed = !this.isLeftSideBarCollapsed;
    this.expandCollapseSidebarEmitter.emit(this.isLeftSideBarCollapsed);

  }

  setPerDayInfo() {
      const projectStartDate = DateService.getFormattedDate(this.dateRange[0]);
      const projectEndDate = DateService.getFormattedDate(this.dateRange[1]);
      let datesBetweenRange: Dayjs[] = DateService.getDates(
          projectStartDate,
          projectEndDate
      );

      let monthName = "";
      datesBetweenRange.forEach((date, index, selectedDates) => {
          let day = DateService.getDayAbbreviationsInLowerCase(date); //gets weekday abbreviations like "mon", tues etc
          let className =
              day == "sat" ? "weekend" : day == "sun" ? "weekend sunday" : "";
          let datePart = date.date();
          let monthNumber = date.month() + 1; //+ 1 is done as JavaScript getMonth() index is 0 based not 1 based;
          let weekName = "";

          if (
              (index == 0 &&
                  selectedDates.length > 0 &&
                  selectedDates[index + 1].date() != 1) ||
              datePart == 1
          ) {
              monthName = DateService.getMonthName(monthNumber);
          }

          if ((index == 0 && selectedDates.length > 1) || day == "mon") {
              weekName = DateService.convertDateInBainFormat(date.toDate());
          }

          day = day[0].toUpperCase() + day.slice(1, 2);
          var dateDetails = {
              date: datePart,
              day: day,
              className: className,
              monthName: monthName,
              weekName: weekName,
              fullDate: DateService.convertDateInBainFormat(date.toDate())
          };

          this.perDayDate.push(dateDetails);
      });
      this.perWeekDate = this.perDayDate.filter((date) => date.weekName != "");
      console.log(this.perDayDate);
      console.log(this.perWeekDate);
      const monthsName = new Set(this.perWeekDate.map(date => date.monthName));
      this.distinctMonths = Array.from(monthsName);

      this.monthRowsCount = this.perWeekDate.reduce((acc, date) => {
        if (!acc[date.monthName]) {
          acc[date.monthName] = 0;
        }
        acc[date.monthName]++;
        return acc;
      }, {});
  }

  getWidth(month: string): number {
    return this.monthRowsCount[month] * 32 * 2;
}

}
