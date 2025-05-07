import { Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { fromEvent, Subject } from 'rxjs';
import { debounceTime, map } from 'rxjs/operators';
import { LaunchDarklyService } from 'src/app/core/services/launch-darkly.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { Project } from 'src/app/shared/interfaces/project.interface';

@Component({
  selector: 'app-case-planning-filter',
  templateUrl: './case-planning-filter.component.html',
  styleUrls: ['./case-planning-filter.component.scss']
})
export class CasePlanningFilterComponent implements OnInit, OnDestroy {
  // -----------------------Local Variables--------------------------------------------//

  projectSearchInputFocus: Boolean;
  asyncProjectString = '';
  feature = ConstantsMaster.appScreens.feature;
  // -----------------------Input Variables--------------------------------------------//
  @Input() clearSearch: Subject<boolean>;
  @Input() searchedProjects: Subject<Project[]>;
  
  // -----------------------Output Events--------------------------------------------//
  @Output() toggleFiltersSectionEmitter = new EventEmitter<any>();
  @Output() getProjectsBySearchString = new EventEmitter<any>();
  @Output() openProjectDetailsDialogFromTypeahead = new EventEmitter();
  @Output() openCasePlanningWhiteboardEmmitter = new EventEmitter();

  // -----------------------Templare Reference Variables--------------------------------------------//
  @ViewChild('projectSearchInput', { static: true }) projectSearchInput: ElementRef;

  constructor(private launchDarklyService : LaunchDarklyService) { }

  ngOnInit() {
    this.attachEventToSearchBox();
    this.launchDarklyService.trackUser(); //used to save the logged in user in launchdarkly
    
  }

  //--------------------Event Handlers --------------------------------//

  // setFeatureFlagVariationOnCasePlanningBoard(){
  //   if(this.launchDarklyService.isClientInitialized){
  //     this.showPlanningBoardFeature = this.launchDarklyService.getFeatureFlagVariation(FeatureFlagNames.CASE_PLANNING_WHITEBOARD);
  //   }else{
  //     this.launchDarklyService.waitForLDClientInitialization().then(() => {
  //       this.showPlanningBoardFeature = this.launchDarklyService.getFeatureFlagVariation(FeatureFlagNames.CASE_PLANNING_WHITEBOARD);
  //     });
  //   }
    
  // }

  toggleFiltersSection(){
    this.toggleFiltersSectionEmitter.emit();
  }

  openCasePlanningWhiteboard(){
    this.openCasePlanningWhiteboardEmmitter.emit();
  }
  // -------------------Helper Functions -----------------------------//

  attachEventToSearchBox() {
    this.clearSearch.subscribe(value => {
      this.clearSearchBox(value);
    });

    fromEvent(this.projectSearchInput.nativeElement, 'keyup').pipe(
      map((event: any) => {
        return event.target.value;
      }),
      debounceTime(500)
      // ,distinctUntilChanged() //removing this as it was craeting testin to and fro resources
    ).subscribe((text: string) => {
      this.searchProjects(text);
    });
  }

  clearSearchBox(clearSearchOnly) {
    this.asyncProjectString = ''
    this.projectSearchInput.nativeElement.value = '';
    if (!clearSearchOnly) {
      this.searchProjects();
    }
  }

  searchProjects(searchText = '') {
    this.getProjectsBySearchString.emit({ typeahead: searchText });
  }

  typeaheadOnSelect(data) {
    this.clearSearchBox(true);
    this.openProjectDetailsDialogFromTypeahead.emit(data.item);
  }

  // ---------------------------Component Unload--------------------------------------------//

  ngOnDestroy() {
    // unsubscribe to ensure no memory leaks
    this.clearSearch.unsubscribe();
  }

}
