import { Component,ElementRef, EventEmitter, OnInit, Output, ViewChild, ViewEncapsulation } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { fromEvent } from 'rxjs';
import { debounceTime, filter, map } from 'rxjs/operators';
import { CasePlanningBoardStaffableTeamViewModel } from 'src/app/shared/interfaces/case-planning-board-staffable-team-view-model';
import { CasePlanningBoardStaffableTeams } from 'src/app/shared/interfaces/case-planning-board-staffable-teams.interface';

@Component({
  selector: 'app-staffable-teams-modal',
  templateUrl: './staffable-teams-modal.component.html',
  encapsulation: ViewEncapsulation.Emulated,
  styleUrls: ['./staffable-teams-modal.component.scss']
})
export class StaffableTeamsModalComponent implements OnInit {
  // Input Params from Parent
  public columnDate: any;
  public staffableTeam: any;// CasePlanningBoardStaffableTeamViewModel;
  public isGcTeamCountVisible: boolean = true;
  public isPegTeamCountVisible: boolean = true;
  //End Input Params from Parent
  
  public filteredStaffableTeams: any;
  staffableTeamsToUpsertList: CasePlanningBoardStaffableTeams[] = [];

  @ViewChild('officeSearchInput', { static: true }) officeSearchInput: ElementRef;
  @Output() upsertCasePlanningBoardStaffableTeamsEmitter = new EventEmitter();

  constructor(private modalRef: BsModalRef) { }

  ngOnInit(): void {
    this.filteredStaffableTeams = JSON.parse(JSON.stringify(this.staffableTeam));
    // this.attachEventToSearchBox();
  }

  attachEventToSearchBox() {

    // this.clearSearch.subscribe(value => {
    //   this.clearSearchBox(value);
    // });

    fromEvent(this.officeSearchInput.nativeElement, 'keyup').pipe(
      map((event: any) => {
        return event.target.value;
      }),
      debounceTime(500),
      filter(text => text.length >= 3)
      // ,distinctUntilChanged() //removing this as it was craeting testin to and fro resources
    ).subscribe((searchText: string) => {
      this.handleSearchQuery(searchText);
    });
  }

  handleSearchQuery(searchText) {
    if (searchText.length >= 3) {
      this.filteredStaffableTeams = this.filterOfficeHierarchyBySearchString(this.filteredStaffableTeams, searchText.toLowerCase());
      
    } else {
      this.filteredStaffableTeams = JSON.parse(JSON.stringify(this.staffableTeam));
    }
  }

  updateCounts() {
    this.upsertCasePlanningBoardStaffableTeamsEmitter.emit({ staffableTeamsToUpsert: this.staffableTeamsToUpsertList, staffableTeam: this.filteredStaffableTeams, weekOf: this.columnDate });
    this.staffableTeamsToUpsertList = [];
    this.closeModal();
  }

  onCountChange(data) {
    // const newValue = Number(data.value);
    // if (!data.value) {
    //   data.value = data.type === 'gc'
    //     ? data.office.gcTeamCount
    //     : data.office.pegTeamCount
    //   return;
    // }
    // data.office.gcTeamCount = data.type === 'gc' ? newValue : data.office.gcTeamCount;
    // data.office.pegTeamCount = data.type === 'peg' ? newValue : data.office.pegTeamCount;

    const objToPass: CasePlanningBoardStaffableTeams = {
      weekOf: this.columnDate,
      officeCode: data.office.officeCode,
      gcTeamCount: data.office.gcTeamCount,
      pegTeamCount: data.office.pegTeamCount,
      lastUpdatedBy: ''
    };

    if (data.type === 'gc') {
      this.staffableTeamsToUpsertList = this.staffableTeamsToUpsertList.filter(y => !(y.officeCode === objToPass.officeCode && y.pegTeamCount === objToPass.pegTeamCount));
    }
    else if (data.type === 'peg') {
      this.staffableTeamsToUpsertList = this.staffableTeamsToUpsertList.filter(y => !(y.officeCode === objToPass.officeCode && y.gcTeamCount === objToPass.gcTeamCount));
    }
    this.staffableTeamsToUpsertList.push(objToPass);
  }

  // Close Modal
  closeModal() {
    this.modalRef.hide();
  }
  
  //filter hierarchy by searchString
  filterOfficeHierarchyBySearchString(officeHierarchy, searchString) {
    let filteredOfficeHeirarchy = JSON.parse(JSON.stringify(officeHierarchy));
    var result = filteredOfficeHeirarchy.staffableTeamChildren.filter(function f(o) {
      if (o.officeName.toLowerCase().includes(searchString)) return true
    
      if (o.staffableTeamChildren) {
        return (o.staffableTeamChildren = o.staffableTeamChildren.filter(f)).length
      }
    })

    if(result && result.length > 0){
      filteredOfficeHeirarchy.staffableTeamChildren = result;
    }else{
      filteredOfficeHeirarchy = [];
    }

    return filteredOfficeHeirarchy;
  }

}
