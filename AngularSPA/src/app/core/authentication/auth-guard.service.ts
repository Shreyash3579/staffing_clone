import { Injectable, inject } from '@angular/core';
import { Router, Route, UrlSegment, ActivatedRouteSnapshot, RouterStateSnapshot, CanMatchFn, CanActivateFn } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { CoreService } from '../core.service';
import { AadAuthenticationService } from './aad-authentication.service';

export const canMatchApplicationRoutes: CanMatchFn = (route: Route, segments: UrlSegment[]) => {
    const authService = inject(AuthGuardService);
      return authService.canLoad(route, segments);
};


// export const canActivateApplicationRoutes: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
//   const authService = inject(AuthGuardService);
//     return authService.canActivate(route, state);
// };

@Injectable({
  providedIn: 'root'
})
export class AuthGuardService  {

  constructor(private router: Router, private coreService: CoreService, private msalService :MsalService, private aadAuthService: AadAuthenticationService) { }

  canLoad(route: Route, segments: UrlSegment[]): boolean {
    const url: string = route.path;
    let queryParams = new URLSearchParams(this.router.getCurrentNavigation().extractedUrl.queryParams)?.toString();

    if(!this.coreService.routeUrl)
      this.coreService.routeUrl = !queryParams ? `/${segments[0].path}` : `/${segments[0].path}?${queryParams}`;

    if (this.isSiteUnderMaintainance()) {
      this.router.navigate(['./offline'], { queryParams: { returnUrl: url } });
      return false;
    }

    if (this.isUserDetailsLoad()) {
      return false;
    }
    return this.redirectUserToAccessiblePage(url);
  }

  private isUserDetailsLoad() {
    return !this.coreService.loggedInUser;
  }

  private hasAccessToRequestedPage(url: string) {
    var accessibleFeatures = this.coreService.loggedInUserClaims?.FeatureAccess.map(x => x.FeatureName);
    var haveAccessToPage = this.getPageAccess(url, accessibleFeatures);
    return haveAccessToPage;
  }

  private getPageAccess(url, accessiblePages) {
    if (!accessiblePages) return false;
    switch (true) {
      case ConstantsMaster.regexUrl.overlay.test(url):
        return accessiblePages.some(x => x === ConstantsMaster.appScreens.page.overlay);
      case ConstantsMaster.regexUrl.analytics.test(url):
        return accessiblePages.some(x => x.includes(ConstantsMaster.appScreens.page.analytics));
      case ConstantsMaster.regexUrl.admin.test(url):
        return accessiblePages.some(x => x === ConstantsMaster.appScreens.page.admin);
      case ConstantsMaster.regexUrl.home.test(url):
        return accessiblePages.some(x => x === ConstantsMaster.appScreens.page.home);
      case ConstantsMaster.regexUrl.resources.test(url):
        return accessiblePages.some(x => x === ConstantsMaster.appScreens.page.resources);
      case ConstantsMaster.regexUrl.casePlanningCopy.test(url):
        return accessiblePages.some(x => x === ConstantsMaster.appScreens.page.casePlanningCopy);
      case ConstantsMaster.regexUrl.casePlanning.test(url):
        return accessiblePages.some(x => x === ConstantsMaster.appScreens.page.casePlanning);
      case ConstantsMaster.regexUrl.intakeForm.test(url):
        return accessiblePages.some(x => x === ConstantsMaster.appScreens.page.intakeForm);
      case ConstantsMaster.regexUrl.staffingInsightsTool.test(url):
        return accessiblePages.some(x => x.includes(ConstantsMaster.appScreens.page.staffingInsightsTool));
      case ConstantsMaster.regexUrl.gainz.test(url):
        return accessiblePages.some(x => x.includes(ConstantsMaster.appScreens.page.gainz));
      case ConstantsMaster.regexUrl.employee.test(url):
        // manually using employee string as 'employee' isn't returned in accessiblePages
        return 'employee';

        // todo : add 'employee' to backend database, and remove hardcoded 'employee' above
        // return accessiblePages.some(x => x === ConstantsMaster.appScreens.page.employee);
      default:
        return false;
    }

  }

  private isSiteUnderMaintainance() {
    //To-Do : Move employee codes to environment.json file
    var employee = [ '57079','60074', '37995', '39209', '58749', '63049', '45088'];
    if (this.coreService.appSettings?.siteUnderMaintainance === 'true' && !employee.includes(this.coreService.loggedInUser?.employeeCode))
      return true;
    else
     return false;
  }

  private isAuthenticated() {
    return this.coreService.loggedInUser?.token != null && this.coreService.loggedInUser?.token !== '';
  }

  private redirectUserToAccessiblePage(url: string) {
    if (!this.isAuthenticated) {
      return this.unauthorizedAccess('application');
    }

    if (this.hasAccessToRequestedPage(url)) {
      return true;
    }

    return this.unauthorizedAccess('page')
  }

  private unauthorizedAccess(accessType) {
    this.router.navigate(['./accessdenied'], { queryParams: { accessType: accessType } });
    return false;
  }
}

