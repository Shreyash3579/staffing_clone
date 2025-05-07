import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { CoreService } from '../core/core.service';
import { BossSearchCriteria } from '../shared/interfaces/azureSearchCriteria.interface';

@Injectable()
export class AzureSearchService {

  // -----------------------Local Variables--------------------------------------------//

  constructor(private http: HttpClient, private coreService: CoreService) {
  }

  // -----------------------Local Functions--------------------------------------------//
  
  getResourcesBySearchStringFromAzure(searchString: string, searchTriggeredFrom : string): Observable<any> {

    var searchStringArray = searchString.split(";");

    const requestBody = {
      'mustHavesSearchString': searchStringArray[0].trim(),
      'niceToHaveSearchString': searchStringArray[1] ? searchStringArray[1].trim() : null,
      'searchTriggeredFrom': searchTriggeredFrom,
      'loggedInUser' : this.coreService.loggedInUser.employeeCode
    }

    return this.http.post<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/search/resourcesBySearchString`, requestBody );
      // `https://localhost:44348/api/search/resourcesBySearchString`, requestBody);
  }

  gesourcesBySearchStringWithinSupply(searchCriteria: BossSearchCriteria): Observable<any> {

    var searchStringArray = searchCriteria.searchString.split(";");

    const requestBody = {
      'mustHavesSearchString': searchStringArray[0].trim(),
      'niceToHaveSearchString': searchStringArray[1] ? searchStringArray[1].trim() : '',
      'searchTriggeredFrom': searchCriteria.searchTriggeredFrom,
      'loggedInUser' : this.coreService.loggedInUser.employeeCode,
      "employeeCodesToSearchIn": searchCriteria.employeeCodesToSearchIn
    }
   

    return this.http.post<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/search/resourcesBySearchStringWithinSupply`, requestBody);
      // `https://localhost:7137/api/search/resourcesBySearchStringWithinSupply`, requestBody);
  }

}
