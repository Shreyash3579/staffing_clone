import { Component, Input, OnInit, SimpleChanges, ChangeDetectorRef, Output, EventEmitter, ViewChild, Directive, ElementRef } from "@angular/core";
import { Observable, Subject, of, concat } from 'rxjs';
import { CaseIntakeLeadership } from "src/app/shared/interfaces/caseIntakeLeadership.interface";
import { CaseIntakeLeadershipGroup } from "src/app/shared/interfaces/caseIntakeLeadershipGroup";
import { Resource } from "src/app/shared/interfaces/resource.interface";
import { ValidationService } from "src/app/shared/validationService";
import { v4 as uuidv4 } from 'uuid';
import { ResourcesTypeahedComponent } from "src/app/standalone-components/resources-typeahed/resources-typeahed.component";
import { Office } from "src/app/shared/interfaces/office.interface";

@Component({
  selector: "app-leadership-role",
  templateUrl: "./leadership-role.component.html",
  styleUrls: ["./leadership-role.component.scss"]
})
export class LeadershipRoleComponent implements OnInit {
  @Input() leadershipGroup: CaseIntakeLeadershipGroup;
  @Output() upsertLeadershipEventEmitter = new EventEmitter<any>();
  @Output() deleteLeadershipEventEmitter = new EventEmitter<any>();
  
  selectedResource: Resource[];

  public errorList = [];
  public isAllocationPercentageInvalid: boolean;
  public isResourceInvalid: boolean;
  public showTypeAhead: boolean = true;
  public showAllocationInput: boolean = true;

  @ViewChild('searchBox') searchBox!: ResourcesTypeahedComponent;
  @ViewChild('allocationInput') allocationInput: ElementRef;
  constructor() {}

  get leadershipNames(): string {
    return this.leadershipGroup.leaderships
      .filter(leadership => leadership?.employeeName)
      .map(leadership => {
        const nameParts = leadership.employeeName.split(',');
        if (nameParts.length === 2) {
          const [lName, fName] = nameParts.map(part => part.trim());
          return `${fName} ${lName} (${leadership.officeAbbreviation})`;
        }
        return leadership.employeeName;
      })
      .join(', ');
  }

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges): void {

    if (changes.leadershipGroup && this.leadershipGroup.leaderships.some(r => r.employeeCode)) {

      this.showTypeAhead = false;
      this.selectedResource = this.leadershipGroup.leaderships.map(r => ({
        ...{} as Resource,
        employeeCode: r.employeeCode,
        fullName: r.employeeName,
        office: { officeAbbreviation: r.officeAbbreviation } as Office
      })) ?? [];
    }

    if (changes.leadershipGroup && this.leadershipGroup.leaderships.some(r => r.allocationPercentage)) {
      this.showAllocationInput = false;
    }
  }

  onSearchItemSelectHandler(selectedResource: Resource[]) {

    this.selectedResource = [...selectedResource];
    if (this.selectedResource.length > 0) {
      this.leadershipGroup.leaderships = this.selectedResource.map((res: any) => {
        const existing = this.leadershipGroup.leaderships?.find((l) => l.employeeCode === res.employeeCode);
        return {
          ...(existing || ({} as CaseIntakeLeadership)),
          caseRoleCode: this.leadershipGroup.caseRoleCode,
          caseRoleName: this.leadershipGroup.caseRoleName,
          employeeCode: res.employeeCode,
          employeeName: res.fullName,
          officeAbbreviation: res.office?.officeAbbreviation,
          lastUpdatedBy: null,
        };
      });
    }
    else {
      this.leadershipGroup.leaderships = [];
    }
  }

  onSearchBoxBlur(event: FocusEvent) {
      this.hideTypeaheadForSearch();
      this.validateField('resource');
      this.saveLeadershipRoleData();
  }

  onAllocationBlur(event: any): void {
    this.hideTypeaheadForAllocationPercentage()

    //convert to integer in case of decimal value
    this.leadershipGroup.leaderships.forEach(r => {
      if (r.allocationPercentage) {
        r.allocationPercentage = Math.floor(Number(r.allocationPercentage));
      }
    });

    this.validateField('expectedallocationPercentage');
    this.saveLeadershipRoleData();
  }

  // validateLeadershipRoleData(){
  //   this.errorList = [];
  //   this.validateField('expectedallocationPercentage');
  //   this.validateField('resource');
  // }

  saveLeadershipRoleData(){
    this.errorList = [];
    //this.validateLeadershipRoleData();
    if (!this.errorList.length) {

      this.leadershipGroup.leaderships = this.leadershipGroup.leaderships.map(r => ({
        ...r,
        id: r.id || uuidv4()
      }));

      const validRoles = this.leadershipGroup.leaderships.filter(r => !(r.employeeCode === null && r.allocationPercentage === null));
      if (validRoles.length === 0) {
        this.deleteLeadershipEventEmitter.emit(this.leadershipGroup.caseRoleCode);
        this.showTypeAhead = true;
      }
      else {
        this.upsertLeadershipEventEmitter.emit(validRoles);
      }
    }
  }

  validateField(fieldName) {
    switch (fieldName) {
      case 'expectedallocationPercentage': {
        // if (this.role.allocationPercentage === undefined || this.role.allocationPercentage == null) {
        //   this.isAllocationPercentageInvalid = true;
        //   this.addToErrorList('required');
        // }
        if (this.leadershipGroup.leaderships.some(r => r.allocationPercentage && !ValidationService.isAllocationValid(r.allocationPercentage))) {
          this.isAllocationPercentageInvalid = true;
          this.addToErrorList('numberInvalid');
        }
        else {
          this.isAllocationPercentageInvalid = false;
        }
        break;
      }
      case 'resource': {
        if (!this.selectedResource || this.selectedResource.length === 0) {
          this.isResourceInvalid = true;
          this.addToErrorList('atleastOneAllocationSelected');
        }
        else if (!this.leadershipGroup || !this.leadershipGroup.leaderships || this.leadershipGroup.leaderships.length === 0 || this.leadershipGroup.leaderships.some(r => !r.employeeCode)) {
          this.isResourceInvalid = true;
          this.addToErrorList('required');
        }  
        else {
          this.isResourceInvalid = false;
        }
        break;
      }
    }
  }

  addToErrorList(type) {
    switch (type) {
      case 'required': {
        if (this.errorList.indexOf(ValidationService.requiredMessage) === -1) {
          this.errorList.push(ValidationService.requiredMessage);
        }
        break;
      }
      case 'numberInvalid': {
        if (this.errorList.indexOf(ValidationService.numberInvalidMessage) === -1) {
          this.errorList.push(ValidationService.numberInvalidMessage);
        }
        break;
      }
      case 'atleastOneAllocationSelected': {
        if (this.errorList.indexOf(ValidationService.invalidAdminUserMsg) === -1) {
          this.errorList.push(ValidationService.invalidAdminUserMsg);
        }
        break;
      }
    }
  }


  editLedershipEmployeeName(event) {
    this.showTypeAhead = true;
    setTimeout(() => {
      if (this.searchBox) {
        this.searchBox.focus();
        }
    }, 100);
  }

  editAllocationPercentage(event) {
    this.showAllocationInput = true;
    setTimeout(() => {
      this.allocationInput.nativeElement.focus();
    });
  }

  hideTypeaheadForSearch() {
    this.showTypeAhead = false;
  }

  hideTypeaheadForAllocationPercentage() {
    this.showAllocationInput = false;
  } 
}
