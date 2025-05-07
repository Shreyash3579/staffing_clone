import { Component, EventEmitter, Input, OnInit, Output, QueryList, SimpleChanges } from "@angular/core";
import { FormGroup } from "@angular/forms";
import { CaseIntakeDetail } from "src/app/shared/interfaces/caseIntakeDetail.interface";
import { CaseIntakeExpertise } from "src/app/shared/interfaces/caseIntakeExpertise.interface";
import { CaseIntakeRoleDetails } from "src/app/shared/interfaces/caseIntakeRoleDetails.interface";
import { CaseIntakeWorkstreamDetails } from "src/app/shared/interfaces/caseIntakeWorkstreamDetails.interface";
import { Language } from "src/app/shared/interfaces/language";
import { PositionGroup } from "src/app/shared/interfaces/position-group.interface";
import { ServiceLine } from "src/app/shared/interfaces/serviceLine.interface";
import { v4 as uuidv4 } from 'uuid';

@Component({
  selector: 'app-workstream',
  templateUrl: './workstream.component.html',
  styleUrls: ['./workstream.component.scss']
})
export class WorkstreamComponent implements OnInit {
  @Input() rolesYetToBeAssigned: CaseIntakeRoleDetails[];
  @Input() workstreamDetails: CaseIntakeWorkstreamDetails[];
  @Input() currentState: {roles: boolean,
    workstreams: boolean,
    details: boolean};
  @Input() combinedExpertiseList: CaseIntakeExpertise[];
  @Input() languages: Language[];
  @Input() positionGroups: PositionGroup[];
  @Input() serviceLine: ServiceLine[];
  @Input() opportunityId: string = null;
  @Input() oldCaseCode: string = null;
  @Input() planningCardId: string = null;
  @Input() caseIntakeDetails: CaseIntakeDetail;


  @Output() roleDetailsChangeEmitter = new EventEmitter();
  @Output() deleteWorkstreamEmitter = new EventEmitter();
  @Output() updateWorkstreamEmitter = new EventEmitter();
  @Output() changeLeadEmitter = new EventEmitter();
  @Output() updateRoleEmitter = new EventEmitter();
  @Output() deleteRoleEmitter = new EventEmitter();
  
  workstreamRoleDetails: CaseIntakeRoleDetails[];
  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges) {
  }

  trackByWorkstream(index: number, workstream: { id: string }): string {
    return workstream.id; // Ensure workstream has a unique 'id'
  }
  
  trackByRole(index: number, role: { id: string }): string {
    return role.id; // Ensure role has a unique 'id'
  }

  roleDetailsChangeHandler(event) {
    this.roleDetailsChangeEmitter.emit(event);
  }

  removeWorkstream(workstream) {
    this.workstreamDetails = this.workstreamDetails.filter(ws => ws.id !== workstream.id);
    this.deleteWorkstreamEmitter.emit(workstream);
  }


  onWorkstreamNameChange(event, workstream)
  {
    workstream.name = event.target.value;
    this.updateWorkstreamEmitter.emit(workstream);
  }

  changeLeadHandler(event) {
    this.changeLeadEmitter.emit(event);
  }

  deleteRoleFromWorkstream(event) {
    this.deleteRoleEmitter.emit(event);
  }

  handleKeydown(event: KeyboardEvent): void {
    if (event.key === ' ') {
      event.stopPropagation(); // Prevent space from toggling the panel
    }
  }

  
  adjustWidth(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    inputElement.style.width = ((inputElement.value.length + 1) * 7) + 'px';
  }


  addNewRole(index, role = "New Role", selectedPositionGroup?: { positionGroupCode: string; positionGroupName: string }) {
    const isFirstRole = this.workstreamDetails[index]?.roles?.length === 0; // Check if first role

    const newlyAddedRole : CaseIntakeRoleDetails = {
      id: uuidv4(),
      name: "New Role",
      serviceLineCode: null,
      positionCode: null,
      officeCodes: this.caseIntakeDetails.officeCodes ?? "",
      officeNames: this.caseIntakeDetails.officeNames ?? "",
      selectedLocation: [],
      selectedLanguages: [],
      selectedExpertise: [],
      mustHaveExpertiseCodes: "",
      niceToHaveExpertiseCodes: this.caseIntakeDetails.expertiseRequirement ?? "",
      mustHaveLanguageCodes: "",
      niceToHaveLanguageCodes: this.caseIntakeDetails.languages ?? "",
      selectedPositionGroup: null,
      selectedServiceLine: null,
      expertiseRequirementCodes: this.caseIntakeDetails.expertiseRequirement ?? "",
      isLead: isFirstRole,
      isAssignedInWorkstream: true,
      languageCodes: this.caseIntakeDetails.languages?? "",
      clientEngagementModel: this.caseIntakeDetails.clientEngagementModel ?? "",
      clientEngagementModelCodes: this.caseIntakeDetails.clientEngagementModelCodes ?? "",
      roleDescription: "",
      oldCaseCode: this.oldCaseCode,
      opportunityId: this.opportunityId,
      planningCardId: this.planningCardId,
      lastUpdated: new Date(),
      lastUpdatedBy: "",
      lastUpdatedByName: "",
      workstreamId: this.workstreamDetails[index].id

    };
    this.roleDetailsChangeEmitter.emit(newlyAddedRole);
  }


  onRoleChangeHandler(role: CaseIntakeRoleDetails) {
    this.updateRoleEmitter.emit(role);
  }

  
}