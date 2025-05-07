import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { Office } from 'src/app/shared/interfaces/office.interface';
import { PDGrade } from 'src/app/shared/interfaces/pdGrade.interface';
import { PositionGroup } from 'src/app/shared/interfaces/position-group.interface';
import { ServiceLine } from 'src/app/shared/interfaces/serviceLine.interface';

@Component({
  selector: 'app-placeholder-form',
  templateUrl: './placeholder-form.component.html',
  styleUrls: ['./placeholder-form.component.scss']
})
export class PlaceholderFormComponent implements OnInit {
  //------------------Input Variables- ----------------------//
  @Input() serviceLines: ServiceLine[];
  @Input() offices: Office[];
  @Input() pdGrades: PDGrade[];
  @Input() commitmentTypes: CommitmentType[];
  @Input() childFormGroupName: FormGroup;
  @Input() positionGroups: PositionGroup[];
  //------------------Output Variables- ----------------------//
  @Output() removePlaceholderAllocationFromProject = new EventEmitter<any>();
  //-----------------Local Variables --------------------------//

  constructor() {
  }

  ngOnInit(): void {
  }

  selectOnFocus(event){
    event.currentTarget?.select();
  }

  removePlaceholderAllocationFromProjectHandler(){
    this.removePlaceholderAllocationFromProject.emit(this.childFormGroupName);
  }

}
