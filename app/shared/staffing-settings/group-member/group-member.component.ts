import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { UserPreferenceSupplyGroupMember } from '../../interfaces/userPreferenceSupplyGroupMember';

@Component({
  selector: 'app-group-member',
  templateUrl: './group-member.component.html',
  styleUrls: ['./group-member.component.scss']
})
export class GroupMemberComponent implements OnInit {

  @Input() groupMember: UserPreferenceSupplyGroupMember;

  @Output() deleteMemberFromGroupEmitter = new EventEmitter();

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges) {
    if ((changes['sharedWithMembers'])) {
      this.groupMember = changes.sharedWithMembers.currentValue;
    }
  }

  deleteMemberFromGroupHandler() {
    this.deleteMemberFromGroupEmitter.emit();
  }

}
