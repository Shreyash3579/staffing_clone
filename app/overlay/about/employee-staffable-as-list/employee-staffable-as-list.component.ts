import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { DateService } from "src/app/shared/dateService";
import { StaffableAsRole } from "src/app/shared/interfaces/staffableAsRole.interface";
import { StaffableAsType } from "src/app/shared/interfaces/staffableAsType.interface";

@Component({
    selector: 'app-employee-staffable-as-list',
    templateUrl: './employee-staffable-as-list.component.html',
    styleUrls: ['./employee-staffable-as-list.component.scss']
})
export class EmployeeStaffableAsListComponent implements OnInit {

    staffableAsRolesList: StaffableAsRole[] = [];
    isDisableAddNewRole = false;

    @Input() staffableAsRoles: StaffableAsRole[];
    @Input() staffableAsTypes: StaffableAsType[];

    @Output() removeStaffableAsRoleEventEmitter = new EventEmitter<any>();
    @Output() upsertStaffableAsRoleEventEmitter = new EventEmitter<any>();

    constructor() { }

    ngOnInit() {
        
    }

    addNewStaffableAsRow() {
        let newStaffableRole = {staffableAsTypeCode: -1, isActive: false} as StaffableAsRole;
        this.staffableAsRoles.push(newStaffableRole);
        this.disableAddNewRoleButton();
    }

    removeStaffableAsRoleEventEmitterHandler(staffableRole) {
        this.staffableAsRoles.splice(this.staffableAsRoles.indexOf(staffableRole), 1);
        if (!staffableRole.id) {
            this.enableAddNewRoleButton();
        } else {
            this.removeStaffableAsRoleEventEmitter.emit(staffableRole.id);
        }
    }

    upsertStaffableAsRoleEventEmitterHandler(event) {
        this.staffableAsRoles.map(x => {
            if (x.id !== event.id) x.isActive = false;
        });
        this.staffableAsRoles = this.staffableAsRoles.filter(x => x.staffableAsTypeCode > 0 && x.effectiveDate !== '');
        this.upsertStaffableAsRoleEventEmitter.emit(this.staffableAsRoles);
        this.enableAddNewRoleButton();
    }

    private enableAddNewRoleButton() {
        this.isDisableAddNewRole = false;
    }

    private disableAddNewRoleButton() {
        this.isDisableAddNewRole = true;
    }
}