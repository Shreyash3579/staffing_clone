import { Component, EventEmitter, OnInit, Output } from "@angular/core";
import { CoreService } from "src/app/core/core.service";
import { EmployeeCaseGroupingEnum, WeeklyMonthlyGroupingEnum } from "src/app/shared/constants/enumMaster";

@Component({
  selector: "resources-grouping",
  templateUrl: "./grouping.component.html",
  styleUrls: ["./grouping.component.scss"]
})
export class GroupingComponent implements OnInit {
  @Output() onToggleEmployeeCaseGroup = new EventEmitter<string>();
  @Output() onToggleWeeklyMonthlyGroup = new EventEmitter<string>();
  @Output() onTogglePracticeView = new EventEmitter<boolean>();
  selectedGroupingOption = "employee";
  selectedGroupingViewOption = "weekly";
  isSelectedPracticeView:boolean = false;

  constructor(private coreService:CoreService) {}

  ngOnInit(): void {
    // commenting this so that Practice Staffing users have staffing view as default
    this.updateViewOptionsForPracticeStaffingUsers();
  }
  

  private isPracticeStaffingUser(): boolean {
    return this.coreService.loggedInUserClaims.Roles?.includes('PracticeStaffing') || false;
  }
  
  private updateViewOptionsForPracticeStaffingUsers(): void {
    this.isSelectedPracticeView = this.isPracticeStaffingUser();   
    this.selectedGroupingViewOption = this.isSelectedPracticeView ? 'monthly' : this.selectedGroupingViewOption;

    if (this.isSelectedPracticeView) {

      this.onToggleWeeklyMonthlyGroup.emit(this.selectedGroupingViewOption);
      this.onTogglePracticeView.emit(this.isSelectedPracticeView);
    }
  }

  onModeChange(event) {
    if (event.target.checked === true) {
      this.selectedGroupingOption = EmployeeCaseGroupingEnum.CASES
    } else {
      this.selectedGroupingOption = EmployeeCaseGroupingEnum.RESOURCES;
    }

    this.onToggleEmployeeCaseGroup.emit(this.selectedGroupingOption);
  }

  onWeeklyViewChange(event) {
    if (event.target.checked === true) {
      this.selectedGroupingViewOption = WeeklyMonthlyGroupingEnum.MONTHLY;
    } else {
      this.selectedGroupingViewOption = WeeklyMonthlyGroupingEnum.WEEKLY;
    }

    this.onToggleWeeklyMonthlyGroup.emit(this.selectedGroupingViewOption);
  }

  onPracticeViewChange(event){
   if(event.target.checked === true){
    this.isSelectedPracticeView = false;
    this.selectedGroupingViewOption = WeeklyMonthlyGroupingEnum.WEEKLY;
   }

   else{
    this.isSelectedPracticeView = true;
    this.selectedGroupingViewOption = WeeklyMonthlyGroupingEnum.MONTHLY;
   }

  this.onToggleWeeklyMonthlyGroup.emit(this.selectedGroupingViewOption);
   this.onTogglePracticeView.emit(this.isSelectedPracticeView);
  }
}
