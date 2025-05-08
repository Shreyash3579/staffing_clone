import { Component, EventEmitter, Input, Output } from "@angular/core";
import { BsDatepickerConfig } from "ngx-bootstrap/datepicker";
import { DateService } from "src/app/shared/dateService";
import { StaffableAsRole } from "src/app/shared/interfaces/staffableAsRole.interface";
import { StaffableAsType } from "src/app/shared/interfaces/staffableAsType.interface";

// ----------------------- System Constants ----------------------------------//
import { BS_DEFAULT_CONFIG } from '../../../../shared/constants/bsDatePickerConfig';

@Component({
    selector: 'app-employee-staffable-as',
    templateUrl: './employee-staffable-as.component.html',
    styleUrls: ['./employee-staffable-as.component.scss']
})
export class EmployeeStaffableAsComponent {
    bsConfig: Partial<BsDatepickerConfig>;
    isEffectiveDateInvalid: boolean = false;

    @Input() staffableAsRole: StaffableAsRole;
    @Input() staffableAsTypes: StaffableAsType[];

    @Output() removeStaffableAsRoleEventEmitter = new EventEmitter<any>();
    @Output() upsertStaffableAsRoleEventEmitter = new EventEmitter<any>();

    constructor() {
    }

    ngOnInit() {
        this.initialiseDatePicker();
        this.transformStaffableAsRoles();
    }

    transformStaffableAsRoles() {
        this.staffableAsRole.effectiveDate = DateService.convertDateInBainFormat(this.staffableAsRole.effectiveDate);
    }

    initialiseDatePicker() {
        this.bsConfig = BS_DEFAULT_CONFIG;
        this.bsConfig.containerClass = 'theme-red calendar-align-right';
    }

    onEffectiveDateChange(data) {
        if (this.isValidDate(data)) {
            this.isEffectiveDateInvalid = false;
            this.staffableAsRole.effectiveDate = DateService.convertDateInBainFormat(data)
            this.upsertStaffableAsRole();
        } else {
            this.isEffectiveDateInvalid = true;
        }
    }

    onStaffableAsRoleTypeChange() {
        if (this.staffableAsRole.staffableAsTypeCode > 0) {
            this.staffableAsRole.staffableAsTypeName = 
            this.staffableAsTypes.find(type => type.staffableAsTypeCode === this.staffableAsRole.staffableAsTypeCode).staffableAsTypeName;
            this.upsertStaffableAsRole();
        }
    }

    removeStaffableRoleHandler() {
        this.removeStaffableAsRoleEventEmitter.emit(this.staffableAsRole);
    }

    private upsertStaffableAsRole() {
        if (this.staffableAsRole.effectiveDate && this.staffableAsRole.staffableAsTypeCode > 0) {
            this.staffableAsRole.isActive = true;
            this.upsertStaffableAsRoleEventEmitter.emit(this.staffableAsRole);
        }
    }

    private isValidDate(data) {
        return !(isNaN(data?.valueOf()) && (this.isNullOrUndefined(data)));
    }

    private isNullOrUndefined(value) {
        return (value === null || value === undefined);
    }
}