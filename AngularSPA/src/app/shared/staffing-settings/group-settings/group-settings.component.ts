import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from "@angular/core";
import { Subject } from "rxjs";
import { debounceTime } from "rxjs/operators";
import { UserPreferenceSupplyGroupMember } from "src/app/shared/interfaces/userPreferenceSupplyGroupMember";
import { UserPreferenceSupplyGroupViewModel } from "src/app/shared/interfaces/userPreferenceSupplyGroupViewModel";

@Component({
    selector: "app-group-settings",
    templateUrl: "./group-settings.component.html",
    styleUrls: ["./group-settings.component.scss"]
})
export class GroupSettingsComponent implements OnInit, OnDestroy {
    @Input() allGroupsArray: UserPreferenceSupplyGroupViewModel[] = [];

    @Output() createGroupEmitter = new EventEmitter();
    @Output() editGroupEmitter = new EventEmitter();
    @Output() shareGroupEmitter = new EventEmitter();
    @Output() defaultGroupEmitter = new EventEmitter();

    defaultGroupArray = [];
    filteredGroupsArray: UserPreferenceSupplyGroupViewModel[] = [];
    public deletedGroupIdArray : string[] = [];

    private groupSearchFilter$: Subject<string> = new Subject();
    groupSearchTerm: string = "";

    constructor() {}

    ngOnInit(): void {
        this.defaultGroupArray = this.allGroupsArray.filter( x => x.isDefault);
        this.filteredGroupsArray = this.allGroupsArray;

        this.attachGroupSearchFilterSubsciption();
    }

    searchGroupsFilterHandler(event) {
        this.groupSearchFilter$.next(event.target.value);
    }

    clearSearchBox(){
        this.groupSearchFilter$.next("");
    }

    private attachGroupSearchFilterSubsciption() {
        this.groupSearchFilter$.pipe(
            debounceTime(500)
        ).subscribe(searchTextValue => {
            this.searchGroupsByName(searchTextValue);
        });
    }

    searchGroupsByName(searchTextValue) {
        this.groupSearchTerm = searchTextValue;

        if (this.groupSearchTerm.length) {
            this.filteredGroupsArray = this.allGroupsArray.filter( x=> x.name.toLowerCase().match(this.groupSearchTerm.toLowerCase()));
        }else{
            this.filteredGroupsArray = this.allGroupsArray;
        }
    }

    collapseExpandContainer(index, event, isDefault) {
        if (isDefault === true) {
            const defaultContainer =
                document.querySelector<HTMLElement>("#default-container");

            if (defaultContainer.classList.contains("collapsed")) {
                defaultContainer.classList.remove("collapsed");
                event.currentTarget.classList.remove("collapsed");
            } else {
                defaultContainer.classList.add("collapsed");
                event.currentTarget.classList.add("collapsed");
            }
        } else {
            const allGroupsContainer = document.querySelector<HTMLElement>(
                `#group-container-${index}`
            );

            if (allGroupsContainer.classList.contains("collapsed")) {
                allGroupsContainer.classList.remove("collapsed");
                event.currentTarget.classList.remove("collapsed");
            } else {
                allGroupsContainer.classList.add("collapsed");
                event.currentTarget.classList.add("collapsed");
            }
        }
    }

    openCreateGroupForm() {
        this.createGroupEmitter.emit();
    }

    editGroup(group, index) {
        const valueToSend = {
            group: group,
            index: index
        };

        this.editGroupEmitter.emit(valueToSend);
    }

    onMakeDefaultClickHandler(group) {
        group.isDefault = true;
        
        if (this.defaultGroupArray.length) {
            this.defaultGroupArray.splice(0, 1, group);
        } else {
            this.defaultGroupArray.push(group);
        }

        this.filteredGroupsArray.forEach((item) => {
            if (item.id === group.id) {
                item.isDefault = true;
            } else {
                item.isDefault = false;
            }
        });

        this.allGroupsArray.forEach((item) => {
            if (item.id === group.id) {
                item.isDefault = true;
            } else {
                item.isDefault = false;
            }
        });

        this.defaultGroupEmitter.emit();
    }

    onRemoveDefaultClickHandler(group){
        this.defaultGroupArray = [];
        this.allGroupsArray.map(x => x.isDefault = false);
        this.filteredGroupsArray.map(x => x.isDefault = false);
        this.defaultGroupEmitter.emit();
    }

    // Remove member from list
    deleteMemberFromGroupHandler(groupTodeleteFrom: UserPreferenceSupplyGroupViewModel, groupMemberToRemove: UserPreferenceSupplyGroupMember) {
        groupTodeleteFrom.groupMembers.splice(
          groupTodeleteFrom.groupMembers.findIndex( X=> X.employeeCode == groupMemberToRemove.employeeCode), 1
        );
    }

    onDeleteGroupHandler(groupToDelete : UserPreferenceSupplyGroupViewModel){
        if (groupToDelete.isDefault === true) {
            this.defaultGroupArray = [];
        } 

        this.deletedGroupIdArray.push(groupToDelete.id);

        // this.allGroupsArray.splice(
        //     this.allGroupsArray.findIndex( x=> x.id == groupToDelete.id), 1
        // );
        const indexToRemove = this.filteredGroupsArray.findIndex( x=> x.id == groupToDelete.id);

        if(indexToRemove > - 1){
            this.filteredGroupsArray.splice(
                indexToRemove, 1
            );
        }

        const index = this.allGroupsArray.findIndex( x=> x.id == groupToDelete.id);

        if(index > - 1){
            this.allGroupsArray.splice(
                index, 1
            );
        }

        this.defaultGroupEmitter.emit();
        
    }

    onShareClickHandler(groupToShare){
        this.shareGroupEmitter.emit(groupToShare);
    }

    onEditClickHandler(groupToEdit){
        this.editGroupEmitter.emit(groupToEdit);
    }

    ngOnDestroy(){
        this.groupSearchFilter$.unsubscribe();
    }
}
