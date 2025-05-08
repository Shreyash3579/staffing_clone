// -------------------Angular Operators---------------------------------------//
import { Component, OnInit, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-home-teams',
  templateUrl: './teams.component.html',
  styleUrls: ['./teams.component.scss']
})
export class TeamsComponent implements OnInit {
  // -----------------------Input Variables--------------------------------------------//

  // -----------------------Output Events--------------------------------------------//
  @Output() openAddTeams = new EventEmitter<any>();
  // -----------------------Local variables--------------------------------------------//
  teamPersonSearchString: string;

  constructor() { }

  // -----------------------Component LifeCycle Events and Functions-------------------//

  ngOnInit() {
  }

  // -----------------------Output Event Handlers-----------------------------------//
  openAddTeamsHandler() {
    this.openAddTeams.emit();
  }
}
