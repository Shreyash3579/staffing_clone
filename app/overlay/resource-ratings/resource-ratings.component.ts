import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import * as internal from 'events';
import { CoreService } from 'src/app/core/core.service';
import { Resource } from 'src/app/shared/interfaces/resource.interface';
import { ResourceReviewRating } from 'src/app/shared/interfaces/resourceReviewRating.interface';

@Component({
  selector: 'app-resource-ratings',
  templateUrl: './resource-ratings.component.html',
  styleUrls: ['./resource-ratings.component.scss']
})
export class ResourceRatingsComponent implements OnInit, OnChanges {

  @Input() ratings: ResourceReviewRating[];
  @Input() resource: Resource;
  public activeTab = "tab0";
  public review: ResourceReviewRating;
  public hasAccessToSeeReview = false;
  public hcpdSupportEmailId = 'HCPDAdministration@bain.com';
  constructor(private _coreService: CoreService) { }

  ngOnInit(): void {
    this.loggedInUserHasAccessToSeeEmployeeReview()
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.resource) {
      this.loggedInUserHasAccessToSeeEmployeeReview();
    }
  }
  toggleNav(index, event) {
    event.preventDefault()
    this.activeTab = `tab${index}`;
    this.review = this.ratings[index];
  }

  loggedInUserHasAccessToSeeEmployeeReview() {
    this.hasAccessToSeeReview = this._coreService.loggedInUserClaims.Roles.some(x => x === 'BOSS-SuperUser');
    if (!this.hasAccessToSeeReview) {
      this.hasAccessToSeeReview = this.isUserHasAccessToSeeEmployeeReviewAsPerHCPD();
    }
  }

  private isUserHasAccessToSeeEmployeeReviewAsPerHCPD() {
    var loggedInUserHcpdSecurity = this._coreService.loggedInUserClaims.HCPDAccess;
    if (loggedInUserHcpdSecurity != null && loggedInUserHcpdSecurity.SecurityAccessList !== null && this.resource != null) {
      let employeeOperatingOffice = this.resource?.schedulingOffice.officeCode;
      let employeePdGrade = this.resource?.levelGrade;
      let hcpdSecurityPerEmployeeOffice = loggedInUserHcpdSecurity.SecurityAccessList
        ?.filter(x => x.Office === employeeOperatingOffice);
      if (this.doesUserNotHasAccessToSeeReviewForResourceOffice(hcpdSecurityPerEmployeeOffice)) {
        return false;
      }
      if (this.isUserHasAccessToSeeReviewForResource(hcpdSecurityPerEmployeeOffice, employeePdGrade)) {
        return true;
      }

    }
  }

  private doesUserNotHasAccessToSeeReviewForResourceOffice(hcpdSecurityPerResourceOffice) {
    return hcpdSecurityPerResourceOffice?.length <= 0;
  }

  private isUserHasAccessToSeeReviewForResource(hcpdSecurityPerResourceOffice, resourcePdGrade: string) {
    let pdGradesUserCanSeeReviewFor = hcpdSecurityPerResourceOffice[0].PDGradeAccess;
    return pdGradesUserCanSeeReviewFor.includes(resourcePdGrade)
      || this.hasAccessToSeeReviewForAllPDGrades(pdGradesUserCanSeeReviewFor);

  }

  private hasAccessToSeeReviewForAllPDGrades(pdGradesUserCanSeeReviewFor) {
    return pdGradesUserCanSeeReviewFor.includes('')
      || pdGradesUserCanSeeReviewFor.includes(null)
  }

}
