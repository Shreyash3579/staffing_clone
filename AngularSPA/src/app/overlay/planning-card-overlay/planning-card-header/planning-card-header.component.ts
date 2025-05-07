import { Component, OnInit, Input, Output, EventEmitter, ViewChild, ElementRef, SimpleChanges } from "@angular/core";
import { SystemconfirmationFormComponent } from "src/app/shared/systemconfirmation-form/systemconfirmation-form.component";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";

// Interfaces
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";
import { PlaceholderAllocation } from "src/app/shared/interfaces/placeholderAllocation.interface";

// Services
import { DateService } from "src/app/shared/dateService";
import { SearchCaseOppDialogService } from "../../dialogHelperService/searchCaseOppDialog.service";
import { SharePlanningCardDialogService } from "../../dialogHelperService/share-planning-card-dialog.service";
import { NotificationService } from "src/app/shared/notification.service";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { BsDatepickerConfig, BsDatepickerDirective } from "ngx-bootstrap/datepicker";
import { ValidationService } from "src/app/shared/validationService";
import { BS_DEFAULT_CONFIG } from "src/app/shared/constants/bsDatePickerConfig";
import * as moment from "moment";
import { CoreService } from "src/app/core/core.service";
import { environment } from "src/environments/environment";
import { staCommitmentDialogService } from "../../dialogHelperService/staCommitmentCaseOppDialog.service";

@Component({
    selector: "app-planning-card-header",
    templateUrl: "./planning-card-header.component.html",
    styleUrls: ["./planning-card-header.component.scss"],
    providers: [SearchCaseOppDialogService, SharePlanningCardDialogService]
})
export class PlanningCardHeaderComponent implements OnInit {

    @ViewChild('startDateParentDiv', { static: false }) startDateParentDiv: ElementRef;
    @ViewChild('pcStartDate', { static: false }) planningCardStartDateElement: ElementRef;
    @ViewChild('pcEndDate', { static: false }) planningCardEndDateElement: ElementRef;
    @ViewChild('startDatepicker', { static: false }) startDatepicker: BsDatepickerDirective;
    @ViewChild('endDatepicker', { static: false }) endDatepicker: BsDatepickerDirective;
    // -----------------------Input Events--------------------------------------------//

    @Input() planningCardDetails: PlanningCard;  
    @Input("emailTo") activeResourcesEmailAddresses: string;

    // // -----------------------Output Events -------------------------------------------//

    @Output() removePlanningCardEmitter = new EventEmitter();
    @Output() sharePlanningCardEmitter = new EventEmitter<PlanningCard>();
    @Output() openPlaceholderForm = new EventEmitter();
    @Output() openSTACommitmentFormHandler = new EventEmitter<any>();
    @Output() updatePlanningCardEmitter = new EventEmitter<any>();
    @Output() openPegRFPopUpEmitter = new EventEmitter();
    @Output() isPegPlanningCardEmitter = new EventEmitter<boolean>();
    // // -----------------------Local variables--------------------------------------------//

      bsModalRef: BsModalRef;
      public offices: any = [];
      public staffingTags: any = [];
      public sharedOffices: any = [];
      public sharedStaffingTags: any = [];
      public showRingFenceIcon = false;
      public isPegPlanningCard:boolean = false;
      caseIntakeFormURL = '';
      shareUrl = '';

      placeholderTrigerredByEvent = 'placeholderForm';
      planningCardStartDateValidationObj = { isValid: true, showMessage: false, errorMessage: '' };
      planningCardEndDateValidationObj = { isValid: true, showMessage: false, errorMessage: '' };
      editableCol = '';
      planningCardStartDate: Date;
      planningCardEndDate: Date;
      changedStartDate: string = null;
      changedEndDate: string = null;
      bsConfig: Partial<BsDatepickerConfig>;
    constructor(
        private modalService: BsModalService,
        private sharePlanningCardDialogService: SharePlanningCardDialogService,
        private localStorageService:LocalStorageService,
        private coreService: CoreService, 
        private searchCaseOppDialogService: SearchCaseOppDialogService,
        private staCommitmentDialogService: staCommitmentDialogService,
    ) {}

    ngOnInit(): void {
        // this.projectResourceComponentElements = this.planningCardDetails.resourceComponent;
        this.bsConfig = BS_DEFAULT_CONFIG;
        this.bsConfig.containerClass = 'theme-red';
        this.offices = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
        this.staffingTags = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTags);
        this.getSharedOffices();
        this.getStaffingTags();
        this.loggedInUserHasAccessToSeePegRFPopUp();
    }

    ngOnChanges(changes:SimpleChanges) {
        if (changes.planningCardDetails && Object.keys(changes.planningCardDetails.currentValue).length !== 0) {
            this.planningCardStartDate = this.planningCardDetails.startDate !== null ?
              this.planningCardDetails.startDate : null;
            this.planningCardEndDate = this.planningCardDetails.endDate !== null ?
              this.planningCardDetails.endDate : null;
            
            this.getSharedOffices();
            this.getStaffingTags();   
            this.getShareUrl(changes.planningCardDetails); 
          }       
    }

    getShareUrl(SimpleChanges) {
      this.shareUrl = environment.settings.environmentUrl + '/overlay?planningCardId=' + SimpleChanges.currentValue.id;
      this.caseIntakeFormURL = environment.settings.environmentUrl + '/intakeForm?planningCardId=' + SimpleChanges.currentValue.id;
    }
    

    getSharedOffices() {
        if(this.planningCardDetails.sharedOfficeCodes){
            const sharedOfficeArray = this.planningCardDetails.sharedOfficeCodes.split(',').map(Number);
            this.sharedOffices = this.offices.filter(office => sharedOfficeArray.includes(office.officeCode));
        }
    }
    
    getStaffingTags() {
        if(this.planningCardDetails.sharedStaffingTags){
            const sharedStaffingTagsArray = this.planningCardDetails.sharedStaffingTags.split(',');
            this.sharedStaffingTags = this.staffingTags.filter(tag => sharedStaffingTagsArray.includes(tag.serviceLineCode));
        }
    }

    openPlaceHolderFormhandler() {
        this.openPlaceholderForm.emit();
    }

    shortTermAvailableCaseOppCommitmentEmitterHandler() {
      //this.staCommitmentDialogService.openSTACommitmentFormHandler({ project: this.planningCardDetails });
      this.openSTACommitmentFormHandler.emit();
    }

    // // -------------------------Output Event Handlers ------------------------------------//

    onAddPlaceHolderHandler() {
        // const placeholder: PlaceholderAllocation = {
        //     id: null,
        //     planningCardId: this.planningCardDetails.id,
        //     oldCaseCode: null,
        //     caseName: null,
        //     clientName: null,
        //     pipelineId: null,
        //     opportunityName: null,
        //     employeeCode: null,
        //     employeeName: null,
        //     operatingOfficeCode: null,
        //     operatingOfficeAbbreviation: null,
        //     currentLevelGrade: null,
        //     serviceLineCode: null,
        //     serviceLineName: null,
        //     allocation: null,
        //     startDate: null,
        //     endDate: null,
        //     isPlaceholderAllocation: true,
        //     investmentCode: null,
        //     investmentName: null,
        //     caseRoleCode: null,
        //     caseTypeCode: null,
        //     caseStartDate: this.planningCardDetails.startDate
        //         ? DateService.convertDateInBainFormat(this.planningCardDetails.startDate)
        //         : null,
        //     caseEndDate: this.planningCardDetails.endDate
        //         ? DateService.convertDateInBainFormat(this.planningCardDetails.endDate)
        //         : null,
        //     opportunityStartDate: null,
        //     opportunityEndDate: null,
        //     lastUpdatedBy: null
        // };
        // this.upsertPlaceholderEmitter.emit(placeholder);
    }

    toggleMergeDialogHandler() {
        let isValidAllocation = true;
        // planningCardAllocations?.every((item, index) => {
        //     if (item.employeeCode !== null) {
        //         isValidAllocation = this.projectResourceComponentElements
        //             .toArray()
        //             [index]?.validateInputForPlaceholder(item);
        //         if (!isValidAllocation) {
        //             return false;
        //         }
        //     }
        //     return true;
        // });

        // if (!isValidAllocation) {
        //     return false;
        // }

        if (this.planningCardDetails.pegOpportunityId) {
            var initialConfig = {
                showMergeAndCopy: false,
                searchCases: true,
                searchOpps: false
            };

            this.searchCaseOppDialogService.openSearchCaseOppDialogHandler(this.planningCardDetails,initialConfig);
        } else {
            this.searchCaseOppDialogService.openSearchCaseOppDialogHandler(this.planningCardDetails);
        }
    }

    deletePlanningCardHandler() {
        const confirmationPopUpBodyMessage = !this.planningCardDetails.isShared
            ? `You are about to delete planning card.
        This will delete all allocations associated to it. Are you sure you want to delete ?`
            : `This planning card had been shared and will be deleted for all the shared resources.
        Continue?`;
        this.openSystemConfirmationPlanningCardHandler({
            planningCardId: this.planningCardDetails.id,
            confirmationPopUpBodyMessage: confirmationPopUpBodyMessage
        });
    }

    openSystemConfirmationPlanningCardHandler(event) {
        const config = {
            class: "modal-dialog-centered",
            ignoreBackdropClick: true,
            initialState: {
                confirmationPopUpBodyMessage: event.confirmationPopUpBodyMessage
            }
        };

        this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);

        this.bsModalRef.content.deleteResourceNote.subscribe(() => {
            this.removePlanningCardAndItsAllocation(this.planningCardDetails.id);
        });
    }

    removePlanningCardAndItsAllocation(id) {
        this.removePlanningCardEmitter.emit({ id: id });
    }

    sharePlanningCardHandler() {
        this.sharePlanningCardEmitter.emit(this.planningCardDetails);
    }
    
    editPlanningCardStartDate(event) {
      // fix fo preventing multiple calls when user keeps clicking inside the same text box
      if (this.editableCol === 'planningCardStartDate') {
        return;
      }
      const existingClasses = this.startDateParentDiv.nativeElement.className;
      this.startDateParentDiv.nativeElement.className = existingClasses + ' active';
      // Manually set style for element because display set to /none/ while using tab functionality
      this.planningCardStartDateElement.nativeElement.style.display = 'block';
      this.editableCol = 'planningCardStartDate';
      setTimeout(() => {
        this.endDatepicker.hide();
        this.startDatepicker.show();
        this.planningCardStartDateElement.nativeElement.select();
        this.planningCardStartDateElement.nativeElement.focus();
      }, 200);
    }

    editPlanningCardEndDate(event) {
      // fix fo preventing multiple calls when user keeps clicking inside the same text box
      if (this.editableCol === 'planningCardEndDate') {
        return;
      }
  
      // Manually set style for element because display set to /none/ while using tab functionality
      this.planningCardEndDateElement.nativeElement.style.display = 'block';
      this.editableCol = 'planningCardEndDate';
      setTimeout(() => {
        this.startDatepicker.hide();
        this.endDatepicker.show();
        this.planningCardEndDateElement.nativeElement.select();
        this.planningCardEndDateElement.nativeElement.focus();
      }, 200);
    }

    onTabbingFromStartDate(event) {
      this.editableCol = '';
      this.planningCardStartDateElement.nativeElement.style.display = 'none';
      this.startDatepicker.hide();
    }

    onTabbingFromEndDate(event) {
      this.editableCol = '';
      this.planningCardEndDateElement.nativeElement.style.display = 'none';
      this.endDatepicker.hide();
    }

    disablePlanningCardStartDateEdit(event) {
        const _self = this;
        setTimeout(() => {
          if (event.relatedTarget && event.relatedTarget.type === 'button') {
            event.preventDefault();
            return false;
          } else {
            _self.startDateParentDiv.nativeElement.classList.remove('active');
            _self.editableCol = '';
            _self.planningCardStartDateElement.nativeElement.style.display = 'none';
            _self.startDatepicker.hide();
            _self.endDatepicker.hide();
          }
        }, 200); // DO NOT decrease the time. We need datepicker change to fire first and then disable to occur
    }

    disablePlanningCardEndDateEdit(event) {
      const _self = this;
      setTimeout(() => {
        if (event.relatedTarget && event.relatedTarget.type === 'button') {
          event.preventDefault();
          return false;
        } else {
          _self.editableCol = '';
          _self.planningCardEndDateElement.nativeElement.style.display = 'none';
          _self.endDatepicker.hide();
          _self.startDatepicker.hide();
        }
      }, 200); // DO NOT decrease the time. We need datepicker change to fire first and then disable to occur
    }

    updatePlanningCardName(selectedDateRange) {
      if (selectedDateRange.target.value.length < 1) {
        return false;
      }
      this.updatePlanningCard();
    }  

    onPlanningCardStartDateChange(changedDate) {
        const validationObj = ValidationService.validateDate(changedDate);
        if (!validationObj.isValid) {
          this.planningCardDetails.startDate = null;
          this.planningCardStartDateValidationObj = { showMessage: true, ...validationObj };
        } else {
          this.planningCardDetails.startDate = DateService.convertDateInBainFormat(changedDate);
          this.changedStartDate = DateService.convertDateInBainFormat(changedDate);
          this.planningCardStartDateValidationObj = { showMessage: false, ...validationObj };
          if (this.validateStartDate() && this.validateEndDate()) {
            this.changedEndDate = DateService.convertDateInBainFormat(this.planningCardDetails.endDate);
            this.updatePlanningCard();
          }
          //This is done to close editing of input as it remained open when you changed date afer correcting an error in input
          this.disablePlanningCardStartDateEdit('');
    
        }
      }

      onPlanningCardEndDateChange(changedDate) {
        const validationObj = ValidationService.validateDate(changedDate);
        if (!validationObj.isValid) {
          this.planningCardDetails.endDate = null;
          this.planningCardEndDateValidationObj = { showMessage: true, ...validationObj };
        } else {
          this.planningCardDetails.endDate = DateService.convertDateInBainFormat(changedDate);
          this.changedEndDate = DateService.convertDateInBainFormat(changedDate);
          this.planningCardEndDateValidationObj = { showMessage: false, ...validationObj };
          if (this.validateEndDate() && this.validateStartDate()) {
            this.changedStartDate = DateService.convertDateInBainFormat(this.planningCardDetails.startDate);
            this.updatePlanningCard();
          }
          // This is done to close editing of input as it remained open when you changed date afer correcting an error in input
          this.disablePlanningCardEndDateEdit('');
        }
      }

      updatePlanningCard() {
      if(this.validateInput()){
        const updatedPlanningCard: PlanningCard = {
          id: this.planningCardDetails.id,
          name: this.planningCardDetails.name,
          startDate: DateService.convertDateInBainFormat(this.planningCardDetails.startDate),
          endDate: DateService.convertDateInBainFormat(this.planningCardDetails.endDate),
          sharedOfficeCodes: this.planningCardDetails.sharedOfficeCodes,
          sharedStaffingTags: this.planningCardDetails.sharedStaffingTags,
          isShared: this.planningCardDetails.isShared,
          probabilityPercent: this.planningCardDetails.probabilityPercent,
          includeInCapacityReporting: this.planningCardDetails.includeInCapacityReporting
        }
        this.updatePlanningCardEmitter.emit(updatedPlanningCard);
      }
      else{
        this.planningCardStartDateValidationObj.showMessage = !!this.planningCardStartDateValidationObj.errorMessage;
        this.planningCardEndDateValidationObj.showMessage = !!this.planningCardEndDateValidationObj.errorMessage;
      }
      }

      private validateStartDate(): boolean {
        if (this.planningCardDetails.startDate === null ) {
          this.planningCardStartDateValidationObj = {
            isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateReqMsg
          };
          return false;
        }
        if (moment(this.planningCardDetails.startDate).isAfter(moment(this.planningCardDetails.endDate))) {
          this.planningCardStartDateValidationObj = {
            isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.startDateCompMsg
          };
          return false;
        }
        this.planningCardStartDateValidationObj = {
          isValid: true, showMessage: false, errorMessage: ''
        };
        return true;
      }

      validateInput() {
        if (this.planningCardStartDateValidationObj.isValid && this.planningCardEndDateValidationObj.isValid ) {
          return true;
        } else {
          return false;
        }
    
      }

      private validateEndDate(): boolean {
        if (this.planningCardDetails.endDate === null ) {
          this.planningCardEndDateValidationObj = {
            isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateReqMsg
          };
          return false;
        }
        if (moment(this.planningCardDetails.endDate).isBefore(moment(this.planningCardDetails.startDate))) {
          this.planningCardEndDateValidationObj = {
            isValid: false, showMessage: true, errorMessage: ConstantsMaster.opportunityConstants.validationMsgs.endDateCompMsg
          };
          return false;
        }
        this.planningCardEndDateValidationObj = {
          isValid: true, showMessage: false, errorMessage: ''
        };
        return true;
      }

      hideValidationMessage(target, event) {
       if (target === 'planningCardStartDate') {
          this.planningCardStartDateValidationObj.showMessage = false;
        } else if (target === 'planningCardEndDate') {
          this.planningCardEndDateValidationObj.showMessage = false;
        }
        this.editableCol = '';
        event.stopPropagation();
      }

    loggedInUserHasAccessToSeePegRFPopUp() {
        this.isPegPlanningCard = !!this.planningCardDetails.pegOpportunityId;
        this.showRingFenceIcon = this.coreService.loggedInUserClaims.PegC2CAccess && !!this.planningCardDetails.pegOpportunityId;
        this.isPegPlanningCardEmitter.emit(this.isPegPlanningCard);
    }

    openPegRFPopUpHandler() {
        this.openPegRFPopUpEmitter.emit(this.planningCardDetails.pegOpportunityId);
    }
}
