import { Component, Input, OnInit } from '@angular/core';
import { EmployeeSchoolHistory } from 'src/app/shared/interfaces/employeeSchoolHistory';


@Component({
    selector: 'app-employee-school-history',
    templateUrl: './employee-school-history.component.html',
    styleUrls: ['./employee-school-history.component.scss']
})
export class EmployeeSchoolHistoryComponent implements OnInit {

    @Input() employeeSchoolHistory: EmployeeSchoolHistory[];

    ngOnInit() {

    }
}