import { Component, EventEmitter, Input, Output } from "@angular/core";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { EmployeeStaffingPreferences } from "src/app/shared/interfaces/employeeStaffingPreferences";

@Component({
    selector: 'app-preferences',
    templateUrl: './preferences.component.html',
    styleUrls: ['./preferences.component.scss']
})
export class PreferencesComponent {
    public activeTab = "industry";
    accessibleFeatures = ConstantsMaster.appScreens.feature;

    @Input() employeeStaffingPreferences: EmployeeStaffingPreferences[];
    @Output() updateEmployeeStaffingPreferences = new EventEmitter();

    constructor() { }

    ngOnInit() {
    }

    toggleNav(tabName, event) {
        event.preventDefault()
        this.activeTab = tabName;
    }

    updatePreferencesHandler(event) {
        
        this.employeeStaffingPreferences.forEach(x => {
            if (x.preferenceType === event.preferencesType) {
                x.staffingPreferences.forEach(y => {
                    let preference = event.preferences.find(z => z.code === y.code);
                    if (preference) {
                        y.interest = preference.interest;
                        y.noInterest = preference.noInterest;
                    }else{
                        y.interest = false;
                        y.noInterest = false;
                    }
                });
            }
        });
        
        const dataToUpsert : EmployeeStaffingPreferences = {
            employeeCode: this.employeeStaffingPreferences[0].employeeCode,
            preferenceType: event.preferencesType,
            staffingPreferences: event.preferences,
            lastUpdatedBy: null
        } 

        this.updateEmployeeStaffingPreferences.emit(dataToUpsert);
    }
}
