import {Component, inject} from "@angular/core";
import {SharedService} from "../../shared/shared.service";
import {Project} from "../../shared/interfaces/project.interface";
import {MatDialog} from "@angular/material/dialog";
import {HttpClient} from "@angular/common/http";
import {ShareUrlDialogComponent} from "../../standalone-components/share-url-dialog/share-url-dialog.component";
import {GainzService} from "../services/gainz.service";
import {sendMessage} from "@microsoft/signalr/dist/esm/Utils";

@Component({
  selector: "app-chat-assistant",
  standalone: false,
  templateUrl: "./chat-assistant.component.html",
  styleUrl: "./chat-assistant.component.scss"
})
export class ChatAssistantComponent {

  projects: Project[] = [];
  resources = [];
  staffingPlans = [];
  allocations = [];
  timelineData = [];
  resourceDetail = null;
  showResourceModal = false;
  chatbotInput: string = "";
  tokenLimit = 1500;
  activeTabIndex: number = 4; //TODO: pass to index 0
  sendMessageExample = "";
  //chatTabs = [];
  readonly dialog = inject(MatDialog);
  chatTabs = [];
  indexChat = 0;
  chatExample = [{
    "id": 2, "sender": "ai", "message": ["Here is you requested Staffing Plan"], "timestamp": "2025-03-10T10:00:02Z", "loading": true
  }, {

    "id": 5, "sender": "ai", loading: false , "table": true, "resources": {
      "resources": [{
        "id": 1,
        "name": "Hillman Corp",
        "code": "010",
        "type": "project",
        "bookingType": "(Sydney)(Team: 4 | Prio: 1 | Lang: English, French)(Start date: Oct 02, 2025 | End date: Dec 31, 2025)",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#8367E7",
        "length": 12
      }, {
        "id": 2,
        "name": "Taza, Cam",
        "deleted": false,
        "code": "001",
        "type": "AC",
        "alertMessage": "Vacation from  07-dec-2025 til 11-dec-2025. In x day(s)",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#F6F1FE",
        "length": 12
      }, {
        "id": 3,
        "name": "Johnson, Mitchell",
        "code": "012",
        "deleted": false,
        "type": "C",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#F6F1FE",
        "length": 12
      }, {
        "id": 4,
        "name": "Markle, Elizabeth",
        "code": "015",
        "type": "C",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#F6F1FE",
        "length": 12
      }, {
        "id": 5, "name": "Epiar, Rohan", "code": "017", "type": "M", "office": "SYD", "startWeek": 1, "filledColor": "#F6F1FE", "length": 12
      }, {
        "id": 6,
        "name": "Intercontinental",
        "code": "016",
        "type": "project",
        "bookingType": "(Sydney)(Team: 4 | Prio: 1 | Lang: Spanish)(Start date: Oct 02, 2025 | End date: Dec 31, 2025)",
        "office": "SYD",
        "startWeek": 3,
        "filledColor": "#8EB97A",
        "length": 12
      }, {
        "id": 7,
        "name": "Jones, Madison",
        "code": "105",
        "type": "AC",
        "office": "SYD",
        "startWeek": 3,
        "filledColor": "#ECF3E8",
        "length": 12
      }, {
        "id": 8,
        "name": "Ozyamp, Clara",
        "code": "090",
        "type": "C",
        "office": "SYD",
        "startWeek": 3,
        "filledColor": "#ECF3E8",
        "length": 12
      }, {
        "id": 9, "name": "Roos, Ethan", "code": "012", "type": "C", "office": "BOS", "startWeek": 3, "filledColor": "#ECF3E8", "length": 12
      }, {
        "id": 10,
        "name": "Haste, Clayton",
        "code": "017",
        "type": "M",
        "office": "CIS",
        "startWeek": 3,
        "filledColor": "#ECF3E8",
        "length": 12
      }, {
        "id": 11,
        "name": "R&D Design",
        "code": "017",
        "type": "project",
        "bookingType": "(Sydney)(Team: 4 | Prio: 1 | Lang: English)(Start date: Oct 02, 2025 | End date: Dec 31, 2025)",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#F95B3F",
        "length": 12
      }, {
        "id": 12,
        "name": "Gavin, Frank",
        "alertMessage": "Vacation from  07-dec-2025 til 11-dec-2025. In x day(s)",
        "code": "070",
        "type": "AC",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "deleted": null,
        "length": 12
      }, {
        "id": 13,
        "name": "Nellia, George",
        "deleted": null,
        "code": "010",
        "type": "C",
        "office": "CIS",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "length": 12
      }, {
        "id": 14,
        "name": "Lewis, Melton",
        "code": "025",
        "type": "C",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "length": 12
      }, {
        "id": 15,
        "name": "Whiteside, Anna",
        "code": "016",
        "type": "M",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "length": 12
      }], "feedback": "", "firstAnswer": true, "showChanges": false
    },

    "timestamp": "2025-03-10T10:01:00Z"
  }, {
    "id": 4, "sender": "ai", "message": ["Here is a version with the modification required"], "timestamp": "2025-03-10T10:01:00Z", loading: true,
  }, {

    "id": 6, "sender": "ai", loading: false , "table": true, "resources": {
      "resources": [{
        "id": 1,
        "name": "Hillman Corp",
        "code": "010",
        "type": "project",
        "bookingType": "(Sydney)(Team: 4 | Prio: 1 | Lang: English, French)(Start date: Oct 02, 2025 | End date: Dec 31, 2025)",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#8367E7",
        "length": 12
      }, {
        "id": 2,
        "name": "Taza, Cam",
        "deleted": false,
        "code": "001",
        "type": "AC",
        "alertMessage": "Vacation from  07-dec-2025 til 11-dec-2025. In x day(s)",
        "office": "SYD",
        "startWeek": 2,
        "filledColor": "#F6F1FE",
        "length": 12
      }, {
        "id": 3,
        "name": "Johnson, Mitchell",
        "code": "012",
        "deleted": false,
        "type": "C",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#F6F1FE",
        "length": 12
      }, {
        "id": 4,
        "name": "Markle, Elizabeth",
        "code": "015",
        "type": "C",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#F6F1FE",
        "length": 12
      }, {
        "id": 5, "name": "Epiar, Rohan", "code": "017", "type": "M", "office": "SYD", "startWeek": 1, "filledColor": "#F6F1FE", "length": 12
      }, {
        "id": 6,
        "name": "Intercontinental",
        "code": "016",
        "type": "project",
        "bookingType": "(Sydney)(Team: 4 | Prio: 1 | Lang: Spanish)(Start date: Oct 02, 2025 | End date: Dec 31, 2025)",
        "office": "SYD",
        "startWeek": 3,
        "filledColor": "#8EB97A",
        "length": 12
      }, {
        "id": 7,
        "name": "Jones, Madison",
        "code": "105",
        "type": "AC",
        "office": "SYD",
        "startWeek": 3,
        "filledColor": "#ECF3E8",
        "length": 12
      }, {
        "id": 8,
        "name": "Ozyamp, Clara",
        "code": "090",
        "type": "C",
        "office": "SYD",
        "startWeek": 3,
        "filledColor": "#ECF3E8",
        "length": 12
      }, {
        "id": 9, "name": "Roos, Ethan", "code": "012", "type": "C", "office": "BOS", "startWeek": 3, "filledColor": "#ECF3E8", "length": 12
      }, {
        "id": 10,
        "name": "Haste, Clayton",
        "code": "017",
        "type": "M",
        "office": "CIS",
        "startWeek": 3,
        "filledColor": "#ECF3E8",
        "length": 12
      }, {
        "id": 11,
        "name": "R&D Design",
        "code": "017",
        "type": "project",
        "bookingType": "(Sydney)(Team: 4 | Prio: 1 | Lang: English)(Start date: Oct 02, 2025 | End date: Dec 31, 2025)",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#F95B3F",
        "length": 12
      }, {
        "id": 12,
        "name": "Gavin, Frank",
        "alertMessage": "Vacation from  07-dec-2025 til 11-dec-2025. In x day(s)",
        "code": "070",
        "type": "AC",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "deleted": null,
        "length": 12
      }, {
        "id": 13,
        "name": "Nellia, George",
        "deleted": null,
        "code": "010",
        "type": "C",
        "office": "CIS",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "length": 12
      }, {
        "id": 14,
        "name": "Lewis, Melton",
        "code": "025",
        "type": "C",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "length": 12
      }, {
        "id": 15,
        "name": "Whiteside, Anna",
        "code": "016",
        "type": "M",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "length": 12
      }], "feedback": "", "firstAnswer": false, "showChanges": false
    },

    "timestamp": "2025-03-10T10:01:00Z"
  }, {
    "id": 4, "sender": "ai",loading: true, "message": ["Here is a version with the modification required"], "timestamp": "2025-03-10T10:01:00Z"
  }, {

    "id": 6, "sender": "ai",loading: false, "table": true, "resources": {
      "resources": [{
        "id": 1,
        "name": "Hillman Corp",
        "code": "010",
        "type": "project",
        "bookingType": "(Sydney)(Team: 4 | Prio: 1 | Lang: English, French)(Start date: Oct 02, 2025 | End date: Dec 31, 2025)",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#8367E7",
        "length": 12
      }, {
        "id": 2,
        "name": "Taza, Cam",
        "deleted": true,
        "code": "001",
        "type": "AC",
        "alertMessage": "Vacation from  07-dec-2025 til 11-dec-2025. In x day(s)",
        "office": "SYD",
        "startWeek": 2,
        "filledColor": "#d1b7fa",
        "length": 12
      }, {
        "id": 3,
        "name": "Johnson, Mitchell",
        "code": "012",
        "deleted": true,
        "type": "C",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#d1b7fa",
        "length": 12
      }, {
        "id": 4,
        "name": "Markle, Elizabeth",
        "code": "015",
        "type": "C",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#F6F1FE",
        "length": 12
      }, {
        "id": 5, "name": "Epiar, Rohan", "code": "017", "type": "M", "office": "SYD", "startWeek": 1, "filledColor": "#F6F1FE", "length": 12
      }, {
        "id": 6,
        "name": "Intercontinental",
        "code": "016",
        "type": "project",
        "bookingType": "(Sydney)(Team: 4 | Prio: 1 | Lang: Spanish)(Start date: Oct 02, 2025 | End date: Dec 31, 2025)",
        "office": "SYD",
        "startWeek": 3,
        "filledColor": "#8EB97A",
        "length": 12
      }, {
        "id": 7,
        "name": "Jones, Madison",
        "code": "105",
        "type": "AC",
        "office": "SYD",
        "startWeek": 3,
        "filledColor": "#ECF3E8",
        "length": 12
      }, {
        "id": 8,
        "name": "Ozyamp, Clara",
        "code": "090",
        "type": "C",
        "office": "SYD",
        "startWeek": 3,
        "filledColor": "#ECF3E8",
        "length": 12
      }, {
        "id": 9, "name": "Roos, Ethan", "code": "012", "type": "C", "office": "BOS", "startWeek": 3, "filledColor": "#ECF3E8", "length": 12
      }, {
        "id": 10,
        "name": "Haste, Clayton",
        "code": "017",
        "type": "M",
        "office": "CIS",
        "startWeek": 3,
        "filledColor": "#ECF3E8",
        "length": 12
      }, {
        "id": 11,
        "name": "R&D Design",
        "code": "017",
        "type": "project",
        "bookingType": "(Sydney)(Team: 4 | Prio: 1 | Lang: English)(Start date: Oct 02, 2025 | End date: Dec 31, 2025)",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#F95B3F",
        "length": 12
      }, {
        "id": 12,
        "name": "Gavin, Frank",
        "alertMessage": "Vacation from  07-dec-2025 til 11-dec-2025. In x day(s)",
        "code": "070",
        "type": "AC",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "deleted": null,
        "length": 12
      }, {
        "id": 13,
        "name": "Nellia, George",
        "deleted": null,
        "code": "010",
        "type": "C",
        "office": "CIS",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "length": 12
      }, {
        "id": 14,
        "name": "Lewis, Melton",
        "code": "025",
        "type": "C",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "length": 12
      }, {
        "id": 15,
        "name": "Whiteside, Anna",
        "code": "016",
        "type": "M",
        "office": "SYD",
        "startWeek": 1,
        "filledColor": "#FDEABA",
        "length": 12
      }], "feedback": "", "firstAnswer": false, "showChanges": false
    },

    "timestamp": "2025-03-10T10:01:00Z"
  }];

  constructor(private sharedService: SharedService, private http: HttpClient, private gainzService: GainzService) {

  }

  ngOnInit(): void {
    this.gainzService.getChats().subscribe(chats => {
      this.chatTabs = chats;

    });

    const clientCodes = null, projectStartIndex = undefined, pageSize = undefined, industryPracticeAreaCodes = undefined,
      capabilityPracticeAreaCodes = undefined;
    const today = new Date();
    const mocked = {
      "startDate": `${today.getFullYear()}-${today.getMonth() + 1}-${today.getDate()}`,
      "endDate": "3000-1-1",
      "officeCodes": "153",
      "caseTypeCodes": "1",
      "caseAttributeNames": "",
      "opportunityStatusTypeCodes": "0,1,2,3,4,5",
      "demandTypes": "Opportunity,NewDemand,CaseEnding,StaffedCase,ActiveCase,PlanningCards",
      "minOpportunityProbability": 0,
      "caseExceptionShowList": "",
      "caseExceptionHideList": "",
      "opportunityExceptionShowList": "",
      "opportunityExceptionHideList": "",
      "caseAllocationsSortBy": "nameAtoZ",
      "planningCardsSortOrder": "",
      "caseOppSortOrder": "",
      "isStaffedFromSupply": false,
      "supplyFilterCriteria": null,
      clientCodes,
      projectStartIndex,
      pageSize,
      industryPracticeAreaCodes,
      capabilityPracticeAreaCodes
    };
    this.sharedService.getProjectsFilteredBySelectedValues(mocked).subscribe(data => {
      this.projects = data.projects;
    });
  }

  onTabChange(index): void {
    this.activeTabIndex = index;
    console.log(this.activeTabIndex);
  }

  enableEditMode(index: number, event: MouseEvent): void {

    event.stopPropagation();
    console.log(index);

    this.chatTabs[index].isEditMode = true;


    setTimeout(() => {

      const inputElement = event.target as HTMLInputElement;
      if (inputElement) {
        inputElement.focus();
      }
    }, 0);


    document.addEventListener("click", this.disableEditMode.bind(this, index), {once: true});
  }


  disableEditMode(index: number, event: MouseEvent): void {
    // Make sure we're not clicking on the input itself
    if (!(event.target as HTMLElement).closest("input")) {
      this.chatTabs[index].isEditMode = false;
    }
  }

  openResourceModal(resourceId: number): void {
    this.showResourceModal = true;
  }

  closeResourceModal(): void {
    this.showResourceModal = false;
  }

  addNewTab() {
    console.log("Adding new tab");
    const currentDate = new Date();
    const formattedDate = `${currentDate.getMonth() + 1}/${currentDate.getDate()}/${currentDate.getFullYear()}`;
    this.chatTabs.push({title: `Staffing Plan ${formattedDate}`, newChat: true, isEditMode: false, chatbotInput: ""});
    setTimeout(() => {
      this.activeTabIndex = this.chatTabs.length - 1;
    }, 100);
  }

  deleteTab(i: number) {

    if (i >= 0 && i < this.chatTabs.length) {

      this.chatTabs.splice(i, 1);


      if (this.chatTabs.length === 0) {

        this.addNewTab();
      } else if (i <= this.activeTabIndex) {

        if (i === this.activeTabIndex && i === this.chatTabs.length) {

          this.activeTabIndex = this.chatTabs.length - 1;
        } else if (i === this.activeTabIndex) {

          this.activeTabIndex = Math.max(0, i);
        } else {

          this.activeTabIndex = Math.max(0, this.activeTabIndex - 1);
        }
      }
      console.log(`Tab ${i} deleted. New active tab index: ${this.activeTabIndex}`);
    } else {
      console.error(`Cannot delete tab at index ${i}: Index out of bounds`);
    }
  }

  openDialog(url): void {
    const dialogRef = this.dialog.open(ShareUrlDialogComponent, {data: {url: url}});

    dialogRef.afterClosed().subscribe(result => {
      console.log("The dialog was closed");
    });
  }
  sendMessage(chatbotInput: string, index: number) {
    if (!chatbotInput || !/\S/.test(chatbotInput)) {
      return;
    }
    console.log(this.chatExample);
    let obj = {
      "id": 1, "sender": "user", "message": [chatbotInput], "timestamp": new Date().toISOString()
    };
    this.chatTabs[index].chat.push(obj);
    console.log("OBJ")
    this.chatTabs[index].chat.push(this.chatExample[this.indexChat]);
    setTimeout(() => {
      this.chatTabs[index].chat[this.chatTabs[index].chat.length - 1].loading = false;
      console.log("TIMEOUT CHANGE", this.chatTabs[index]);
      this.indexChat += 1;
      this.chatTabs[index].chat.push(this.chatExample[this.indexChat]);
      setTimeout(() => {
        this.chatTabs[index].chat[this.chatTabs[index].chat.length - 1].loading = false;
        this.indexChat += 1;
      }, 4000);
    }, 4000);

    this.chatTabs[index].chatbotInput = "";
  }

  onSelectedProjectsChange($event: any[]) {
    this.chatTabs[this.activeTabIndex].selectedProjects = $event;
  }
}
