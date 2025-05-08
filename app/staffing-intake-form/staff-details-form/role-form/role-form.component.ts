import { CommonModule } from "@angular/common";
import {
  CUSTOM_ELEMENTS_SCHEMA,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnInit,
  Output,
  Renderer2,
  ViewChild
} from "@angular/core";
import { Form, FormArray, FormControl, FormGroup } from "@angular/forms";
import { CaseIntakeRoleDetails } from "src/app/shared/interfaces/caseIntakeRoleDetails.interface";
import { Language } from "src/app/shared/interfaces/language";
import { PositionGroup } from "src/app/shared/interfaces/position-group.interface";
import { Position } from "src/app/shared/interfaces/position.interface";
import { PracticeArea } from "src/app/shared/interfaces/practiceArea.interface";
import { ServiceLine } from "src/app/shared/interfaces/serviceLine.interface";
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { CaseIntakeDetail } from "src/app/shared/interfaces/caseIntakeDetail.interface";
import { CaseIntakeWorkstreamDetails } from "src/app/shared/interfaces/caseIntakeWorkstreamDetails.interface";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { CaseIntakeExpertise } from "src/app/shared/interfaces/caseIntakeExpertise.interface";
import { Places } from "src/app/shared/interfaces/places.interface";
import {NgSelectComponent} from "@ng-select/ng-select";

@Component({
  selector: 'app-role-form',
  templateUrl: './role-form.component.html',
  styleUrls: ['./role-form.component.scss']
})
export class RoleFormComponent implements OnInit {
  @Input() role: CaseIntakeRoleDetails;
  @Input() combinedExpertiseList: CaseIntakeExpertise[];
  @Input() languages: Language[];
  @Input() positionGroups: PositionGroup[];
  @Input() serviceLine: ServiceLine[];
  @Input() renderInWorkstream: boolean;
  @Input() caseIntakeDetails : CaseIntakeDetail;
  @Input() collapseAll: boolean = false;
  @Input() workstream: CaseIntakeWorkstreamDetails;


  @Output() updateRoleEmitter = new EventEmitter();
  @Output() deleteRoleEmitter = new EventEmitter();
  @Output() changeLeadEmitter = new EventEmitter();

  @Input() resources: FormArray;
  @Input() listNotWorkstream: string[];
  @Input() currentState: {roles: boolean,
    workstreams: boolean,
    details: boolean};
  @Input() locations: { id: number, name: string }[];
  @Input() roles: { id: number, name: string }[];
  @Input() reportRoles: { id: number, name: string }[];
  @Input() readyToLead: boolean;
  @Output() draggingStarted = new EventEmitter<boolean>();
  @Output() removeResource_ = new EventEmitter<number>();
  @Output() openedPanels = new EventEmitter<boolean>();
  @Output() selectedLead_ = new EventEmitter<number>();
  @ViewChild('languageSelect') languageSelect: NgSelectComponent;
  @ViewChild('expertiseSelect') expertiseSelect: NgSelectComponent;
  toggleMenu = new Map<Number, boolean>();
  selectedLead: number;

  listOfMustHaveExpertise:CaseIntakeExpertise[] = []
  listOfNiceToHaveExpertise: CaseIntakeExpertise[] = [];

  listOfMustHaveLanguages:Language[] = []
  listOfNiceToHaveLanguages: Language[] = [];

  activeToolTip: number | null;
  activeToolTipLanguage: number | null;

  clientEngagementModel = ConstantsMaster.clientEngagementModelOptions;
  selectedClientEngagementModel : string;
  selectedClientEngagementModelObject : any; //change the type
  selectedClientEngagementModelCodes : string;

  selectedPlace: Places[] = [];
  topTooltipStyle: number;
  leftTooltipStyle: number;


  //private clickListener: () => void;
  // constructor(private renderer: Renderer2, private elRef: ElementRef) {
  //   this.clickListener = this.renderer.listen('document', 'click', (event: MouseEvent) => {
  //     this.onDocumentClick(event);
  //   });
  // }

  constructor(private renderer: Renderer2, private elRef: ElementRef) {
    this.renderer.listen("document", "click", (event: MouseEvent) => {
      this.handleClickOutside(event);
    });
  }


  ngOnInit(): void {
    this.initializeCaseIntakeRoleDetails();
  }

  private initializeCaseIntakeRoleDetails() {
    if (!this.role) {
      this.role = {} as CaseIntakeRoleDetails;
    }
  }

  ngOnChanges(changes) {
    if (changes.role && this.role) {
        this.updateSelectedValues();
      }
    if(changes.combinedExpertiseList && this.combinedExpertiseList){
      this.updateSelectedValues();
    }
  }

  onDocumentClick(event: MouseEvent) {
    const tooltipElement = this.elRef.nativeElement.querySelector('.selection-container');
    const clickedInsideTooltip = tooltipElement && tooltipElement.contains(event.target as Node);
    const clickedInsideComponent = this.elRef.nativeElement.contains(event.target);
    // TODO: solve here
    //console.log(clickedInsideComponent, !clickedInsideTooltip , this.activeToolTip, tooltipElement.contains(event.target as Node));
    if (!clickedInsideComponent) {
      this.activeToolTip = null;
      this.activeToolTipLanguage = null;
    }
  }

  updateRoleDetails() {
    this.role.clientEngagementModel = this.caseIntakeDetails.clientEngagementModel;
    this.role.officeCodes = this.caseIntakeDetails.officeCodes;
    this.role.officeNames = this.caseIntakeDetails.officeNames;
    this.role.languageCodes = this.caseIntakeDetails.languages;
    this.role.expertiseRequirementCodes = this.caseIntakeDetails.expertiseRequirement;
    this.role.clientEngagementModelCodes = this.caseIntakeDetails.clientEngagementModelCodes;
    this.role.niceToHaveExpertiseCodes = this.caseIntakeDetails.expertiseRequirement;
    this.role.niceToHaveLanguageCodes = this.caseIntakeDetails.languages;

  }

  updateSelectedValues() {

    this.selectedClientEngagementModelObject = this.clientEngagementModel && this.role.clientEngagementModelCodes
          ? this.clientEngagementModel.find((x) => x.id === this.role.clientEngagementModelCodes) : [];

    this.selectedClientEngagementModel = this.role.clientEngagementModel;
    this.selectedClientEngagementModelCodes = this.role.clientEngagementModelCodes;

    this.listOfMustHaveLanguages = this.languages && this.role?.mustHaveLanguageCodes
    ? this.languages.filter(x =>
        this.role.mustHaveLanguageCodes.split(',').some(code => code.trim() === x.id.toString())
      )
    : [];

    this.listOfNiceToHaveLanguages = this.languages && this.role?.niceToHaveLanguageCodes
    ? this.languages.filter(x =>
        this.role.niceToHaveLanguageCodes.split(',').some(code => code.trim() === x.id.toString())
      )
    : [];

    this.listOfMustHaveExpertise = this.combinedExpertiseList && this.role?.mustHaveExpertiseCodes
    ? this.combinedExpertiseList.filter(x =>
        this.role.mustHaveExpertiseCodes.split(',').some(code => code.trim() === x.expertiseAreaCode.toString())
      )
    : [];

    this.listOfNiceToHaveExpertise = this.combinedExpertiseList && this.role?.niceToHaveExpertiseCodes
    ? this.combinedExpertiseList.filter(x =>
        this.role.niceToHaveExpertiseCodes.split(',').some(code => code.trim() === x.expertiseAreaCode.toString())
      )
    : [];


    this.role.selectedLanguages = this.languages && (this.role?.mustHaveLanguageCodes || this.role?.niceToHaveLanguageCodes)
      ? this.languages.filter(x =>
          // Combine the must-have and nice-to-have codes into one array and check against it
          [
            ...(this.role.mustHaveLanguageCodes ? this.role.mustHaveLanguageCodes.split(',') : []),
            ...(this.role.niceToHaveLanguageCodes ? this.role.niceToHaveLanguageCodes.split(',') : [])
          ].some(code => code.trim() === x.id.toString())
        )
      : [];


    this.role.selectedExpertise = this.combinedExpertiseList && (this.role?.mustHaveExpertiseCodes || this.role?.niceToHaveExpertiseCodes)
      ? this.combinedExpertiseList.filter(x =>
          [
            ...(this.role.mustHaveExpertiseCodes ? this.role.mustHaveExpertiseCodes.split(',') : []),
            ...(this.role.niceToHaveExpertiseCodes ? this.role.niceToHaveExpertiseCodes.split(',') : [])
          ].some(code => code.trim() === x.expertiseAreaCode.toString())
        )
      : [];

    this.role.selectedServiceLine = this.serviceLine && this.role?.serviceLineCode
      ? this.serviceLine.find(x => this.role.serviceLineCode.includes(x.serviceLineCode.toString()))
      : null;

    this.role.selectedPositionGroup = this.positionGroups && this.role?.positionCode
      ? this.positionGroups.find(x => this.role.positionCode.includes(x.positionGroupCode.toString()))
      : null;

    if (this.role.officeCodes?.trim()) { // Check for null, undefined, or empty string
        this.selectedPlace = this.role.officeCodes
            .split(';')
            .filter(code => code.trim() !== '') // Remove empty strings after split
            .map(code => ({
                ...{} as Places,
                full_path: code.trim() // Trim whitespace from each value
            }));
    }
  }

  onSearchItemSelectHandler(selectedPlace: Places[]){
    this.selectedPlace = selectedPlace;
    if(selectedPlace){
      this.role.officeCodes = selectedPlace
      .map(place => place.full_path) // Extract full_path
      .join(';');
    }
    else{
      this.role.officeCodes = null;
    }
    this.emitRoleDetails();
  }


  onServiceLineChange(event) {
    this.role.selectedServiceLine = event;
    this.role.serviceLineCode = event ? event.serviceLineCode : null;
    this.emitRoleDetails();
  }

  onRoleChange(event) {
    this.role.selectedPositionGroup = event;
    this.role.positionCode = event ? event.positionGroupCode : null;
    this.emitRoleDetails();
  }

  onRoleNameChange(event) {
    this.role.name = event.target.value;
    this.emitRoleDetails();
  }

  onClientEngagementModelChange(event) {
    this.selectedClientEngagementModelObject = event;
    this.selectedClientEngagementModelCodes = event?.id ?? "";
    this.role.clientEngagementModelCodes = event?.id ?? "";
    if(this.selectedClientEngagementModelCodes != "5") {
      this.selectedClientEngagementModel = "";
    }
    this.emitRoleDetails();
  }

  onDescribeOtherClientEngagementModelChange(event) {
    this.selectedClientEngagementModel = event.target.value;
    this.role.clientEngagementModel = event.target.value;
    this.emitRoleDetails();
  }

  onExpertiseChange(event) {
    this.listOfNiceToHaveExpertise = event.filter(
      expertise => !this.listOfMustHaveExpertise.some(
        mustHave => mustHave.expertiseAreaCode === expertise.expertiseAreaCode
      )
    );
    this.listOfMustHaveExpertise = event.filter(
      expertise => this.listOfMustHaveExpertise.some(
        mustHave => mustHave.expertiseAreaCode === expertise.expertiseAreaCode
      )
    );

    this.role.niceToHaveExpertiseCodes = this.listOfNiceToHaveExpertise.map(expertise => expertise.expertiseAreaCode.toString()).join(',');
    this.role.mustHaveExpertiseCodes = this.listOfMustHaveExpertise.map(expertise => expertise.expertiseAreaCode.toString()).join(',');
    this.role.selectedExpertise = this.listOfMustHaveExpertise.concat(this.listOfNiceToHaveExpertise);
    this.emitRoleDetails();
  }

  onLanguageChange(event) {

    this.listOfNiceToHaveLanguages = event.filter(
      language => !this.listOfMustHaveLanguages.some(
        mustHave => mustHave.id === language.id
      )
    );
    this.listOfMustHaveLanguages = event.filter(
      language => this.listOfMustHaveLanguages.some(
        mustHave => mustHave.id === language.id
      )
    );

    this.role.niceToHaveLanguageCodes = this.listOfNiceToHaveLanguages.map(language => language.id.toString()).join(',');
    this.role.mustHaveLanguageCodes = this.listOfMustHaveLanguages.map(language => language.id.toString()).join(',');

    this.role.selectedLanguages = this.listOfMustHaveLanguages.concat(this.listOfNiceToHaveLanguages);
    this.emitRoleDetails();
  }

  addSharableRol() {
    this.role.roleDescription = "";
  }

  onRoleDescriptionModelChange(event) {
    this.role.roleDescription = event.target.value;
    this.emitRoleDetails();
  }

  emitRoleDetails() {
    this.updateRoleEmitter.emit(this.role);
  }

  setSelectedLead(role) {
    role.isLead = !role.isLead;
    const obj = {
      role: role,
      workstream: this.workstream
    }
    this.changeLeadEmitter.emit(obj);
  }

  removeResourceFromWorkstream(role) {
    this.deleteRoleEmitter.emit(role);
  }




  updateDragStatus(state: boolean) {
    this.draggingStarted.emit(state);
  }

  adjustWidth(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    inputElement.style.width = ((inputElement.value.length + 1) * 7) + 'px';
  }

  handleKeydown(event: KeyboardEvent): void {
    if (event.key === ' ') {
      event.stopPropagation(); // Prevent space from toggling the panel
    }
  }

  updateAccordionItem(event) {
    this.openedPanels.emit(event);
  }

  setMustHaveExpertise(event: any) {
    this.setMustAndNiceToHave(event, this.listOfMustHaveExpertise);
    this.removeMustAndNiceToHave(event, this.listOfNiceToHaveExpertise);

    this.role.mustHaveExpertiseCodes = this.listOfMustHaveExpertise.map(expertise => expertise.expertiseAreaCode.toString()).join(',');
    this.role.niceToHaveExpertiseCodes = this.listOfNiceToHaveExpertise.map(expertise => expertise.expertiseAreaCode.toString()).join(',');
    this.emitRoleDetails();
  }

  removeMustHaveExpertise(expertiseArea: any) {
    this.setMustAndNiceToHave(expertiseArea, this.listOfNiceToHaveExpertise);
    this.removeMustAndNiceToHave(expertiseArea, this.listOfMustHaveExpertise);

    this.role.mustHaveExpertiseCodes = this.listOfMustHaveExpertise.map(expertise => expertise.expertiseAreaCode.toString()).join(',');
    this.role.niceToHaveExpertiseCodes = this.listOfNiceToHaveExpertise.map(expertise => expertise.expertiseAreaCode.toString()).join(',');
    this.emitRoleDetails();

  }


  setMustHaveLanguage(event: any) {
    this.closeTooltipLanguage()
    this.setMustAndNiceToHaveForLanguages(event, this.listOfMustHaveLanguages);
    this.removeMustAndNiceToHaveForLanguages(event, this.listOfNiceToHaveLanguages);

    this.role.mustHaveLanguageCodes = this.listOfMustHaveLanguages.map(language => language.id.toString()).join(',');
    this.role.niceToHaveLanguageCodes = this.listOfNiceToHaveLanguages.map(language => language.id.toString()).join(',');
    this.emitRoleDetails();

  }

  removeMustHaveLanguage(event: any) {
    this.closeTooltipLanguage()
    this.setMustAndNiceToHaveForLanguages(event, this.listOfNiceToHaveLanguages);
    this.removeMustAndNiceToHaveForLanguages(event, this.listOfMustHaveLanguages);

    this.role.mustHaveLanguageCodes = this.listOfMustHaveLanguages.map(language => language.id.toString()).join(',');
    this.role.niceToHaveLanguageCodes = this.listOfNiceToHaveLanguages.map(language => language.id.toString()).join(',');

    this.emitRoleDetails();
  }

  setMustAndNiceToHave(event: any, list: any[]) {
    // Check if the item already exists in the list
    const exists = list.some(item => item.expertiseAreaCode === event.expertiseAreaCode);
    if (!exists) {
      list.push(event);
    }
  }

  removeMustAndNiceToHave(event: any, list: any[]) {
    // Find the index of the item to be removed
    const index = list.findIndex(item => item.expertiseAreaCode === event.expertiseAreaCode);
    if (index !== -1) {
      list.splice(index, 1);
    }
  }

  setMustAndNiceToHaveForLanguages(event: any, list: any[]) {
    // Check if the item already exists in the list
    const exists = list.some(item => item.id === event.id);
    if (!exists) {
      list.push(event);
    }
  }

  removeMustAndNiceToHaveForLanguages(event: any, list: any[]) {
    // Find the index of the item to be removed
    const index = list.findIndex(item => item.id === event.id);
    if (index !== -1) {
      list.splice(index, 1);
    }
  }



  @HostListener('document:click', ['$event'])
  private handleClickOutside(event: MouseEvent): void {
    const clickedInside = this.elRef.nativeElement.contains(event.target);
    if (!clickedInside) {
      this.activeToolTip = null;
      this.activeToolTipLanguage = null;
    }
  }

  // Toggle tooltip for expertise
  showTooltip(item: any, event: MouseEvent): void {
    event.stopImmediatePropagation();
    this.activeToolTip = this.activeToolTip === item.expertiseAreaCode ? null : item.expertiseAreaCode;

    // Add document listener for outside click
    this.renderer.listen('document', 'click', (outsideEvent: MouseEvent) => this.handleClickOutside(outsideEvent));
  }

  showTooltipLanguage(item: any, event: MouseEvent): void {
    event.stopPropagation();
    this.activeToolTipLanguage = this.activeToolTipLanguage === item.name ? null : item.name;
    
    if (this.activeToolTipLanguage) {
      this.topTooltipStyle = event.clientY + window.scrollY;
      this.leftTooltipStyle = event.clientX + window.scrollX;

    }

    console.log(this.activeToolTipLanguage);
    this.closeLanguageSelect()
    // Add document listener for outside click
    this.renderer.listen('document', 'click', (outsideEvent: MouseEvent) => this.handleClickOutside(outsideEvent));
  }
  closeLanguageSelect() {
    if (this.languageSelect) {
      this.languageSelect.close();
    }
  }
  closeExpertiseSelect() {
    if (this.expertiseSelect) {
      this.expertiseSelect.close();
    }
  }
  removeSelectedExpertise(item: any, event: Event): void {
    event.stopPropagation(); // Prevent parent clicks from interfering
    this.role.selectedExpertise = this.role.selectedExpertise.filter(expertise => expertise.expertiseAreaCode !== item.expertiseAreaCode);

    this.role.niceToHaveExpertiseCodes = this.role.niceToHaveExpertiseCodes ? this.role.niceToHaveExpertiseCodes.split(',').filter(code => code !== item.expertiseAreaCode).join(',') : '';
    this.role.mustHaveExpertiseCodes = this.role.mustHaveExpertiseCodes ? this.role.mustHaveExpertiseCodes.split(',').filter(code => code !== item.expertiseAreaCode).join(',') : '';
  }

  removeSelectedLanguage(item: any, event: Event): void {
    event.stopPropagation(); // Prevent parent clicks from interfering
    this.role.selectedLanguages = this.role.selectedLanguages.filter(language => language.id !== item.id);

    this.role.niceToHaveLanguageCodes = this.role.niceToHaveLanguageCodes ? this.role.niceToHaveLanguageCodes.split(',').filter(code => code !== item.id.toString()).join(',') : '';
    this.role.mustHaveLanguageCodes = this.role.mustHaveLanguageCodes ? this.role.mustHaveLanguageCodes.split(',').filter(code => code !== item.id.toString()).join(',') : '';
    this.emitRoleDetails();
  }

  isMustHave(expertiseAreaCode: string): boolean {
    // Ensure mustHaveExpertiseCodes is an array
    if (!this.role?.mustHaveExpertiseCodes) {
      return false;
    }

    const mustHaveCodesArray = Array.isArray(this.role.mustHaveExpertiseCodes)
      ? this.role.mustHaveExpertiseCodes
      : this.role.mustHaveExpertiseCodes.split(',');

    // Perform an exact match using a Set for efficiency
    const mustHaveSet = new Set(mustHaveCodesArray.map(code => code.trim()));
    return mustHaveSet.has(expertiseAreaCode);
  }

  closeTooltipLanguage() {
    this.activeToolTipLanguage = null;
    this.closeLanguageSelect();
    console.log(this.caseIntakeDetails)
  }

  closeTooltipExpertise() {
    this.activeToolTip = null;
    this.closeExpertiseSelect();
  }
}
