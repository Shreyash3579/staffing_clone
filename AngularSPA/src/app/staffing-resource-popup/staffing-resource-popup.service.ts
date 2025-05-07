import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { CoreService } from "../core/core.service";
import { ConstantsMaster } from "../shared/constants/constantsMaster";
import { Observable } from "rxjs";
import { ResourceOrCasePlanningViewNote } from "../shared/interfaces/resource-or-case-planning-view-note.interface";

@Injectable({
  providedIn: "root"
})
export class StaffingResourcePopupService {
  constructor(private http: HttpClient, private coreService: CoreService) {}

  getEmployeeInfo(employeeCode): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        employeeCode: employeeCode
      }
    });

    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.employeeInfoWithGxcAffiliations}`,
      { params: params }
    );
  }

  getAllCommitmentsForEmployee(employeeCode, effectiveFromDate): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        employeeCode: employeeCode,
        effectiveFromDate: effectiveFromDate
      }
    });

    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.resourceCommitments}`,
      { params: params }
    );
  }

  getResourceNotes(employeeCode, noteTypeCode): Observable<ResourceOrCasePlanningViewNote[]> {
    const loggedInEmployeeCode = this.coreService.loggedInUser.employeeCode;
    const params = new HttpParams({
      fromObject: {
        employeeCode: employeeCode,
        loggedInEmployeeCode: loggedInEmployeeCode,
        noteTypeCode: noteTypeCode
      }
    });

    return this.http.get<ResourceOrCasePlanningViewNote[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/resourceNotes`,
      { params: params }
    );
  }
}
