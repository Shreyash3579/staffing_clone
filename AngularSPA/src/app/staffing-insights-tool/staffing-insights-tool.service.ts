import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CoreService } from '../core/core.service';
import { Observable } from 'rxjs';
import { StaffingPreferenceForInsightsTool } from '../shared/interfaces/staffingPreferenceForInsightsTool';

@Injectable()
export class StaffingInsightsToolService {

  constructor( private http: HttpClient, 
    private coreService: CoreService
  ) { }

  getEmployeeStaffingPreferences(employeeCode: string): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });
    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingPreferences/getEmployeePreferences`,
      { params: params });
  }

  upsertEmployeePreferences(dataToUpsert: StaffingPreferenceForInsightsTool): Observable<any> {
    dataToUpsert.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.post<StaffingPreferenceForInsightsTool>(`
     ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingPreferences/upsertEmployeePreferences`,dataToUpsert);
  }

}
