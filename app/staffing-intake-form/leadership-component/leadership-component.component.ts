import { Component, Input, OnInit, SimpleChanges, Output, EventEmitter } from "@angular/core";
import { CaseIntakeLeadership } from "src/app/shared/interfaces/caseIntakeLeadership.interface";
import { CaseIntakeLeadershipGroup } from "src/app/shared/interfaces/caseIntakeLeadershipGroup";

@Component({
  selector: "app-leadership-component",
  templateUrl: "./leadership-component.component.html",
  styleUrls: ["./leadership-component.component.scss"]
})
export class LeadershipComponentComponent implements OnInit {
  @Input() leadershipsData: CaseIntakeLeadership[];
  @Input() opportunityId: string = null;
  @Input() oldCaseCode: string = null;
  @Input() planningCardId: string = null;

  @Output() upsertLeadershipEventEmitter = new EventEmitter<any>();
  @Output() deleteLeadershipEventEmitter = new EventEmitter<any>();

  leadershipGroups: CaseIntakeLeadershipGroup[] = [];
  LeadershipRolesList = [
    { caseRoleName: 'SMAP', caseRoleCode: 'SMAP'},
    { caseRoleName: 'Operating VP', caseRoleCode: 'OVP'},
    { caseRoleName: 'Senior Partner', caseRoleCode: 'SVP' },
    { caseRoleName: 'Advisor', caseRoleCode: 'AD'}
  ];

  constructor() {}

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.leadershipsData) {
      this.initializeLeadershipData();
    }
  }

  private initializeLeadershipData(): void {
    this.leadershipGroups = [];
    this.LeadershipRolesList.forEach(role => {

      let leaders: CaseIntakeLeadership[] = this.leadershipsData?.filter(leader => leader.caseRoleCode === role.caseRoleCode) ?? [];
      leaders.forEach(leader => {
        leader.caseRoleCode = role.caseRoleCode;
        leader.caseRoleName = role.caseRoleName;
      });

      let leadershipGroup: CaseIntakeLeadershipGroup = {
        caseRoleCode: role.caseRoleCode,
        caseRoleName: role.caseRoleName,
        leaderships: leaders
      };
      this.leadershipGroups.push(leadershipGroup);
    });
  }

  upsertLeadershipEventHandler(leaders: CaseIntakeLeadership[]): void {
   
    const updatedLeaderships: CaseIntakeLeadership[] = leaders.map(leader => ({
      ...leader,
      oldCaseCode: this.oldCaseCode ?? null,
      opportunityId: this.opportunityId ?? null,
      planningCardId: this.planningCardId ?? null,
    }));
    this.upsertLeadershipEventEmitter.emit(updatedLeaderships);
  }

  deleteLeadershipEventHandler(caseRoleCode: string) {

    this.deleteLeadershipEventEmitter.emit(caseRoleCode);
  }
}
