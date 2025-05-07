import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CaseGrouping } from 'src/app/shared/constants/enumMaster';

@Component({
    selector: 'app-case-planning-grouping',
    templateUrl: './case-planning-grouping.component.html',
    styleUrls: ['./case-planning-grouping.component.scss']
})
export class CasePlanningGroupingComponent implements OnInit {
    @Output() onToggleCasePlanningGroup = new EventEmitter<string>();

    selectedGroupingOption = "placeholder";

    constructor() { }

    ngOnInit(): void { }

    onModeChange(event) {
        if (event.target.checked === true) {
            this.selectedGroupingOption = CaseGrouping.FLAGGED
        } else {
            this.selectedGroupingOption = CaseGrouping.ALL;
        }

        this.onToggleCasePlanningGroup.emit(this.selectedGroupingOption);
    }
}