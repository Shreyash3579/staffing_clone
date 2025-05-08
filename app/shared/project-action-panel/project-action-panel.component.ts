import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-project-action-panel',
  templateUrl: './project-action-panel.component.html',
  styleUrls: ['./project-action-panel.component.scss']
})
export class ProjectActionPanelComponent implements OnInit {
  @Input() showEllipsis = false;
  @Input() allowEmail = true;
  @Input() allowPin = true;
  @Input() allowHide = true;
  @Input() showCaseRoll = true;
  @Input() showplaceholder = false;
  @Input() showMergeIcon = false;
  @Input() showCardDeleteIcon = false;
  @Input() showQuickPeek = false;
  @Input() showShareIcon = false;
  @Input() showShareUrlIcon = false;
  @Input() placeholderTrigerredByEvent: string;
  @Input() shareUrl: string;

  @Input() emailTo = '';
  @Input() isPinned = false;
  @Input() isHidden = false;
  @Input() isCaseOnRoll = false;
  @Input() isSTACommitmentCreated = false;
  @Input() iconSizes: string;
  @Input() isPegPlanningCard = false;
  @Input()  allowSTACommitment = true;


  @Output() togglePinEmitter = new EventEmitter<any>();
  @Output() toggleHideEmitter = new EventEmitter<any>();
  @Output() caseRollEmitter = new EventEmitter<any>();
  @Output() shortTermAvailableCaseOppCommitmentEmitter = new EventEmitter<any>();
  @Output() addPlaceHolderEmitter = new EventEmitter();
  @Output() openPlaceHolderFormEmitter = new EventEmitter();
  @Output() toggleMergeDialogEmitter = new EventEmitter();
  @Output() deleteCardEmitter = new EventEmitter();
  @Output() openQuickPeekIntoReosurcesCommitments = new EventEmitter();
  @Output() sharePlanningCardEmitter = new EventEmitter();

 

  constructor() { }

  ngOnInit() {
  }

  togglePin() {
    this.isPinned = !this.isPinned;
    this.isHidden = false;  // pinned project will be unhidden

    this.togglePinEmitter.emit(this.isPinned);
  }

  toggleHide() {
    // Do not allow hide if project already pinned. TODO: change once decide what should be the business requirement here
    if (this.isPinned) {
      return;
    }

    this.isHidden = !this.isHidden;
    this.toggleHideEmitter.emit(this.isHidden);
  }

  onCaseRollClick() {
    this.caseRollEmitter.emit();
  }

  onSTACommitmentClick(){
    this.shortTermAvailableCaseOppCommitmentEmitter.emit();
  }

  addPlaceHolder() {
    if (this.placeholderTrigerredByEvent === 'placeholderForm') {
      this.openPlaceHolderFormEmitter.emit();
    }
    this.addPlaceHolderEmitter.emit();
  }

  toggleMergeDialog() {
    this.toggleMergeDialogEmitter.emit();
  }

  onDeleteCardClick() {
    this.deleteCardEmitter.emit();
  }

  onQuickPeekClick() {
    this.openQuickPeekIntoReosurcesCommitments.emit();
  }

  sharePlanningCard() {
    this.sharePlanningCardEmitter.emit();
  }

}
