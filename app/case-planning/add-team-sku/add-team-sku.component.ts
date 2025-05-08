import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  ViewEncapsulation
} from "@angular/core";
import {
  FormGroup,
  FormBuilder,
  FormArray,
  Validators,
  ValidationErrors,
  FormControl
} from "@angular/forms";

import { BsModalRef } from "ngx-bootstrap/modal";
import { Observable } from "rxjs";
import { debounceTime, mergeMap } from "rxjs/operators";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { FieldsEnum } from "src/app/shared/constants/enumMaster";
import { DateService } from "src/app/shared/dateService";
import { CommitmentType } from "src/app/shared/interfaces/commitmentType.interface";
import { Office } from "src/app/shared/interfaces/office.interface";
import { PDGrade } from "src/app/shared/interfaces/pdGrade.interface";
import { PlaceholderAllocation } from "src/app/shared/interfaces/placeholderAllocation.interface";
import { Resource } from "src/app/shared/interfaces/resource.interface";
import { ServiceLine } from "src/app/shared/interfaces/serviceLine.interface";
import { SkuTerm } from "src/app/shared/interfaces/skuTerm.interface";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { ResourceAllocationService } from "src/app/shared/services/resourceAllocation.service";
import { SharedService } from "src/app/shared/shared.service";
import { NotificationService } from "src/app/shared/notification.service";
import { PositionGroup } from "src/app/shared/interfaces/position-group.interface";
import { CortexSkuMapping } from "src/app/shared/interfaces/cortex-sku-mapping.interface";
import { CaseOppCortexTeamSize } from "src/app/shared/interfaces/case-opp-cortex-team-size.interface";

@Component({
  selector: 'app-add-team-sku',
  templateUrl: './add-team-sku.component.html',
  styleUrls: ['./add-team-sku.component.scss']
})
export class AddTeamSkuComponent implements OnInit {

  // -----------------------Input Variables to Modal from outside the component -----------------//
  public selectedCase: any;
  public autoCalculate = false;
  public projectTitle = "";
  public skuTerms: SkuTerm[];
  public serviceLines: ServiceLine[];
  public offices: Office[];
  public pdGrades: PDGrade[];
  public ringfences: CommitmentType[] = [];
  private placeholderIdsToRemove = [];
  public selectedSkuTerms: SkuTerm[] = [];
  public positionGroups: PositionGroup[];
  public isCopyCortexHidden: boolean = true;

  form: FormGroup<any>;
  existingForm: FormGroup<any>;
  placeholderAllocations: PlaceholderAllocation[] = [];
  resourcesData: Resource[];
  asyncResourcesSearchString = "";
  isValid = true;
  errorMessages = [];
  showSkuDropdown = false;
  isCopyCortexOriginalyDisabled = false;
  isCopyCortexClickedByUser = false;



  // -----------------------Output Events -----------------//
  @Output() upsertPlaceholderAllocationsToProject = new EventEmitter<any>();
  @Output() deletePlaceHoldersByIds = new EventEmitter<any>();
  @Output() insertSKUCaseTerms = new EventEmitter<any>();
  @Output() updateSKUCaseTerms = new EventEmitter<any>();
  @Output() upsertPlaceholderCreatedForCortexPlaceholders = new EventEmitter<any>();

  constructor(
    public modalRef: BsModalRef,
    private fb: FormBuilder,
    private sharedService: SharedService,
    private _resourceAllocationService: ResourceAllocationService,
    private localStorageService: LocalStorageService,
    private notificationService: NotificationService
  ) {
    this.form = fb.group({
      existingPlaceholders: fb.array([]),
      newPlaceholders: fb.array([])
    });
  }

  ngOnInit() {

    this.loadLookUpDataFromLocalStorage();
    this.projectTitle =
      this.selectedCase.caseName ?? this.selectedCase.opportunityName;
    this.isCopyCortexOriginalyDisabled = this.selectedCase.isPlaceholderCreatedFromCortex;
    this.isCopyCortexClickedByUser = !this.selectedCase.isPlaceholderCreatedFromCortex;
    this.attachEventForResourcesSearch();
    this.loadExistingPlaceholdersFormArray();
    this.setSelectedSkuTermOnLoad();

  }

  // Copy Cortex
  copyCortexSKU(cortexToCopy: any) {
    this.isCopyCortexClickedByUser = true;
    const cortexSkuMappings: CortexSkuMapping[] = this.localStorageService.get(ConstantsMaster.localStorageKeys.cortexSkuMappings);
    const selectedCortexSkuMapping = cortexSkuMappings.find(x => x.cortexSKU === this.selectedCase.estimatedTeamSize)?.mappedStaffingSKU
    const selectedCortexSkuMappingParsed = !selectedCortexSkuMapping ? null : JSON.parse(selectedCortexSkuMapping);
    const entries = !selectedCortexSkuMappingParsed ? [] : Object.entries(selectedCortexSkuMappingParsed);
    const [startDate, endDate] = this._resourceAllocationService.getAllocationDates(this.selectedCase.startDate, this.selectedCase.endDate);

    if (!entries.length) {
      this.isCopyCortexClickedByUser = false;
      this.notificationService.showInfo('"Other" SKU not available for automated copy');
      return;
    }
    this.setFormDirty();

    for (var i = 0; i < entries.length; i++) {
      const levelGrade = entries[i][0];
      const placeholderCount = Number(entries[i][1]);
      for (var j = 0; j < placeholderCount; j++) {
        const placeholderAllocationData = this.createPlaceholderDataByLevelGrade(levelGrade);
        this.pushPlaceholderData(placeholderAllocationData, startDate, endDate);
      }
    }
  }

  createPlaceholderDataByLevelGrade(levelGrade) {
    let placeholderAllocation: PlaceholderAllocation = {
      id: null,
      planningCardId: this.selectedCase.planningCardId,
      caseName: this.selectedCase.caseName,
      caseTypeCode: this.selectedCase.caseTypeCode,
      clientName: this.selectedCase.clientName,
      oldCaseCode: this.selectedCase.oldCaseCode,
      employeeCode: null,
      employeeName: "",
      currentLevelGrade: levelGrade,
      commitmentTypeCode: null,
      serviceLineCode: ConstantsMaster.ServiceLine.GeneralConsulting,
      serviceLineName: "General Consulting",
      operatingOfficeCode: this.selectedCase.staffingOfficeCode ?? this.selectedCase?.billingOfficeCode,
      operatingOfficeAbbreviation: this.selectedCase.staffingOfficeAbbreviation ?? this.selectedCase.billingOfficeAbbreviation,
      pipelineId: this.selectedCase.pipelineId,
      opportunityName: this.selectedCase.opportunityName,
      investmentCode: null,
      investmentName: null,
      caseRoleCode: null,
      allocation: 100,
      startDate: this.selectedCase.startDate,
      endDate: this.selectedCase.endDate,
      lastUpdatedBy: null,
      isPlaceholderAllocation: true,
      isConfirmed: false
    };

    return placeholderAllocation;
  }

  closeForm() {
    this.modalRef.hide();
  }


  loadLookUpDataFromLocalStorage() {
    this.offices = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
    this.serviceLines = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLines);
    this.pdGrades = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGrades);
    this.ringfences = this.getRingfencesFromLocalStorage();
    this.skuTerms = this.localStorageService.get(ConstantsMaster.localStorageKeys.skuTermList);
    this.positionGroups = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsGroups);
  }

  getRingfencesFromLocalStorage() {
    const ringfences: CommitmentType[] = this.localStorageService.get(ConstantsMaster.localStorageKeys.ringfences);
    const dummyRingfence: CommitmentType = { commitmentTypeCode: null, commitmentTypeName: null, precedence: 0 }

    return [dummyRingfence, ...ringfences]

  }

  loadExistingPlaceholdersFormArray() {
    const existingPlaceholdersForm = this.form.get(
      "existingPlaceholders"
    ) as FormArray;

    this.selectedCase.placeholderAllocations?.forEach((x) => {
      const formControl = {
        id: x.id,
        employeeCode: x.employeeCode,
        employeeName: x.employeeName,
        serviceLineCode: [x.serviceLineCode, Validators.required],
        serviceLineName: x.serviceLineName,
        operatingOfficeCode: [x.operatingOfficeCode, Validators.required],
        operatingOfficeAbbreviation: x.operatingOfficeAbbreviation,
        currentLevelGrade: [x.currentLevelGrade, Validators.required],
        commitmentTypeCode: x.commitmentTypeCode,
        commitmentTypeName: x.commitmentTypeName,
        positionGroupCode: x.positionGroupCode,
        allocation: [
          x.allocation,
          [
            Validators.required,
            Validators.min(0),
            Validators.max(1000),
            Validators.pattern("^-?[0-9]*$")
          ]
        ],
        startDate: x.startDate,
        endDate: x.endDate
      };

      existingPlaceholdersForm.push(this.fb.group(formControl));
    });
  }

  setSelectedSkuTermOnLoad() {
    if (this.selectedCase.skuCaseTerms?.skuTerms) {
      const skuTermCodes = this.selectedCase.skuCaseTerms.skuTerms.map(
        (x) => x.code
      );
      this.selectedSkuTerms = this.skuTerms.filter((x) =>
        skuTermCodes.includes(x.code)
      );
    }

    // if(this.selectedSkuTerm && !this.selectedCase.placeholderAllocations?.length){
    //   this.onSkuTermChange();
    // }
  }

  onSkuAdd(skuData) {
    this.setFormDirty();

    //start and end dates would be case dates by default
    let [startDate, endDate] = this._resourceAllocationService.getAllocationDates(this.selectedCase.startDate, this.selectedCase.endDate);

    const allUnnamedPlaceholderControls = this.placeholdersArray.controls.filter(x => !x.value.employeeCode);
    const existingUnnamedPlaceholderControls = this.existingPlaceholdersArray.controls.filter(x => !x.value.employeeCode);
    var lastCell;

    if (allUnnamedPlaceholderControls?.length) {
      lastCell = allUnnamedPlaceholderControls[allUnnamedPlaceholderControls?.length - 1].value;
    }
    else if (this.existingPlaceholdersArray?.length) {
      lastCell = existingUnnamedPlaceholderControls[existingUnnamedPlaceholderControls?.length - 1]?.value;
    }
    else { }

    for (var i = 0; i < skuData.size; i++) {
      this.pushPlaceholderData(lastCell, startDate, endDate);
    }
  }

  pushPlaceholderData(placeholderData, startDate, endDate) {
    this.placeholdersArray.push(
      this.fb.group({
        id: null,
        employeeCode: placeholderData?.employeeCode,
        employeeName: placeholderData?.employeeName,
        serviceLineCode: [placeholderData?.serviceLineCode, Validators.required],
        serviceLineName: placeholderData?.serviceLineName,
        operatingOfficeCode: [placeholderData?.operatingOfficeCode, Validators.required],
        operatingOfficeAbbreviation: placeholderData?.operatingOfficeAbbreviation,
        currentLevelGrade: [placeholderData?.currentLevelGrade, Validators.required],
        commitmentTypeName: placeholderData?.commitmentTypeName,
        commitmentTypeCode: placeholderData?.commitmentTypeCode,
        positionGroupCode: placeholderData?.positionGroupCode,
        allocation: [
          placeholderData?.allocation ?? 100,
          [
            Validators.required,
            Validators.min(0),
            Validators.max(1000),
            Validators.pattern("^-?[0-9]*$")
          ]
        ],
        startDate: startDate,
        endDate: endDate
      })
    );
  }

  onSkuRemove(skuData) {
    this.setFormDirty();

    for (var i = 0; i < skuData.value.size; i++) {
      const emptyPlaceholderIndex = this.placeholdersArray.value?.findIndex(
        (x) =>
          !(x.serviceLineCode && x.operatingOfficeCode && x.currentLevelGrade)
      );

      if (emptyPlaceholderIndex > -1) {
        this.placeholdersArray.removeAt(emptyPlaceholderIndex);
      } else {
        return;
      }
    }
  }


  onAddNewResourcePlaceholder(data) {
    this.setFormDirty();
    let startDate, endDate;
    const newPlaceholdersData = this.placeholdersArray;
    const selectedResource: Resource = data.item;
    
    if (this.selectedCase.planningCardId &&  this.selectedCase.startDate && this.selectedCase.endDate) {
      startDate = DateService.convertDateInBainFormat(this.selectedCase.startDate);
      endDate = DateService.convertDateInBainFormat(this.selectedCase.endDate);
    } 
    else {
      [startDate, endDate] = this._resourceAllocationService.getAllocationDates(this.selectedCase.startDate, this.selectedCase.endDate);
    }
    const newPlaceholderResource = {
      id: null,
      employeeCode: [selectedResource.employeeCode, Validators.required],
      employeeName: selectedResource.fullName,
      serviceLineCode: [
        selectedResource.serviceLine.serviceLineCode,
        Validators.required
      ],
      serviceLineName: selectedResource.serviceLine.serviceLineName,
      operatingOfficeCode: [
        selectedResource.schedulingOffice.officeCode,
        Validators.required
      ],
      operatingOfficeAbbreviation:
        selectedResource.schedulingOffice.officeAbbreviation,
      currentLevelGrade: [selectedResource.levelGrade, Validators.required],
      allocation: [
        parseInt((selectedResource.fte * 100).toString()),
        [
          Validators.required,
          Validators.min(0),
          Validators.max(1000),
          Validators.pattern("^-?[0-9]*$")
        ]
      ],
      startDate: startDate,
      endDate: endDate
    };

    newPlaceholdersData.push(this.fb.group(newPlaceholderResource));

    this.asyncResourcesSearchString = "";
  }

  removeExistingPlaceholderAllocationFromProjectHandler(placeholderItem) {
    this.setFormDirty();

    const placeholderId = placeholderItem.value.id;

    if (placeholderId) {
      this.placeholderIdsToRemove.push(placeholderId);
      const existingPlaceholdersForm = this.form.get(
        "existingPlaceholders"
      ) as FormArray;
      existingPlaceholdersForm.removeAt(
        existingPlaceholdersForm.value.findIndex((x) => x.id == placeholderId)
      );
    }
  }

  removeNewPlaceholderAllocationFromProjectHandler(data, index) {
    this.setFormDirty();

    const newPlaceholdersForm = this.form.get("newPlaceholders") as FormArray;
    newPlaceholdersForm.removeAt(index);
  }

  isFormValid() {
    this.isValid = true;

    this.placeholdersArray.controls.forEach((element) => {
      if (element.invalid) {
        this.isValid = false;
        return false;
      }
    });

    if (this.isValid) {
      this.existingPlaceholdersArray.controls.forEach((element) => {
        if (element.invalid) {
          this.isValid = false;
          return false;
        }
      });
    }

    return this.isValid;
  }

  onSubmit() {
    if (!this.form.dirty) {
      this.modalRef.hide();
      return;
    }

    if (!this.isFormValid()) {
      this.getFormValidationErrors();
      return;
    }

    const newPlaceholdersData = this.placeholdersArray.value?.filter(
      (x) => x.serviceLineCode && x.currentLevelGrade && x.operatingOfficeCode
    );
    const existingPlaceholdersData = this.existingPlaceholdersArray.value;

    newPlaceholdersData.forEach((element) => {
      this.placeholderAllocations.push(
        this.populatePlacholderAllocationProperties(this.selectedCase, element)
      );
    });

    existingPlaceholdersData.forEach((element) => {
      this.placeholderAllocations.push(
        this.populatePlacholderAllocationProperties(this.selectedCase, element)
      );
    });

    //autoCacultate is used to denote new logic
    if (this.autoCalculate) {
      this.savePlaceholdersData();
    } else {
      this.saveSkuData();
    }

    this.modalRef.hide();
  }

  //-------------private helpers ----------------------

  get placeholdersArray(): FormArray {
    return this.form.get("newPlaceholders") as FormArray;
  }

  get existingPlaceholdersArray(): FormArray {
    return this.form.get("existingPlaceholders") as FormArray;
  }

  attachEventForResourcesSearch() {
    this.resourcesData = Observable.create((observer: any) => {
      observer.next(this.asyncResourcesSearchString);
    }).pipe(
      debounceTime(1000),
      mergeMap((token: string) =>
        this.sharedService.getResourcesBySearchString(token)
      )
    );
  }

  setFormDirty() {
    this.form.markAsDirty();
  }

  private populatePlacholderAllocationProperties(selectedCase, allocation) {
    let [startDate, endDate] =
      this._resourceAllocationService.getAllocationDates(
        selectedCase.startDate,
        selectedCase.endDate
      );

    let placeholderAllocation: PlaceholderAllocation = {
      id: allocation.id ?? null,
      planningCardId: selectedCase.planningCardId,
      caseName: selectedCase.caseName,
      clientName: selectedCase.clientName,
      oldCaseCode: selectedCase.oldCaseCode,
      employeeCode: allocation.employeeCode,
      employeeName: allocation.employeeName ?? "",
      currentLevelGrade: allocation.currentLevelGrade,
      serviceLineCode: allocation.serviceLineCode,
      serviceLineName:
        this.serviceLines.find(
          (x) => x.serviceLineCode === allocation.serviceLineCode
        )?.serviceLineName ?? "",
      operatingOfficeCode: allocation.operatingOfficeCode,
      operatingOfficeAbbreviation:
        this.offices.find(
          (x) => x.officeCode === allocation.operatingOfficeCode
        )?.officeAbbreviation ?? "",
      pipelineId: selectedCase.pipelineId,
      opportunityName: selectedCase?.opportunityName,
      investmentCode: null,
      investmentName: null,
      caseRoleCode: null,
      allocation: parseInt(allocation.allocation, 10),
      startDate: allocation.startDate, //DateService.getFormattedDate(selectedCase.startDate),
      endDate: allocation.endDate, //DateService.getFormattedDate(selectedCase.endDate),
      caseStartDate: allocation.oldCaseCode ? selectedCase.startDate : null,
      caseEndDate: allocation.oldCaseCode ? selectedCase.endDate : null,
      opportunityStartDate: !allocation.oldCaseCode
        ? selectedCase.startDate
        : null,
      opportunityEndDate: !allocation.oldCaseCode ? selectedCase.endDate : null,
      lastUpdatedBy: null,
      notes: null,
      isPlaceholderAllocation: true,
      commitmentTypeCode:
        this.ringfences.find(
          (x) => x.commitmentTypeCode === allocation.commitmentTypeCode
        )?.commitmentTypeCode ?? "",
      commitmentTypeName:
        this.ringfences.find(
          (x) => x.commitmentTypeName === allocation.commitmentTypeName
        )?.commitmentTypeName ?? "",
      positionGroupCode: allocation.positionGroupCode

    };

    return placeholderAllocation;
  }

  savePlaceholdersData() {
    if (this.placeholderIdsToRemove.length) {
      let placeholderDeletedMsg = !this.placeholderAllocations.length
        ? "All Placeholder's Deleted"
        : "";
      this.deletePlaceHoldersByIds.emit({
        placeholderIds: this.placeholderIdsToRemove.join(","),
        notifyMessage: placeholderDeletedMsg
      });
    }

    if (this.placeholderAllocations.length) {
      this.upsertPlaceholderAllocationsToProject.emit({
        placeholderAllocations: this.placeholderAllocations
      });
      this.upsertPlaceholderCreatedFromCortexSkusForCaseOpp();
    }
  }

  upsertPlaceholderCreatedFromCortexSkusForCaseOpp() {
    if ((!!this.selectedCase.oldCaseCode || !!this.selectedCase.pipelineId) && this.isCopyCortexClickedByUser) {
      const caseOppCortexTeamSize = this.convertToCaseOppCortexTeamSizeModel();
      this.upsertPlaceholderCreatedForCortexPlaceholders.emit(caseOppCortexTeamSize);
    }
  }

  convertToCaseOppCortexTeamSizeModel() {
    const caseOppCortexTeamSize: CaseOppCortexTeamSize = {
      oldCaseCode: this.selectedCase.oldCaseCode,
      pipelineId: this.selectedCase.pipelineId,
      cortexOpportunityId: this.selectedCase.cortexOpportunityId,
      estimatedTeamSize: this.selectedCase.estimatedTeamSize,
      isPlaceholderCreatedFromCortex: true,
      lastUpdatedBy: null
    };

    return caseOppCortexTeamSize;
  }

  saveSkuData() {
    //TODO: improve the logic
    if (this.selectedCase.skuCaseTerms?.skuTerms) {
      const existingSkuTermCodes = this.selectedCase.skuCaseTerms.skuTerms.map(
        (x) => x.code
      );

      if (
        !(
          this.selectedCase.skuCaseTerms.skuTerms.length ===
          this.selectedSkuTerms.length &&
          this.selectedSkuTerms.every((x) =>
            existingSkuTermCodes.includes(x.code)
          )
        )
      ) {
        //this.selectedCase.skuCaseTerms.skuTerms[0].code = this.selectedSkuTerms.code;
        const skuTab = {
          id: this.selectedCase.skuCaseTerms.id,
          oldCaseCode: this.selectedCase.skuCaseTerms.oldCaseCode,
          pipelineId: this.selectedCase.skuCaseTerms.pipelineId,
          effectiveDate: this.selectedCase.skuCaseTerms.effectiveDate,
          skuTermsCodes: this.selectedSkuTerms
            .map((caseTerm) => {
              return caseTerm.code;
            })
            .join(",")
        };

        const skuTabData = {
          skuTab: skuTab
        };

        this.updateSKUCaseTerms.emit(skuTabData);
      }
    } else {
      const skuTab = {
        id: null,
        oldCaseCode: this.selectedCase.oldCaseCode,
        pipelineId: this.selectedCase.pipelineId,
        effectiveDate: DateService.getBainFormattedToday(),
        skuTermsCodes: this.selectedSkuTerms.map((x) => x.code).join(",")
      };

      const skuTabData = {
        skuTab: skuTab
      };

      this.insertSKUCaseTerms.emit(skuTabData);
    }

    if (this.placeholderIdsToRemove.length) {
      this.deletePlaceHoldersByIds.emit({
        placeholderIds: this.placeholderIdsToRemove.join(",")
      });
    }

    if (this.placeholderAllocations.length) {
      this.upsertPlaceholderAllocationsToProject.emit({
        placeholderAllocations: this.placeholderAllocations
      });
    }
  }

  getFormValidationErrors() {
    this.errorMessages = [];

    this.placeholdersArray.controls.forEach((formArray) => {
      Object.keys((formArray as FormArray).controls).forEach((key) => {
        const controlErrors: ValidationErrors = formArray.get(key).errors;
        if (controlErrors != null) {
          Object.keys(controlErrors).forEach((keyError) => {
            let message = "";
            switch (keyError) {
              case "required": {
                message = `${FieldsEnum[key]} is ${keyError}`;
                break;
              }
              case "min":
              case "max": {
                message = `${keyError} value for ${FieldsEnum[key]} is ${controlErrors[keyError][keyError]}`;
                break;
              }
              case "pattern": {
                message = `Please enter a valid value for ${FieldsEnum[key]}`;
                break;
              }
            }
            //const message = `${key} is ${keyError}`;

            if (!this.errorMessages.includes(message))
              this.errorMessages.push(message);
            // console.log('Key control: ' + key + ', keyError: ' + keyError + ', err value: ', controlErrors[keyError]);
          });
        }
      });
    });

    this.existingPlaceholdersArray.controls.forEach((formArray) => {
      Object.keys((formArray as FormArray).controls).forEach((key) => {
        const controlErrors: ValidationErrors = formArray.get(key).errors;
        if (controlErrors != null) {
          Object.keys(controlErrors).forEach((keyError) => {
            let message = "";
            switch (keyError) {
              case "required": {
                message = `${FieldsEnum[key]} is ${keyError}`;
                break;
              }
              case "min":
              case "max": {
                message = `${keyError} value for ${FieldsEnum[key]} is ${controlErrors[keyError][keyError]}`;
                break;
              }
              case "pattern": {
                message = `Please enter a valid value for ${FieldsEnum[key]}`;
                break;
              }
            }

            if (!this.errorMessages.includes(message))
              this.errorMessages.push(message);
            // console.log('Key control: ' + key + ', keyError: ' + keyError + ', err value: ', controlErrors[keyError]);
          });
        }
      });
    });
  }

}
