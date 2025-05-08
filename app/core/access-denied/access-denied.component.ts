import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { CoreService } from '../core.service';

@Component({
  selector: 'app-access-denied',
  templateUrl: './access-denied.component.html',
  styleUrls: ['./access-denied.component.scss']
})
export class AccessDeniedComponent implements OnInit {

  accessType = 'application';
  redirectToPage = ConstantsMaster.appScreens.page.home;
  constructor(private route: ActivatedRoute,
    private coreService: CoreService,
    private router: Router) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (!!params.accessType && params.accessType === 'page') {
        this.accessType = params.accessType;
        this.redirectUser();
      }
    });
  }

  redirectUser() {
    let accessibleScreenArray = this.coreService.loggedInUserClaims.FeatureAccess.map(x => x.FeatureName);
    if (accessibleScreenArray.some(x => x === ConstantsMaster.appScreens.page.home)) {
      this.redirectToPage = ConstantsMaster.appScreens.page.home.toLowerCase();
    } else if (accessibleScreenArray.some(x => x === ConstantsMaster.appScreens.page.resources)) {
      this.redirectToPage = ConstantsMaster.appScreens.page.resources.toLowerCase();
    } else if (accessibleScreenArray.some(x => x === ConstantsMaster.appScreens.page.analytics)) {
      this.redirectToPage = ConstantsMaster.appScreens.page.analytics.toLowerCase();
    }
    this.router.navigate([`/${this.redirectToPage}`]);
  }

}
