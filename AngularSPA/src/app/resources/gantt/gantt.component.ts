// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, Input, Output, EventEmitter, ViewChild, ElementRef, SimpleChanges, OnChanges, ViewChildren, QueryList } from '@angular/core';
// import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';

// --------------------------Interfaces -----------------------------------------//
import { SupplyFilterCriteria } from 'src/app/shared/interfaces/supplyFilterCriteria.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { Commitment } from 'src/app/shared/interfaces/commitment.interface';
import { EmployeeCaseGroupingEnum } from 'src/app/shared/constants/enumMaster';
import { GanttBodyComponent } from '../gantt-body/gantt-body.component';
import { Observable, merge, Subscription } from 'rxjs';
import { ResourcesCount } from 'src/app/shared/interfaces/resourcesCount.interface';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { ResourceStaffing } from 'src/app/shared/interfaces/resourceStaffing.interface';
import { Resource } from 'src/app/shared/interfaces/resource.interface';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { CoreService } from '../../core/core.service';

@Component({
  selector: 'resources-gantt',
  templateUrl: './gantt.component.html',
  styleUrls: ['./gantt.component.scss']
})
export class GanttComponent implements OnInit, OnChanges {

  public isLeftSideBarCollapsed = false;
  public objGanttCollapsedRows = {
    isAllCollapsed: false,
    exceptionRowIndexes: []
  };

  public employeeCaseGroupingEnum = EmployeeCaseGroupingEnum;
  public isTopbarCollapsed: boolean = true;

  // -----------------------Input Events-----------------------------------------------//

  @Input() resourcesStaffing: any;
  @Input() supplyFilterCriteriaObj: SupplyFilterCriteria;
  @Input() dateRange: [Date, Date];
  @Input() selectedCommitmentTypes: string[];
  @Input() thresholdRangeValue; // Threshold Input
  @Input() selectedEmployeeCaseGroupingOption: string;
  @Input() selectedWeeklyMonthlyGroupingOption: string;
  @Input() isSelectedPracticeView: boolean;
  @Input() isPdfExport: boolean;
  @Input() resourcesCountOnCaseOpp: ResourcesCount[];

  // ------------------------Output Events--------------------------------------------//

  @Output() updateResourceAssignmentToProject = new EventEmitter<ResourceAllocation>();
  @Output() upsertResourceAllocationsToProject = new EventEmitter<ResourceAllocation[]>();
  @Output() upsertPlaceholderAllocationsToProject = new EventEmitter<PlaceholderAllocation>();
  @Output() updateResourceCommitment = new EventEmitter<Commitment>();
  @Output() openQuickAddForm = new EventEmitter<any>();
  @Output() openResourceDetailsDialog = new EventEmitter();
  @Output() openSplitAllocationPopup = new EventEmitter();
  @Output() openCaseDetailsDialog = new EventEmitter();
  @Output() upsertResourceViewNote = new EventEmitter<any>();
  @Output() deleteResourceViewNotes = new EventEmitter<any>();
  @Output() upsertResourceRecentCD = new EventEmitter<any>();
  @Output() deleteResourceRecentCD = new EventEmitter<any>();
  @Output() upsertResourceCommercialModel = new EventEmitter<any>();
  @Output() deleteResourceCommercialModel = new EventEmitter<any>();
  @Output() selectedResourceViewTab = new EventEmitter<any>();
  @Output() ganttBodyLoadedEmitter = new EventEmitter();
  @Output() openOverlappedTeamsForm = new EventEmitter<any>();


  //------------------------Referenced by Parent--------------------------------------------//
  @ViewChild('ganttContainer', { static: false }) ganttContainer: ElementRef; //used by parent to reset scrolling
  @ViewChildren(GanttBodyComponent) caseGroupRowComponents: QueryList<GanttBodyComponent>;
  @ViewChild(CdkVirtualScrollViewport, { static: false }) viewport!: CdkVirtualScrollViewport;

  childrenDetector: Observable<any>;
  itemSizeForVirtualScroll;
  
  // TODO: Currently export functionality is working with the setTimeout() in ngAfterViewInit(). We need to remove that and make it work with the Subscription.
  // Commenting as of now to resolve prod bug: STAF-3915
  // private _subscription:Subscription;

  constructor(
    private coreService: CoreService) {
    }

  trackByCaseId(index: number, item: any): string {
    return !item.caseDetails.oldCaseCode ? !item.caseDetails.pipelineId ? item.caseDetails.planningCardId : item.caseDetails.pipelineId : item.caseDetails.oldCaseCode;
  }

  trackById(index: number, item: ResourceStaffing): string {
    return item.trackById;
  }

  trackByResourcesId(index: number, item: Resource): string {
    return item.employeeCode;
  }

  // ------------------------Life Cycle Events--------------------------------------------//
  ngOnInit() {

    this.itemSizeForVirtualScroll = this.coreService.appSettings.itemSizeForVirtualScroll;
  }

  ngAfterViewInit() {

    setTimeout(() => {
      this.ganttBodyLoadedEmitter.emit();
    }, 5000);
  }

  ngOnChanges(changes: SimpleChanges) {

    if (changes.selectedEmployeeCaseGroupingOption && this.selectedEmployeeCaseGroupingOption) {
      if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {
        this.objGanttCollapsedRows = {
          isAllCollapsed: true,
          exceptionRowIndexes: []
        };
      } else {
        this.objGanttCollapsedRows = {
          isAllCollapsed: false,
          exceptionRowIndexes: []
        };
      }

      this.isTopbarCollapsed = this.objGanttCollapsedRows.isAllCollapsed;
    }
  }

  ngOnDestroy() {
    // this._subscription.unsubscribe();
  }

  // Export functionality: future requirements, should not be removed
  // ganttBodyLoadedEmitterHandler() {
  //   this.ganttBodyLoadedEmitter.emit();
  // }

  updateResourceAssignmentToProjectHandler(resourceAllocation) {
    this.updateResourceAssignmentToProject.emit(resourceAllocation);
  }

  upsertResourceAllocationsToProjectHandler(resourceAllocations) {
    this.upsertResourceAllocationsToProject.emit(resourceAllocations);
  }
  upsertPlaceholderAllocationsToProjectHandler(resourceAllocations) {
    this.upsertPlaceholderAllocationsToProject.emit(resourceAllocations);
  }

  updateResourceCommitmentHandler(resourceCommitment) {
    this.updateResourceCommitment.emit(resourceCommitment);
  }

  openQuickAddFormHandler(event) {
    this.openQuickAddForm.emit(event);
  }
  openResourceDetailsDialogHandler(event) {
    this.openResourceDetailsDialog.emit(event);
  }

  scrollEvent = (event: any): void => {
    document.dispatchEvent(new Event('click'));
  }

  openSplitAllocationPopupHandler(event) {
    this.openSplitAllocationPopup.emit(event);
  }

  openOverlappedTeamsPopupHandler(event) {
    this.openOverlappedTeamsForm.emit(event);
  }


  openCaseDetailsDialogHandler(event) {
    this.openCaseDetailsDialog.emit(event);
  }


  upsertResourceViewNoteHandler(event) {
    this.upsertResourceViewNote.emit(event);
  }

  deleteResourceViewNotesHandler(event) {
    this.deleteResourceViewNotes.emit(event);
  }

  upsertResourceRecentCDHandler(event) {
    this.upsertResourceRecentCD.emit(event);
  }
  upsertResourceCommercialModelHandler(event) {
    this.upsertResourceCommercialModel.emit(event);
  }
  deleteResourceCommercialModelHandler(event) {
    this.deleteResourceCommercialModel.emit(event);
  }

  deleteResourceRecentCDHandler(event) {
    this.deleteResourceRecentCD.emit(event);
  }
  selectedResourceViewTabHandler(event) {
    this.selectedResourceViewTab.emit(event);
  }

  expandCollapseSidebarHandler(isLeftSideBarCollapsed) {
    this.isLeftSideBarCollapsed = isLeftSideBarCollapsed;
  }

  expandCollapseTopbarHandler(isTopbarCollapsed) {
    this.objGanttCollapsedRows = { ...this.objGanttCollapsedRows, isAllCollapsed: isTopbarCollapsed };
    this.isTopbarCollapsed = isTopbarCollapsed;
  }

  collapseExpandIndividualCaseGroupHandler(isGroupCollapsed, rowIndex) {
    let row = document.querySelector<HTMLElement>(
      `#case-group-row-${rowIndex}`
    );

    row.classList.toggle('collapsed');

    this.caseGroupRowComponents.toArray()
      .filter(x => x.rowIndex === rowIndex)
      .map(y => {
        y.isRowCollapsed = !isGroupCollapsed;
        y.ganttResourceComponent.isRowCollapsed = !isGroupCollapsed;
        y.ganttTaskomponent.isRowCollapsed = !isGroupCollapsed;
      });

  }
}
