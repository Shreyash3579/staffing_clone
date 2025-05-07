import { CdkDragDrop, moveItemInArray, transferArrayItem } from "@angular/cdk/drag-drop";
import { Component, EventEmitter, Input, Output } from "@angular/core";

@Component({
    selector: 'app-preference-list',
    templateUrl: './preference-list.component.html',
    styleUrls: ['./preference-list.component.scss']
})
export class PreferenceListComponent {
    public preferenceSet = [];            
    public preferenceInterest = [];
    public preferenceNoInterest = [];
    public showInterestDropArea: boolean = true;
    public showNoInterestDropArea: boolean = true;

    @Input() preferenceTitle: string;
    @Input() preferencesType: string;
    @Input() employeeStaffingPreferences = [];

    public preferences = [];

    @Output() updatePreferenceListEmitter = new EventEmitter();

    constructor() { }

    ngOnInit() {
        this.getPreferencesByType();
        this.splitEmployeeStaffingPreferences();
    }

    drop(event: CdkDragDrop<string[]>) {
        this.showInterestDropArea = true;
        this.showNoInterestDropArea = true;
        if (this.isItemPositionUpdated(event)) {
            if (event.previousContainer === event.container) {
                moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
                this.concatPreferences();
            } else {
                transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
                this.concatPreferences();                
            }
            this.splitEmployeeStaffingPreferences();
            this.updateEmployeeStaffingPreferences();
        }
    }

    private isItemPositionUpdated(event) {
        return event.previousContainer !== event.container || event.previousIndex !== event.currentIndex;
    }

    private getPreferencesByType() {
        this.preferences = this.employeeStaffingPreferences.find(x => x.preferenceType === this.preferencesType).staffingPreferences;
    }

    private updateEmployeeStaffingPreferences() {
        let updatedPreferences = this.preferences.filter(value => value.interest == true || value.noInterest == true);
        this.updatePreferenceListEmitter.emit({
            preferences: updatedPreferences,
            preferencesType: this.preferencesType
        });
    }

    private splitEmployeeStaffingPreferences() {
        const staffingPreferences = this.preferences;
        this.preferenceSet = this.mapObjects(staffingPreferences,false).filter(value => value.interest == false && value.noInterest == false);        
        this.preferenceInterest = staffingPreferences.filter(value => value.interest == true);
        this.preferenceNoInterest = staffingPreferences.filter(value => value.noInterest == true);
        this.preferenceInterest = this.mapObjects(this.preferenceInterest,true);
        this.preferenceNoInterest = this.mapObjects(this.preferenceNoInterest,true);        
    }

    private concatPreferences(){
        this.preferences = this.preferenceSet.concat(this.preferenceInterest,this.preferenceNoInterest);
        this.preferences.forEach((value) => {
            value.interest = false;
            value.noInterest = false;
            let isInterestAvailable = this.preferenceInterest.indexOf(value);
            if(isInterestAvailable >=0){
                this.preferences[this.preferences.indexOf(value)].interest = true;
            }
            let isNoInterestAvailable = this.preferenceNoInterest.indexOf(value);
            if(isNoInterestAvailable >= 0){
                this.preferences[this.preferences.indexOf(value)].noInterest = true;
            }
        })
    }

    private mapObjects(data,includeSortOrder){
        return data.map((value,index) => {
            return {
                code: value.code,
                name: value.name,
                label: includeSortOrder == true ? index + 1 + '.' +  value.name  : value.name,
                interest: value?.interest,
                noInterest: value?.noInterest
            };
        });        
    }    

    dropEnteredInterest(event){
        this.showInterestDropArea = false;
    }

    dropEnteredNoInterest(event){
        this.showNoInterestDropArea = false;
    }
}