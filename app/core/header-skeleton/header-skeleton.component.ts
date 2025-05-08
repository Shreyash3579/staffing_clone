import { Component, OnInit, OnDestroy } from '@angular/core';
import { Employee } from '../../shared/interfaces/employee.interface';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-header-skeleton',
  templateUrl: './header-skeleton.component.html',
  styleUrls: ['./header-skeleton.component.scss']
})
export class HeaderSkeletonComponent implements OnInit, OnDestroy {

  // -----------------------Local Variables--------------------------------------------//
  employee: Employee;
  environmentName = '';

  constructor() {
  }

  // -----------------------Component LifeCycle Events and Functions--------------------------------------------//
  ngOnInit() {
    this.environmentName = environment.name.toLowerCase();
  }

  // -------------------Component Event Handlers-------------------------------------//


  ngOnDestroy() {
  }
}

