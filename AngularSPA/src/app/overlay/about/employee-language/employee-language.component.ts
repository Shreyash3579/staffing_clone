import { Component, Input, OnInit } from '@angular/core';
import { Language } from 'src/app/shared/interfaces/employeeLanguage.interface';

@Component({
  selector: 'app-employee-language',
  templateUrl: './employee-language.component.html',
  styleUrls: ['./employee-language.component.scss']
})
export class EmployeeLanguageComponent implements OnInit {
  @Input() languages: Language[];
  constructor() { }

  ngOnInit(): void {
  }

}
