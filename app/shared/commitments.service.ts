import { CoreService } from './../core/core.service';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Injectable } from '@angular/core';
import { ResourceCommitment } from './interfaces/resourceCommitment';
import { CommitmentType } from './interfaces/commitmentType.interface';
import { ConstantsMaster } from './constants/constantsMaster';

@Injectable()
export class CommitmentsService {
  constructor(private http: HttpClient, private coreService: CoreService) { }

  getResourcesCommitmentsWithinDateRangeByCommitmentTypes(employeeCodes, startDate, endDate, commitmentTypeCodes?): Observable<ResourceCommitment> {
    const payload = {
      'employeeCodes': employeeCodes,
      'startDate': startDate,
      'endDate': endDate,
      'commitmentTypes': commitmentTypeCodes || null
    };
    return this.http.post<ResourceCommitment>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAggregator/resourcescommitmentsWithinDateRange`, payload);
  }

  getCommitmentTypes(showHidden: boolean = false): Observable<CommitmentType[]> {
    const params = new HttpParams({
      fromObject: {
        'showHidden': showHidden
      }
    });
    return this.http.get<CommitmentType[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.commitmentTypes}`, { params: params });
  }

  upsertCommitmentType(commitmentType: CommitmentType): Observable<CommitmentType> {
    commitmentType.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    
    return this.http.post<CommitmentType>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/commitment/upsertCommitmentTypes`, commitmentType);
  }

}
