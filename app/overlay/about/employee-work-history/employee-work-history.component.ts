import { Component, Input, OnInit } from '@angular/core';
import { EmployeeWorkHistory } from 'src/app/shared/interfaces/employeeWorkHistory';


@Component({
    selector: 'app-employee-work-history',
    templateUrl: './employee-work-history.component.html',
    styleUrls: ['./employee-work-history.component.scss']
})
export class EmployeeWorkHistoryComponent implements OnInit {

    @Input() employeeWorkHistory: EmployeeWorkHistory[];

    ngOnInit() {

    }
}