// -------------------Angular References---------------------------------------//
import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';

// -------------------Interfaces---------------------------------------//
import { Project } from '../interfaces/project.interface';
import { DateService } from 'src/app/shared/dateService';

// ---------------------External Libraries ---------------------------------//
import { Observable } from 'rxjs';
import { debounceTime, map, mergeMap } from 'rxjs/operators';
import { SharedService } from '../shared.service';
import * as moment from 'moment';
import { PlanningCard } from '../interfaces/planningCard.interface';
import { PlanningCardModel } from '../interfaces/planningCardModel.interface';
import { CommonService } from '../commonService';
import { ConstantsMaster } from '../constants/constantsMaster';
import { LocalStorageService } from '../local-storage.service';
import { ValidationService } from '../validationService';

@Component({
  selector: 'planning-card-typeahead',
  templateUrl: './planning-card-typeahead.component.html',
  styleUrls: ['./planning-card-typeahead.component.scss']
})
export class PlanningCardTypeAheadComponent implements OnInit {

  //-------------------- Input events ------------------------ //
  @Input() isProjectInvalid;
  

  //-------------------- Output events ------------------------ //
  @Output() onPlanningCardSearchItemSelect = new EventEmitter();

  //----------------------Local Variables ---------------------------- /
  public projects$: Observable<PlanningCardModel[]>;
  startDateLabel: string;
  endDateLabel: string;
  public noCaseOppFound = false;
  @Input() asyncProjectString: string;
  public noResultsFoundMsg = ValidationService.noResultsFoundMsg;

  constructor(private sharedService: SharedService, 
    private localStorageService: LocalStorageService) { }

  ngOnInit(): void {
    this.attachEventForProjectSearch();
  }

  attachEventForProjectSearch() {
    this.projects$ = new Observable((observer: any) => {
      // Runs on every search
      observer.next(this.asyncProjectString);
    }).pipe(
      debounceTime(1000),
      mergeMap((token: string) => {
        return this.sharedService.getPlanningCardsBySearchString(token)
        .pipe(
          map(data => {
            const officeFlatList = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
            const staffingTagsList = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTags);
            return CommonService.ConvertToPlanningCardViewModel(data, officeFlatList, staffingTagsList);
          })
        )
      })
    );
  }

  typeaheadOnSelect(data) {
    this.onPlanningCardSearchItemSelect.emit(data.item);
  }

  typeaheadNoResultsHandler(event: boolean): void {
    this.noCaseOppFound = event;
  }
}
