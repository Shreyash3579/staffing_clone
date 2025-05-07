import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgSelectComponent, NgSelectModule } from '@ng-select/ng-select';
import { catchError, concat, distinctUntilChanged, Observable, of, Subject, switchMap, tap } from 'rxjs';
import { Resource } from 'src/app/shared/interfaces/resource.interface';
import { SharedService } from 'src/app/shared/shared.service';

@Component({
  selector: 'shared-resources-typeahed',
  standalone: true,
  imports: [NgSelectModule, CommonModule, FormsModule ],
  templateUrl: './resources-typeahed.component.html',
  styleUrl: './resources-typeahed.component.scss',
  providers: [SharedService]
})
export class ResourcesTypeahedComponent implements  OnInit, OnChanges{
  @Input() multiple = false;
  @Input() selectedResource: Resource;

  @ViewChild(NgSelectComponent, { static: false }) selectedResourceInput!: NgSelectComponent;

  //-----------------------Output Events--------------------------------------------//
  @Output() onSearchItemSelect = new EventEmitter();
  resourcesData$: Observable<Resource[]>;
  resourceInput$ = new Subject<string>();
  isResourceSearchOpen = false;
  
  constructor(private sharedService: SharedService) {}

  ngOnInit(): void {
    this.attachEventForResourcesSearch();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if(changes.selectedResource){

    }
  }

  attachEventForResourcesSearch() {
    this.resourcesData$ = concat(
      of([]), // default items
      this.resourceInput$.pipe(
        distinctUntilChanged(),
        switchMap(term => this.sharedService.getResourcesBySearchString(term).pipe(
          catchError(() => of([])), // empty list on error
        )),
        tap(() => {
          this.isResourceSearchOpen = true;
        })
      )
    );

  }
  
  onResourceSearchChange($event) {
    if ($event.term.length > 2) {
      this.resourceInput$.next($event.term);
    }

    // to reset search term if keyword's length is less than 3
    if ($event.term.length < 3) {
      this.resourceInput$.next(null);
    }

    // TODO: below condition should be removed once permanent solution is applied
    if ($event.term.length < 1) {
      this.isResourceSearchOpen = false;
    }
  }

  selectedResourcesChange(selectedItem) {
    this.isResourceSearchOpen = false;
    this.onSearchItemSelect.emit(selectedItem);
  }

  focus() {
    if (this.selectedResourceInput) {
      this.selectedResourceInput.focus();
    }
  }
}
