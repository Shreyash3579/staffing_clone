import { Component, OnInit, Input ,EventEmitter,Output, SimpleChanges, ViewChild, OnDestroy} from "@angular/core";
import { NgSelectComponent } from "@ng-select/ng-select";
import { concat, of } from "rxjs";
import { Observable } from "rxjs/internal/Observable";
import { Subject } from "rxjs/internal/Subject";
import { catchError, distinctUntilChanged, switchMap, tap } from "rxjs/operators";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { EmployeeStaffingInfo } from "src/app/shared/interfaces/employeeStaffingInfo";
import { Resource } from "src/app/shared/interfaces/resource.interface";
import { SharedService } from "src/app/shared/shared.service";
import { environment } from "src/environments/environment";
import { v4 as uuidv4 } from 'uuid';

@Component({
    selector: "app-resource-overlay-header",
    templateUrl: "./resource-overlay-header.component.html",
    styleUrls: ["./resource-overlay-header.component.scss"]
})
export class ResourceOverlayHeaderComponent implements OnInit, OnDestroy {
    shareUrl: string;
    irisBaseUrl : string;
    @Input() resourceDetails: any;
    @Input() activeStaffableAsRoleName: any;
    @Input('employeeStaffingDetails') existingEmployeeStaffingDetails:EmployeeStaffingInfo  ;
    @Output() UpdateEmployeeStaffingResponsibleData =  new EventEmitter<EmployeeStaffingInfo[]>();
    timeInLevelInfo = ConstantsMaster.TimeInLevelDefination;
    staffingResponsibleInfo = ConstantsMaster.StaffingResponsibleDefination;
    pdLeadInfo = ConstantsMaster.PdLeadDefination;
    notifyUponStaffingInfo = ConstantsMaster.NotifyUponStaffingDefination;
    advisorInfo = ConstantsMaster.AdvisorDefination;
    menteesInfo = ConstantsMaster.MenteesDefination;
    mentorInfo = ConstantsMaster.MentorDefination;
    accessibleFeatures = ConstantsMaster.appScreens.feature;
    showTypeaheadSR: boolean = false; 
    showTypeaheadPD: boolean = false;
    showTypeaheadNS: boolean = false;
    resourceInputSR$ = new Subject<string>();
    resourceInputPD$ = new Subject<string>();
    resourceInputNS$ = new Subject<string>();
    resourcesDataStaffingResponsible$ : Observable<Resource[]> 
    resourcesDataPdLead$: Observable<Resource[]>;
    resourcesDataNotifyUponStaffing$: Observable<Resource[]>;

    isResourceSearchOpenStaffingResponsible = false;
    isResourceSearchOpenPdLead = false;
    isResourceSearchOpenNotifyUponStaffing = false;
    id = uuidv4();
    
    //staffingResponsible
    public staffingResponsibleEmployees: any[]=[];
    public formattedStaffingResponsibleResourcesToShow = "";

    //pdLead
    public pdLeadEmployees: any[]=[];
    public formattedpdLeadResourcesToShow = "";

    //notify upon staffing
    public notifyUponStaffingEmployees: any[]=[];
    public formattedNotifyUponStaffingResourcesToShow = "";
    
    selectedStaffingResponsibleResources: Resource[] = [];
    selectedStaffingPDResources: Resource[] = [];
    @ViewChild('searchBoxSR') searchBoxSR: NgSelectComponent;
    @ViewChild('searchBoxPD') searchBoxPD: NgSelectComponent;
    @ViewChild('searchBoxNS') searchBoxNS: NgSelectComponent;

    get menteesString(): string {
      return this.resourceDetails?.mentees
        ?.filter(m => m?.fullName)
        .map(m => m.fullName)
        .join(', ') || '';
    }

    constructor(private sharedService: SharedService,) {}

    ngOnInit(): void {
        this.shareUrl = environment.settings.environmentUrl; 
        this.irisBaseUrl = environment.settings.irisProfileBaseUrl
        this.attachEventForResourcesSearchStaffingResponsible();
        this.attachEventForResourcesSearchPdLead();
        this.attachEventForResourcesSearchNotifyUponStaffing();
    }

    ngOnChanges(changes: SimpleChanges){
        if(changes.existingEmployeeStaffingDetails && this.existingEmployeeStaffingDetails)
        {
            this.getStaffingResponsibleDetails();
            this.getPdLeadDetails();
            this.getNotifyUponStaffingDetails();
        }
    }

    getStaffingResponsibleDetails(){
      if(this.existingEmployeeStaffingDetails){
        this.staffingResponsibleEmployees = this.existingEmployeeStaffingDetails.responsibleForStaffingDetails ?? [];
        this.formattedStaffingResponsibleResourcesToShow = this.getFormattedNames(this.staffingResponsibleEmployees);
      }
    }

    getPdLeadDetails(){
      if(this.existingEmployeeStaffingDetails){
        this.pdLeadEmployees = this.existingEmployeeStaffingDetails.pdLeadDetails ?? [];
        this.formattedpdLeadResourcesToShow = this.getFormattedNames(this.pdLeadEmployees);
      }
    }

    getNotifyUponStaffingDetails(){
      if(this.existingEmployeeStaffingDetails){
        this.notifyUponStaffingEmployees = this.existingEmployeeStaffingDetails.notifyUponStaffingDetails ?? [];
        this.formattedNotifyUponStaffingResourcesToShow = this.getFormattedNames(this.notifyUponStaffingEmployees);
      }
    }


    formatNameWithFirstNameThenLastName(name: string): string {
        const [lastName, firstName] = name.split(',');
        return `${firstName} ${lastName}`;
    }

    editStaffingResponsible(event){
        this.showTypeaheadSR =true;
        setTimeout(() => {
            this.searchBoxSR.focus();
        });
    }

    editPdLead(event){
        this.showTypeaheadPD =true;
        setTimeout(() => {
            this.searchBoxPD.focus();
        });
    }

    editNotifyUponStaffing(event){
      this.showTypeaheadNS =true;
      setTimeout(() => {
          this.searchBoxNS.focus();
      });
  }

    getAffiliationImage(role) {
        if (role.toLowerCase().includes("connected")) {
            return "assets/img/Affiliations_1.svg";
        }
        if (role.toLowerCase().includes("l2")) {
            return "assets/img/Affiliations_2.svg";
        }
        if (role.toLowerCase().includes("l1")) {
            return "assets/img/Affiliations_3.svg";
        }
        return "assets/img/Affiliations_4.svg";
    }

    saveStaffingInfoHandler($event) {
      this.hideStaffingInfoTypeaheads();
    
      const previousResponsibleCodes = this.existingEmployeeStaffingDetails?.responsibleForStaffingCodes;
      const previousPdLeadCodes = this.existingEmployeeStaffingDetails?.pdLeadCodes;
      const previousNotifyUponStaffingCodes = this.existingEmployeeStaffingDetails?.notifyUponStaffingCodes;
    
      const newResponsibleCodes = this.getEmployeeCodes(this.staffingResponsibleEmployees);
      const newPdLeadCodes = this.getEmployeeCodes(this.pdLeadEmployees);
      const newNotifyUponStaffingCodes = this.getEmployeeCodes(this.notifyUponStaffingEmployees);
    
      if (this.isDataChanged(previousResponsibleCodes, newResponsibleCodes) || this.isDataChanged(previousPdLeadCodes, newPdLeadCodes) || this.isDataChanged(previousNotifyUponStaffingCodes, newNotifyUponStaffingCodes)) {
        const dataToUpsert: EmployeeStaffingInfo = {
          id: this.existingEmployeeStaffingDetails?.id || this.id,
          employeeCode: this.resourceDetails.resource.employeeCode,
          responsibleForStaffingCodes: newResponsibleCodes,
          notifyUponStaffingCodes: newNotifyUponStaffingCodes,
          pdLeadCodes: newPdLeadCodes,
          lastUpdatedBy: null,
          
        };
    
        const dataListToUpsert: EmployeeStaffingInfo[] = [dataToUpsert];
        this.UpdateEmployeeStaffingResponsibleData.emit(dataListToUpsert);
    
        this.existingEmployeeStaffingDetails = {
          ...this.existingEmployeeStaffingDetails,
          responsibleForStaffingCodes: newResponsibleCodes,
          pdLeadCodes: newPdLeadCodes,
          notifyUponStaffingCodes: newNotifyUponStaffingCodes,
        };

        this.updateFormattedResourcesToShow();
      }
      
    }
    
    hideStaffingInfoTypeaheads() {
      this.showTypeaheadSR = false;
      this.showTypeaheadPD = false;
      this.showTypeaheadNS = false;
    }
    
    getEmployeeCodes(employees: any) {
      return employees?.map(obj => obj.employeeCode).join(',');
    }
    
    isDataChanged(prevData, newData) {
      //handle case when prevData is null and newData is empty string. In that case prevData !== newData returns true.
      prevData = prevData || '';
      newData = newData || '';
      
      return  prevData !== newData;
    }
    
    updateFormattedResourcesToShow() {
      this.formattedStaffingResponsibleResourcesToShow = this.getFormattedNames(this.staffingResponsibleEmployees);
      this.formattedpdLeadResourcesToShow = this.getFormattedNames(this.pdLeadEmployees);
      this.formattedNotifyUponStaffingResourcesToShow = this.getFormattedNames(this.notifyUponStaffingEmployees);
    }
    
    getFormattedNames(employees: any) {
      return employees?.map(x => this.formatNameWithFirstNameThenLastName(x.fullName)).join(',');
    }
      

    onResourceSearchChangeStaffingResponsible($event) {
        if ($event.term.length > 2) {
          this.resourceInputSR$.next($event.term);
        }
    
        // to reset search term if keyword's length is less than 3
        if ($event.term.length < 3) {
          this.resourceInputSR$.next(null);
        }
    
        // TODO: below condition should be removed once permanent solution is applied
        if ($event.term.length < 1) {
          this.isResourceSearchOpenStaffingResponsible = false;
        }
        
      }

      onResourceSearchChangePdLead($event) {
        if ($event.term.length > 2) {
          this.resourceInputPD$.next($event.term);
        }
    
        // to reset search term if keyword's length is less than 3
        if ($event.term.length < 3) {
          this.resourceInputPD$.next(null);
        }
    
        // TODO: below condition should be removed once permanent solution is applied
        if ($event.term.length < 1) {
          this.isResourceSearchOpenPdLead = false;
        }
      }

      onResourceSearchChangeNotifyUponStaffing($event) {
        if ($event.term.length > 2) {
          this.resourceInputNS$.next($event.term);
        }
    
        // to reset search term if keyword's length is less than 3
        if ($event.term.length < 3) {
          this.resourceInputNS$.next(null);
        }

      // TODO: below condition should be removed once permanent solution is applied
      if ($event.term.length < 1) {
          this.isResourceSearchOpenNotifyUponStaffing = false;
       }

      }

      attachEventForResourcesSearchStaffingResponsible() {
        this.resourcesDataStaffingResponsible$ = concat(
          of([]), // default items
          this.resourceInputSR$.pipe(
            distinctUntilChanged(),
            switchMap(term => this.sharedService.getResourcesBySearchString(term).pipe(
              catchError(() => of([])), // empty list on error
            )),
            tap(() => {
              this.isResourceSearchOpenStaffingResponsible = true;
            })
          )
        );
      }

      attachEventForResourcesSearchPdLead() {
        this.resourcesDataPdLead$ = concat(
          of([]), // default items
          this.resourceInputPD$.pipe(
            distinctUntilChanged(),
            switchMap(term => this.sharedService.getResourcesBySearchString(term).pipe(
              catchError(() => of([])), // empty list on error
            )),
            tap(() => {
              this.isResourceSearchOpenPdLead = true;
            })
          )
        );
      }
      attachEventForResourcesSearchNotifyUponStaffing() {
        this.resourcesDataNotifyUponStaffing$ = concat(
          of([]), // default items
          this.resourceInputNS$.pipe(
            distinctUntilChanged(),
            switchMap(term => this.sharedService.getResourcesBySearchString(term).pipe(
              catchError(() => of([])), // empty list on error
            )),
            tap(() => {
              this.isResourceSearchOpenNotifyUponStaffing = true;
            })
          )
        );
      }

    onStaffingResponsibleAdd($event){
        if(this.staffingResponsibleEmployees.findIndex(x=>x.employeeCode == $event.employeeCode) < 0 ){
            this.staffingResponsibleEmployees.push({employeeCode: $event.employeeCode, fullName: $event.fullName});
        }
        
    }

    onStaffingResponsibleRemove($event){
        this.staffingResponsibleEmployees = this.staffingResponsibleEmployees.filter((existingResource) => {
            return (existingResource.employeeCode !== $event.employeeCode);
        });
    }

    onPdLeadAdd($event){
        if(this.pdLeadEmployees.findIndex(x=>x.employeeCode == $event.employeeCode) < 0){
        this.pdLeadEmployees.push({employeeCode: $event.employeeCode, fullName: $event.fullName});
        }
        
    }

    onPdLeadRemove($event){
        this.pdLeadEmployees = this.pdLeadEmployees.filter((existingResource) => {
            return (existingResource.employeeCode !== $event.employeeCode);
        });
    }

    onClearAllSelectedPdLeads(){
        this.pdLeadEmployees = [];
    }

    onNotifyUponStaffingAdd($event){
      if(this.notifyUponStaffingEmployees.findIndex(x=>x.employeeCode == $event.employeeCode) < 0){
      this.notifyUponStaffingEmployees.push({employeeCode: $event.employeeCode, fullName: $event.fullName});
      }    
     }

    onNotifyUponStaffingRemove($event){
        this.notifyUponStaffingEmployees = this.notifyUponStaffingEmployees.filter((existingResource) => {
            return (existingResource.employeeCode !== $event.employeeCode);
        });
    }

    onClearAllSelectedNotifyUponStaffing(){
        this.notifyUponStaffingEmployees = [];
    }

    onClearAllSelectedStaffingResponsible(){
        this.staffingResponsibleEmployees = [];
    }

    swapAffiliation(affiliationText) {
      if (affiliationText.indexOf("-") > -1) {
        const affiliationContent = affiliationText.split("-");
        return affiliationContent[1].trim() + " - " + affiliationContent[0].trim();
      }
      return affiliationText;
    }

    ngOnDestroy(): void {
    }
}
