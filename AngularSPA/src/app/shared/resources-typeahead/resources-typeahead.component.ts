import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { concat, Observable, Observer, of, Subject } from 'rxjs';
import { catchError, distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { Resource } from '../interfaces/resource.interface';
import { SharedService } from '../shared.service';

@Component({
  selector: 'app-resources-typeahead',
  templateUrl: './resources-typeahead.component.html',
  styleUrls: ['./resources-typeahead.component.scss']
})
export class ResourcesTypeaheadComponent implements OnInit {

  searchInputFocus: Boolean;
  
  resourcesData$: Observable<Resource[]>;
  resourceInput$ = new Subject<string>();

  //-----------------------Input Events--------------------------------------------//
  @Input() searchString: string = '';
  @Input() clearSearchOnSelect:boolean = false; //If passed, will clear search term
  
  //-----------------------Output Events--------------------------------------------//
  @Output() onSearchItemSelect = new EventEmitter();
  
  constructor(private sharedService:  SharedService) { }

  ngOnInit(): void {
    this.attachEventForResourcesSearch();
  }

  // -------------------Helper Functions -----------------------------//

  attachEventForResourcesSearch() {
    this.resourcesData$ = new Observable((observer: Observer<string | undefined>) => {
      observer.next(this.searchString);
    }).pipe(
      switchMap((searchQuery: string) => {
        if (searchQuery) {
          return this.sharedService.getResourcesBySearchString(searchQuery)
            .pipe(
            map((data: Resource[]) => {
              
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
    this.searchInputFocus = true;
  }

  // ---------------------------Component Unload--------------------------------------------//

  ngOnDestroy() {
  }


}
