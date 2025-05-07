import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { CoreService } from '../core.service';
import { CommonService } from 'src/app/shared/commonService';
@Injectable({
  providedIn: 'root'
})
export class PlanningCardService {

  constructor(private http: HttpClient, private coreService: CoreService) {
  }

  updatePlanningCard(planningCard: PlanningCard): Observable<PlanningCard> {
    planningCard.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    return this.http.put<PlanningCard>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/planningCard`, planningCard);
  }

  mergePlanningCardAndAllocations(planningCard : PlanningCard, resourceAllocations: ResourceAllocation[], placeholderAllocations: ResourceAllocation[]){
    const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
         
    resourceAllocations = CommonService.updateResourceAllocations(resourceAllocations, lastUpdatedBy);
    
    placeholderAllocations.forEach(alloc => alloc.lastUpdatedBy = this.coreService.loggedInUser.employeeCode);

    const payload = {
      resourceAllocations: resourceAllocations,
      placeholderAllocations : placeholderAllocations,
      planningCard: planningCard
    }

    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/planningCard/mergePlanningCard`, payload);
  }
  
}
