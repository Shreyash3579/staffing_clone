import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { DateService } from 'src/app/shared/dateService';
import { ResourceAllocationService } from '../services/resourceAllocation.service';
import { PopupDragService } from '../services/popupDrag.service';
import { ConstantsMaster } from '../constants/constantsMaster';

@Component({
  selector: 'app-backfill-form',
  templateUrl: './backfill-form.component.html',
  styleUrls: ['./backfill-form.component.scss'],
  providers: [PopupDragService]
})
export class BackfillFormComponent implements OnInit {

  // --------------------------Input Variables To Form---------------------------------//
  public project: Project;
  public resourceAllocation: ResourceAllocation;
  public isPlaceholderAllocation = false;
  public showMoreThanYearWarning = false;
  public allocationDataBeforeSplitting;

  // --------------------------Local Variables---------------------------------//
  public projectName: string;
  public backfillResources: ResourceAllocation[];
  public selectedBackfillResource: ResourceAllocation;
  public isBackfill = false;


  @Output() upsertResourceAllocationsToProject = new EventEmitter();
  @Output() deletePlaceHoldersByIds = new EventEmitter();

  constructor(public bsModalRef: BsModalRef,
    private _resourceAllocationService: ResourceAllocationService,
    private _popupDragService: PopupDragService) { }


  // --------------------------Life Cycle Event handlers---------------------------------//

  ngOnInit() {
    this.projectName = this.project.type === 'Opportunity' ? this.project.opportunityName : this.project.caseName;

    this.loadResourcesDropDown();
    this._popupDragService.dragEvents();
  }

  loadResourcesDropDown() {
    const today = DateService.getToday();

    // resources whose end date is lesser than case end date need to be backfilled
    this.backfillResources = this.project.allocatedResources.filter(resource => {
      return (Date.parse(resource.endDate) >= today
        && Date.parse(resource.endDate) < Date.parse(this.project.endDate)
        // don't allow backfill for a backfill resource
        && resource.investmentCode !== ConstantsMaster.InvestmentCategory.Backfill.investmentCode);
    });

    // default selected - resource ending soonest on the case
    this.selectedBackfillResource = this.backfillResources.reduce((a, b) => Date.parse(a.endDate) < Date.parse(b.endDate) ? a : b);
  }


  // --------------------------Event handlers---------------------------------//

  addBackfill() {
    let dto = [];

    if (this.isBackfill) {
      // create 2 rows for backfill. One for 1 week of overlap and other for the remaining duration of case
      const backfillAllocation: ResourceAllocation = {
        oldCaseCode: this.project.oldCaseCode,
        caseName: this.project.caseName,
        clientName: this.project.clientName,
        pipelineId: this.project.pipelineId,
        opportunityName: this.project.opportunityName,
        employeeCode: this.resourceAllocation.employeeCode,
        employeeName: this.resourceAllocation.employeeName,
        operatingOfficeCode: this.resourceAllocation.operatingOfficeCode,
        operatingOfficeAbbreviation: this.resourceAllocation.operatingOfficeAbbreviation,
        currentLevelGrade: this.resourceAllocation.currentLevelGrade,
        serviceLineCode: this.resourceAllocation.serviceLineCode,
        serviceLineName: this.resourceAllocation.serviceLineName,
        investmentName: ConstantsMaster.InvestmentCategory.Backfill.investmentName,
        investmentCode: ConstantsMaster.InvestmentCategory.Backfill.investmentCode,
        caseRoleCode: this.resourceAllocation.caseRoleCode,
        startDate: DateService.addDays(this.selectedBackfillResource.endDate, -6),
        endDate: this.selectedBackfillResource.endDate,
        allocation: this.resourceAllocation.allocation,
        caseStartDate: this.project.oldCaseCode ? this.project.startDate : null,
        caseEndDate: this.project.oldCaseCode ? this.project.endDate : null,
        opportunityStartDate: !this.project.oldCaseCode ? this.project.startDate : null,
        opportunityEndDate: !this.project.oldCaseCode ? this.project.endDate : null,
        lastUpdatedBy: null,
        notes: this.resourceAllocation.notes
      };

      const regularAllocation: ResourceAllocation = {
        id: this.resourceAllocation.id,
        oldCaseCode: this.project.oldCaseCode,
        caseName: this.project.caseName,
        clientName: this.project.clientName,
        pipelineId: this.project.pipelineId,
        opportunityName: this.project.opportunityName,
        employeeCode: this.resourceAllocation.employeeCode,
        employeeName: this.resourceAllocation.employeeName,
        operatingOfficeCode: this.resourceAllocation.operatingOfficeCode,
        operatingOfficeAbbreviation: this.resourceAllocation.operatingOfficeAbbreviation,
        currentLevelGrade: this.resourceAllocation.currentLevelGrade,
        serviceLineCode: this.resourceAllocation.serviceLineCode,
        serviceLineName: this.resourceAllocation.serviceLineName,
        investmentCode: this.resourceAllocation.investmentCode,
        investmentName: this.resourceAllocation.investmentName,
        caseRoleCode: this.resourceAllocation.caseRoleCode,
        startDate: DateService.addDays(this.selectedBackfillResource.endDate, 1),
        endDate: DateService.convertDateInBainFormat(this.project.endDate),
        allocation: this.resourceAllocation.allocation,
        caseStartDate: this.project.oldCaseCode ? this.project.startDate : null,
        caseEndDate: this.project.oldCaseCode ? this.project.endDate : null,
        opportunityStartDate: !this.project.oldCaseCode ? this.project.startDate : null,
        opportunityEndDate: !this.project.oldCaseCode ? this.project.endDate : null,
        lastUpdatedBy: null,
        notes: this.resourceAllocation.notes
      };

      dto.push(backfillAllocation);
      dto.push(regularAllocation);

      this.allocationDataBeforeSplitting.forEach(x => {
          x.endDate = DateService.convertDateInBainFormat(this.project.endDate)
        });

    } else {
      // If not backfill then check for pre-post
      const projectStartDate = DateService.convertDateInBainFormat(this.project.startDate);
      const projectEndDate = DateService.convertDateInBainFormat(this.project.endDate);

      if (projectStartDate && projectEndDate) {

        dto = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(this.resourceAllocation);

      } else {

        dto.push(this.resourceAllocation);

      }

    }

    if (this.isPlaceholderAllocation) {
      this.deletePlaceHoldersByIds.emit({ 
          placeholderAllocation: [].concat(this.resourceAllocation),
          placeholderIds: this.resourceAllocation.id,
          notifyMessage: null });
    }

    this.upsertResourceAllocationsToProject.emit({
      resourceAllocation: dto,
      event: 'backfill',
      showMoreThanYearWarning: this.showMoreThanYearWarning,
      allocationDataBeforeSplitting: this.allocationDataBeforeSplitting
    });

    this.closeForm();
  }

  closeForm() {
    this.bsModalRef.hide();
  }
}
