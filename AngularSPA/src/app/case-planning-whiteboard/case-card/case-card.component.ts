// -------------------Angular References---------------------------------------//
import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
// --------------------------External library/Utils -----------------------------------------//
import { DateService } from 'src/app/shared/dateService';
import { BS_DEFAULT_CONFIG } from 'src/app/shared/constants/bsDatePickerConfig';
import { ValidationService } from 'src/app/shared/validationService';
import { CaseOppChanges } from 'src/app/shared/interfaces/caseOppChanges.interface';
import { ProjectType } from 'src/app/shared/constants/enumMaster';
import * as moment from 'moment';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { SystemconfirmationFormComponent } from 'src/app/shared/systemconfirmation-form/systemconfirmation-form.component';
import { CoreService } from 'src/app/core/core.service';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { ResourceOrCasePlanningViewNote } from 'src/app/shared/interfaces/resource-or-case-planning-view-note.interface';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';

@Component({
  selector: 'case-planning-whiteboard-case-card',
  templateUrl: './case-card.component.html',
  styleUrls: ['./case-card.component.scss']
})
export class CaseCardComponent implements OnInit, OnDestroy {

  // -----------------------Local Variables--------------------------------------------//
  public isExpanded = false;
  public skuTerm;
  public projectNameWithPriority = '';
  public xCordinateForContextMenu: Number;
  public yCordinateForContextMenu: Number;
  public contextMenuOptionsTemplate = { text: null, value: null, action: null };
  bsConfig: Partial<BsDatepickerConfig>;
  elementsTobeUpdated: any;
  public bsModalRef: BsModalRef;
  showPegRFIcon = false;
  public caseNotes: ResourceOrCasePlanningViewNote[] = [];
  public caseNoteToUpsert: ResourceOrCasePlanningViewNote[] = [];
  public caseNoteToDelete: ResourceOrCasePlanningViewNote[] = [];
  _latestCaseNote: ResourceOrCasePlanningViewNote;
  
  startDateValidationObj = { isValid: true, showMessage: false, errorMessage: '' };
  endDateValidationObj = { isValid: true, showMessage: false, errorMessage: '' };
  // -----------------------Input Events--------------------------------------------//

  @Input() project;
  @Input() planningBoard: any[];
  @Input() contextMenuOptions = [];
  @Input()
  set latestCaseNote(value: ResourceOrCasePlanningViewNote) {
    this._latestCaseNote = value;
    if (value && this.isProjectsSame(this.project, value)) {
      this.updateProjectWithLatestCasePlanningBoardViewNote(value);
    }
  }
  get latestCaseNote() {
    return this._latestCaseNote;
  }

  /**
   * Checks whether or not to show the "Include in demand" checkbox
   * depending on which bucket the case card is in.
   * Is true if card is in "Action needed", false for anything else
  */
  @Input() showIncludeInDemandCheckbox: boolean;

  // -----------------------Output Events--------------------------------------------//
  @Output() contextMenuClick = new EventEmitter<any>();
  @Output() updateProjectChanges = new EventEmitter();
  @Output() updatePlanningCardEmitter = new EventEmitter();
  @Output() openAddTeamSkuForm = new EventEmitter();
  @Output() openPegRFPopUpEmitter = new EventEmitter();
  @Output() openNotesModalEmmiter = new EventEmitter(); 
  @Output() includeProjectInDemandEmitter = new EventEmitter();

  constructor(private modalService: BsModalService,
    private coreService: CoreService) { }

  ngOnInit(): void {
    this.bsConfig = BS_DEFAULT_CONFIG;
    this.bsConfig.containerClass = 'theme-red calendar-pop-up calendar-align-planning-board';

    if (this.project.startDate) {
      this.project.startDate = new Date(this.project.startDate);
    }
    if (this.project.endDate) {
      this.project.endDate = new Date(this.project.endDate);
    }

    if (!this.project.type) {
      this.project.type = ProjectType.PlanningCard;
    }

    this.projectNameWithPriority = this.project.clientName || this.project.name;
    if (this.project.clientPriority) {
      this.projectNameWithPriority += ` (${this.project.clientPriority})`;
    }

    this.project.startDateOverriden = false;
    this.loggedInUserHasAccessToSeePegRFPopUp();
  }

  // Toggle New Note Modal
  toggleNoteModal() {
    const cardObject = {
      name: this.projectNameWithPriority,
      data: this.project,
      casePlanningNotes: this.caseNotes,
    };

    this.openNotesModalEmmiter.emit(cardObject)
  }

  toggleCardExpansion() {
    this.isExpanded = !this.isExpanded
  }

  updateStartDate(selectedDate) {
    this.project.isStartDateUpdatedInBOSS = this.project.type !== ProjectType.PlanningCard;
    this.project.startDate = DateService.convertDateInBainFormat(selectedDate);

    this.updateProject();
  }

  resetStartDate(selectedDate, isStartDateUpdatedInBOSSBeforeChange: boolean) {
    this.project.isStartDateUpdatedInBOSS = isStartDateUpdatedInBOSSBeforeChange;
    this.project.startDate = DateService.convertDateInBainFormat(selectedDate);
  }

  onDateChange(selectedDate: Date, elementName: string) {
    switch (elementName) {
      case 'startDate': {

        const validationObj = ValidationService.validateDate(selectedDate);
        if (!validationObj.isValid) {
          this.project.startDate = '';
          this.startDateValidationObj = { showMessage: true, ...validationObj };

        }
        else {

          let startDateBeforeChange = this.project.startDate;
          let isStartDateUpdatedInBOSSBeforeChange = this.project.isStartDateUpdatedInBOSS;
          this.project.startDate = selectedDate;// DateService.convertDateInBainFormat(selectedDate);
          this.project.isStartDateUpdatedInBOSS = this.project.type !== ProjectType.PlanningCard;

          if (this.validateStartDate() && this.validateEndDate()) {
            // NOTE : Commenting this logic as now we have resolved the bug due to which we were not able to fetch the projects that had start date outside the selected date range
            // if (this.isDateOutsidePlanningWindow(selectedDate)) {
            //   this.getDatesOutsideWindowConfirmation(selectedDate, startDateBeforeChange, isStartDateUpdatedInBOSSBeforeChange);
            // }
            // else {
            this.updateStartDate(selectedDate);
            // }
          }

        }

        break;
      }
      case 'endDate': {

        const validationObj = ValidationService.validateDate(selectedDate);
        if (!validationObj.isValid) {
          this.project.endDate = '';
          this.endDateValidationObj = { showMessage: true, ...validationObj };

        } else {
          this.project.endDate = DateService.convertDateInBainFormat(selectedDate);
          this.project.isEndDateUpdatedInBOSS = this.project.type !== ProjectType.PlanningCard;

          if (this.validateEndDate() && this.validateStartDate()) {

            this.project.endDate = DateService.convertDateInBainFormat(selectedDate);

            this.updateProject();
          }

        }

        break;
      }
    }
  }

  toggleIncludeInDemand() {
    this.project.includeInDemand = !this.project.includeInDemand;
    this.includeProjectInDemandEmitter.emit(this.project);
  }
  
  updateProject() {
    if (!this.validateDates()) {
      this.startDateValidationObj.showMessage = !!this.startDateValidationObj.errorMessage;
      this.endDateValidationObj.showMessage = !!this.endDateValidationObj.errorMessage;

      return;
    }

    if (this.project.type === ProjectType.PlanningCard) {

      let updatedPlanningCard: PlanningCard = {
        id: this.project.planningCardId,
        name: this.project.name,
        startDate: DateService.convertDateInBainFormat(this.project.startDate),
        endDate: DateService.convertDateInBainFormat(this.project.endDate),
        sharedOfficeCodes: this.project.sharedOfficeCodes,
        sharedStaffingTags: this.project.sharedStaffingTags,
        isShared: this.project.isShared
      }

      this.updatePlanningCardEmitter.emit(updatedPlanningCard);

    } else {

      let updatedProjectData: CaseOppChanges = {
        pipelineId: this.project.pipelineId,
        oldCaseCode: this.project.oldCaseCode,
        startDate: DateService.convertDateInBainFormat(this.project.startDate),
        endDate: DateService.convertDateInBainFormat(this.project.endDate),
        probabilityPercent: this.project.probabilityPercent,
        notes: this.project.notes,
        caseServedByRingfence: this.project.caseServedByRingfence,
        staffingOfficeCode: this.project.staffingOfficeCode
      }

      this.updateProjectChanges.emit(updatedProjectData);
    }
  }

  onRightClick() {
    // NOTE: The changes in the z-index of this class were done to open context menu on case-planning board as it was opening behind the board
    this.elementsTobeUpdated = document.querySelectorAll(".cdk-overlay-container");
    if (this.elementsTobeUpdated && this.elementsTobeUpdated.length > 0) {
      this.elementsTobeUpdated.forEach(element => {
        element.style.zIndex = '1060';
      });
    }
  }

  hideErrorMessage(elementName) {
    switch (elementName) {
      case 'startDate': {
        this.startDateValidationObj.showMessage = false;
        break;
      }
      case 'endDate': {
        this.endDateValidationObj.showMessage = false;
        break;
      }
    }
  }

  //-----------------------Context Menu Events-----------------------------------//

  contextMenuHandler(option, project) {
    if (project) {
      this.contextMenuClick.emit({ project: project, week: option });
    }
  }


  //-------------------------Validations --------------------------------------//

  validateDates() {
    return this.startDateValidationObj.isValid && this.endDateValidationObj.isValid;
  }

  private validateEndDate(): boolean {
    // if (this.project.endDate === null || !(this.project.endDate.length > 0)) {
    //   this.endDateValidationObj = {
    //     isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateReqMsg
    //   };
    //   return false;
    // }
    if (moment(this.project.endDate).isBefore(moment(this.project.startDate))) {
      this.endDateValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateCompMsg
      };
      return false;
    }
    this.endDateValidationObj = {
      isValid: true, showMessage: false, errorMessage: ''
    };
    return true;
  }

  private validateStartDate(): boolean {
    // if (this.project.startDate === null || !(this.project.startDate.length > 0)) {
    //   this.startDateValidationObj = {
    //     isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateReqMsg
    //   };
    //   return false;
    // }
    if (moment(this.project.startDate).isAfter(moment(this.project.endDate))) {
      this.startDateValidationObj = {
        isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.startDateCompMsg
      };
      return false;
    }
    this.startDateValidationObj = {
      isValid: true, showMessage: false, errorMessage: ''
    };
    return true;
  }

  //------------------------- SKU Pop-up --------------------------------------//
  openAddTeamSkuFormHandler() {
    this.openAddTeamSkuForm.emit(this.project);
  }

  //-------------------------Confirmation Pop-up -----------------------------//
  getDatesOutsideWindowConfirmation(selectedDate: Date, dateBeforeChange, isStartDateUpdatedInBOSSBeforeChange: boolean) {
    const confirmationPopUpBodyMessage = 'You are changing the date of this card to fall outside of the case planning window. Confirming this change will remove the card from your view, do you want to proceed?';
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        confirmationPopUpBodyMessage: confirmationPopUpBodyMessage
      }
    };

    this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);

    this.bsModalRef.content.onActionResult.subscribe((actionResult) => {
      if (actionResult) {
        this.updateStartDate(selectedDate);
      } else {
        this.resetStartDate(dateBeforeChange, isStartDateUpdatedInBOSSBeforeChange);
      }
    });
  }

  isDateOutsidePlanningWindow(date: Date) {
    var startOfWeek = DateService.getStartOfWeek();

    let endof5weeks = new Date(startOfWeek);
    endof5weeks.setDate(endof5weeks.getDate() + 3 * 7 + 4); //need data till end of 4 weeks

    return !(moment(date).isBetween(startOfWeek, endof5weeks, 'day', '[]'));

  }

  loggedInUserHasAccessToSeePegRFPopUp() {
    this.showPegRFIcon = this.coreService.loggedInUserClaims.PegC2CAccess && !!this.project.pegOpportunityId;
  }

  openPegRFPopUpHandler() {
    this.openPegRFPopUpEmitter.emit(this.project.pegOpportunityId);
  }

  isProjectsSame(project1, project2) {
    return !!project1.oldCaseCode && project2.oldCaseCode === project1.oldCaseCode
      || !!project1.pipelineId && project2.pipelineId === project1.pipelineId
      || !!project1.planningCardId && project2.planningCardId === project1.planningCardId
  }

  updateProjectWithLatestCasePlanningBoardViewNote(value) {
    this.project.latestCasePlanningBoardViewNote = value;
  }

  // ---------------------------Destroy Event --------------------------

  ngOnDestroy(): void {
    // NOTE: We are reverting the changes in z-index on closing the popup so that it does not impact any other screen
    if (this.elementsTobeUpdated && this.elementsTobeUpdated.length > 0) {
      this.elementsTobeUpdated.forEach(element => {
        element.style.zIndex = '1000';
      });
    }
  }

}
