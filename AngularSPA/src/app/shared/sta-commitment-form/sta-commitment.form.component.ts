// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, Output, EventEmitter, ViewEncapsulation, ViewChild } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';

// ----------------------- Component/Service References ----------------------------------//
import { DateService } from '../dateService';
import { ValidationService } from 'src/app/shared/validationService';
import { PopupDragService } from '../services/popupDrag.service';
import { SharedService } from '../shared.service';
import { OverlayService } from 'src/app/overlay/overlay.service';

// --------------------------Interfaces -----------------------------------------//
import { ResourceAllocation } from '../interfaces/resourceAllocation.interface';

// ----------------------- External Libraries References ----------------------------------//
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { BsModalRef } from 'ngx-bootstrap/modal';

// ----------------------- Constants/Enums ----------------------------------//
import { BS_DEFAULT_CONFIG } from '../constants/bsDatePickerConfig';
import { ShortAvailableCaseOppCommitmentOptions } from '../constants/enumMaster';
import { Subscription } from 'rxjs';
import { CaseOppCommitment } from '../interfaces/caseOppCommitment.interface';


@Component({
  selector: 'app-sta-commitment-form',
  templateUrl: './sta-commitment-form.component.html',
  styleUrls: ['./sta-commitment-form.component.scss'],
  providers: [PopupDragService],
  encapsulation: ViewEncapsulation.None
})
export class STACommitmentFormComponent implements OnInit {
  // --------------------------Local Variables---------------------------------//
  public errorList = [];
  public isDateInvalid: boolean;
 
  public isAllocationNotSelected: boolean;
 
  public selectedSTACommitmentOption = ShortAvailableCaseOppCommitmentOptions.AddShortTermAvailableCommitment;
  public STACommitmentDialogTitle = '';
  bsConfig: Partial<BsDatepickerConfig>;
  gridData = [];
  isSelectAll = false;
  isSTACommitmentCreated= false;
  shortTermAvailableCaseOppCommitmentEnum = ShortAvailableCaseOppCommitmentOptions;
  isAllocationGridDisabled = false;
  public allocationsGridData: ResourceAllocation[] = [];
  public allUpcomingAllocations : ResourceAllocation[] = [];
  public  staCommitmentData : CaseOppCommitment[] = [];  
  storeSub: Subscription = new Subscription();
  // -----------------------Variables affected from outside the component -----------------//
  public project: any;
  
  // --------------------------Ouput Events--------------------------------//
  @Output() insertCaseOppCommitments = new EventEmitter<any>();
  @Output() deleteCaseOppCommitments = new EventEmitter<any>();


  constructor(public bsModalRef: BsModalRef,
    private sharedService: SharedService,
    private overlayService: OverlayService,
    private _popupDragService: PopupDragService,
 ) { }

  // --------------------------Life Cycle Event handlers---------------------------------//
  ngOnInit() {


    this.getProjectAllocations();

    if (this.project.isSTACommitmentCreated) {

      this.STACommitmentDialogTitle = 'View Short Term Available Commitment';
      this.isSTACommitmentCreated = true;

     this.selectedSTACommitmentOption = ShortAvailableCaseOppCommitmentOptions.RevertShortTermAvailableCommitment;

    }
     else {
      this.STACommitmentDialogTitle = 'Add Short Term Available Commitment';
      this.isSTACommitmentCreated = false;
    }

    this.enableDisableAllocationGrid();
    this._popupDragService.dragEvents();
  }


  initializeDatePicker() {
    this.bsConfig = BS_DEFAULT_CONFIG;
  }

  getProjectAllocations() {
   
    
    let projectAllocations$: Observable<any>;
    const futureDate = DateService.convertDateInBainFormat(DateService.addDays(new Date(), +1));

    // Dynamically set which API to call based on available fields
    if (this.project.oldCaseCode) {
      projectAllocations$ = this.sharedService.getCaseAllocations(this.project.oldCaseCode, futureDate);
    } else if (this.project.pipelineId) {
      projectAllocations$ = this.overlayService.getOpportunityAllocations(this.project.pipelineId, futureDate);
    } else {
      projectAllocations$ = this.overlayService.getPlanningCardAllocations(this.project.id, futureDate);
    }

    let staCommitmentDetails$: Observable<any> = of(null);
  
    // If the STA commitment is created, fetch related data
    if (this.project.isSTACommitmentCreated) {
      staCommitmentDetails$ = this.sharedService.getProjectSTACommitmentDetails(this.project.oldCaseCode, this.project.pipelineId, this.project.id);
    }

    // Use forkJoin to fetch data concurrently
    forkJoin([projectAllocations$, staCommitmentDetails$]).subscribe(([allocations, staCommitmentData]) => {
      this.allUpcomingAllocations = allocations ? allocations.filter(x => !x.isPlaceholderAllocation) : [];
      this.staCommitmentData = staCommitmentData;

      if(this.staCommitmentData) {
          this.staCommitmentData = staCommitmentData.map(item => ({
            ...item,
            startDate: new Date(item.startDate + 'Z').toISOString() ,
            endDate:  new Date(item.endDate + 'Z').toISOString() 
          }));
     }
      // Apply additional filtering logic if needed
      this.allocationsGridData = this.getFilteredAllocationsForGrid();
      if(  !this.allocationsGridData.length) {
        this.addToErrorList('addSTACommitmentFornoUpcomingAllocations');
      }

      this.loadEmployeeGrid();
    });
}

  

  loadEmployeeGrid() {
    this.gridData = [];
  
    if (this.allocationsGridData) {
      this.enableDisableAllocationGrid();
  
      const mondayOfCurrentWeek = this.getMondayOfCurrentWeek();

      this.allocationsGridData.forEach((allocation, index) => {
        // Find matching commitment data based on scheduleId

        const allocationStartDate = new Date(allocation.startDate);
        const endDate = this.subtractDays(allocationStartDate, 1);
        const commitment = this.staCommitmentData?.find(data => data.scheduleId === allocation.id);
  
        // If no matching commitment, set default values
        const commitmentStartDate = commitment?.startDate || DateService.convertDateInBainFormat(mondayOfCurrentWeek);
        const commitmentEndDate = commitment?.endDate || DateService.convertDateInBainFormat(endDate);
  
        const row = {
          id: 'row-' + index,
          data: {
            ...allocation, // Spread allocation data
            commitmentStartDate,
            commitmentEndDate
          },
        checked: true //this.isAllocationGridDisabled
        //     ? true // Select All filtered rolled resources
        //     : Date.parse(allocation.endDate) === Date.parse(this.project.endDate) // Select if allocation end date is same as case end date
        };
  
        this.gridData.push(row);
      });
  
      this.checkUncheckSelectAll();
    }
  }

  private getMondayOfCurrentWeek(): Date {
    const today = new Date();
    const day = today.getDay();
    const diff = today.getDate() - (day === 0 ? 6 : day - 1); // Adjust if Sunday
    return new Date(today.setDate(diff));
  }
  
  private subtractDays(date: Date, days: number): Date {
    const result = new Date(date);
    result.setDate(result.getDate() - days);
    return result;
  }

  // --------------------------Event handlers---------------------------------//


  OnSelectAllChanged(value) {
    this.isSelectAll = !this.isSelectAll;

    this.gridData.forEach(row => {
      row.checked = this.isSelectAll;
    });
  }

  OnSelectRowChanged(row) {
    row.checked = !row.checked;
    this.checkUncheckSelectAll();
  }

  checkUncheckSelectAll() {
    const isRowUnchecked = this.gridData.find(x => !x.checked);
    this.isSelectAll = isRowUnchecked ? false : true;
  }


  validateField(fieldName) {
    switch (fieldName) {
      case 'atleastOneAllocationSelected': {
        if (this.gridData.length === 0) {
          this.isAllocationNotSelected = true;
          this.addToErrorList('addSTACommitmentFornoUpcomingAllocations');
        }
        else if (!this.gridData.find(x => x.checked)) {
          this.isAllocationNotSelected = true;
          this.addToErrorList('atleastOneAllocationSelected');
        } else {
          this.isAllocationNotSelected = false;
        }
        break;
      }
    }
  }

  addToErrorList(type) {
    switch (type) {

      case 'atleastOneAllocationSelected': {
        if (this.errorList.indexOf(ValidationService.atleastOneAllocationSelected) === -1) {
          this.errorList.push(ValidationService.atleastOneAllocationSelected);
        }
        break;
      }
      case 'addSTACommitmentFornoUpcomingAllocations': {
        if (this.errorList.indexOf(ValidationService.addSTACommitmentFornoUpcomingAllocations) === -1) {
          this.errorList.push(ValidationService.addSTACommitmentFornoUpcomingAllocations);
        }
        break;
      }

    }
  }

  isDataValid() {
    this.errorList = [];1

    this.validateField('atleastOneAllocationSelected');

    if (this.isDateInvalid || this.errorList.length) {
      return false;
    } else {
      return true;
    }
  }


addShortTermAvailableCommitment() {

    switch (this.selectedSTACommitmentOption) {


        case ShortAvailableCaseOppCommitmentOptions.AddShortTermAvailableCommitment:
            {
            if (!this.isDataValid()) {
                return false;
            }
            this.addSTACommitment();
            break;
            }
        case ShortAvailableCaseOppCommitmentOptions.RevertShortTermAvailableCommitment:
            {
            if (!this.isDataValid()) {
                return false;
            }
            this.revertSTACommitment();
            break;
            }
        }

}

addSTACommitment() {
  const selectedCommitments: CaseOppCommitment[] = this.gridData
    .filter(row => row.checked)
    .map(row => {
      const data = row.data;
      return {
        scheduleId: data.id,
        employeeCode: data.employeeCode,
        commitmentTypeCode: 'ST', 
        commitmentTypeReasonCode:  null,
        startDate: data.commitmentStartDate,
        endDate: data.commitmentEndDate,
        allocation:  100,
        notes: data.notes || '',
        oldCaseCode: data.oldCaseCode || null,
        planningCardId: data.planningCardId || null,
        opportunityId: data.pipelineId || null,
        lastUpdatedBy: null
      } as CaseOppCommitment;
    });

  this.insertCaseOppCommitments.emit({commitments:selectedCommitments});
  this.closeForm();
}

  revertSTACommitment() {
    var comitmentIds = this.staCommitmentData.map(x => x.commitmentId ).join(',');

    this.deleteCaseOppCommitments.emit({commitmentIds:comitmentIds, oldCaseCode:this.project.oldCaseCode, opportunityId:this.project.pipelineId, planningCardId:this.project.id});
    this.closeForm();
  }


  enableDisableAllocationGrid() {
    if (this.isSTACommitmentCreated && this.selectedSTACommitmentOption === ShortAvailableCaseOppCommitmentOptions.RevertShortTermAvailableCommitment) {
      this.isAllocationGridDisabled = true;
    }
    else {
      this.isAllocationGridDisabled = false;
    }
  }

  getFilteredAllocationsForGrid() {
    const uniqueAllocationsMap = new Map<string, any>();
    const uniqueAllocations = this.allUpcomingAllocations?.filter(x => {
      if (!uniqueAllocationsMap.has(x.id)) {
        uniqueAllocationsMap.set(x.id, x); 
        return true;  
      }
      return false;  
    });
  
    if (this.isSTACommitmentCreated) {
      // Filter uniqueAllocations based on staCommitmentData
      return uniqueAllocations?.filter(x => 
        this.staCommitmentData?.some(data => data.scheduleId === x.id)
      );
    } else {
      return this.getEarliestUniqueAllocationsByEmployee(uniqueAllocations ?? []);
    }
  }


   getEarliestUniqueAllocationsByEmployee(allocations: any[]): any[] {
    const futureDateObj = DateService.addDays(new Date(), 1); 

    const filteredAllocations = allocations.filter(x => {
      const startDateObj = new Date(x.startDate);
      return x.investmentCode !== 4 && startDateObj >= new Date(futureDateObj);
    });
  
    //  Pick the earliest start date per employeeCode
    const earliestAllocationsMap = new Map<string, any>();
  
    filteredAllocations.forEach(allocation => {
      const existing = earliestAllocationsMap.get(allocation.employeeCode);
  
      if (!existing) {
        earliestAllocationsMap.set(allocation.employeeCode, allocation);
      } else {
        const existingStartDate = new Date(existing.startDate);
        const currentStartDate = new Date(allocation.startDate);
  
        if (currentStartDate < existingStartDate) {
          earliestAllocationsMap.set(allocation.employeeCode, allocation);
        }
      }
    });
  
    return Array.from(earliestAllocationsMap.values());
  }
  


  closeForm() {
    this.bsModalRef.hide();
  }



}
