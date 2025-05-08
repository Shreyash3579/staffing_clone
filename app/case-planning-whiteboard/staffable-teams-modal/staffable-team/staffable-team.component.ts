import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ValidationService } from 'src/app/shared/validationService';

@Component({
  selector: 'app-staffable-team',
  templateUrl: './staffable-team.component.html',
  styleUrls: ['./staffable-team.component.scss']
})
export class StaffableTeamComponent implements OnInit {
  @Input() officeHierarchy: any;
  @Input() isGcTeamCountVisible: boolean;
  @Input() isPegTeamCountVisible: boolean;
  @Input() padding: number;

  @Output() countChange = new EventEmitter();

  constructor() { }

  ngOnInit(): void {
  }

  onCountChange(value, office, type) {
    if(this.officeHierarchy.staffableTeamChildren.length){
      //sum all children gccount properties
      // this.officeHierarchy.staffableTeamChildren.forEach(child => {
      //   value += child.staffableTeamGcCount;
      // }

      this.officeHierarchy.gcTeamCount = this.officeHierarchy.staffableTeamChildren.reduce(
        (accumulator, staffableTeamChild) => accumulator + staffableTeamChild.gcTeamCount
        ,0
      );

      this.officeHierarchy.pegTeamCount = this.officeHierarchy.staffableTeamChildren.reduce(
        (accumulator, staffableTeamChild) => accumulator + staffableTeamChild.pegTeamCount
        ,0
      );

    }

    this.countChange.emit({value, office, type});
  }

  validateValue(value, oldValueObj, type) {
    let isValid = false;
    if (!this.isValueSame(value, oldValueObj, type) && this.validateNegativeValues(value) && this.validateDecimalValue(value))
      isValid = true;
    return isValid;
  }

  isValueSame(newValue, oldValueObj, type) {
    return type === 'gc' ? newValue === oldValueObj.gcTeamCount : newValue === oldValueObj.pegTeamCount;
  }

  validateNegativeValues(value) {
    return value >= 0;
  }

  validateDecimalValue(value) {
    return ValidationService.isInteger(value);
  }

}
