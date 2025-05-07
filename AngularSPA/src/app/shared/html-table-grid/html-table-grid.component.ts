import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from "@angular/core";

@Component({
    selector: 'app-html-grid',
    templateUrl: './html-table-grid.component.html',
    styleUrls: ['./html-table-grid.component.scss']
})
export class HtmlGridComponent {
    @Input() tableData = [];
    @Input() gridTitle: string;
    @Input() tableDef = [];
    @Output() resourceToBeDeleted = new EventEmitter();

    @ViewChild('tableRef', {static: false}) tableRef : ElementRef;
    constructor() {}

    isDataAnArray(data) {
        return Array.isArray(data) && data.length > 0;
    }

    isDataNotAnArray(data) {
        return !Array.isArray(data) && data?.length > 0;
    }

    isDataEmpty(data) {
        return (Array.isArray(data) && data?.length === 0) || data === undefined;
    }
    deleteResource(employeeCode){
        this.resourceToBeDeleted.emit(employeeCode);
    }
}
