import { Component, ElementRef, Input, OnInit, SimpleChanges } from "@angular/core";
import { FormArray, FormBuilder, FormGroup } from "@angular/forms";
import { debounceTime, filter, last } from "rxjs/operators";
import { LocalStorageService } from "../shared/local-storage.service";
import { ConstantsMaster } from "../shared/constants/constantsMaster";
import { from, Subscription, combineLatest } from "rxjs";
import * as fromStaffingIntake from "./State/staffing-intake.reducer";
import { select, Store } from "@ngrx/store";
import { CaseIntakeLeadership } from "../shared/interfaces/caseIntakeLeadership.interface";
import { CaseIntakeDetail } from "../shared/interfaces/caseIntakeDetail.interface";
import * as StaffingIntakeActions from "./State/staffing-intake.actions";
import { ActivatedRoute } from "@angular/router";
import { ProjectDetails } from "../shared/interfaces/projectDetails.interface";
import { PracticeArea } from "../shared/interfaces/practiceArea.interface";
import { CaseIntakeRoleDetails } from "../shared/interfaces/caseIntakeRoleDetails.interface";
import { CaseIntakeWorkstreamDetails } from "../shared/interfaces/caseIntakeWorkstreamDetails.interface";
import { CoreService } from "../core/core.service";
import { PlanningCard } from "../shared/interfaces/planningCard.interface";
import { CaseIntakeExpertise } from "../shared/interfaces/caseIntakeExpertise.interface";
import { StaffingPreferenceLookupOption } from "../shared/interfaces/staffingPreferenceLookupOption";
import { CaseIntakeBasicDetails } from "../shared/interfaces/caseIntakeBasicDetails.interface";
import { v4 as uuidv4 } from 'uuid'; 
import { StaffingPreferenceGroupEnum } from '../shared/constants/enumMaster';

@Component({
  selector: "app-staffing-intake-form",
  templateUrl: "./staffing-intake-form.component.html",
  styleUrls: ["./staffing-intake-form.component.scss"]
})
export class StaffingIntakeFormComponent implements OnInit {

  @Input() opportunityId : string = null;
  @Input() oldCaseCode : string = null;
  @Input() planningCardId : string = null;
  public combinedExpertiseList: CaseIntakeExpertise[] = [];
  public expertiseList: StaffingPreferenceLookupOption[];
  public newExpertiseList: CaseIntakeExpertise[] = [];
  public industryAndCapabilities: PracticeArea[];
  public capabilityPracticeAreas: PracticeArea[];
  public industryPracticeAreas: PracticeArea[];
  
  public leadershipDetails: CaseIntakeLeadership[] = [];
  public caseIntakeDetails: CaseIntakeDetail = {} as CaseIntakeDetail;
  public caseBasicDetails: any;
  public planningCardBasicDetails: PlanningCard;
  public roleDetails: CaseIntakeRoleDetails[] = [];
  public workstreamDetails: CaseIntakeWorkstreamDetails[] = [];
  public lastUpdated: Date;
  public lastUpdatedByName: string;

  public languages: any;
  public serviceLines: any;
  public positionGroups: any;
  public selectedCapabilityPracticeAreas : any;
  public selectedIndustryPracticeAreas : any;

  public isPlanningCardNotFound: boolean = false;
  
  
  public storeSub: Subscription = new Subscription();

  constructor(private fb: FormBuilder,
    private localStorageService: LocalStorageService,
    private store: Store<fromStaffingIntake.State>,
    private route: ActivatedRoute,
    private coreService: CoreService
    
  ) { }

  ngOnInit(): void {
    if(!this.opportunityId && !this.oldCaseCode && !this.planningCardId) {
      this.route.queryParams.subscribe(params => {
        this.opportunityId = params['opportunityId'];
        this.oldCaseCode = params['oldCaseCode'];
        this.planningCardId = params['planningCardId'];
      });
    }

    if(this.opportunityId || this.oldCaseCode || this.planningCardId) {
      this.getStaffingIntakeDetails();
      this.getLookupListFromLocalStorage();
      this.setStoreSubscription();
    }

  }

  ngOnChanges(changes : SimpleChanges) {
  
  }

  private getStaffingIntakeDetails(){
    this.getCaseInatkeExpertise();
    this.getLeadershipAndEarlyInputData(this.opportunityId, this.oldCaseCode, this.planningCardId);
    this.getPlanningCardBasicDetails(this.planningCardId);
    this.getCaseOppBasicDetails(this.opportunityId, this.oldCaseCode);
    this.getRoleAndWorkStreamDetails(this.opportunityId, this.oldCaseCode, this.planningCardId);
    this.getLastUpdatedInfo();
  }

  private getCaseInatkeExpertise(){
    this.store.dispatch(
      new StaffingIntakeActions.GetExpertiseRequirementList());
  }

  private getLastUpdatedInfo() {
    this.store.dispatch(
      new StaffingIntakeActions.LoadLastUpdatedByChanges({
        opportunityId: this.opportunityId ?? null,
        oldCaseCode: this.oldCaseCode ?? null,
        planningCardId: this.planningCardId ?? null
      }));
  }

  private getRoleAndWorkStreamDetails(opportunityId : string, oldCaseCode: string, planningCardId: string){
    
    const payload = {
      demandId : {
        opportunityId: opportunityId || null,
        oldCaseCode: oldCaseCode || null,
        planningCardId: planningCardId || null
      }
    };
    
    this.store.dispatch(
      new StaffingIntakeActions.LoadRoleAndWorkstreamDetails(
        payload));
  }

  private getCaseOppBasicDetails(opportunityId : string, oldCaseCode: string){
    if(oldCaseCode != null) {
      this.store.dispatch(
        new StaffingIntakeActions.LoadCaseBasicDetails(oldCaseCode ?? null));
    }
    else if(opportunityId != null){
      this.store.dispatch(
        new StaffingIntakeActions.LoadOpportunityDetails(opportunityId ?? null));
    }
      
  }

  private getPlanningCardBasicDetails(planningCardId : string){
    if(planningCardId != null)
    {
    this.store.dispatch(
      new StaffingIntakeActions.LoadPlanningCardDetails(planningCardId ?? null));
  
    }
  }

  private getLeadershipAndEarlyInputData(opportunityId : string, oldCaseCode: string, planningCardId : string){
    this.getFormDataForLeadership(opportunityId, oldCaseCode, planningCardId);
    this.getFormDataForCaseIntakeDetails(opportunityId, oldCaseCode, planningCardId);
  }

  private getFormDataForLeadership(opportunityId : string, oldCaseCode: string, planningCardId : string){
    this.store.dispatch(
      new StaffingIntakeActions.LoadLeadershipDetails({
        opportunityId: opportunityId ?? null,
        oldCaseCode: oldCaseCode ?? null,
        planningCardId: planningCardId ?? null
      }));  
  }

  private getFormDataForCaseIntakeDetails(opportunityId : string, oldCaseCode: string, planningCardId : string){
    this.store.dispatch(
      new StaffingIntakeActions.LoadCaseIntakeDetails({
        opportunityId: opportunityId ?? null,
        oldCaseCode: oldCaseCode ?? null,
        planningCardId: planningCardId ?? null
      }));
  }

  private setStoreSubscription(){
    this.clearState();
    this.getExpertiseList();
    this.getBasicCaseDetailsFromStore();
    this.getBasicOpportunityDetailsFromStore();
    this.getPlanningCardBasicDetailsFromStore();
    this.getLeadershipDetailsFromStore();
    this.getCaseIntakeDetailsFromStore();
    this.getRoleDetailsFromStore();
    this.getWorkStreamDetailsFromStore();
    this.getLastUpdatedInfoFromStore();
  }

  private clearState(){
    this.store.dispatch(new StaffingIntakeActions.ClearStaffingIntakeState());
  }

  private getBasicCaseDetailsFromStore(){
    this.storeSub.add(this.store.pipe(
      select(fromStaffingIntake.getCaseBasicDetails),
      filter(caseBasicDetails => caseBasicDetails != null)
    ).subscribe((caseBasicDetails: ProjectDetails) => {
      this.caseBasicDetails = caseBasicDetails;
      
    }));
  }

  private getBasicOpportunityDetailsFromStore(){
    this.storeSub.add(this.store.pipe(
      select(fromStaffingIntake.getOpportunityBasicDetails),
      filter(caseBasicDetails => caseBasicDetails != null)
    ).subscribe((caseBasicDetails: ProjectDetails) => {
      this.caseBasicDetails = caseBasicDetails;
    }));
  }

  private getPlanningCardBasicDetailsFromStore(){
    this.storeSub.add(this.store.pipe(
      select(fromStaffingIntake.getPlanningCardDetails),
      filter(planningCardDetails => planningCardDetails != null)
    ).subscribe((planningCardDetails: PlanningCard) => {
      this.planningCardBasicDetails = planningCardDetails;
    }));

    this.storeSub.add(
      this.store.pipe(
        select(fromStaffingIntake.getPlanningCardNotFound),
        filter(planningCardNotFound => planningCardNotFound === true)
      ).subscribe(() => {
        this.planningCardBasicDetails = null;
        this.isPlanningCardNotFound = true;
      })
    );
  }

  private getLeadershipDetailsFromStore(){
    this.leadershipDetails = [];
    this.storeSub.add(this.store.pipe(
      select(fromStaffingIntake.getLeadershipDetails),
        filter(leadershipDetails => leadershipDetails != null)
    ).subscribe((leadershipDetails: CaseIntakeLeadership[]) => {
      this.leadershipDetails = leadershipDetails;
    }));
  }

  private getCaseIntakeDetailsFromStore(){
    this.storeSub.add(this.store.pipe(
      select(fromStaffingIntake.getcaseIntakeDetails),
      filter(caseIntakeDetails => caseIntakeDetails != null)
    ).subscribe((caseIntakeDetails: CaseIntakeDetail) => {
      this.caseIntakeDetails = caseIntakeDetails;
      
      this.selectedCapabilityPracticeAreas = this.capabilityPracticeAreas.find((x) => this.caseIntakeDetails?.capabilityPracticeAreaCodes === x.practiceAreaCode.toString());
      this.selectedIndustryPracticeAreas = this.industryPracticeAreas.find((x) => this.caseIntakeDetails?.industryPracticeAreaCodes === x.practiceAreaCode.toString());
    
    }));
  }



  private getExpertiseList() {
    const expertiseList$ = this.getExpertiseRequirementListFromStore();
    const staffingPreferences$ = this.coreService.loadLookupListForStaffingPreferences();
  
    this.storeSub.add(
      combineLatest([expertiseList$, staffingPreferences$]).subscribe(([expertiseList, staffingPreferences]) => {
        this.newExpertiseList = expertiseList;
        this.expertiseList = staffingPreferences.filter(pref => pref.preferenceGroupCode === StaffingPreferenceGroupEnum.PD);
        this.createCombinedExpertiseList();
      })
    );
  }
  
  private getExpertiseRequirementListFromStore() {
    return this.store.pipe(
      select(fromStaffingIntake.getExpertiseRequirementList),
      filter(expertiseList => expertiseList != null)
    );
  }

  private createCombinedExpertiseList(){
    const convertedExpertise = this.expertiseList 
        .map(pref => ({
            expertiseAreaCode: pref.preferenceTypeCode,
            expertiseAreaName: pref.preferenceTypeName
        }));
  
    // Normalize `this.newexpertiseList` to match the structure
    const normalizedNewExpertiseList = this.newExpertiseList.map((expertise) => ({
      expertiseAreaCode: expertise.expertiseAreaCode?.toString(), // Convert number to string if needed
      expertiseAreaName: expertise.expertiseAreaName // Map name directly
    }));
  
    // Combine both lists
    this.combinedExpertiseList = convertedExpertise.concat(normalizedNewExpertiseList);

  }

  private getRoleDetailsFromStore(){
    this.storeSub.add(this.store.pipe(
      select(fromStaffingIntake.getRoleDetails),
      filter(roleDetails => roleDetails != null)
    ).subscribe((roleDetails: CaseIntakeRoleDetails[]) => {
      this.roleDetails = roleDetails;
    }));
  }

  private getWorkStreamDetailsFromStore(){
    this.storeSub.add(this.store.pipe(
      select(fromStaffingIntake.getWorkstreamDetails),
      filter(workstreamDetails => workstreamDetails != null)
    ).subscribe((workstreamDetails: CaseIntakeWorkstreamDetails[]) => {
      this.workstreamDetails = workstreamDetails.map(workstream => {
        workstream.roles = this.roleDetails.filter(role => role.workstreamId == workstream.id);
        return workstream;
      });
    }));
  }

  private getLastUpdatedInfoFromStore() {
    this.storeSub.add(this.store.pipe(
      select(fromStaffingIntake.getLastUpdatedInfo),
      filter(lastUpdatedInfo => lastUpdatedInfo != null)
    ).subscribe((lastUpdatedInfo: { lastUpdated: Date; lastUpdatedByName: string }) => {
      this.lastUpdated = lastUpdatedInfo.lastUpdated;
      this.lastUpdatedByName = lastUpdatedInfo.lastUpdatedByName;
    }));
  }

  getTimezoneAbbreviation(): string {
    if (this.lastUpdated) {
      const date = new Date(this.lastUpdated);
      // Extract the numeric GMT offset (like GMT-0500)
      const gmtOffset = date.toTimeString().match(/GMT[+-]\d{4}/)[0];
  
      // Extract the full timezone name (like Eastern Standard Time)
      const timezoneName = Intl.DateTimeFormat('en-US', { timeZoneName: 'long' })
        .formatToParts(date)
        .find(part => part.type === 'timeZoneName')?.value;
  
      // Return both the GMT offset and the full timezone name in the desired format
      return `${gmtOffset} (${timezoneName || 'GMT'})`;
    } else {
      return '';
    }
  }

  caseIntakeDetailsChangeHandler(caseIntakeDetails: CaseIntakeDetail) {
    this.store.dispatch(
      new StaffingIntakeActions.UpsertCaseIntakeDetails(caseIntakeDetails));
  }

  upsertLeadershipHandler(leadershipDetails: CaseIntakeLeadership[]) {
    this.store.dispatch(
      new StaffingIntakeActions.UpsertLeadershipDetails(leadershipDetails));
  }

  deleteLeadershipHandler(caseRoleCode: string) {
    const deleteLeadershipDetail: CaseIntakeBasicDetails = {
      id: null,
      caseRoleCode: caseRoleCode,
      oldCaseCode: this.oldCaseCode ?? null,
      opportunityId: this.opportunityId ?? null,
      planningCardId: this.planningCardId ?? null,
      lastUpdatedBy: this.coreService.loggedInUser.employeeCode
    };
  
    this.store.dispatch(new StaffingIntakeActions.DeleteLeadershipDetail(deleteLeadershipDetail));
  }

  newExpertiseListChangeHandler(newExpertiseList: CaseIntakeExpertise[]) {
    this.store.dispatch(
      new StaffingIntakeActions.UpsertExpertiseRequirementList(newExpertiseList));
  }


  private getLookupListFromLocalStorage() {
    this.languages = this.localStorageService.get(ConstantsMaster.localStorageKeys.languages);
    this.serviceLines = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLines);
    this.serviceLines = this.serviceLines.filter(x => x.inActive === false);
    this.positionGroups = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsGroups);
    if (this.positionGroups && this.positionGroups.length > 0) {
      const positionCodesToShow = ConstantsMaster.positionGroupsCodesToShow;
      this.positionGroups = this.positionGroups.filter(pg => pg.positionGroupCode !== '' && positionCodesToShow.includes(pg.positionGroupCode))
                            .sort((a, b) => a.positionGroupName.localeCompare(b.positionGroupName));
    }
    this.industryPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.industryPracticeAreas);
    this.capabilityPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.capabilityPracticeAreas);
  }

  onIndustryPracticeAreaChange(event) {
    this.selectedIndustryPracticeAreas = event;
    this.caseIntakeDetails.industryPracticeAreaCodes = event ? event.practiceAreaCode : null;
    this.caseIntakeDetailsChangeHandler(this.caseIntakeDetails);
  }

  onCapabilityPracticeAreaChange(event) {
    this.selectedCapabilityPracticeAreas = event;
    this.caseIntakeDetails.capabilityPracticeAreaCodes = event ? event.practiceAreaCode : null;
    this.caseIntakeDetailsChangeHandler(this.caseIntakeDetails);
  }
 
  ngOnDestroy() {
    this.storeSub.unsubscribe();
    this.store.dispatch(new StaffingIntakeActions.ClearStaffingIntakeState());
  }
}

