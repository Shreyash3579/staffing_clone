import { Component, OnInit, Input, OnChanges, EventEmitter, Output, ViewChild, ElementRef, SimpleChanges } from '@angular/core';
import { ProjectDetails } from 'src/app/shared/interfaces/projectDetails.interface';
import { ProjectType, ProjectTypeCssClass } from 'src/app/shared/constants/enumMaster';
import { DateService } from 'src/app/shared/dateService';
import { CaseOppChanges } from 'src/app/shared/interfaces/caseOppChanges.interface';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { BsDatepickerDirective } from 'ngx-bootstrap/datepicker';
import { ValidationService } from 'src/app/shared/validationService';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import * as moment from 'moment';
import { environment } from 'src/environments/environment';
import { BS_DEFAULT_CONFIG } from 'src/app/shared/constants/bsDatePickerConfig';
import { ThemePalette } from '@angular/material/core';
import { CoreService } from 'src/app/core/core.service';
import { staCommitmentDialogService } from '../../dialogHelperService/staCommitmentCaseOppDialog.service';

@Component({
  selector: 'app-project-header',
  templateUrl: './project-header.component.html',
  styleUrls: ['./project-header.component.scss']
})
export class ProjectHeaderComponent implements OnInit, OnChanges {

  // -----------------------Input Events--------------------------------------------//
  @ViewChild('startDateParentDiv', { static: false }) startDateParentDiv: ElementRef;
  @ViewChild('oppStartDate', { static: false }) oppStartDateElement: ElementRef;
  @ViewChild('oppEndDate', { static: false }) oppEndDateElement: ElementRef;
  @ViewChild('probabilityPercent', { static: false }) probabilityPercentElement: ElementRef;
  @ViewChild('startDatepicker', { static: false }) startDatepicker: BsDatepickerDirective;
  @ViewChild('endDatepicker', { static: false }) endDatepicker: BsDatepickerDirective;

  @Input() projectHeaderDetails: ProjectDetails;
  @Input('emailTo') activeResourcesEmailAddresses: string;
  @Input() isPinned: boolean;
  @Input() isHidden: boolean;
  @Input() isCaseOnRoll: boolean;
  @Input() isSTACommitmentCreated: boolean;
  @Input() caseRoleAllocations: [];

  // -----------------------Output Events -------------------------------------------//

  @Output() onTogglePin = new EventEmitter();
  @Output() onToggleHide = new EventEmitter();
  @Output() openCaseRollForm = new EventEmitter();
  @Output() openSTACommitmentForm = new EventEmitter();
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() updateProjectChanges = new EventEmitter();
  @Output() openPegRFPopUpEmitter = new EventEmitter();

  // -----------------------Local variables--------------------------------------------//
  showPlaceholder = true;
  showShareUrlIcon = true;
  shareUrl = '';
  caseIntakeFormURL = '';
  placeholderTrigerredByEvent = 'placeholderForm';
  projectTitle = '';
  projectTitleClass = '';
  caseAttributes : string [];
  editableCol = '';
  updatedProjectData: CaseOppChanges = {} as CaseOppChanges;
  materialSlideToggleColor: ThemePalette = 'accent';
  oppStartDateValidationObj = { isValid: true, showMessage: false, errorMessage: '' };
  oppEndDateValidationObj = { isValid: true, showMessage: false, errorMessage: '' };
  oppPercentValidationObj = { isValid: true, showMessage: false, errorMessage: '' };

  projectStartDate: Date;
  projectEndDate: Date;
  bsConfig: Partial<BsDatepickerConfig>;
  changedStartDate: string = null;
  changedEndDate: string = null;
  changedProbabilityPercentage: number = null;
  accessibleFeatures = ConstantsMaster.appScreens.feature;
  showRingFenceIcon = false;
  constructor(private coreService: CoreService,
    private staCommitmentDialogService: staCommitmentDialogService
  ) { }

  // ----------------------- Angular Life Cycle Hooks --------------------------------//
  ngOnInit() {

    this.bsConfig = BS_DEFAULT_CONFIG;
    this.bsConfig.containerClass = 'theme-red';    
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.projectHeaderDetails && Object.keys(changes.projectHeaderDetails.currentValue).length !== 0) {
      this.projectStartDate = this.projectHeaderDetails.startDate !== null ?
        new Date(this.projectHeaderDetails.startDate) : null;
      this.projectEndDate = this.projectHeaderDetails.endDate !== null ?
        new Date(this.projectHeaderDetails.endDate) : null;
      this.setProjectTitleAndClass();
      this.setCaseAttributes();
      this.getShareUrl(changes.projectHeaderDetails);
      this.loggedInUserHasAccessToSeePegRFPopUp();

    }
  }

  // -----------------Componenet Event Handlers -------------------------------------//

  getShareUrl(SimpleChanges) {
    this.shareUrl = SimpleChanges.currentValue.oldCaseCode ?
     environment.settings.environmentUrl + '/overlay?oldCaseCode=' + SimpleChanges.currentValue.oldCaseCode :
     environment.settings.environmentUrl + '/overlay?pipelineId=' + SimpleChanges.currentValue.pipelineId;
     this.caseIntakeFormURL = SimpleChanges.currentValue.oldCaseCode ?
     environment.settings.environmentUrl + '/intakeForm?oldCaseCode=' + SimpleChanges.currentValue.oldCaseCode :
     environment.settings.environmentUrl + '/intakeForm?opportunityId=' + SimpleChanges.currentValue.pipelineId;
  }
  
  loggedInUserHasAccessToSeePegRFPopUp() {
    this.showRingFenceIcon = this.coreService.loggedInUserClaims.PegC2CAccess && !!this.projectHeaderDetails.pegOpportunityId;
}

  editProbabilityStartDate(event) {
    // fix fo preventing multiple calls when user keeps clicking inside the same text box
    if (this.editableCol === 'probabilityStartDate') {
      return;
    }
    const existingClasses = this.startDateParentDiv.nativeElement.className;
    this.startDateParentDiv.nativeElement.className = existingClasses + ' active';
    // Manually set style for element because display set to /none/ while using tab functionality
    this.oppStartDateElement.nativeElement.style.display = 'block';
    this.editableCol = 'probabilityStartDate';
    setTimeout(() => {
      this.endDatepicker.hide();
      this.startDatepicker.show();
      this.oppStartDateElement.nativeElement.select();
      this.oppStartDateElement.nativeElement.focus();
    }, 200);
  }

  editProbabilityEndDate(event) {
    // fix fo preventing multiple calls when user keeps clicking inside the same text box
    if (this.editableCol === 'probabilityEndDate') {
      return;
    }

    // Manually set style for element because display set to /none/ while using tab functionality
    this.oppEndDateElement.nativeElement.style.display = 'block';
    this.editableCol = 'probabilityEndDate';
    setTimeout(() => {
      this.startDatepicker.hide();
      this.endDatepicker.show();
      this.oppEndDateElement.nativeElement.select();
      this.oppEndDateElement.nativeElement.focus();
    }, 200);
  }

  editProbabilityPercent(event) {
    // fix fo preventing multiple calls when user keeps clicking inside the same text box
    if (this.editableCol === 'probabilityPercent') {
      return;
    }

    // Manually set style for element because display set to /none/ while using tab functionality
    this.probabilityPercentElement.nativeElement.style.display = 'block';
    this.editableCol = 'probabilityPercent';

    setTimeout(() => {
      this.probabilityPercentElement.nativeElement.select();
      this.probabilityPercentElement.nativeElement.focus();
    });
  }

  onTabbingFromStartDate(event) {
    this.editableCol = '';
    this.oppStartDateElement.nativeElement.style.display = 'none';
    this.startDatepicker.hide();
  }

  onTabbingFromEndDate(event) {
    this.editableCol = '';
    this.oppEndDateElement.nativeElement.style.display = 'none';
    this.endDatepicker.hide();
  }

  disableOppStartDateEdit(event) {
    const _self = this;
    setTimeout(() => {
      if (event.relatedTarget && event.relatedTarget.type === 'button') {
        event.preventDefault();
        return false;
      } else {
        _self.startDateParentDiv.nativeElement.classList.remove('active');
        _self.editableCol = '';
        _self.oppStartDateElement.nativeElement.style.display = 'none';
        _self.startDatepicker.hide();
        _self.endDatepicker.hide();
      }
    }, 200); // DO NOT decrease the time. We need datepicker change to fire first and then disable to occur
  }

  disableOppEndDateEdit(event) {
    const _self = this;
    setTimeout(() => {
      if (event.relatedTarget && event.relatedTarget.type === 'button') {
        event.preventDefault();
        return false;
      } else {
        _self.editableCol = '';
        _self.oppEndDateElement.nativeElement.style.display = 'none';
        _self.endDatepicker.hide();
        _self.startDatepicker.hide();
      }
    }, 200); // DO NOT decrease the time. We need datepicker change to fire first and then disable to occur
  }

  disableEndDateEdit(event) {
    this.editableCol = '';
    this.startDatepicker.hide();
  }

  disableEdit(event) {
    this.editableCol = '';
    if (event.currentTarget.style.display !== 'none') {
      event.currentTarget.style.display = 'none';
    }
  }

  hideValidationMessage(target, event) {
    if (target === 'probabilityPercent') {
      this.oppPercentValidationObj.showMessage = false;
    } else if (target === 'probabilityStartDate') {
      this.oppStartDateValidationObj.showMessage = false;
    } else if (target === 'probabilityEndDate') {
      this.oppEndDateValidationObj.showMessage = false;
    }
    this.editableCol = '';
    event.stopPropagation();
  }

  validateInput() {
    if (this.oppStartDateValidationObj.isValid && this.oppEndDateValidationObj.isValid && this.oppPercentValidationObj.isValid) {
      return true;
    } else {
      return false;
    }

  }

  onOppStartDateChange(changedDate) {
    const validationObj = ValidationService.validateDate(changedDate);
    if (!validationObj.isValid) {
      this.projectHeaderDetails.startDate = '';
      this.oppStartDateValidationObj = { showMessage: true, ...validationObj };
    } else {
      this.projectHeaderDetails.startDate = DateService.convertDateInBainFormat(changedDate);
      this.changedStartDate = DateService.convertDateInBainFormat(changedDate);
      this.oppStartDateValidationObj = { showMessage: false, ...validationObj };
      if (this.validateStartDate() && this.validateEndDate()) {
        this.changedEndDate = DateService.convertDateInBainFormat(this.projectHeaderDetails.endDate);
        this.updateProject();
      }
      // This is done to close editing of input as it remained open when you changed date afer correcting an error in input
      this.disableOppStartDateEdit('');

    }
  }

  onOppEndDateChange(changedDate) {
    const validationObj = ValidationService.validateDate(changedDate);
    if (!validationObj.isValid) {
      this.projectHeaderDetails.endDate = '';
      this.oppEndDateValidationObj = { showMessage: true, ...validationObj };
    } else {
      this.projectHeaderDetails.endDate = DateService.convertDateInBainFormat(changedDate);
      this.changedEndDate = DateService.convertDateInBainFormat(changedDate);
      this.oppEndDateValidationObj = { showMessage: false, ...validationObj };
      if (this.validateEndDate() && this.validateStartDate()) {
        this.changedStartDate = DateService.convertDateInBainFormat(this.projectHeaderDetails.startDate);
        this.updateProject();
      }
      // This is done to close editing of input as it remained open when you changed date afer correcting an error in input
      this.disableOppEndDateEdit('');
    }
  }

  onOppPercentChange(event) {
    const validationObj = ValidationService.validateProbablePercentage(this.projectHeaderDetails.probabilityPercent);
    if (!validationObj.isValid) {
      this.oppPercentValidationObj = { showMessage: true, ...validationObj };
    } else {
      this.oppPercentValidationObj = { showMessage: false, ...validationObj };
      this.changedProbabilityPercentage = this.projectHeaderDetails.probabilityPercent;
      this.updateProject();
    }
  }

  updateProject() {
    if (this.validateInput()) {
      this.updatedProjectData.pipelineId = this.projectHeaderDetails.pipelineId;
      this.updatedProjectData.oldCaseCode = this.projectHeaderDetails.oldCaseCode;
      this.updatedProjectData.startDate = this.changedStartDate ?? this.projectHeaderDetails.startDate;
      this.updatedProjectData.endDate = this.changedEndDate ?? this.projectHeaderDetails.endDate;
      this.updatedProjectData.probabilityPercent = this.changedProbabilityPercentage;
      this.updatedProjectData.notes = this.projectHeaderDetails.notes;
      this.updatedProjectData.caseServedByRingfence = this.projectHeaderDetails.caseServedByRingfence;
      this.updateProjectChanges.emit(this.updatedProjectData);
    } else {
      this.oppStartDateValidationObj.showMessage = !!this.oppStartDateValidationObj.errorMessage;
      this.oppEndDateValidationObj.showMessage = !!this.oppEndDateValidationObj.errorMessage;
      this.oppPercentValidationObj.showMessage = !!this.oppPercentValidationObj.errorMessage;
    }
  }

  toggleCaseServedByRF(caseServedByRF) {
    this.projectHeaderDetails.caseServedByRingfence = caseServedByRF.checked;
    this.updateProject();
  }

  // -------------------------Output Event Handlers ------------------------------------//

  onTogglePinHandler(event) {
    this.onTogglePin.emit(event);
  }

  onToggleHideHandler(event) {
    this.onToggleHide.emit(event);
  }

  onCaseRollHandler() {
    this.openCaseRollForm.emit();
  }

  openPlaceHolderFormhandler() {
    this.openPlaceholderForm.emit();
  }

  openPegRFPopUpHandler() {
    this.openPegRFPopUpEmitter.emit(this.projectHeaderDetails.pegOpportunityId);
    }


  shortTermAvailableCaseOppCommitmentEmitterHandler() {
    this.openSTACommitmentForm.emit();
  }
  /*-------------------------Local Functions ------------------------------------------*/

  setProjectTitleAndClass() {
    switch (this.projectHeaderDetails.type) {
      case ProjectType.Opportunity: {

        this.projectTitle = this.projectHeaderDetails.opportunityName;
        this.projectTitleClass = ProjectTypeCssClass.Opportunity;

        break;
      }
      case ProjectType.NewDemand: {

        this.projectTitle = this.projectHeaderDetails.caseName;
        this.projectTitleClass = ProjectTypeCssClass.NewDemand;

        break;
      }
      case ProjectType.ActiveCase: {

        this.projectTitle = this.projectHeaderDetails.caseName;
        this.projectTitleClass = ProjectTypeCssClass.ActiveCase;

        break;
      }
    }
  }

  setCaseAttributes(){
    if(this.projectHeaderDetails.caseAttributes?.length){
      this.caseAttributes = this.projectHeaderDetails.caseAttributes.split("|");
    }
  }
  private validateEndDate(): boolean {
    if (this.projectHeaderDetails.endDate === null || !(this.projectHeaderDetails.endDate.length > 0)) {
      this.oppEndDateValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateReqMsg
      };
      return false;
    }
    if (moment(this.projectHeaderDetails.endDate).isBefore(moment(this.projectHeaderDetails.startDate))) {
      this.oppEndDateValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateCompMsg
      };
      return false;
    }
    this.oppEndDateValidationObj = {
      isValid: true, showMessage: false, errorMessage: ''
    };
    return true;
  }

  private validateStartDate(): boolean {
    if (this.projectHeaderDetails.startDate === null || !(this.projectHeaderDetails.startDate.length > 0)) {
      this.oppStartDateValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateReqMsg
      };
      return false;
    }
    if (moment(this.projectHeaderDetails.startDate).isAfter(moment(this.projectHeaderDetails.endDate))) {
      this.oppStartDateValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.startDateCompMsg
      };
      return false;
    }
    this.oppStartDateValidationObj = {
      isValid: true, showMessage: false, errorMessage: ''
    };
    return true;
  }
}
