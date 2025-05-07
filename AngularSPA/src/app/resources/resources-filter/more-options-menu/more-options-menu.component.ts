import { Component, EventEmitter, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-more-options-menu',
  templateUrl: './more-options-menu.component.html',
  styleUrls: ['./more-options-menu.component.scss']
})
export class MoreOptionsMenuComponent implements OnInit {

  moreOptionsList = [
    { label: "Set As Default", value: "default" },
    { label: "Rename", value: "rename" },
    // { label: "Edit Sharing", value: "edit" },
    { label: "Delete", value: "delete" }
  ];

  @Output() defaultEmitter = new EventEmitter();
  @Output() renameEmitter = new EventEmitter();
  @Output() editEmitter = new EventEmitter();
  @Output() deleteEmitter = new EventEmitter();

  constructor() { }

  ngOnInit(): void {
  }

  handleMoreOptionsClick(event) {
    switch (event) {
      case "default":
        this.defaultEmitter.emit();
        break;
      case "rename":
        this.renameEmitter.emit()
        break;
      case "edit":
        this.editEmitter.emit();
        break;
      case "delete":
        this.deleteEmitter.emit();
        break;
    }
  }
}
