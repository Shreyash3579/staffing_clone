import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
} from "@angular/core";
import { ServiceLine } from "src/app/shared/interfaces/serviceLine.interface";
import { CaseIntakeRoleDetails } from "src/app/shared/interfaces/caseIntakeRoleDetails.interface";
import { CaseIntakeWorkstreamDetails } from "src/app/shared/interfaces/caseIntakeWorkstreamDetails.interface";
import { PracticeArea } from "src/app/shared/interfaces/practiceArea.interface";
import { Language } from "src/app/shared/interfaces/language";
import { PositionGroup } from "src/app/shared/interfaces/position-group.interface";
import { Store } from "@ngrx/store";
import * as fromStaffingIntake from "./../State/staffing-intake.reducer";
import * as StaffingIntakeActions from "./../State/staffing-intake.actions";
import { CaseIntakeDetail } from "src/app/shared/interfaces/caseIntakeDetail.interface";
import { Subscription, filter } from "rxjs";
import { CaseIntakeExpertise } from "src/app/shared/interfaces/caseIntakeExpertise.interface";
import { v4 as uuidv4 } from 'uuid';
import { CaseIntakeBasicDetails } from "src/app/shared/interfaces/caseIntakeBasicDetails.interface";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { CortexSkuMapping } from "src/app/shared/interfaces/cortex-sku-mapping.interface";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { NotificationService } from "src/app/shared/notification.service";
import { ValidationService } from "src/app/shared/validationService";
import { TeamSizeOperation } from "src/app/shared/constants/enumMaster";

@Component({
  selector: "app-staff-details-form",
  templateUrl: "./staff-details-form.component.html",
  styleUrls: ["./staff-details-form.component.scss"]
})
export class StaffDetailsFormComponent implements OnInit {
  @Input() expertises: PracticeArea[];
  @Input() combinedExpertiseList: CaseIntakeExpertise[];
  @Input() languages: Language[];
  @Input() positionGroups: PositionGroup[];
  @Input() serviceLine: ServiceLine[];
  @Input() roleDetails: CaseIntakeRoleDetails[] = [];
  @Input() workstreamDetails: CaseIntakeWorkstreamDetails[];
  @Input() opportunityId: string = null;
  @Input() oldCaseCode: string = null;
  @Input() planningCardId: string = null;
  @Input() caseIntakeDetails : CaseIntakeDetail;
  @Output() caseIntakeDetailsChangeForNotesEmitter = new EventEmitter<CaseIntakeDetail>();

  selectedReadyToStaffNotes: string;
  public cortexSkuMapping: CortexSkuMapping[];
  public selectedCortexSkuMapping: CortexSkuMapping;
  public workstreamCreated = false;
  public workstreamTeamsize = '';

  
  public storeSub: Subscription = new Subscription();

  
  constructor(private store: Store<fromStaffingIntake.State>,
    private localStorageService: LocalStorageService,
    private notificationService: NotificationService  
  ) {
  }

  ngOnInit(): void {
    this.initializeCaseIntakeDetail();
    this.getCortexSkuMapping();
  }

  ngOnChanges(changes) {

    if ((changes.roleDetails && this.roleDetails) || (this.workstreamDetails && changes.workstreamDetails)) {
    this.createUpdatedWorkstreamObject(this.workstreamDetails, this.roleDetails);
    }

    if (changes.roleDetails)
    {
      this.updateTeamSizeFromRoles();
    }
    
    if(changes.caseIntakeDetails && this.caseIntakeDetails)
    {
      this.selectedReadyToStaffNotes = this.caseIntakeDetails?.readyToStaffNotes; 
      
    }

  }

  getCortexSkuMapping() {
    this.cortexSkuMapping = this.localStorageService.get(ConstantsMaster.localStorageKeys.cortexSkuMappings);    
  }

  private createUpdatedWorkstreamObject(workstreamDetails: CaseIntakeWorkstreamDetails[],roleDetails: CaseIntakeRoleDetails[]) {
    const combinedObject: CaseIntakeWorkstreamDetails[] = workstreamDetails.map(workstream => {
      // Assign roles based on workstreamId
      const assignedRoles = roleDetails
        .filter(role => role.workstreamId === workstream.id)
        .map(role => ({ ...role, isAssignedInWorkstream: true })); // Ensure immutability
  
      return {
        id: workstream.id,
        name: workstream.name,
        skuSize: workstream.skuSize,
        oldCaseCode: workstream.oldCaseCode,
        opportunityId: workstream.opportunityId,
        planningCardId: workstream.planningCardId,
        lastUpdated: workstream.lastUpdated,
        lastUpdatedBy: workstream.lastUpdatedBy,
        lastUpdatedByName: workstream.LastUpdatedByName,
        roles: assignedRoles // Inject roles dynamically
      };
    });
    
    this.workstreamDetails = combinedObject;

  }
  

  private initializeCaseIntakeDetail() {
    if (!this.caseIntakeDetails) {
      this.caseIntakeDetails = {} as CaseIntakeDetail;
    }
    this.caseIntakeDetails.oldCaseCode = this.oldCaseCode ?? null;
    this.caseIntakeDetails.opportunityId = this.opportunityId ?? null;
    this.caseIntakeDetails.planningCardId = this.planningCardId ?? null;
    this.caseIntakeDetails.readyToStaffNotes = this.selectedReadyToStaffNotes ?? "";
    
  }

  onReadyToStaffNotesModelChange(event) {
    this.selectedReadyToStaffNotes = event.target.value;
    this.caseIntakeDetails.readyToStaffNotes = event.target.value;
    this.emitCaseIntakeDetails();
  }
  
  emitCaseIntakeDetails() {
    this.caseIntakeDetailsChangeForNotesEmitter.emit(this.caseIntakeDetails);
  }

  roleDetailsChangeHandler(role) {
    
    const existingRole = this.roleDetails.find(r => r.id === role.id);
    const operationType = existingRole ? TeamSizeOperation.Update : TeamSizeOperation.Add;

    const { isValid, message } = this.validateTeamSizeChange(role, operationType);
    if (!isValid) {
      this.notificationService.showWarning(message);
      return;
    }


    this.store.dispatch(
      new StaffingIntakeActions.UpsertRoleDetails([role]));
  
  }

  deleteWorkstreamHandler(workstream) {
    
      const deleteWorkstreamDetail: CaseIntakeBasicDetails = {
        id: workstream.id,
        caseRoleCode: null,
        oldCaseCode: this.oldCaseCode ?? null,
        opportunityId: this.opportunityId ?? null,
        planningCardId: this.planningCardId ?? null,
        lastUpdatedBy:null
      };
    
      this.store.dispatch(new StaffingIntakeActions.DeleteWorstreamsById(deleteWorkstreamDetail));
   
  }

  updateWorkstreamHandler(workstream) {
    const workstreamArray = [workstream];  

    this.store.dispatch(
        new StaffingIntakeActions.UpsertRolesAndWorkstreamDetails({
            roleDetails: [], 
            workstreamDetails: workstreamArray,  
            oldCaseCode: this.oldCaseCode,
            opportunityId: this.opportunityId,
            planningCardId: this.planningCardId
        })
    );
}

addWorkstream() {
  let workstream = [
      {
          id: uuidv4(),
          name: "Workstream name",
          oldCaseCode: this.oldCaseCode,
          opportunityId: this.opportunityId,
          planningCardId: this.planningCardId,
      }
  ];

  const newlyAddedRole = {} as CaseIntakeRoleDetails;

  newlyAddedRole.id = uuidv4();
  newlyAddedRole.name = "New Role";
  newlyAddedRole.workstreamId = workstream[0].id;
  newlyAddedRole.isLead = true;
  newlyAddedRole.isAssignedInWorkstream = true;
  newlyAddedRole.lastUpdated = new Date();
  newlyAddedRole.oldCaseCode = this.oldCaseCode;
  newlyAddedRole.opportunityId = this.opportunityId;
  newlyAddedRole.planningCardId = this.planningCardId;
  newlyAddedRole.officeCodes = this.caseIntakeDetails?.officeCodes ?? "";
  newlyAddedRole.officeNames = this.caseIntakeDetails?.officeNames ?? "";
  newlyAddedRole.expertiseRequirementCodes = this.caseIntakeDetails?.expertiseRequirement ?? "";
  newlyAddedRole.niceToHaveExpertiseCodes = this.caseIntakeDetails?.expertiseRequirement ?? "";
  newlyAddedRole.languageCodes = this.caseIntakeDetails?.languages ?? "";
  newlyAddedRole.niceToHaveLanguageCodes = this.caseIntakeDetails?.languages ?? "";
  newlyAddedRole.clientEngagementModel = this.caseIntakeDetails?.clientEngagementModel ?? "";
  newlyAddedRole.clientEngagementModelCodes = this.caseIntakeDetails?.clientEngagementModelCodes ?? "";
  newlyAddedRole.selectedLocation = [];
  newlyAddedRole.selectedLanguages = [];
  newlyAddedRole.selectedExpertise = [];
  newlyAddedRole.roleDescription = "";
  newlyAddedRole.serviceLineCode = null;
  newlyAddedRole.positionCode = null;
  newlyAddedRole.selectedPositionGroup = null;
  newlyAddedRole.selectedServiceLine = null;
  newlyAddedRole.lastUpdatedBy = "";
  newlyAddedRole.lastUpdatedByName = "";

  this.store.dispatch(
      new StaffingIntakeActions.UpsertRolesAndWorkstreamDetails({
          roleDetails: [newlyAddedRole], 
          workstreamDetails: workstream,
          oldCaseCode: this.oldCaseCode,
          opportunityId: this.opportunityId,
          planningCardId: this.planningCardId
      })
  );
}


createCortexTeamWorkstream() {
  const selectedMapping = this.selectedCortexSkuMapping?.mappedStaffingSKU;
  const parsedMapping = selectedMapping ? JSON.parse(selectedMapping) : null;

  if(parsedMapping === null) {
    this.notificationService.showWarning("Please select a Cortex SKU mapping.");
    return;
  }

  const workstream = [
    {
      id: uuidv4(),
      name: this.selectedCortexSkuMapping.cortexSKU,
      skuSize: this.selectedCortexSkuMapping.cortexSKU,
      oldCaseCode: this.oldCaseCode,
      opportunityId: this.opportunityId,
      planningCardId: this.planningCardId,
    }
  ];

  const newlyAddedRoles = [];

  Object.entries(parsedMapping).forEach(([levelGrade, count]) => {
    for (let i = 0; i < Number(count); i++) {

      const positionGroupCode = ConstantsMaster.levelGradetoPositionGroupMapping[levelGrade] || null;

      const role = {} as CaseIntakeRoleDetails;

      role.id = uuidv4();
      role.name = `New Role ${levelGrade}`;
      role.serviceLineCode = "SL0001";
      role.positionCode = positionGroupCode;
      role.oldCaseCode = this.oldCaseCode;
      role.opportunityId = this.opportunityId;
      role.planningCardId = this.planningCardId;
      role.workstreamId = workstream[0].id;
      role.officeCodes = this.caseIntakeDetails?.officeCodes ?? "";
      role.officeNames = this.caseIntakeDetails?.officeNames ?? "";
      role.mustHaveExpertiseCodes = "";
      role.niceToHaveExpertiseCodes = this.caseIntakeDetails?.expertiseRequirement ?? "";
      role.mustHaveLanguageCodes = "";
      role.niceToHaveLanguageCodes = this.caseIntakeDetails?.languages ?? "";
      role.expertiseRequirementCodes = this.caseIntakeDetails?.expertiseRequirement ?? "";
      role.languageCodes = this.caseIntakeDetails?.languages ?? "";
      role.clientEngagementModel = this.caseIntakeDetails?.clientEngagementModel ?? "";
      role.clientEngagementModelCodes = this.caseIntakeDetails?.clientEngagementModelCodes ?? "";
      role.isLead = false;
      role.isAssignedInWorkstream = true;
      role.roleDescription = "";
      role.lastUpdated = new Date();
      role.lastUpdatedBy = "";
      role.lastUpdatedByName = "";

      newlyAddedRoles.push(role);

    }
  });

  // Set first role as lead
  if (newlyAddedRoles.length > 0) {
    newlyAddedRoles[0].isLead = true;
  }

  this.store.dispatch(
    new StaffingIntakeActions.UpsertRolesAndWorkstreamDetails({
      roleDetails: newlyAddedRoles,
      workstreamDetails: workstream,
      oldCaseCode: this.oldCaseCode,
      opportunityId: this.opportunityId,
      planningCardId: this.planningCardId
    })
  );

  this.workstreamCreated = true;
}




  changeLeadHandler(event) {
    const role = event.role;
    //iterate in the roleDetails and if any role has the same workstreamId then make its islead false and update which new idlead
    this.roleDetails.forEach(r => {
      if(r.workstreamId === role.workstreamId){
        r.isLead = false;
      }
    });
    //find the role in the roleDetails and update its islead
    this.roleDetails.find(x => x.id == role.id).isLead = role.isLead;      
    this.store.dispatch(
      new StaffingIntakeActions.UpsertRoleDetails(this.roleDetails));
  
    
  }


  updateRoleHandler(role) { 

    const existingRole = this.roleDetails.find(r => r.id === role.id);
    const operationType = existingRole ? TeamSizeOperation.Update : TeamSizeOperation.Add;

    const { isValid, message } = this.validateTeamSizeChange(role, operationType);
    
    if (!isValid) {
      this.notificationService.showWarning(message);
      return;
    }

    this.store.dispatch(
      new StaffingIntakeActions.UpsertRoleDetails([role]));
  }


  deleteRoleFromWorkstream(role) {

    const { isValid, message } = this.validateTeamSizeChange(role, TeamSizeOperation.Delete);
    
    if (!isValid) {
      this.notificationService.showWarning(message);
      return;
    }

    const deleteRoleDetail: CaseIntakeBasicDetails = {
      id: role.id,
      caseRoleCode: null,
      oldCaseCode: this.oldCaseCode ?? null,
      opportunityId: this.opportunityId ?? null,
      planningCardId: this.planningCardId ?? null,
      lastUpdatedBy:null
    };

    this.store.dispatch(
      new StaffingIntakeActions.DeleteRolesById(deleteRoleDetail)
    );
    
  }

  private updateTeamSizeFromRoles(): void {

    const workstreamsToUpdate: CaseIntakeWorkstreamDetails[] = [];

    for (const ws of this.workstreamDetails) {
      if (!ws.skuSize) continue; 

      const roleCount = ws.roles?.length ?? 0;
      const calculatedSkuSize = ConstantsMaster.roleCountToSKUSizeMapping[roleCount] ?? `${roleCount}+`;

      if (ws.skuSize !== calculatedSkuSize) {
        workstreamsToUpdate.push({
          ...ws,
          skuSize: calculatedSkuSize
        });
      }
    }


  
    if (workstreamsToUpdate.length > 0) {
      this.store.dispatch(
        new StaffingIntakeActions.UpsertRolesAndWorkstreamDetails({
          roleDetails: [],
          workstreamDetails: workstreamsToUpdate,
          oldCaseCode: this.oldCaseCode,
          opportunityId: this.opportunityId,
          planningCardId: this.planningCardId
        })
      );
    }
  
    this.workstreamCreated = this.workstreamDetails.some(x => !!x.skuSize);
    this.workstreamTeamsize = this.workstreamDetails.find(x => !!x.skuSize)?.skuSize ?? '';
  }

  private validateTeamSizeChange(role: CaseIntakeRoleDetails, operation: TeamSizeOperation): { isValid: boolean; message?: string } {
    
    const workstream = this.workstreamDetails.find(ws => ws.id === role.workstreamId);
  
    if (!workstream || !workstream.skuSize) {
      return { isValid: true };
    }
  
    const rolesInSameWorkstream = this.roleDetails.filter(r => r.workstreamId === role.workstreamId);
    let newCount = rolesInSameWorkstream.length;
  
    if (operation === TeamSizeOperation.Add) newCount += 1;
    else if (operation === TeamSizeOperation.Delete) newCount -= 1;
  
    if (newCount < 3 || newCount > 7) {
      return {
        isValid: false,
        message: ValidationService.skuSizeValidationMessage
      };
    }
  
    return { isValid: true };
  }
  
  
  
  
  
  
}
