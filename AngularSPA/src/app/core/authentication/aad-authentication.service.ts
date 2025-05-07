import { Injectable } from '@angular/core';
import { BrowserCacheLocation, IPublicClientApplication, PublicClientApplication } from '@azure/msal-browser';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AadAuthenticationService {
  public redirectUrl: string;
  private isIE = window.navigator.userAgent.indexOf("MSIE ") > -1 || window.navigator.userAgent.indexOf("Trident/") > -1; // Remove this line to use Angular Universal

  constructor() { }

  public getMSALInstanceFactory() : IPublicClientApplication{
    //TODO: replace hardcoded secrests with GitHub secrets
    return new PublicClientApplication({
      auth: {
        clientId: environment.azureActiveDirectorySettings.clientId,
        authority: `https://login.microsoftonline.com/${environment.azureActiveDirectorySettings.tenantId}`,
        redirectUri: environment.azureActiveDirectorySettings.redirectUri
      },
      cache: {
        cacheLocation: BrowserCacheLocation.LocalStorage,
        storeAuthStateInCookie: this.isIE, 
      }
      // system: {
      //   loggerOptions: {
      //     loggerCallback,
      //     logLevel: LogLevel.Info,
      //     piiLoggingEnabled: false
      //   }
      // }
    });
  }
}
