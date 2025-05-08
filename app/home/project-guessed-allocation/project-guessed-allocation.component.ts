import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { Office } from 'src/app/shared/interfaces/office.interface';
import { PDGrade } from 'src/app/shared/interfaces/pdGrade.interface';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { ServiceLine } from 'src/app/shared/interfaces/serviceLine.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { PositionGroup } from 'src/app/shared/interfaces/position-group.interface';

@Component({
  selector: 'app-project-guessed-allocation',
  templateUrl: './project-guessed-allocation.component.html',
  styleUrls: ['./project-guessed-allocation.component.scss']
})
export class ProjectGuessedAllocationComponent implements OnInit {

  @Input() placeholderAllocation: PlaceholderAllocation;
  @Output() upsertPlaceholderAllocation = new EventEmitter();
  @Output() removeGuessedPlaceholderEmitter = new EventEmitter();
  @Output() confirmGuessedPlaceholderEmitter = new EventEmitter();
  serviceLines: ServiceLine[];
  positionGroups: PositionGroup[];
  ringfences: CommitmentType[];
  offices: Office[];
  levelGrades: PDGrade[];
  selectedServiceLine: string;
  selectedRingfence: string;
  selectedOffice: number;
  selectedLevelGrade: string;
  selectedPositionGroup: string;
  showConfirmIcon = false;

  constructor(private localStorageService: LocalStorageService) { }

  ngOnInit(): void {
    this.serviceLines = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLines);
    this.serviceLines = this.serviceLines.filter(x => x.inActive === false);
    this.offices = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
    this.levelGrades = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGrades);
    this.positionGroups = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsGroups);
    this.ringfences = this.getRingfencesFromLocalStorage();
    this.selectedLevelGrade = this.placeholderAllocation.currentLevelGrade;
    this.selectedOffice = this.placeholderAllocation.operatingOfficeCode;
    this.selectedServiceLine = this.placeholderAllocation.serviceLineCode;
    this.selectedRingfence = this.placeholderAllocation.commitmentTypeCode;
    this.selectedPositionGroup = this.placeholderAllocation.positionGroupCode;
    this.showConfirmIcon = this.toggleConfirmIconVisibility();
  }

  getRingfencesFromLocalStorage() {
    const ringfences: CommitmentType[] = this.localStorageService.get(ConstantsMaster.localStorageKeys.ringfences);
    const dummyRingfence: CommitmentType = { commitmentTypeCode: null, commitmentTypeName: null, precedence: 0 }

    return [dummyRingfence, ...ringfences]

  }

  private toggleConfirmIconVisibility() {
    if (this.placeholderAllocation.planningCardId) {
      return false;
    }
    return !!(this.placeholderAllocation.serviceLineCode
      && this.placeholderAllocation.operatingOfficeCode
      && this.placeholderAllocation.currentLevelGrade);
  }

  removePlaceHolderEmitter() {
    this.removeGuessedPlaceholderEmitter.emit();
  }

  confirmPlaceholder() {
    this.confirmGuessedPlaceholderEmitter.emit();
  }

  addGuessedPlaceholder(event, type) {
    switch (type) {
      case 'serviceLine':
        this.placeholderAllocation.serviceLineCode = event.serviceLineCode;
        this.placeholderAllocation.serviceLineName = event.serviceLineName;
        break;
      case 'office':
        this.placeholderAllocation.operatingOfficeAbbreviation = event.officeAbbreviation;
        this.placeholderAllocation.operatingOfficeCode = event.officeCode;
        break;
      case 'pdGrade':
        this.placeholderAllocation.currentLevelGrade = event.pdGradeName;
        break;
      case 'ringfence':
        this.placeholderAllocation.commitmentTypeCode = event.commitmentTypeCode;
        this.placeholderAllocation.commitmentTypeName = event.commitmentTypeName;
        break;
      case 'positionGroup':
        this.placeholderAllocation.positionGroupCode = event.positionGroupCode;
        break;
    }
    if (this.selectedServiceLine && this.selectedOffice && this.selectedLevelGrade) {
      this.showConfirmIcon = this.toggleConfirmIconVisibility();
      this.placeholderAllocation.allocation = 100;
      this.upsertPlaceholderAllocation.emit(this.placeholderAllocation);
    }
  }

}
