import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ConstantsMaster } from './constants/constantsMaster';
import { Observable } from 'rxjs';
import { CortexSkuMapping } from './interfaces/cortex-sku-mapping.interface';
import { CoreService } from '../core/core.service';

@Injectable({
  providedIn: 'root'
})
export class CortexSkuService {

  constructor(private http: HttpClient, private coreService: CoreService) { }

  getCortexSkuMappings(): Observable<CortexSkuMapping[]> {
    return this.http.get<CortexSkuMapping[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.cortexSkuMappings}`);
  }

}
