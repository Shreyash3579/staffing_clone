import { Component, Inject, OnDestroy, OnInit } from "@angular/core";
import { CoreService } from "./core/core.service";
import { NavigationStart, Router, RouterEvent } from "@angular/router";
import { AppInsightsService } from "./app-insights.service";
import { SwUpdate } from "@angular/service-worker";
import { UrlService } from "./core/services/url.service";
import { MsalBroadcastService, MsalGuardConfiguration, MsalService, MSAL_GUARD_CONFIG } from "@azure/msal-angular";
import { EventMessage, EventType, InteractionStatus, RedirectRequest } from "@azure/msal-browser";
import { filter, first, takeUntil } from "rxjs/operators";
import { BehaviorSubject, Observable, Subject } from "rxjs";
import { environment } from "src/environments/environment";
import { SignalrService } from "./shared/signalR.service";

import { Store } from "@ngrx/store";
import * as fromUserSpecificDetailsStore from 'src/app/state/reducers/user-specific-details.reducer';
import * as UserSpecificDetailsAction from 'src/app/state/actions/user-specific-details.action';


@Component({
  selector: "app-root",
  templateUrl: "./app.component.html"
})
export class AppComponent implements OnInit, OnDestroy {
  title = "staffing";
  isuserDetailsLoaded = false;
  isSiteUnderMaintainance = false;
  private readonly _destroying$ = new Subject<void>();
  private isLoggedIn = false;
  private login$ = new BehaviorSubject<boolean>(this.isLoggedIn);
  urlStateBeforeAADRedirect = null;

  constructor(
    @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration,
    private coreService: CoreService,
    private appInsightsService: AppInsightsService,
    private swUpdate: SwUpdate,
    private router: Router,
    private urlService: UrlService,
    private broadcastService: MsalBroadcastService,
    private msAuthService: MsalService,
    private signalrService: SignalrService,
    private userSpecificDetailsStore: Store<fromUserSpecificDetailsStore.State>,
  ) {}

  ngOnInit() {

    this.deleteAllCacheStorage();
    if (!environment.azureActiveDirectorySettings.enableAAD) {
      this.loadUserDetailsAndRedirect();
    } else {
      this.saveURLForSuccessfulAADLoginRedirection();
      this.initiateUserAuthenticationFromAzureAD();
    }
    
  }

  saveURLForSuccessfulAADLoginRedirection() {
    /*
        This is done to save the application URL before AAD redirect so that user can be redirected to original URL. If not done, then user will be redirected to the re-direct url set in AZure app registration
        we save the user URL when BOSS is loaded for first time and use that URL  after AAD redirect
      */
    (this.router.events as Observable<RouterEvent>)
      .pipe(
        filter((event: RouterEvent) => event instanceof NavigationStart),
        takeUntil(this._destroying$)
      )
      .subscribe((event) => {
        this.urlStateBeforeAADRedirect = event.url;
      });
  }

  initiateUserAuthenticationFromAzureAD() {
    this.broadcastService.msalSubject$
      .pipe(
        filter((msg: EventMessage) => msg.eventType === EventType.LOGIN_SUCCESS),
        takeUntil(this._destroying$)
      )
      .subscribe((result: EventMessage) => {
        if (result) {
          // we were not able to re-direct to home when we had '/' or ''. That's why applied this specific check.
          // Can think of removing it later
          this.coreService.routeUrl =
            result.payload["state"] === ("/" || "") ? this.coreService.routeUrl : result.payload["state"];
        }
        this.checkAndSetActiveAccount();
      });

    this.checkAndSetActiveAccount();

    if (!this.isLoggedIn) {
      this.login();
    }

    this.login$.pipe(takeUntil(this._destroying$)).subscribe((res) => {
      if (res) {
        const loggedInEmployeeCode = this.checkAndGetImpersonationUserForAutomationTesting(
          this.msAuthService.instance.getAllAccounts()[0]
        );
        this.loadUserDetailsAndRedirect(loggedInEmployeeCode);
      }
    });
  }

  checkAndGetImpersonationUserForAutomationTesting(loggedInEmployeeData) {
    // //NO automtion testing ipersonation allowed in Production
    // if (environment.name === "Production") {
    //   return loggedInEmployeeData.idTokenClaims.employeeid as string;
    // }

    if (
      environment.azureActiveDirectorySettings.staffing_automation_testing_username
        .toLowerCase()
        .includes(loggedInEmployeeData.username.toLowerCase())
    ) {
      return "60074"; //hard-coding tester user code for impersonation while application is run by automation testing
    } else {
     
      return loggedInEmployeeData.idTokenClaims.employeeid as string;
    }
  }

  loadUserDetailsAndRedirect(loggedInEmployeeCode = null) {
    this.loadUserDetails(loggedInEmployeeCode);
    this.notifyUserForNewVersionOfApp();
    this.urlService.setRedirectUrl(this.router);
  }
  

  login() {
    this.broadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None),
        takeUntil(this._destroying$)
      )
      .pipe(first())
      .subscribe(() => {
        if (!this.isLoggedIn) {
          if (this.msalGuardConfig.authRequest) {
            this.msAuthService.loginRedirect({ ...this.msalGuardConfig.authRequest } as RedirectRequest);
          } else {
            let loginRequest = {
              scopes: ["user.read"],
              state: this.urlStateBeforeAADRedirect
            };
            this.msAuthService.loginRedirect(loginRequest);
          }
        }
      });
  }

  checkAndSetActiveAccount() {
    if (!this.isLoggedIn && this.msAuthService.instance.getAllAccounts().length > 0) {
      this.isLoggedIn = this.msAuthService.instance.getAllAccounts().length > 0;
      this.login$.next(this.isLoggedIn);
    }
  }

  checkIfLoggedIn() {
    if (!this.isLoggedIn && this.msAuthService.instance.getAllAccounts().length == 0) {
      this.authenticateUserFromAAD();
    }
  }
  authenticateUserFromAAD() {
    this.msAuthService.loginRedirect();
  }

  private loadUserDetails(loggedInEmployeeCode: string) {
    const url = this.urlStateBeforeAADRedirect || window.location.href.toLowerCase();
    this.coreService.loadEmployee(loggedInEmployeeCode, url).subscribe(() => {
      this.createSignalR(loggedInEmployeeCode);
        this.loadApp();
    });
  }

  private createSignalR(loggedInEmployeeCode: string){
    this.signalrService.createConnection(this.coreService.appSettings.environmentUrl,loggedInEmployeeCode);
    this.signalrService.registerOnServerEvents();
    this.signalrService.startConnection();
  }

  private loadApp() {
    this.checkForSiteMaintainance();
    this.authenticateUser();
    this.userSpecificDetailsStore.dispatch(new UserSpecificDetailsAction.GetMostRecentSharedWithEmployeeGroups());
  }
  private authenticateUser() {
    this.isuserDetailsLoaded = true;
    if (!this.isUserHasToken()) {
      this.router.navigate(["./accessdenied"]);
    } else {
      this.coreService.loadAllUserPreferences().subscribe(() => {
        this.router.navigateByUrl(this.coreService.routeUrl);

      });
    }
    this.appInsightsService.logPageView(this.coreService.loggedInUser.fullName, this.router.url);
  }


  

  private isUserHasToken() {
    return !!this.coreService.loggedInUser.token;
  }

  private checkForSiteMaintainance() {
    //To-Do : Move employee codes to environment.json file
    var employee = ["60074", "37995", "39209", "58749", "63049", "45088"];
    if (
      this.coreService.appSettings.siteUnderMaintainance === "true" &&
      !employee.includes(this.coreService.loggedInUser?.employeeCode)
    ) {
      this.isSiteUnderMaintainance = true;
      this.router.navigate(["./offline"]);
    } else {
      this.isSiteUnderMaintainance = false;
    }
  }

  deleteAllCacheStorage(deleteCacheStorage = false) {
    //clear service worker cache
    if (this.coreService.appSettings.clearLocalStorage || deleteCacheStorage) {
      caches.keys().then(function (names) {
        for (let name of names) caches.delete(name);
      });
    }
  }

  private notifyUserForNewVersionOfApp() {
    if (this.swUpdate.isEnabled) {
      this.swUpdate.versionUpdates.subscribe((event) => {
        if (event.type === "VERSION_READY") {
          if (confirm("New version available. Load New Version?")) {
            this.deleteAllCacheStorage(true);
            window.location.reload();
          }
        }
      });
     
      this.swUpdate.checkForUpdate();
    }
  }

  ngOnDestroy(): void {
    this._destroying$.next(undefined);
    this._destroying$.complete();
  }
}
