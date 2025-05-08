import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CoveoAnalyticsClickParams } from 'src/app/shared/interfaces/coveo-analytics-click-params.interface';
import { CoreService } from '../core.service';
import { Observable } from 'rxjs';
import { ResourceCommitment } from 'src/app/shared/interfaces/resourceCommitment';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { map } from 'rxjs/operators';


@Injectable()
export class SeachbarService {
  constructor(private http: HttpClient, private coreService: CoreService) { }

  // This will call coveo API client
  // NOTE: Needs more refactoring
  getSearchByQuery(searhTerm,source) {
    const params = new HttpParams({
        fromObject: {
          'searchTerm': searhTerm,
          'source': source,
          'userDisplayName': this.coreService.loggedInUser.fullName,
          'username': this.coreService.loggedInUser.internetAddress
        }
      });

     return this.http.get<any>(`
     ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/Coveo/searchByQuery`,
      { params: params });
  }

  analyticsClickLog(coveoAnalyticsClick: CoveoAnalyticsClickParams){
    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/Coveo/logClickEvent`, coveoAnalyticsClick);
  }


}
