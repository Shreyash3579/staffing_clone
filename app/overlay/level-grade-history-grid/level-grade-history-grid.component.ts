import { Component, ViewEncapsulation, OnInit, OnDestroy, Input } from '@angular/core';
import { DateService } from 'src/app/shared/dateService';
import { LevelGradeTransactionModel } from 'src/app/shared/interfaces/level-grade-transaction.interface';

@Component({
    selector: 'app-level-grade-history',
    templateUrl: './level-grade-history-grid.component.html',
    styleUrls: ['./level-grade-history-grid.component.scss']
})
export class LevelGradeHistoryComponent implements OnInit, OnDestroy {
    @Input() levelGradeHistoryData: LevelGradeTransactionModel[];
    @Input() isLoading = true;

    constructor() { }

    ngOnInit() {
    }

    ngOnDestroy() {
        this.levelGradeHistoryData = null;
    }

    getFormattedDate(date: string) {
        return DateService.convertDateInBainFormat(date);
    }
}
