import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { PositionGroup } from 'src/app/shared/interfaces/position-group.interface';
import { ServiceLine } from 'src/app/shared/interfaces/serviceLine.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';

@Component({
  selector: 'app-allocated-placeholder',
  templateUrl: './allocated-placeholder.component.html',
  styleUrls: ['./allocated-placeholder.component.scss']
})
export class AllocatedPlaceholderComponent implements OnInit {
  // ------------------------Input Events---------------------------------------
  @Input() skuResources: any;

  // ------------------------Output Events---------------------------------------
  // @Output() openAddTeamSkuForm = new EventEmitter();
  @Output() closePlaceholdersOverlayEmitter = new EventEmitter();

  // ------------------------Local Variables---------------------------------------
  serviceLines: ServiceLine[];
  commitmentTypes: CommitmentType[];
  positionGroups: PositionGroup[];

  constructor(private localStorageService: LocalStorageService) { }

  ngOnInit() {
    this.serviceLines = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLines);
    this.commitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes);
    this.positionGroups = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsGroups);
    this.setNamesByCodes();
  }

  setNamesByCodes() {
    this.skuResources.placeholderAllocations?.map(x => {
      x.serviceLineName = this.serviceLines.find(y => y.serviceLineCode === x.serviceLineCode).serviceLineName;
      x.commitmentTypeName = this.commitmentTypes.find(y => y.commitmentTypeCode && y.commitmentTypeCode === x.commitmentTypeCode)?.commitmentTypeName;
      x.positionGroupName = this.positionGroups.find(y => y.positionGroupCode && y.positionGroupCode === x.positionGroupCode)?.positionGroupName;

      return x;
    });
  }

  closePlaceholdersOverlay() {
    this.closePlaceholdersOverlayEmitter.emit();
  }

  // openAddTeamSkuFormHandler(employeeCode) {
  //   this.openAddTeamSkuForm.emit(employeeCode);
  // }
}
