import { Component, ViewEncapsulation, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { Certification } from 'src/app/shared/interfaces/employeeCertification.interface';
import { Language } from 'src/app/shared/interfaces/employeeLanguage.interface';
import { EmployeeSchoolHistory } from 'src/app/shared/interfaces/employeeSchoolHistory';
import { EmployeeWorkHistory } from 'src/app/shared/interfaces/employeeWorkHistory';
import { LevelGradeTransactionModel } from 'src/app/shared/interfaces/level-grade-transaction.interface';
import { Training } from "src/app/shared/interfaces/training";
import { StaffableAsRole } from 'src/app/shared/interfaces/staffableAsRole.interface';
import { StaffableAsType } from 'src/app/shared/interfaces/staffableAsType.interface';
import { TransferTransactionModel } from 'src/app/shared/interfaces/tranfer-transaction.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';

@Component({
    selector: 'app-about',
    templateUrl: './about.component.html',
    styleUrls: ['./about.component.scss'],
    encapsulation: ViewEncapsulation.None
})
export class AboutComponent implements OnInit {
    staffableAsTypes: StaffableAsType[];
    accessibleFeatures = ConstantsMaster.appScreens.feature;

    @Input() languages: Language[];
    @Input() levelGradeHistoryData: LevelGradeTransactionModel[];
    @Input() globalTrainingsData: Training[];
    @Input() certificates: Certification[];
    @Input() employeeSchoolHistory: EmployeeSchoolHistory[];
    @Input() employeeWorkHistory: EmployeeWorkHistory[];
    @Input() staffableAsRoles: StaffableAsRole[];
    @Input() isLoading = true;
    @Input() isStaffableAsRoleLoaded;
    @Input() tranferData: TransferTransactionModel[];

    @Output() removeStaffableAsRoleEventEmitter = new EventEmitter<any>();
    @Output() upsertStaffableAsRoleEventEmitter = new EventEmitter<any>();

    constructor(private localStorageService: LocalStorageService) { }

    ngOnInit() {
        this.staffableAsTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffableAsTypes);
    }

    removeStaffableAsRoleEventEmitterHandler(event) {
        this.removeStaffableAsRoleEventEmitter.emit(event);
    }

    upsertStaffableAsRoleEventEmitterHandler(event) {
        this.upsertStaffableAsRoleEventEmitter.emit(event);
    }
}
