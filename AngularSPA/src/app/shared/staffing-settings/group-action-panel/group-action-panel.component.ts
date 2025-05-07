import { Component, EventEmitter, Input, OnInit, Output, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'app-group-action-panel',
  templateUrl: './group-action-panel.component.html',
  styleUrls: ['./group-action-panel.component.scss']
})
export class GroupActionPanelComponent implements OnInit {

  @Input() showEditIcon = false;
  @Input() showDeleteIcon = false;
  @Input() showDefault = false;
  @Input() isDefault = false;
  @Input() isShared = false;
  @Input() defaultType = 1;

  @Output() onEditClickEmitter = new EventEmitter<any>();
  @Output() onDeleteClickEmitter = new EventEmitter<any>();
  @Output() onShareClickEmitter = new EventEmitter<any>();
  @Output() onMakeDefaultClickEmitter = new EventEmitter<any>();
  @Output() onRemoveDefaultClickEmitter = new EventEmitter<any>();

  constructor() { }

  ngOnInit(): void {
  }

  getColor() : string {
    return this.isShared ? "#616568" : "#284ce0";
  }
  
  onEditClick(){
    this.onEditClickEmitter.emit();
  }

  onShareClick(){
    this.onShareClickEmitter.emit();
  }

  onDeleteClick(){
    this.onDeleteClickEmitter.emit();
  }

  onMakeDefaultClick(){
    this.onMakeDefaultClickEmitter.emit();
  }

  onRemoveDefaultClick(){
    this.onRemoveDefaultClickEmitter.emit();
  }
}
