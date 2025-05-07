import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { Observable, Observer, of, Subject } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { DateService } from '../dateService';
import { Project } from '../interfaces/project.interface';
import { SharedService } from '../shared.service';

@Component({
  selector: 'app-cases-typeahead',
  templateUrl: './cases-typeahead.component.html',
  styleUrls: ['./cases-typeahead.component.scss']
})
export class CasesTypeaheadComponent implements OnInit, OnDestroy {
  projectSearchInputFocus: Boolean;
  searchString: string = '';
  searchedProjects$: Observable<Project[]>;

  //-----------------------Input Events--------------------------------------------//
  @Input() minStartDate: Date; //If passed, will return only projects having startdate on or after this date 
  @Input() searchOpps:boolean = true; //If passed, will not search for opportunities
  @Input() clearSearchOnSelect:boolean = true; //If passed, will clear search term
  
  //-----------------------Output Events--------------------------------------------//
  @Output() onSearchItemSelect = new EventEmitter();
  
  constructor(private sharedService:  SharedService) { }

  ngOnInit(): void {
    this.attachEventToSearchBox();
  }

  // -------------------Helper Functions -----------------------------//

  attachEventToSearchBox() {

    this.searchedProjects$ = new Observable((observer: Observer<string | undefined>) => {
      observer.next(this.searchString);
    }).pipe(
      switchMap((searchQuery: string) => {
        if (searchQuery) {
          return this.sharedService.getProjectsBySearchString(searchQuery)
            .pipe(
            map((data: Project[]) => {
              
              if(this.minStartDate){
                data = data.filter( x=> DateService.isSameOrAfter(x.startDate, this.minStartDate));
              }

              if(!this.searchOpps){
                data = data.filter( x=> x.oldCaseCode);
              }

              return data || [];
            })
          );
        }
 
        return of([]);
      })
    );

  }

  clearSearchBox() {
    this.searchString = '';
  }

  typeaheadOnSelect(data) {
    if(this.clearSearchOnSelect){
      this.clearSearchBox();
    }
    
    this.onSearchItemSelect.emit(data.item);
  }

  // ---------------------------Component Unload--------------------------------------------//

  ngOnDestroy() {
  }

}
