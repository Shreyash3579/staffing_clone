import { Action } from '@ngrx/store';
import { ResourceStaffing } from 'src/app/shared/interfaces/resourceStaffing.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { Commitment } from 'src/app/shared/interfaces/commitment.interface';
import { ResourceFilter } from 'src/app/shared/interfaces/resource-filter.interface';
import { StaffableAsRole } from 'src/app/shared/interfaces/staffableAsRole.interface';
import { ResourceOrCasePlanningViewNote } from 'src/app/shared/interfaces/resource-or-case-planning-view-note.interface';
import { ResourceLastBillableDate } from 'src/app/shared/interfaces/resource-last-billable-date.interface';
import { ResourcesCount } from 'src/app/shared/interfaces/resourcesCount.interface';
import { ResourceViewCD } from 'src/app/shared/interfaces/resource-view-cd.interface';
import { ResourceViewCommercialModel } from 'src/app/shared/interfaces/resource-view-commercial-model.interface';

export enum ResourcesActionTypes {
  LoadResourcesStaffing = '[Resources List Page] Load',
  LoadResourcesStaffingSuccess = '[Resources API] Load Success',
  LoadResourcesStaffingFail = '[Resources API] Load Fail',
  LoadResourcesStaffingBySearchString = '[Resources List Page] Load Resources By Search String',
  LoadResourcesStaffingBySearchStringSuccess = '[Resources API] Load Resources By Search String Success',
  LoadResourcesStaffingBySearchStringFail = '[Resources API] Load Resources By Search String Fail',
  LoadGroupedResourcesStaffing = '[Resources Group List Page] Load',
  LoadResourcesRecentCDList = '[Resources Recent CD List] Load',
  LoadResourcesRecentCDListSuccess = '[Resources Recent CD List] Load Success',
  LoadResourcesCommercialModelList = '[Resources Commercial Model List] Load',
  LoadResourcesCommercialModelListSuccess = '[Resources Commercial Model List]  Load Success',
  UpdateResource = '[Resources List Page] Update Current Resource',
  UpdateResourceSuccess = '[Resources API] Update Current Resource Success',
  UpdateResourceFail = '[Resources API] Update Current Resource Fail',
  DeleteResourceStaffing = '[Resources List Page] Delete Current Resource Staffing',
  DeleteResourcesStaffing = '[Resources List Page] Delete Selected Resources Staffing',
  DeleteResourceStaffingSuccess = '[Resources API] Delete Current Resource Staffing Success',
  DeleteAllocationsCommitmentsStaffing = '[Resources List Page] Delete Selected Allocations Commitments Staffing',
  DeleteAllocationsCommitmentsStaffingSuccess = '[Resources API] Delete Current Allocations Commitments Staffing Success',
  DeleteResourceStaffingFail = '[Resources API] Delete Current Resource Staffing Fail',
  DeleteResourcePlaceholderAllocation = '[Resources List Page] Delete Current Resource Placeholder Allocation',
  DeleteResourcePlaceholderAllocationSuccess = '[Resources API] Delete Current Resource Placeholder Allocation Success',
  AddResourceStaffing = '[Resources List Page] Add Current Resource Staffing',
  AddResourceStaffingSuccess = '[Resources API] Add Current Resource Staffing Success',
  UpsertResourceStaffing = '[Resources List Page] Upsert Current Resource Staffing',
  UpsertResourceStaffingSuccess = '[Resources List Page] Upsert Current Resource Staffing Success',
  UpsertPlaceholderStaffing = '[Resources List Page] Upsert Current Placeholder Staffing',
  UpsertPlaceholderStaffingSuccess = '[Resources List Page] Upsert Current Placeholder Staffing Success',
  UpsertResourceStaffingFail = '[Resources List Page] Upsert Current Resource Staffing Fail',
  ClearSearchData = '[Resources List Page] Remove searched resources data',
  RefreshCaseAndResourceOverlay = '[Case And Resource Overlay] Refresh',
  ResourcesLoader = '[Resources Loader] Loader',
  AddResourceCommitment = '[Resources List Page] Add Current Resource Commitment',
  AddResourceCommitmentSuccess = '[Resources API] Add Current Resource Commitment Success',
  UpdateResourceCommitment = '[Resources List Page] Update Current Resource Commitment',
  UpdateResourceCommitmentSuccess = '[Resources API] Update Current Resource Commitment Success',
  DeleteResourceCommitment = '[Resources List Page] Delete Current Resource Commitment',
  DeleteResourceCommitmentSuccess = '[Resources API] Delete Current Resource Commitment Success',
  LoadResourcesStaffingOnPageScroll = '[Resources List Page] Load On Page Scroll',
  LoadResourcesStaffingOnPageScrollSuccess = '[Resources API] Load On Page Scroll Success',
  LoadSavedResourceFilters = '[Staffing API] Load Saved Resource Filters',
  LoadSavedResourceFiltersSuccess = '[Staffing API] Load Saved Resource Filters Success',
  LoadSavedResourceFiltersFail = '[Staffing API] Load Saved Resource Filters Fail',
  UpsertResourceFilters = '[Staffing API] Upsert Resource Filters',
  UpsertResourceFiltersSuccess = '[Staffing API] Upsert Resource Filters Success',
  UpsertResourceFiltersFail = '[Staffing API] Upsert Resource Filters Fail',
  DeleteSavedResourceFilter = '[Resources Saved Filters Page] Delete Selected Saved Filter',
  DeleteSavedResourceFilterSuccess = '[Resources Saved Filters API] Delete Selected Saved Filter Success',
  ClearResourcesStaffingData = '[Resources Saved Filters API] Clear Store Data On Tab Switch',
  UpsertResourceStaffableAsRole = '[Resources List Page] Upsert Resource Staffable As Role',
  DeleteResourceStaffableAsRole = '[Resources List Page] Delete Resource Staffable As Role',
  UpsertResourceStaffableAsRoleSuccess = '[Resources List Page] Upsert Resource Staffable As Role Success',
  DeleteResourceStaffableAsRoleSuccess = '[Resources List Page] Delete Resource Staffable As Role Success',
  UpsertResourceViewNote = '[Resources List Page] Upsert Resource View Note',
  UpsertResourceViewNoteSuccess = '[Resources List Page] Upsert Resource View Note Success',
  UpsertResourceViewNoteFail = '[Resources List Page] Upsert Resource View Note Fail',
  DeleteResourceViewNotes = '[Resources List Page] Delete Resource View Notes',
  DeleteResourceViewNotesSuccess = '[Resources List Page] Delete Resource View Notes Success',
  DeleteResourceViewNotesFail = '[Resources List Page] Delete Resource View Notes Fail',
  UpsertResourceViewCD = '[Resources List Page] Upsert Resource View Recent CDs',
  UpsertResourceViewCDSuccess = '[Resources List Page] Upsert Resource View Recent CDs Success',
  UpsertResourceViewCDFail = '[Resources List Page] Upsert Resource View Recent CDs Fail',
  DeleteResourceViewCD = '[Resources List Page] Delete Resource View Recent CDs',
  DeleteResourceViewCDSuccess = '[Resources List Page] Delete Resource View Recent CDs Success',
  DeleteResourceViewCDFail = '[Resources List Page] Delete Resource View CDs Fail',
  UpsertResourceViewCommercialModel = '[Resources List Page] Upsert Resource View Recent Commercial Model ',
  UpsertResourceViewCommercialModelSuccess = '[Resources List Page] Upsert Resource View Recent Commercial Model Success',
  DeleteResourceViewCommercialModel= '[Resources List Page] Delete Resource View Commercial Model ',
  DeleteResourceViewCommercialModelSuccess = '[Resources List Page] Delete Resource View Recent Commercial Model Success',
  RefreshLastBillableDate = '[Last Billable Date] Refresh By Employee Codes',
  LoadLastBillableDateForResources = '[Last Billable Date] Load Last Billable Date By Employee Codes',
  LoadLastBillableDateForResourcesSuccess = '[Last Billable Date] Load Last Billable Date By Employee Codes Success',
  LoadLastBillableDateForResourcesFail = '[Last Billable Date] Load Last Billable Date By Employee Codes Fail',
  LoadResourcesCountOnCaseOpp = '[Resources Count] Load Resources Count by case opp Codes',
  LoadResourcesCountOnCaseOppSuccess = '[Resources Count] Load Resources Count by case opp Codes Success',
  LoadResourcesCountOnCaseOppFail = '[Resources Count] Load Resources Count by case opp Codes Fail'
  //RefreshResourcesCountOnCaseOpp = '[Resources Count] Refresh by case opp Codes'

}

export class LoadResourcesStaffing implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesStaffing;

  constructor(public payload: any) { }
}

export class LoadResourcesStaffingSuccess implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesStaffingSuccess;

  constructor(public payload: ResourceStaffing[]) { }
}

export class LoadResourcesStaffingFail implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesStaffingFail;

  constructor(public payload: string) { }
}

export class LoadGroupedResourcesStaffing implements Action {
  readonly type = ResourcesActionTypes.LoadGroupedResourcesStaffing;

  constructor(public payload: any) { }
}

export class LoadResourcesRecentCDList implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesRecentCDList;

  constructor(public payload: any) { }
}

export class LoadResourcesRecentCDListSuccess implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesRecentCDListSuccess;

  constructor(public payload: any) { }
}

export class LoadResourcesCommercialModelList implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesCommercialModelList;

  constructor(public payload: any) { }
}

export class LoadResourcesCommercialModelListSuccess implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesCommercialModelListSuccess;

  constructor(public payload: any) { }
}


export class LoadResourcesStaffingBySearchString implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesStaffingBySearchString;

  constructor(public payload: any) { }
}

export class LoadResourcesStaffingBySearchStringSuccess implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesStaffingBySearchStringSuccess;

  constructor(public payload: ResourceStaffing[]) { }
}

export class LoadResourcesStaffingBySearchStringFail implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesStaffingBySearchStringFail;

  constructor(public payload: string) { }
}

export class UpdateResource implements Action {
  readonly type = ResourcesActionTypes.UpdateResource;

  constructor(public payload: ResourceAllocation) { }
}

export class UpdateResourceSuccess implements Action {
  readonly type = ResourcesActionTypes.UpdateResourceSuccess;

  constructor(public payload: ResourceAllocation) { }
}

export class UpdateResourceFail implements Action {
  readonly type = ResourcesActionTypes.UpdateResourceFail;

  constructor(public payload: string) { }
}

export class DeleteResourceStaffing implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceStaffing;

  constructor(public payload: string) { }
}

export class DeleteResourcePlaceholderAllocation implements Action {
  readonly type = ResourcesActionTypes.DeleteResourcePlaceholderAllocation;

  constructor(public payload: string) { }
}

export class DeleteResourcePlaceholderAllocationSuccess implements Action {
  readonly type = ResourcesActionTypes.DeleteResourcePlaceholderAllocationSuccess;

  constructor(public payload: string) { }
}

export class DeleteResourcesStaffing implements Action {
  readonly type = ResourcesActionTypes.DeleteResourcesStaffing;

  constructor(public payload: string) { }
}

export class DeleteResourceStaffingSuccess implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceStaffingSuccess;

  constructor(public payload: string) { }
}

export class DeleteAllocationsCommitmentsStaffing implements Action {
  readonly type = ResourcesActionTypes.DeleteAllocationsCommitmentsStaffing;

  constructor(public payload: string) { }
}

export class DeleteAllocationsCommitmentsStaffingSuccess implements Action {
  readonly type = ResourcesActionTypes.DeleteAllocationsCommitmentsStaffingSuccess;

  constructor(public payload: any) { }
}

export class DeleteResourceStaffingFail implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceStaffingFail;

  constructor(public payload: string) { }
}

export class AddResourceStaffing implements Action {
  readonly type = ResourcesActionTypes.AddResourceStaffing;

  constructor(public payload: ResourceAllocation[]) { }
}

export class AddResourceStaffingSuccess implements Action {
  readonly type = ResourcesActionTypes.AddResourceStaffingSuccess;

  constructor(public payload: ResourceAllocation[]) { }
}

export class UpsertResourceStaffing implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceStaffing;

  constructor(public payload: any) { }
}

export class UpsertResourceStaffingSuccess implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceStaffingSuccess;

  constructor(public payload: ResourceAllocation[]) { }
}

export class UpsertPlaceholderStaffing implements Action {
  readonly type = ResourcesActionTypes.UpsertPlaceholderStaffing;

  constructor(public payload: any) { }
}

export class UpsertPlaceholderStaffingSuccess implements Action {
  readonly type = ResourcesActionTypes.UpsertPlaceholderStaffingSuccess;

  constructor(public payload: any) { }
}

export class UpsertResourceStaffingFail implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceStaffingFail;

  constructor(public payload: ResourceAllocation[]) { }
}

export class ClearSearchData implements Action {
  readonly type = ResourcesActionTypes.ClearSearchData;

  constructor(public payload: [] = []) { }
}

export class RefreshCaseAndResourceOverlay implements Action {
  readonly type = ResourcesActionTypes.RefreshCaseAndResourceOverlay;

  constructor(public payload: boolean) { }
}
export class ResourcesLoader implements Action {
  readonly type = ResourcesActionTypes.ResourcesLoader;
  constructor(public payload: boolean) { }
}


export class AddResourceCommitment implements Action {
  readonly type = ResourcesActionTypes.AddResourceCommitment;

  constructor(public payload: Commitment[]) { }
}

export class AddResourceCommitmentSuccess implements Action {
  readonly type = ResourcesActionTypes.AddResourceCommitmentSuccess;

  constructor(public payload: Commitment[]) { }
}

export class UpdateResourceCommitment implements Action {
  readonly type = ResourcesActionTypes.UpdateResourceCommitment;

  constructor(public payload: Commitment[]) { }
}

export class UpdateResourceCommitmentSuccess implements Action {
  readonly type = ResourcesActionTypes.UpdateResourceCommitmentSuccess;

  constructor(public payload: Commitment[]) { }
}

export class DeleteResourceCommitment implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceCommitment;

  constructor(public payload: string) { }
}

export class DeleteResourceCommitmentSuccess implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceCommitmentSuccess;

  constructor(public payload: string) { }
}

export class LoadResourcesStaffingOnPageScroll implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesStaffingOnPageScroll;

  constructor(public payload: any) { }
}

export class LoadResourcesStaffingOnPageScrollSuccess implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesStaffingOnPageScrollSuccess;

  constructor(public payload: ResourceStaffing[]) { }
}

export class LoadSavedResourceFilters implements Action {
  readonly type = ResourcesActionTypes.LoadSavedResourceFilters;

  constructor(public payload: any) { }
}

export class LoadSavedResourceFiltersSuccess implements Action {
  readonly type = ResourcesActionTypes.LoadSavedResourceFiltersSuccess;

  constructor(public payload: ResourceFilter[]) { }
}

export class LoadSavedResourceFiltersFail implements Action {
  readonly type = ResourcesActionTypes.LoadSavedResourceFiltersFail;

  constructor(public payload: string) { }
}

export class UpsertResourceFilters implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceFilters;

  constructor(public payload: ResourceFilter[]) { }
}

export class UpsertResourceFiltersSuccess implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceFiltersSuccess;

  constructor(public payload: ResourceFilter[]) { }
}

export class UpsertResourceFiltersFail implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceFiltersFail;

  constructor(public payload: string) { }
}

export class DeleteSavedResourceFilter implements Action {
  readonly type = ResourcesActionTypes.DeleteSavedResourceFilter;

  constructor(public payload: string) { }
}

export class DeleteSavedResourceFilterSuccess implements Action {
  readonly type = ResourcesActionTypes.DeleteSavedResourceFilterSuccess;

  constructor(public payload: string) { }
}

export class ClearResourcesStaffingData implements Action {
  readonly type = ResourcesActionTypes.ClearResourcesStaffingData;
}

export class UpsertResourceStaffableAsRole implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceStaffableAsRole;

  constructor(public payload: any) { }
}

export class DeleteResourceStaffableAsRole implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceStaffableAsRole;

  constructor(public payload: string) { }
}

export class UpsertResourceStaffableAsRoleSuccess implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceStaffableAsRoleSuccess;

  constructor(public payload: any) { }
}

export class DeleteResourceStaffableAsRoleSuccess implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceStaffableAsRoleSuccess;

  constructor(public payload: string) { }
}

export class UpsertResourceViewNote implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceViewNote;

  constructor(public payload: any) { }
}

export class UpsertResourceViewNoteSuccess implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceViewNoteSuccess;

  constructor(public payload: ResourceOrCasePlanningViewNote) { }
}

export class UpsertResourceViewNoteFail implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceViewNoteFail;

  constructor(public payload: any) { }
}

export class DeleteResourceViewNotes implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceViewNotes;

  constructor(public payload: any) { }
}

export class DeleteResourceViewNotesSuccess implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceViewNotesSuccess;

  constructor(public payload: string[]) { }
}

export class DeleteResourceViewNotesFail implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceViewNotesFail;

  constructor(public payload: any) { }
}

export class UpsertResourceViewCD implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceViewCD;

  constructor(public payload: any) { }
}

export class UpsertResourceViewCDSuccess implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceViewCDSuccess;

  constructor(public payload: ResourceViewCD) { }
}

export class UpsertResourceViewCDFail implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceViewCDFail;

  constructor(public payload: any) { }
}

export class DeleteResourceViewCD implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceViewCD;

  constructor(public payload: any) { }
}

export class DeleteResourceViewCDSuccess implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceViewCDSuccess;

  constructor(public payload: string[]) { }
}

export class DeleteResourceViewCDFail implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceViewCDFail;

  constructor(public payload: any) { }
}


export class UpsertResourceViewCommercialModel implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceViewCommercialModel;

  constructor(public payload: any) { }
}

export class UpsertResourceViewCommercialModelSuccess implements Action {
  readonly type = ResourcesActionTypes.UpsertResourceViewCommercialModelSuccess;

  constructor(public payload: ResourceViewCommercialModel) { }
}


export class DeleteResourceViewCommercialModel implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceViewCommercialModel;

  constructor(public payload: any) { }
}

export class DeleteResourceViewCommercialModelSuccess implements Action {
  readonly type = ResourcesActionTypes.DeleteResourceViewCommercialModelSuccess;

  constructor(public payload: string[]) { }
}

export class RefreshLastBillableDate implements Action {
  readonly type = ResourcesActionTypes.RefreshLastBillableDate;

  constructor(public payload: boolean) { }
}

export class LoadLastBillableDateForResources implements Action {
  readonly type = ResourcesActionTypes.LoadLastBillableDateForResources;

  constructor(public payload: any) { }
}

export class LoadLastBillableDateForResourcesSuccess implements Action {
  readonly type = ResourcesActionTypes.LoadLastBillableDateForResourcesSuccess;

  constructor(public payload: ResourceLastBillableDate[], public employeeCodes) { }
}

export class LoadLastBillableDateForResourcesFail implements Action {
  readonly type = ResourcesActionTypes.LoadLastBillableDateForResourcesFail;

  constructor(public payload: string) { }
}

export class LoadResourcesCountOnCaseOpp implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesCountOnCaseOpp;

  constructor(public payload: any) { }
}

export class LoadResourcesCountOnCaseOppSuccess implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesCountOnCaseOppSuccess;

  constructor(public payload: ResourcesCount[]) { }
}

export class LoadResourcesCountOnCaseOppFail implements Action {
  readonly type = ResourcesActionTypes.LoadResourcesCountOnCaseOppFail;

  constructor(public payload: string) { }
}

export type ResourcesActions =
  LoadResourcesStaffing
  | LoadResourcesStaffingSuccess
  | LoadResourcesStaffingFail
  | LoadGroupedResourcesStaffing
  | LoadResourcesRecentCDList
  | LoadResourcesRecentCDListSuccess
  | LoadResourcesCommercialModelList
  | LoadResourcesCommercialModelListSuccess
  | LoadResourcesStaffingBySearchString
  | LoadResourcesStaffingBySearchStringSuccess
  | LoadResourcesStaffingBySearchStringFail
  | UpdateResource
  | UpdateResourceSuccess
  | UpdateResourceFail
  | DeleteResourceStaffing
  | DeleteResourceStaffingSuccess
  | DeleteResourceStaffingFail
  | AddResourceStaffing
  | AddResourceStaffingSuccess
  | UpsertResourceStaffing
  | UpsertResourceStaffingSuccess
  | UpsertPlaceholderStaffing
  | UpsertPlaceholderStaffingSuccess
  | UpsertResourceStaffingFail
  | ClearSearchData
  | RefreshCaseAndResourceOverlay
  | AddResourceCommitment
  | AddResourceCommitmentSuccess
  | UpdateResourceCommitment
  | UpdateResourceCommitmentSuccess
  | DeleteResourceCommitment
  | DeleteResourceCommitmentSuccess
  | ResourcesLoader
  | LoadResourcesStaffingOnPageScroll
  | LoadResourcesStaffingOnPageScrollSuccess
  | LoadSavedResourceFilters
  | LoadSavedResourceFiltersSuccess
  | LoadSavedResourceFiltersFail
  | UpsertResourceFilters
  | UpsertResourceFiltersSuccess
  | UpsertResourceFiltersFail
  | DeleteSavedResourceFilter
  | DeleteSavedResourceFilterSuccess
  | ClearResourcesStaffingData
  | DeleteAllocationsCommitmentsStaffing
  | DeleteAllocationsCommitmentsStaffingSuccess
  | DeleteResourcePlaceholderAllocation
  | DeleteResourcePlaceholderAllocationSuccess
  | UpsertResourceStaffableAsRole
  | DeleteResourceStaffableAsRole
  | UpsertResourceStaffableAsRoleSuccess
  | DeleteResourceStaffableAsRoleSuccess
  | UpsertResourceViewNote
  | UpsertResourceViewNoteSuccess
  | UpsertResourceViewNoteFail
  | DeleteResourceViewNotes
  | DeleteResourceViewNotesSuccess
  | DeleteResourceViewNotesFail
  | UpsertResourceViewCD
  | UpsertResourceViewCDSuccess
  | UpsertResourceViewCDFail
  | DeleteResourceViewCD
  | DeleteResourceViewCDSuccess
  | DeleteResourceViewCDFail
  | UpsertResourceViewCommercialModel
  | UpsertResourceViewCommercialModelSuccess
  | DeleteResourceViewCommercialModel
  | DeleteResourceViewCommercialModelSuccess
  | DeleteResourceViewCDFail
  | RefreshLastBillableDate
  | LoadLastBillableDateForResources
  | LoadLastBillableDateForResourcesSuccess
  | LoadLastBillableDateForResourcesFail
  | LoadResourcesCountOnCaseOpp
  | LoadResourcesCountOnCaseOppSuccess
  | LoadResourcesCountOnCaseOppFail
  ;
