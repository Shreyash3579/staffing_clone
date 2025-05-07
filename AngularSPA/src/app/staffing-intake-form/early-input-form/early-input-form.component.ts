import { Component, ElementRef, EventEmitter, Input, OnInit, Output, SimpleChanges, ViewChild } from "@angular/core";
import { debounceTime, fromEvent, map, Subject } from "rxjs";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { CaseIntakeDetail } from "src/app/shared/interfaces/caseIntakeDetail.interface";
import { CaseIntakeExpertise } from "src/app/shared/interfaces/caseIntakeExpertise.interface";
import { Language } from "src/app/shared/interfaces/language";
import { Office } from "src/app/shared/interfaces/office.interface";
import { Places } from "src/app/shared/interfaces/places.interface";
import { PracticeArea } from "src/app/shared/interfaces/practiceArea.interface";
import { v4 as uuidv4 } from 'uuid'; 

@Component({
  selector: "app-early-input-form",
  templateUrl: "./early-input-form.component.html",
  styleUrls: ["./early-input-form.component.scss"]
})
export class EarlyInputFormComponent implements OnInit {
  @Input() opportunityId : string = null;
  @Input() oldCaseCode : string = null;
  @Input() planningCardId : string = null;
  @Input() expertises: PracticeArea[];
  @Input() languages: Language[];
  @Input() combinedExpertiseList: CaseIntakeExpertise[];

  selectedPlace: Places[] = [];

  @Input() caseIntakeDetails : CaseIntakeDetail = {} as CaseIntakeDetail;
  @Output() caseIntakeDetailsChangeEmitter = new EventEmitter<CaseIntakeDetail>();
  @Output() newExpertiseEmitter = new EventEmitter<CaseIntakeExpertise>();
  @Output() getPlacesBySearchString = new EventEmitter<any>();
  

  clientEngagementModel = ConstantsMaster.clientEngagementModelOptions;
  selectedLocations : Office[]=[];
  selectedLanguages : Language[]=[];
  selectedExpertise : CaseIntakeExpertise[]=[];
  selectedClientEngagementModel : string;
  selectedClientEngagementModelObject : any; //change the type
  selectedClientEngagementModelCodes : string;
  selectedbackgroundCheckNotes: string;


  constructor() {
  }

  ngOnInit(): void {
    this.initializeCaseIntakeDetail();
  }

  ngOnChanges(changes: SimpleChanges) {

    if (changes.caseIntakeDetails && this.caseIntakeDetails) {
      
      if (this.caseIntakeDetails.officeCodes?.trim()) { // Check for null, undefined, or empty string
        this.selectedPlace = this.caseIntakeDetails.officeCodes
            .split(';')
            .filter(code => code.trim() !== '') // Remove empty strings after split
            .map(code => ({
                ...{} as Places,
                full_path: code.trim() // Trim whitespace from each value
            }));
      }
  
      this.selectedClientEngagementModel = this.caseIntakeDetails.clientEngagementModel;
      this.selectedClientEngagementModelCodes = this.caseIntakeDetails.clientEngagementModelCodes;
      this.selectedClientEngagementModelObject = this.clientEngagementModel && this.caseIntakeDetails.clientEngagementModelCodes
          ? this.clientEngagementModel.find((x) => x.id === this.caseIntakeDetails.clientEngagementModelCodes) : [];

      this.selectedLanguages = this.languages && this.caseIntakeDetails.languages 
        ? this.languages.filter((x) =>
            this.caseIntakeDetails.languages.split(',').some(id => id.trim() === x.id.toString())
          ) : [];
      
      this.selectedExpertise = this.combinedExpertiseList && this.caseIntakeDetails.expertiseRequirement 
          ? this.combinedExpertiseList.filter((x) =>
              this.caseIntakeDetails.expertiseRequirement.split(',').some(code => code.trim() === x.expertiseAreaCode.toString())
            ) : [];

      this.selectedbackgroundCheckNotes = this.caseIntakeDetails?.backgroundCheckNotes;
    }

    if(changes.combinedExpertiseList && this.combinedExpertiseList)
    {
      this.selectedExpertise = this.combinedExpertiseList && this.caseIntakeDetails.expertiseRequirement 
        ? this.combinedExpertiseList.filter((x) =>
            this.caseIntakeDetails.expertiseRequirement.split(',').some(code => code.trim() === x.expertiseAreaCode.toString())
          ) : [];
    }
  }

  private initializeCaseIntakeDetail() {
    if (!this.caseIntakeDetails) {
      this.caseIntakeDetails = {} as CaseIntakeDetail;
    }
    this.caseIntakeDetails.oldCaseCode = this.oldCaseCode ?? null;
    this.caseIntakeDetails.opportunityId = this.opportunityId ?? null;
    this.caseIntakeDetails.planningCardId = this.planningCardId ?? null;
    this.caseIntakeDetails.clientEngagementModel = this.selectedClientEngagementModel ?? "";
    this.caseIntakeDetails.clientEngagementModelCodes = this.selectedClientEngagementModelCodes ?? "";
    this.caseIntakeDetails.officeCodes = this.selectedLocations.map(location => location.officeCode.toString()).join(',') ?? "";
    this.caseIntakeDetails.expertiseRequirement = this.selectedExpertise.map(expertise => expertise.expertiseAreaCode.toString()).join(',') ?? "";
    this.caseIntakeDetails.languages = this.selectedLanguages.map(language => language.id.toString()).join(',') ?? "";
    this.caseIntakeDetails.caseDescription = this.caseIntakeDetails.caseDescription ?? "";
    this.caseIntakeDetails.backgroundCheckNotes = this.selectedbackgroundCheckNotes ?? "";
  }

  onSearchItemSelectHandler(selectedPlace: Places[]){
    this.selectedPlace = selectedPlace;
    if(selectedPlace){
      this.caseIntakeDetails.officeCodes = selectedPlace
      .map(place => place.full_path) // Extract full_path
      .join(';');
    }
    else{
      this.caseIntakeDetails.officeCodes = null;
    }
    //this.validateField('resource');
    this.emitCaseIntakeDetails();
  }

  onInputChange(event) {
    this.caseIntakeDetails.caseDescription = event.target.value;
    this.emitCaseIntakeDetails();
  }

  updateCaseDescription(event) {
    this.caseIntakeDetails.caseDescription = event.target.value;
    this.emitCaseIntakeDetails();
  }

  onOfficeListChange(event) {
    this.selectedLocations = event;
    this.caseIntakeDetails.officeCodes = event.map(location => location.officeCode.toString()).join(',');
    this.emitCaseIntakeDetails();
  }

  onClientEngagementModelChange(event) {
    this.selectedClientEngagementModelObject = event;
    this.selectedClientEngagementModelCodes = event?.id ?? "";
    this.caseIntakeDetails.clientEngagementModelCodes = event?.id ?? "";
    if(this.selectedClientEngagementModelCodes != "5") {
      this.selectedClientEngagementModel = "";
    }
    this.emitCaseIntakeDetails();
  }

  onDescribeOtherClientEngagementModelChange(event) {
    this.selectedClientEngagementModel = event.target.value;
    this.caseIntakeDetails.clientEngagementModel = event.target.value;
    this.emitCaseIntakeDetails();
  }

  onExpertiseChange(event) {
    this.selectedExpertise = event;
    this.caseIntakeDetails.expertiseRequirement = event.map(expertise => expertise.expertiseAreaCode.toString()).join(',');
    this.emitCaseIntakeDetails();
  }

  createNewExpertise = (expertise) => {
    const expertiseObj = {
      expertiseAreaCode: uuidv4(),
      expertiseAreaName: expertise
    };

    this.selectedExpertise.push(expertiseObj);
    this.caseIntakeDetails.expertiseRequirement = this.selectedExpertise.map(expertise => expertise.expertiseAreaCode.toString()).join(',');
    this.emitNewExpertise(expertiseObj);
    this.emitCaseIntakeDetails();
    //this.expertises.push(expertiseObj)
    return expertiseObj;
  };

  onLanguageChange(event) {
    this.selectedLanguages = event;
    this.caseIntakeDetails.languages = event.map(language => language.id.toString()).join(',');
    this.emitCaseIntakeDetails();
  }

  onBackgroundCheckModelChange(event) {
    this.selectedbackgroundCheckNotes = event.target.value;
    this.caseIntakeDetails.backgroundCheckNotes = event.target.value;
    this.emitCaseIntakeDetails();
  }

  emitCaseIntakeDetails() {
    this.caseIntakeDetailsChangeEmitter.emit(this.caseIntakeDetails);
  } 
  
  emitNewExpertise(expertise: CaseIntakeExpertise) {
    this.newExpertiseEmitter.emit(expertise);
  }

}
