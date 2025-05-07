import { Component, Directive, ElementRef, EventEmitter, Input, OnInit, Output } from "@angular/core";

@Component({
    selector: 'app-context-menu',
    templateUrl: './context-menu.component.html',
    styleUrls: ['./context-menu.component.scss']
})
export class StaffingContextMenuComponent implements OnInit{
    constructor(private _eref: ElementRef) { }

    @Input() xCordinateValue = 0;
    @Input() yCordinateValue = 0;
    @Input() contextMenuOptions = [];

    @Output() menuOptionClicked = new EventEmitter();

    ngOnInit() {
    }

    menuOptionClick(event) {
        this.menuOptionClicked.emit({option: event});
    }
}