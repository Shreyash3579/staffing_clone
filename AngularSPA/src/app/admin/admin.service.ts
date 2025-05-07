import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CoreService } from '../core/core.service';
import { SecurityUserDetail } from '../shared/interfaces/securityUserDetail';
import { SecurityGroup } from '../shared/interfaces/securityGroup';

@Injectable()
export class AdminService {

    constructor(private http: HttpClient, private coreService: CoreService) {
    }

    getSecurityUsersDetails(): Observable<SecurityUserDetail[]> {
        return this.http.get<SecurityUserDetail[]>(`
        ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/getAllSecurityUsersDetails`);
    }

    upsertSecurityUser(data: any): Observable<SecurityUserDetail> {
        const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
        const requesParam = {
            'employeeCode': data.employeeCode,
            'roleCodes': data.roleCodes,
            'lastUpdatedBy': lastUpdatedBy,
            'isAdmin': data.isAdmin,
            'override': data.override,
            'notes': data.notes,
            'endDate': data.endDate,
            'userTypeCode': data.userTypeCode,
            'geoType' : data.geoType,
            'officeCodes' : data.officeCodes,
            'serviceLineCodes' : data.serviceLineCodes,
            'positionGroupCodes' : data.positionGroupCodes,
            'levelGrades' : data.levelGrades,
            'practiceAreaCodes' : data.practiceAreaCodes,
            'ringfenceCodes' : data.ringfenceCodes,
            'hasAccessToAISearch' : data.hasAccessToAISearch,
            'hasAccessToStaffingInsightsTool' : data.hasAccessToStaffingInsightsTool,
            'hasAccessToRetiredStaffingTab' : data.hasAccessToRetiredStaffingTab
        };
        return this.http.post<SecurityUserDetail>(`
        ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/securityUser/upsertSecurityUser`, requesParam);
    }

    getSecurityGroupsDetails(): Observable<SecurityGroup[]> {
        return this.http.get<SecurityGroup[]>(`
        ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/securityUser/getAllSecurityGroups`);
    }

    upsertSecurityGroup(securityGroupDataToUpsert: SecurityGroup): Observable<SecurityGroup> {
        securityGroupDataToUpsert.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
       
        return this.http.post<SecurityGroup>(`
        ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/securityUser/upsertSecurityGroup`, securityGroupDataToUpsert);
    }

    deleteSecurityGroup(groupIdToDelete: string) {
        const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
        const requesParam = new HttpParams({
            fromObject: {
                'groupIdToDelete': groupIdToDelete,
                'lastUpdatedBy': lastUpdatedBy
            }
        });
        return this.http.delete<any>(`
          ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/securityUser/deleteSecurityGroup`, { params: requesParam });
    }

    deleteSecurityUser(employeeCode: string) {
        const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
        const requesParam = new HttpParams({
            fromObject: {
                'employeeCode': employeeCode,
                'lastUpdatedBy': lastUpdatedBy
            }
        });
        return this.http.delete<any>(`
          ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/securityUser/deleteSecurityUser`, { params: requesParam });
    }
}
