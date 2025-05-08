import { Component, OnInit, Output, Input, EventEmitter } from "@angular/core";
import { Subject } from "rxjs";

// services
import { CoreService } from "src/app/core/core.service";

// interfaces
import { LevelGrade } from "src/app/shared/interfaces/levelGrade.interface";
import { OfficeHierarchy } from "src/app/shared/interfaces/officeHierarchy.interface";
import { ServiceLineHierarchy } from "src/app/shared/interfaces/serviceLineHierarchy";
import { PracticeArea } from "src/app/shared/interfaces/practiceArea.interface";
import { PositionHierarchy } from "src/app/shared/interfaces/positionHierarchy.interface";

// external
import { BsDatepickerConfig } from "ngx-bootstrap/datepicker";
import { AffiliationRole } from "src/app/shared/interfaces/affiliationRole.interface";
import { OpportunityStatusType } from "src/app/shared/interfaces/opportunityStatusType";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { takeUntil } from "rxjs/operators";
import { CaseType } from "src/app/shared/interfaces/caseType.interface";

@Component({
  selector: "[app-menubar]",
  templateUrl: "./menubar.component.html",
  styleUrls: ["./menubar.component.scss"]
})
export class MenubarComponent implements OnInit {
  destroy$: Subject<boolean> = new Subject<boolean>();
  
  // inputs
  @Input() isStaffedFromSupply: boolean;
  @Input() isMenuOpen: boolean;
  // outputs
  @Output() toggleMenu = new EventEmitter();
  @Output() getProjectsOnAdvancedFilterChange = new EventEmitter();
  @Output() getResourcesOnAdvancedFilterChange = new EventEmitter();
  @Output() getAllocationsSortedBySelectedValueEmitter = new EventEmitter<any>();
  
  // local variables
  userPreferences: any;
  officeHierarchy: OfficeHierarchy;
  staffingTagsHierarchy: ServiceLineHierarchy[];
  positionsHierarchy: PositionHierarchy[];
  levelGrades: LevelGrade[];
  practiceAreas: PracticeArea[];
  isMinOppProbabilityFilterShown: boolean = true;
  opportunityStatusTypes: OpportunityStatusType[];
  industryPracticeAreas: PracticeArea[];
  capabilityPracticeAreas: PracticeArea[];
  caseTypes: CaseType[];
  demandTypes: any[];
  availabilityIncludes: any;
  groupsBy: any;
  sortsBy: any;
  affiliationRoles: AffiliationRole[];

  bsConfig: Partial<BsDatepickerConfig>;
  selectedDateRange: any;

  showFilterTab: number = 1;
  filters = [
    { label: "Resources", selected: false, tab: 1 },
    { label: "Cases/Opportunites", selected: false, tab: 2 }
  ];

  constructor(private coreService: CoreService,
    private localStorageService: LocalStorageService) {}

  ngOnInit(): void {
    this.subscribeUserPreferences();
    this.getLookupListFromLocalStorage();
    this.loadMasterDataForResourcesDropdown();
  }

  subscribeUserPreferences() {
    this.coreService.getUserPreferences().pipe(takeUntil(this.destroy$))
    .subscribe((userPreferences) => {
      if (userPreferences) {
        // set default startDate to always be Monday of current week
        const startDate = new Date();
        startDate.setDate(startDate.getDate() - ((startDate.getDay() + 6) % 7));

        const endDate = new Date();
        endDate.setDate(startDate.getDate() + 28);

        this.selectedDateRange = [startDate, endDate];
      }
      this.userPreferences = userPreferences;
    });

  }

  getLookupListFromLocalStorage() {
    this.opportunityStatusTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.opportunityStatusTypes);
    this.industryPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.industryPracticeAreas);
    this.capabilityPracticeAreas = this.localStorageService.get(
      ConstantsMaster.localStorageKeys.capabilityPracticeAreas
    ); 
    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.positionsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsHierarchy);
    this.levelGrades = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGradesHierarchy);
    this.practiceAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.practiceAreas);
    this.staffingTagsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTagsHierarchy);
    this.caseTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseTypes);
    this.demandTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.demandTypes);
    this.affiliationRoles = this.localStorageService.get(ConstantsMaster.localStorageKeys.affiliationRoles);
  }

  loadMasterDataForResourcesDropdown() {
    this.availabilityIncludes = ConstantsMaster.availabilityIncludes;
    this.groupsBy = ConstantsMaster.groupBy;
    this.sortsBy = ConstantsMaster.sortBy;
  }


  // select a filter tab
  selectFilterTab(tab: number) {
    this.filters.forEach((filter) => {
      if (filter.tab == tab) {
        filter.selected = true;
        this.showFilterTab = filter.tab;
      } else {
        filter.selected = false;
      }
    });
  }

  toggleMenubar() {
    this.toggleMenu.emit();
  }

  getProjectsOnAdvancedFilterChangeHandler(event){
    this.getProjectsOnAdvancedFilterChange.emit(event);
  }

  getResourcesOnAdvancedFilterChangeHandler(event){
    this.getResourcesOnAdvancedFilterChange.emit(event);
  }

  getAllocationsSortedBySelectedValueHandler(event) {
    this.getAllocationsSortedBySelectedValueEmitter.emit(event);
  }

  ngOnDestroy() {
    this.destroy$.unsubscribe();
  }

}