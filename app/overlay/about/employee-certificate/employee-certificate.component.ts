import { Component, Input, OnInit } from '@angular/core';
import { DateService } from 'src/app/shared/dateService';
import { Certification } from 'src/app/shared/interfaces/employeeCertification.interface';

@Component({
  selector: 'app-employee-certificate',
  templateUrl: './employee-certificate.component.html',
  styleUrls: ['./employee-certificate.component.scss']
})
export class EmployeeCertificateComponent implements OnInit {

  @Input() certificates: Certification[];
  constructor() { }

  ngOnInit(): void {
  }

  getFormattedDate(date: string) {
    if (date == null) {
      return null;
    }
    return DateService.convertDateInBainFormat(date);
  }

}
