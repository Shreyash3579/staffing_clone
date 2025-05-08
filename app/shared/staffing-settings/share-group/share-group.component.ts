import { Component, Input, OnInit, SimpleChanges } from "@angular/core";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { Resource } from "src/app/shared/interfaces/resource.interface";
import { UserPreferenceSupplyGroupMember } from "src/app/shared/interfaces/userPreferenceSupplyGroupMember";
import { UserPreferenceSupplyGroupSharedInfo } from "src/app/shared/interfaces/UserPreferenceSupplyGroupSharedInfo";
import { UserPreferenceSupplyGroupViewModel } from "src/app/shared/interfaces/userPreferenceSupplyGroupViewModel";
import { UserPreferenceSupplyGroupSharedInfoViewModel } from "src/app/shared/interfaces/userPrefernceSupplyGroupSharedInfoViewModel";
import { LocalStorageService } from "src/app/shared/local-storage.service";

@Component({
    selector: "app-share-group",
    templateUrl: "./share-group.component.html",
    styleUrls: ["./share-group.component.scss"]
})
export class ShareGroupComponent implements OnInit {
    @Input() groupToShare: UserPreferenceSupplyGroupViewModel = {} as UserPreferenceSupplyGroupViewModel;
    @Input() sharedWithMembers: UserPreferenceSupplyGroupSharedInfoViewModel[] = [] as UserPreferenceSupplyGroupSharedInfoViewModel[];
    @Input() showHeader : boolean = true;
    // Variables
    pageHeader;
    group : UserPreferenceSupplyGroupViewModel;
    sharedWith : UserPreferenceSupplyGroupSharedInfoViewModel[];
    errorMessage: string[] = [];

  constructor(
    private localStorageService: LocalStorageService) {}

    ngOnInit(): void {
        this.pageHeader = "Share Group";
        this.group = this.groupToShare;  
        this.sharedWith = this.sharedWithMembers;
    }

    ngOnChanges(changes: SimpleChanges) {
        if((changes['sharedWithMembers'])) {
            this.sharedWith = changes.sharedWithMembers.currentValue;
        }
    }


    onSearchItemSelectHandler(selectedResource: Resource){
      this.errorMessage = [];
      const groupMemberToAdd : UserPreferenceSupplyGroupMember = {
        employeeCode: selectedResource.employeeCode,
        employeeName: selectedResource.fullName,
        currentLevelGrade: selectedResource.levelGrade,
        positionName: selectedResource.position?.positionName,
        operatingOfficeAbbreviation: selectedResource.schedulingOffice?.officeAbbreviation
      }

      if(!(this.sharedWith.some(x => x.sharedWith == selectedResource.employeeCode)))
      {
        this.sharedWith.push({
          sharedWith: selectedResource.employeeCode,
          userPreferenceGroupId: this.group.id,
          lastUpdatedBy: this.group.lastUpdatedBy,
          sharedWithMemberDetails: groupMemberToAdd
        });
      }
      else {
        this.errorMessage.push(`"${selectedResource.fullName}" already has access to "${this.group.name}" group`)
    }
    }

    // Remove member from list
    deleteMemberFromGroupHandler(groupMemberToRemove: UserPreferenceSupplyGroupSharedInfo) {
        this.sharedWith.splice(
          this.sharedWith.findIndex( X=> X.sharedWith === groupMemberToRemove.sharedWith),
          1
        );
    }
}
