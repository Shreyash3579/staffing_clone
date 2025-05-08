import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse,
  HttpResponse
} from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError, scan, retryWhen, map } from 'rxjs/operators';
import { CoreService } from '../core.service';
import { environment } from 'src/environments/environment';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { LocalStorageService } from 'src/app/shared/local-storage.service';

@Injectable()
export class HttpConfigInterceptor implements HttpInterceptor {

  constructor(private coreService: CoreService,
    private localStorageService: LocalStorageService) { }


  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    const jwtBearerToken = sessionStorage.getItem('jwtBearerToken')

    if (jwtBearerToken && (request.url.search('/api/securityUser/loggedinuser') === -1)
      && (request.url.search('assets') === -1))  {
      request = request.clone({ 
        headers: request.headers
          .set('Authorization', 'Bearer ' + jwtBearerToken) });
    }
    if (this.coreService.loggedInUser) {
      request = request.clone({ 
        headers: request.headers
          .set('EmployeeCode', this.coreService.loggedInUser.employeeCode) });
      
    }

    if (request.url.search("aip") > -1) {
      request = request.clone({
        headers: request.headers
          .set('Ocp-Apim-Subscription-Key', environment.resourcesApiConfig.subscriptionKey)
          .set('APIM-API-Version', environment.resourcesApiConfig.version)
          .set('Authorization', 'Bearer ' + environment.resourcesApiConfig.token)
      });
    };

    return next.handle(request).pipe(
      retryWhen(e => e.pipe(scan((errorCount, error) => {
        if (errorCount >= 1 || request.method.toUpperCase() !== 'GET') {
            throw error;
        }
        return errorCount + 1;
      }, 0)
      )),
      catchError((error: HttpErrorResponse) => {
        return throwError(error);
      }),
      map((httpEvent: HttpEvent<any>) => {
        // Skip request
        if(httpEvent.type === 0){
          return;
        } 

        if (httpEvent instanceof HttpResponse) {
          
          //TODO: Hard coding for now to proivde EMEA practice staffing users access to only EMEA resources data on resources search. 
          //DELETE once integrated multiple-role based office security is implemented           
          const accessibleOfficeCodeListForUser = this.localStorageService.get(ConstantsMaster.localStorageKeys.accessibleOfficeCodeListForUser);

          if(this.coreService.loggedInUserClaims?.Roles?.includes('PracticeStaffing')){
            if(httpEvent.url.includes('employeesBySearchString')){
              var data = httpEvent.body.filter(x => accessibleOfficeCodeListForUser.includes(x.schedulingOffice?.officeCode.toString()));
  
              httpEvent = httpEvent.clone({body: data});
            }
            else if(httpEvent.url.includes('resourcesIncludingTerminatedStaffingBySearchString')){
              var data = httpEvent.body.filter(x => accessibleOfficeCodeListForUser.includes(x.resource.schedulingOffice?.officeCode.toString()));
              httpEvent = httpEvent.clone({body: data});
            }
          }
        }

        return httpEvent;
      })
    );
  }
  
}
