import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { catchError, concat, distinctUntilChanged, Observable, of, Subject, switchMap, tap } from 'rxjs';
import { Places } from 'src/app/shared/interfaces/places.interface';
import { SharedService } from 'src/app/shared/shared.service';
import { StaffingIntakeFormService } from 'src/app/staffing-intake-form/staffing-intake-form.service';

@Component({
  selector: 'shared-places-typeahed',
  standalone: true,
  imports: [NgSelectModule, CommonModule, FormsModule ],
  templateUrl: './places-typeahed.component.html',
  styleUrl: './places-typeahed.component.scss',
  providers: [SharedService, StaffingIntakeFormService]
})
export class PlacesTypeahedComponent implements OnInit {
  @Input() multiple = false;
  @Input() selectedPlace: Places;

  @Output() onSearchItemSelect = new EventEmitter<Places>();

  placesData$: Observable<Places[]>;
  placesInput$ = new Subject<string>();

  constructor(private caseIntakeService: StaffingIntakeFormService) {}

  ngOnInit(): void {
    this.attachEventForResourcesSearch();
  }

  attachEventForResourcesSearch() {
    this.placesData$ = this.placesInput$.pipe(
      distinctUntilChanged(),
      switchMap(term =>
        term && term.length >= 3
          ? this.caseIntakeService.getPlacesBySearchString(term).pipe(
            catchError(() => of([])) // empty list on error
          )
          : of([])
      )
    );
  }

  onResourceSearchChange($event) {
    this.placesInput$.next($event.term);
  }

  selectedResourcesChange(selectedItem: Places) {
    this.onSearchItemSelect.emit(selectedItem);
  }

  onClear() {
    this.selectedPlace = null;
  }
}
