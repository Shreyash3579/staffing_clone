import { Injectable } from '@angular/core';
import { CoreService } from "./core.service";
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class Logger {

  constructor(private http: HttpClient, private coreService: CoreService) { }

  logExceptionInDatabase(stackTrace: string = '', errorMessage: string = ''): Observable<any> {
    return this.http.post<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/errorlogs/logclienterrors`, { stackTrace: stackTrace, errorMessage: errorMessage });
  };
}
