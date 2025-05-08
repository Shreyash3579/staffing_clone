// -------------------Angular Operators---------------------------------------//
import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { Subject } from 'rxjs';


// -------------------Interfaces---------------------------------------//
import { LevelGrade } from 'src/app/shared/interfaces/levelGrade.interface';
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
import { ResourceGroup } from '../../shared/interfaces/resourceGroup.interface';
import { ServiceLineHierarchy } from 'src/app/shared/interfaces/serviceLineHierarchy';
import { MatTabGroup } from '@angular/material/tabs';
import { PracticeArea } from 'src/app/shared/interfaces/practiceArea.interface';
import { PositionHierarchy } from 'src/app/shared/interfaces/positionHierarchy.interface';
import { AffiliationRole } from 'src/app/shared/interfaces/affiliationRole.interface';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  // -----------------------Input Variables--------------------------------------------//
  @ViewChild(MatTabGroup, { static: true }) tabGroup: MatTabGroup;

  // -----------------------Input Variables--------------------------------------------//
  @Input() clearSearch: Subject<boolean>;
  @Input() filteredResourceGroups: ResourceGroup[]; // Store resources received from Database
  @Input() searchedResourceGroups: ResourceGroup[];
  @Input() officeHierarchy: OfficeHierarchy;
  @Input() staffingTagsHierarchy: ServiceLineHierarchy[];
  @Input() positionsHierarchy: PositionHierarchy[];
  @Input() levelGrades: LevelGrade[];
  @Input() practiceAreas: PracticeArea[];
  @Input() affiliationRoles: AffiliationRole[];
  @Input() resourceLength: number;

  // -----------------------Output Events--------------------------------------------//

  @Output() openResourceDetailsDialog = new EventEmitter();
  @Output() getResources = new EventEmitter();
  @Output() getResourcesIncludingTerminatedBySearchString = new EventEmitter();
  @Output() openQuickAddForm = new EventEmitter();
  @Output() getResourcesSortAndGroupBySelectedValuesEmitter = new EventEmitter<any>();
  @Output() getResourcesAvailabilityBySelectedValuesEmitter = new EventEmitter<any>();
  @Output() openAddTeams = new EventEmitter();
  @Output() openResourcesCommitmentsDialog = new EventEmitter();

  constructor() { }

  // -----------------------Component LifeCycle Events and Functions-------------------//

  ngOnInit() { }

  // -----------------------Output Event Handlers-----------------------------------//

  openResourceDetailsDialogHandler(event) {
    this.openResourceDetailsDialog.emit(event);
  }

  getResourcesHandler(event) {
    this.getResources.emit(event);
  }

  getResourcesIncludingTerminatedBySearchStringHandler(event) {
    this.getResourcesIncludingTerminatedBySearchString.emit(event);
  }

  /*
   * Event triggered from quick add button in filters component
   * Propogates event to parent for opening bootstrap modal
  */
  openQuickAddFormHandler(event) {
    this.openQuickAddForm.emit(event);
  }

  getResourcesSortAndGroupBySelectedValuesHandler(event) {
    this.getResourcesSortAndGroupBySelectedValuesEmitter.emit(event);
  }

  getResourcesAvailabilityBySelectedValuesHandler(event) {
    this.getResourcesAvailabilityBySelectedValuesEmitter.emit(event);
  }

  openAddTeamsHandler(event) {
    this.openAddTeams.emit(event);
    this.tabGroup.selectedIndex = 0;
  }

  openResourcesCommitmentsDialogHandler(event) {
    this.openResourcesCommitmentsDialog.emit(event);
  }
}
