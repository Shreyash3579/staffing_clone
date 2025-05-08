import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule, ErrorHandler } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment'
import { ToastrModule } from 'ngx-toastr';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { HttpConfigInterceptor } from './core/interceptor/httpconfig.interceptor';
import { DragDropModule } from '@angular/cdk/drag-drop';
// import { HttpClientInMemoryWebApiModule } from 'angular-in-memory-web-api';
// import { InMemoryDataService } from './in-memory-data.service';

import { ProjectAllocationsEffects } from './state/effects/project-allocations.effect';
import { GlobalErrorHandler } from './core/services/global-error-handler.service';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { LicenseManager, ModuleRegistry, provideGlobalGridOptions,  } from 'ag-grid-enterprise';
import { AllEnterpriseModule } from 'ag-grid-enterprise';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { MsalBroadcastService, MsalGuard, MsalGuardConfiguration, MsalRedirectComponent, MsalService, MSAL_GUARD_CONFIG, MSAL_INSTANCE } from '@azure/msal-angular';
import { BrowserCacheLocation, InteractionType, IPublicClientApplication, PublicClientApplication } from '@azure/msal-browser';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AadAuthenticationService } from './core/authentication/aad-authentication.service';
import { ResourceCommitmentEffects } from './state/effects/resource-commitment.effect';
import { projectAllocationsReducer } from './state/reducers/project-allocations.reducer';
import { resourceCommitmentReducer } from './state/reducers/resource-commitment.reducer';
import { ProjectOverlayEffects } from './state/effects/project-overlay.effect';
import { ResourceOverlayEffects } from './state/effects/resource-overlay.effect';
import { planningCardOverlayReducer } from './state/reducers/planning-card-overlay.reducer';
import { PlanningCardOverlayEffects } from './state/effects/planning-card-overlay.effect';
import { HomeService } from './home-copy/home.service';
import { UserSpecificDetailsEffects } from './state/effects/user-specific-details.effect';
import { userSpecificDetailsReducer } from './state/reducers/user-specific-details.reducer';
import './shared/prototypes/arrayPrototypes'; 


LicenseManager.setLicenseKey('Using_this_AG_Grid_Enterprise_key_( AG-045130 )_in_excess_of_the_licence_granted_is_not_permitted___Please_report_misuse_to_( legal@ag-grid.com )___For_help_with_changing_this_key_please_contact_( info@ag-grid.com )___( Bain & Company, Inc. )_is_granted_a_( Multiple Applications )_Developer_License_for_( 18 )_Front-End_JavaScript_developers___All_Front-End_JavaScript_developers_need_to_be_licensed_in_addition_to_the_ones_working_with_AG_Grid_Enterprise___This_key_has_not_been_granted_a_Deployment_License_Add-on___This_key_works_with_AG_Grid_Enterprise_versions_released_before_( 16 June 2025 )____[v2]_MTc1MDAyODQwMDAwMA==4f18f2d9647078bb3a21aab19569b416');
ModuleRegistry.registerModules([AllEnterpriseModule]);
provideGlobalGridOptions({ theme: "legacy" });

// export function intializeApp(appInializerService: AppInitializerService): Function {
//   return () => appInializerService.initializeApp();
// }

export function MSALInstanceFactory(aadAuthService: AadAuthenticationService): IPublicClientApplication {
  // return aadAuthService.getMSALInstanceFactory();
  return new PublicClientApplication({
    auth: {
      clientId: environment.azureActiveDirectorySettings.clientId,
      authority: `https://login.microsoftonline.com/${environment.azureActiveDirectorySettings.tenantId}`,
      redirectUri: environment.azureActiveDirectorySettings.redirectUri
    },
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage,
      storeAuthStateInCookie: this.isIE, 
    },
     system: {
      allowRedirectInIframe : true
    }
  });
}

export function MSALGuardConfigFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
  };
}

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserAnimationsModule,
    CoreModule,
    HttpClientModule,
    DragDropModule, 
    /**
     * TODO : Comment before check-in
     * Intercepts HttpClient request and redirects them to InmemoryDataService
     */
    //  HttpClientInMemoryWebApiModule.forRoot(InMemoryDataService, { passThruUnknownUrl: true }),
    AppRoutingModule,
    ToastrModule.forRoot(), // ToastrModule added
    StoreModule.forRoot({
      'projectAllocation': projectAllocationsReducer,
      'resourceCommitment': resourceCommitmentReducer,
      'planningCardOverlay': planningCardOverlayReducer,
      'userSpecificDetails': userSpecificDetailsReducer
    }, {
      runtimeChecks: {
        strictStateImmutability: false,
        strictActionImmutability: false,
      },
    }),
    EffectsModule.forRoot([PlanningCardOverlayEffects,
      ProjectAllocationsEffects, 
      ResourceCommitmentEffects, 
      ProjectOverlayEffects,
      ResourceOverlayEffects,
      UserSpecificDetailsEffects]),
    ServiceWorkerModule.register('ngsw-worker.js',{
      enabled: environment.production
    }), 
    NgbModule
    // for debugging purposes only
    // StoreDevtoolsModule.instrument(),
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpConfigInterceptor,
      multi: true
    },
    {
      provide: ErrorHandler,
      useClass: GlobalErrorHandler
    },
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory,
      deps: [AadAuthenticationService]
    },
    {
      provide: MSAL_GUARD_CONFIG,
      useFactory: MSALGuardConfigFactory
    },
    MsalGuard,
    MsalService,
    MsalBroadcastService ,
    LicenseManager,
    HomeService
    // HttpCancelService,
    // { provide: HTTP_INTERCEPTORS, useClass: ManageHttpInterceptor, multi: true }
  ],
  bootstrap: [AppComponent, MsalRedirectComponent]
})
export class AppModule { }
