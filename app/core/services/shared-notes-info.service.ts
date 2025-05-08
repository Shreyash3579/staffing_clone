import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ResourceNotesAlert } from 'src/app/shared/interfaces/resource-notes-alert';
import { CoreService } from '../core.service';

@Injectable({
  providedIn: 'root'
})
export class SharedNotesService {
  constructor(private http: HttpClient, private coreService: CoreService) { }

  getSharedNotes(loggedInEmployeeCode: string): Observable<ResourceNotesAlert[]> {
    const loggedInUser = loggedInEmployeeCode ? loggedInEmployeeCode : this.coreService.loggedInUser.employeeCode;
    return this.http.get<ResourceNotesAlert[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/StaffingAggregator/notesalert?employeeCode=${loggedInUser}`);
  }

  updateSharedNotesStatus(loggedInEmployeeCode: string) {
    return this.http.post(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/Note/updateNotesAlertStatus?employeeCode=${loggedInEmployeeCode}`, null);
  }

  getCaseIntakeAlerts(loggedInEmployeeCode: string): Observable<any> {
    const loggedInUser = loggedInEmployeeCode ? loggedInEmployeeCode : this.coreService.loggedInUser.employeeCode;
    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/caseIntake/caseIntakeAlert?employeeCode=${loggedInUser}`);
  }

  //can use same endpoint for both shared notes and case intake alerts read status update
  updateCaseIntakeAlertsStatus(loggedInEmployeeCode: string) {
    return this.http.post(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/Note/updateNotesAlertStatus?employeeCode=${loggedInEmployeeCode}`, null);
  }

}
