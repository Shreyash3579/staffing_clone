using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class CaseService : ICaseService
    {
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CaseService(ICCMApiClient ccmApiClient, IStaffingApiClient staffingApiClient,
            IResourceApiClient resourceApiClient, IHttpContextAccessor httpContextAccessor)
        {
            _ccmApiClient = ccmApiClient;
            _staffingApiClient = staffingApiClient;
            _resourceApiClient = resourceApiClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CaseData> GetCaseDataByCaseCodes(string oldCaseCodes)
        {
            var casesData = await _ccmApiClient.GetCaseDataByCaseCodes(oldCaseCodes);

            var caseData = casesData.FirstOrDefault();

            var cases = new CaseData
            {
                CaseCode = caseData.CaseCode,
                CaseName = caseData.CaseName,
                ClientCode = caseData.ClientCode,
                ClientName = caseData.ClientName,
                OldCaseCode = caseData.OldCaseCode,
                CaseType = caseData.CaseType,
                ManagingOfficeAbbreviation = caseData.ManagingOfficeAbbreviation,
                StartDate = caseData.StartDate,
                EndDate = caseData.EndDate,
                IsPrivateEquity = caseData.IsPrivateEquity,
                CaseAttributes = caseData.CaseAttributes,
                Type = caseData.Type
            };

            return cases;
        }

        public async Task<CaseDetails> GetCaseDetailsByCaseCode(string oldCaseCode)
        {
            var caseData = await _ccmApiClient.GetCaseDetailsByCaseCode(oldCaseCode);
            //get team size here
            var caseDataWithTeamSizeTask = _staffingApiClient.GetCaseTeamSizeByOldCaseCodes(oldCaseCode);

            var caseOnRollTask = _staffingApiClient.GetCasesOnRollByCaseCodes(oldCaseCode);

            var caseChangesTask = _staffingApiClient.GetCaseChangesByOldCaseCodes(oldCaseCode);

            var staCommitmentDetailsTask = _staffingApiClient.GetProjectSTACommitmentDetails(oldCaseCode, null , null);

            var employeesTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(caseOnRollTask, caseChangesTask, employeesTask, staCommitmentDetailsTask);

            var caseBillingPartnerData =
                employeesTask.Result.FirstOrDefault(r => r.EmployeeCode == caseData.CaseBillingPartnerCode);
            var caseManagerData = employeesTask.Result.FirstOrDefault(r => r.EmployeeCode == caseData.CaseManagerCode);
            var caseRollData = caseOnRollTask.Result.FirstOrDefault();
            var caseChanges = caseChangesTask.Result.FirstOrDefault();
            var caseDataWithTeamSize = caseDataWithTeamSizeTask.Result.FirstOrDefault();
            var staCommitmentDetails = staCommitmentDetailsTask.Result;

            return ConvertToCaseDetailsModel(caseData, caseManagerData, caseBillingPartnerData, caseRollData, caseChanges, caseDataWithTeamSize, staCommitmentDetails);
        }

        public async Task<CaseDetails> GetCaseAndAllocationsByCaseCode(string oldCaseCode)
        {
            var caseData = await _ccmApiClient.GetCaseDetailsByCaseCode(oldCaseCode);

            var resourcesAllocationTask = _staffingApiClient.GetResourceAllocationsByCaseCode(oldCaseCode);
            var employeesTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var officesTask = _ccmApiClient.GetOfficeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();

            await Task.WhenAll(resourcesAllocationTask, employeesTask, officesTask, investmentCategoriesTask);

            var resourcesAllocation = resourcesAllocationTask.Result;
            var employees = employeesTask.Result;
            var offices = officesTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;

            caseData.AllocatedResources =
                ConvertToResourceAssignmentViewModel(resourcesAllocation, caseData, employees, offices, investmentCategories);

            return caseData;
        }

        private string GetCaseTypeBasedOnSelectedFilterValues(CaseData caseData, DateTime startDate, DateTime endDate)
        {
            string type = "";

            if (caseData.StartDate >= startDate)
            {
                type += Constants.DemandType.NewDemand + ",";
            }
            if (caseData.AllocatedResources != null && caseData.AllocatedResources.Count() > 0)
            {
                type += Constants.DemandType.StaffedCase + ",";
            }
            if (caseData.EndDate >= startDate && caseData.EndDate <= endDate)
            {
                type += Constants.DemandType.CaseEnding + ",";
            }
            if (caseData.StartDate < startDate)
            {
                type += Constants.DemandType.ActiveCase + ",";
            }

            return type.TrimEnd(',');
        }

        private IList<CaseData> GetCasesFilteredByDemandTypes(DemandFilterCriteria filterObj, IList<CaseData> cases)
        {
            var filteredCases = new List<CaseData>();

            foreach (var caseData in cases)
            {
                string type = GetCaseTypeBasedOnSelectedFilterValues(caseData, filterObj.StartDate, filterObj.EndDate);

                var demandTypesFilter = filterObj.DemandTypes?.Split(',').FirstOrDefault(x => type.Contains(x));

                if (!string.IsNullOrEmpty(demandTypesFilter))
                {
                    filteredCases.Add(caseData);
                }
            }

            return filteredCases;

        }

        private IList<CaseData> GetCasesFilteredByPracticeArea(DemandFilterCriteria filterObj, IList<CaseData> cases)
        {
            var filteredCases = cases;

            if (!string.IsNullOrEmpty(filterObj.IndustryPracticeAreaCodes))
            {
                filteredCases = filteredCases?.Where(x => filterObj.IndustryPracticeAreaCodes.Split(",")
                                                                .Contains(x.IndustryPracticeAreaCode?.ToString())).ToList();
            }
            if (!string.IsNullOrEmpty(filterObj.CapabilityPracticeAreaCodes))
            {
                filteredCases = filteredCases?.Where(x => filterObj.CapabilityPracticeAreaCodes.Split(",")
                                                                .Contains(x.CapabilityPracticeAreaCode?.ToString())).ToList();
            }

            return filteredCases;
        }

        private IList<CaseData> GetCasesFilteredByCaseTypes(DemandFilterCriteria filterObj, IList<CaseData> cases)
        {
            var filteredCases = cases;

            if (!string.IsNullOrEmpty(filterObj.CaseTypeCodes))
            {
                filteredCases = filteredCases?.Where(x => filterObj.CaseTypeCodes.Split(",")
                                                                .Contains(x.CaseTypeCode?.ToString())).ToList();
            }

            return filteredCases;
        }

        private IList<CaseData> UpdateDemandTypeByStartDate(DemandFilterCriteria filterObj, IList<CaseData> cases)
        {
            foreach (var caseData in cases)
            {
                caseData.Type = caseData.StartDate >= filterObj.StartDate ? Constants.DemandType.NewDemand : Constants.DemandType.ActiveCase;
            }
            return cases;
        }

        private IList<CaseData> GetCasesFilteredByDateRange(DemandFilterCriteria filterObj, IList<CaseData> cases)
        {
            var filteredCases = cases;
            filteredCases = filteredCases?.Where(x => x.StartDate >= filterObj.StartDate && x.StartDate <= filterObj.EndDate).ToList();

            return filteredCases;
        }

        private IList<CaseData> GetOngoingCasesFilteredByDateRange(DemandFilterCriteria filterObj, IList<CaseData> cases)
        {
            var filteredCases = cases;
            filteredCases = filteredCases?.Where(x => x.StartDate < filterObj.EndDate && x.EndDate >= filterObj.StartDate).ToList();

            return filteredCases;
        }



        private IList<CaseData> GetCasesFilteredByStaffFromSupply(DemandFilterCriteria filterObj, IEnumerable<string> planningBoardOldCaseCodes, IList<CaseData> cases)
        {
            if (filterObj.IsStaffedFromSupply)
            {
                cases = cases?.Where(x => planningBoardOldCaseCodes.Contains(x.OldCaseCode)).ToList();
            }

            return cases;
        }

        public async Task<IEnumerable<CaseData>> GetNewDemandCasesAndAllocationsByOffices(DemandFilterCriteria filterObj, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps,
            IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply, string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(filterObj.OfficeCodes) || string.IsNullOrEmpty(filterObj.CaseTypeCodes))
                return Enumerable.Empty<CaseData>();

            #region CCM API Calls

            Task<IList<CaseData>> newDemandsTask;
            //If only cases staffed by supply is selected then do not fetch cases basis demand filter
            if (filterObj.DemandTypes != Constants.DemandType.CasesStaffedBySupply)
            {
                newDemandsTask = _ccmApiClient.GetNewDemandCasesByOffices(filterObj.OfficeCodes, filterObj.CaseTypeCodes, filterObj.StartDate, filterObj.EndDate, filterObj.ClientCodes);
            }
            else
            {
                newDemandsTask = Task.FromResult<IList<CaseData>>(new List<CaseData>());
            }

            var lstOldCaseCodesFromSupply = (allocationsStaffedBySupply != null) ? string.Join(",", allocationsStaffedBySupply?.Select(x => x.OldCaseCode).Distinct()) : "";
            var pinnedCasesAndCasesStaffedBySupplyTask = _ccmApiClient.GetCaseDataByCaseCodes((filterObj.CaseExceptionShowList + "," + lstOldCaseCodesFromSupply).Trim(','));

            await Task.WhenAll(newDemandsTask, pinnedCasesAndCasesStaffedBySupplyTask);

            #endregion

            var unfilteredNewDemandCasesData = newDemandsTask.Result;
            var pinnedCasesAndCasesStaffedBySupply = pinnedCasesAndCasesStaffedBySupplyTask.Result;

            var newDemandCasesData = GetCasesIncludingStaffedBySupplyAndUserPreferences(filterObj, unfilteredNewDemandCasesData, pinnedCasesAndCasesStaffedBySupply, lstOldCaseCodesFromSupply, Constants.DemandType.NewDemand);

            var newDemandCasesAllocationAndSkuTerms = await GetCasesAllocationsAndSkuTermsData(newDemandCasesData, filterObj.StartDate, filterObj.EndDate, employeesIncludingTerminated, offices,
                skuTerms, investmentCategories, lstCasesUpdatedInBOSSInDateRange, loggedInUser);

            var pinnedNewDemands = newDemandCasesAllocationAndSkuTerms.Where(c => (bool)filterObj.CaseExceptionShowList?.Contains(c.OldCaseCode)).ToList();
            var newDemandsStaffedBySupply = newDemandCasesAllocationAndSkuTerms.Where(c => (bool)lstOldCaseCodesFromSupply?.Contains(c.OldCaseCode)).ToList();
            newDemandCasesAllocationAndSkuTerms = newDemandCasesAllocationAndSkuTerms.Except(pinnedNewDemands).ToList();
            newDemandCasesAllocationAndSkuTerms = newDemandCasesAllocationAndSkuTerms.Except(newDemandsStaffedBySupply).ToList();

            var newDemands = GetCasesFilteredByPracticeArea(filterObj, newDemandCasesAllocationAndSkuTerms);
            newDemands = GetCasesFilteredByDemandTypes(filterObj, newDemands);

            if (newDemands.Count > 0)
            {
                newDemands = await FilterCasesByCaseAttributes(filterObj, newDemands, revenueByServiceLinesOpps);
            }

            if (filterObj.IsStaffedFromSupply)
            {
                newDemands = newDemands?.Where(x => planningBoardOldCaseCodes.Contains(x.OldCaseCode)).ToList();
            }

            newDemands = newDemands.Concat(newDemandsStaffedBySupply).ToList();

            newDemands = GetCasesFilteredByStaffFromSupply(filterObj, planningBoardOldCaseCodes, newDemands);

            newDemands = newDemands.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();

            ((List<CaseData>)newDemands).InsertRange(0, pinnedNewDemands);

            return newDemands;
        }

        public async Task<IEnumerable<CaseData>> GetNewDemandCasesAndAllocationsByOfficesForNewStaffingTab(DemandFilterCriteria filterObj,IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply,  IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps,
            string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(filterObj.OfficeCodes) ||
                string.IsNullOrEmpty(filterObj.CaseTypeCodes) ||
                (!filterObj.DemandTypes.Contains(Constants.DemandType.NewDemand) && !filterObj.DemandTypes.Contains(Constants.DemandType.CasesStaffedBySupply)))
                return Enumerable.Empty<CaseData>();

            var lstOldCaseCodesFromSupply = (allocationsStaffedBySupply != null) ? string.Join(",", allocationsStaffedBySupply?.Select(x => x.OldCaseCode).Distinct()) : "";
            var casesStaffedBySupplyTask = _ccmApiClient.GetCaseDataByCaseCodes(lstOldCaseCodesFromSupply);

            Task<IList<CaseData>> newDemandsCasesTask;

            if (filterObj.DemandTypes != Constants.DemandType.CasesStaffedBySupply)
            {
                newDemandsCasesTask = _ccmApiClient.GetNewDemandCasesByOffices(filterObj.OfficeCodes, filterObj.CaseTypeCodes, filterObj.StartDate, filterObj.EndDate, filterObj.ClientCodes);
            }

            else
            {
                newDemandsCasesTask = Task.FromResult<IList<CaseData>>(new List<CaseData>());
            }

            await Task.WhenAll(casesStaffedBySupplyTask, newDemandsCasesTask);

            var casesStaffedBySupply = casesStaffedBySupplyTask.Result;
            var newDemandCases = newDemandsCasesTask.Result;


            newDemandCases = newDemandCases.Concat(casesStaffedBySupply.Where(x => !newDemandCases.Select(y => y.OldCaseCode).Contains(x.OldCaseCode))).ToList();

            var newDemandCasesAllocationAndSkuTerms = await GetCasesAllocationsAndSkuTermsData(newDemandCases, filterObj.StartDate, filterObj.EndDate, employeesIncludingTerminated, offices,
                skuTerms, investmentCategories, lstCasesUpdatedInBOSSInDateRange, loggedInUser);

            var newDemandsStaffedBySupply = newDemandCasesAllocationAndSkuTerms.Where(c => (bool)lstOldCaseCodesFromSupply?.Contains(c.OldCaseCode)).ToList();
            newDemandCasesAllocationAndSkuTerms = newDemandCasesAllocationAndSkuTerms.Except(newDemandsStaffedBySupply).ToList();

            IList<CaseData> newDemands = await FilterCasesByFilterCriteria(filterObj, revenueByServiceLinesOpps, planningBoardOldCaseCodes, newDemandCasesAllocationAndSkuTerms);
            newDemands = newDemands.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();

            newDemands = newDemands.Concat(newDemandsStaffedBySupply).ToList();

            return newDemands;
        }

        public async Task<IEnumerable<CaseData>> GetPinnedCasesDetails(DemandFilterCriteria filterObj, string caseExceptionShowList, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps,
            string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(caseExceptionShowList))
                return Enumerable.Empty<CaseData>();

            var pinnedCases = await _ccmApiClient.GetCaseDataByCaseCodes(caseExceptionShowList);

            var startDate = filterObj != null ? filterObj.StartDate : pinnedCases.FirstOrDefault().StartDate;
            var endDate = filterObj != null ? filterObj.EndDate : pinnedCases.FirstOrDefault().EndDate;

            //TODO: Check with team if sku terms required between the date range for pinned cases and uncomment accordingly
            var pinnedCasesAllocationAndSkuTerms = await GetCasesAllocationsAndSkuTermsData(pinnedCases, startDate, endDate, employeesIncludingTerminated, offices,
                skuTerms, investmentCategories, lstCasesUpdatedInBOSSInDateRange, loggedInUser);

            //IList<CaseData> newDemands = await FilterCasesByFilterCriteria(filterObj, revenueByServiceLinesOpps, planningBoardOldCaseCodes, pinnedCasesAllocationAndSkuTerms);
            pinnedCasesAllocationAndSkuTerms = pinnedCasesAllocationAndSkuTerms.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();

            return pinnedCasesAllocationAndSkuTerms;
        }

        public async Task<IEnumerable<CaseData>> GetHiddenCasesDetails(DemandFilterCriteria filterObj, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps,
            string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(filterObj.OfficeCodes) ||
                string.IsNullOrEmpty(filterObj.CaseTypeCodes) ||
                string.IsNullOrEmpty(filterObj.CaseExceptionHideList))
                return Enumerable.Empty<CaseData>();

            var hiddenCases = await _ccmApiClient.GetCaseDataByCaseCodes(filterObj.CaseExceptionHideList);

            var hiddenCasesWithDetails = await GetCasesAllocationsAndSkuTermsData(hiddenCases, filterObj.StartDate, filterObj.EndDate, employeesIncludingTerminated, offices,
                skuTerms, investmentCategories, lstCasesUpdatedInBOSSInDateRange, loggedInUser);

            //IList<CaseData> newDemands = await FilterCasesByFilterCriteria(filterObj, revenueByServiceLinesOpps, planningBoardOldCaseCodes, hiddenCasesAllocationAndSkuTerms);
            hiddenCasesWithDetails = hiddenCasesWithDetails.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();

            return hiddenCasesWithDetails;
        }

        private async Task<IList<CaseData>> FilterCasesByFilterCriteria(DemandFilterCriteria filterObj, IList<Revenue> revenueByServiceLinesOpps, IEnumerable<string> planningBoardOldCaseCodes, IList<CaseData> newDemandCasesAllocationAndSkuTerms)
        {
            var newDemands = GetCasesFilteredByPracticeArea(filterObj, newDemandCasesAllocationAndSkuTerms);
            
            if (filterObj.DemandTypes?.Length > 0)
            {
                newDemands = GetCasesFilteredByDemandTypes(filterObj, newDemands);
            }

            if (newDemands.Count > 0)
            {
                newDemands = await FilterCasesByCaseAttributes(filterObj, newDemands, revenueByServiceLinesOpps);
            }

            if (filterObj.IsStaffedFromSupply)
            {
                newDemands = newDemands?.Where(x => planningBoardOldCaseCodes.Contains(x.OldCaseCode)).ToList();
            }

            newDemands = GetCasesFilteredByStaffFromSupply(filterObj, planningBoardOldCaseCodes, newDemands);
            return newDemands;
        }
        public async Task<IEnumerable<CaseData>> GetNewDemandCasesByOldCaseCodesAndFilterValues(string oldCaseCodes, DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IList<Revenue> revenueByServiceLinesCases, string loggedInUser = null)
        {
            if ((string.IsNullOrEmpty(filterObj.OfficeCodes) || string.IsNullOrEmpty(filterObj.CaseTypeCodes)) && string.IsNullOrEmpty(oldCaseCodes))
                return Enumerable.Empty<CaseData>();

            #region CCM API Calls

            Task<IList<CaseData>> newDemandsByFilterValuesTask;
            //If only cases staffed by supply is selected then do not fetch cases basis demand filter
            if (filterObj.DemandTypes != Constants.DemandType.CasesStaffedBySupply)
            {
                newDemandsByFilterValuesTask = _ccmApiClient.GetNewDemandCasesByOffices(filterObj.OfficeCodes, filterObj.CaseTypeCodes, filterObj.StartDate, filterObj.EndDate, filterObj.ClientCodes);
            }
            else
            {
                newDemandsByFilterValuesTask = Task.FromResult<IList<CaseData>>(new List<CaseData>());
            }

            var newDemandsByOldCaseCodesTask = _ccmApiClient.GetCaseDataByCaseCodes(oldCaseCodes);

            await Task.WhenAll(newDemandsByFilterValuesTask, newDemandsByOldCaseCodesTask);

            #endregion
            var newDemandsByFilterValues = newDemandsByFilterValuesTask.Result;

            var newDemandsByOldCaseCodes = newDemandsByOldCaseCodesTask.Result;
            newDemandsByOldCaseCodes = GetCasesFilteredByCaseTypes(filterObj, newDemandsByOldCaseCodes);
            newDemandsByOldCaseCodes = UpdateDemandTypeByStartDate(filterObj, newDemandsByOldCaseCodes);

            var cases = newDemandsByFilterValues.Concat(newDemandsByOldCaseCodes).ToList();

            if (cases.Count > 0)
            {
                cases = cases.GroupBy(x => x.OldCaseCode).Select(grp => grp.First()).ToList(); //get distinct cases
                cases = GetCasesFilteredByDemandTypes(filterObj, cases).ToList();
                cases = (List<CaseData>)await FilterCasesByCaseAttributes(filterObj, cases, revenueByServiceLinesCases); //TODO: test this since filtering doesn't use SKU logic here
                cases = GetCasesFilteredByPracticeArea(filterObj, cases).ToList();
            }
            var lstOldCaseCodes = string.Join(',', cases.Select(x => x.OldCaseCode.ToString()));
            var caseSkuTermsTask = _staffingApiClient.GetSKUTermForProjects(lstOldCaseCodes, null, null);

            var casesOnRollTask = _staffingApiClient.GetCasesOnRollByCaseCodes(lstOldCaseCodes);
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var caseChangesByOldCaseCodesTask = _staffingApiClient.GetCaseChangesByOldCaseCodes(lstOldCaseCodes);
            //get team size here
            var caseDataWithCortexTeamSizeTask = _staffingApiClient.GetCaseTeamSizeByOldCaseCodes(lstOldCaseCodes);
            //get case view notes here
            Task<IEnumerable<CaseViewNote>> casePlanningViewNotesTask;
            if (!string.IsNullOrEmpty(loggedInUser))
            {
                casePlanningViewNotesTask = _staffingApiClient.GetLatestCaseViewNotes(lstOldCaseCodes, null, null, loggedInUser);
            }
            else
            {
                casePlanningViewNotesTask = Task.FromResult<IEnumerable<CaseViewNote>>(new List<CaseViewNote>());
            }

            await Task.WhenAll(caseSkuTermsTask, casesOnRollTask, caseRoleTypeListTask, caseChangesByOldCaseCodesTask, caseDataWithCortexTeamSizeTask, casePlanningViewNotesTask);

            var caseSkuDemand = caseSkuTermsTask.Result;
            var casesOnRoll = casesOnRollTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var caseChanges = caseChangesByOldCaseCodesTask.Result;
            var caseDataWithCortexTeamSize = caseDataWithCortexTeamSizeTask.Result;
            var casePlanningViewNotes = casePlanningViewNotesTask.Result;

            caseChanges.Join(cases, (caseChange) => caseChange.OldCaseCode, (caseData) => caseData.OldCaseCode, (caseChange, caseData) =>
            {
                caseData.Notes = caseChange.Notes;
                caseData.CaseServedByRingfence = caseChange.CaseServedByRingfence;
                caseData.StaffingOfficeCode = caseChange.StaffingOfficeCode;

                return caseData;
            }).ToList();

            cases = GetCasesFilteredByDateRange(filterObj, cases).ToList();
            var employees = employeesIncludingTerminated.ToList();

            var newDemands = ConvertToCaseDataModel(cases, employees, skuTermLookup, null, offices, casesOnRoll, caseDataWithCortexTeamSize, caseSkuDemand, casePlanningViewNotes);

            //TODO: remove the following line once new SKU logic is implemented
            //the line is added to prevent filtering of cases by SKU
            foreach (var newDemand in newDemands)
            {
                newDemand.SKUCaseTerms = null;
            };

            newDemands = newDemands
                .Where(x => (!x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.ManagingOfficeCode.ToString()))
                             || x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.StaffingOfficeCode.ToString())).ToList();

            newDemands = newDemands.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();

            return newDemands;
        }

        public async Task<IEnumerable<CaseData>> GetFilteredNewDemandCasesByOldCaseCodes(string oldCaseCodes, DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IList<Revenue> revenueByServiceLinesCases, string loggedInUser = null)
        {
            if ((string.IsNullOrEmpty(filterObj.OfficeCodes) || string.IsNullOrEmpty(filterObj.CaseTypeCodes)) && string.IsNullOrEmpty(oldCaseCodes))
                return Enumerable.Empty<CaseData>();

            var newDemandsByOldCaseCodes = await _ccmApiClient.GetCaseDataByCaseCodes(oldCaseCodes);

            var cases = newDemandsByOldCaseCodes.ToList();
            if (cases.Count > 0)
            {
                cases = cases.GroupBy(x => x.OldCaseCode).Select(grp => grp.First()).ToList(); //get distinct cases
                cases = (List<CaseData>)await FilterCasesByCaseAttributes(filterObj, cases, revenueByServiceLinesCases); //TODO: test this since filtering doesn't use SKU logic here
                cases = GetCasesFilteredByPracticeArea(filterObj, cases).ToList();
            }
            else
                return Enumerable.Empty<CaseData>();

            var lstOldCaseCodes = string.Join(',', cases.Select(x => x.OldCaseCode.ToString()));

            var caseSkuTermsTask = _staffingApiClient.GetSKUTermForProjects(lstOldCaseCodes, null, null);

            var casesOnRollTask = _staffingApiClient.GetCasesOnRollByCaseCodes(lstOldCaseCodes);
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var caseChangesByOldCaseCodesTask = _staffingApiClient.GetCaseChangesByOldCaseCodes(lstOldCaseCodes);
            var caseDataWithCortexTeamSizeTask = _staffingApiClient.GetCaseTeamSizeByOldCaseCodes(lstOldCaseCodes);
            //get team size here

            //get case view notes here
            Task<IEnumerable<CaseViewNote>> casePlanningViewNotesTask;
            if (!string.IsNullOrEmpty(loggedInUser))
            {
                casePlanningViewNotesTask = _staffingApiClient.GetLatestCaseViewNotes(lstOldCaseCodes, null, null, loggedInUser);
            }
            else
            {
                casePlanningViewNotesTask = Task.FromResult<IEnumerable<CaseViewNote>>(new List<CaseViewNote>());
            }

            await Task.WhenAll(caseSkuTermsTask, casesOnRollTask, caseRoleTypeListTask, caseChangesByOldCaseCodesTask, caseDataWithCortexTeamSizeTask, casePlanningViewNotesTask);

            var caseSkuDemand = caseSkuTermsTask.Result;
            var casesOnRoll = casesOnRollTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var caseChanges = caseChangesByOldCaseCodesTask.Result;
            var caseDataWithCortexTeamSize = caseDataWithCortexTeamSizeTask.Result;
            var casePlanningViewNotes = casePlanningViewNotesTask.Result;

            caseChanges.Join(cases, (caseChange) => caseChange.OldCaseCode, (caseData) => caseData.OldCaseCode, (caseChange, caseData) =>
            {
                caseData.Notes = caseChange.Notes;
                caseData.CaseServedByRingfence = caseChange.CaseServedByRingfence;
                caseData.StaffingOfficeCode = caseChange.StaffingOfficeCode;

                return caseData;
            }).ToList();

            var employees = employeesIncludingTerminated.ToList();

            var newDemands = ConvertToCaseDataModel(cases, employees, skuTermLookup, null, offices, casesOnRoll, caseDataWithCortexTeamSize, caseSkuDemand, casePlanningViewNotes);

            //TODO: remove the following line once new SKU logic is implemented
            //the line is added to prevent filtering of cases by SKU
            foreach (var newDemand in newDemands)
            {
                newDemand.SKUCaseTerms = null;
            }

            newDemands = newDemands
                .Where(x => (!x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.ManagingOfficeCode.ToString()))
                             || x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.StaffingOfficeCode.ToString())).ToList();

            newDemands = newDemands.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();

            return newDemands;
        }

        public async Task<IEnumerable<CaseData>> GetNewDemandCasesByFilterValues(DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IList<Revenue> revenueByServiceLinesCases)
        {
            if (string.IsNullOrEmpty(filterObj.OfficeCodes) || string.IsNullOrEmpty(filterObj.CaseTypeCodes))
                return Enumerable.Empty<CaseData>();

            #region CCM API Calls

            IList<CaseData> newDemandsByFilterValues = new List<CaseData>();
            //If only cases staffed by supply is selected then do not fetch cases basis demand filter
            if (filterObj.DemandTypes != Constants.DemandType.CasesStaffedBySupply)
            {
                newDemandsByFilterValues = await _ccmApiClient.GetNewDemandCasesByOffices(filterObj.OfficeCodes, filterObj.CaseTypeCodes, filterObj.StartDate, filterObj.EndDate, filterObj.ClientCodes);
            }

            #endregion
            newDemandsByFilterValues = GetCasesFilteredByDemandTypes(filterObj, newDemandsByFilterValues);

            var cases = newDemandsByFilterValues.ToList();
            if (cases.Count > 0)
            {
                cases = cases.GroupBy(x => x.OldCaseCode).Select(grp => grp.First()).ToList(); //get distinct cases
                cases = (List<CaseData>)await FilterCasesByCaseAttributes(filterObj, cases, revenueByServiceLinesCases); //TODO: test this since filtering doesn't use SKU logic here
                cases = GetCasesFilteredByPracticeArea(filterObj, cases).ToList();
            }
            else
                return Enumerable.Empty<CaseData>();

            var lstOldCaseCodes = string.Join(',', cases.Select(x => x.OldCaseCode.ToString()));

            var caseSkuTermsTask = _staffingApiClient.GetSKUTermForProjects(lstOldCaseCodes, null, null);

            var casesOnRollTask = _staffingApiClient.GetCasesOnRollByCaseCodes(lstOldCaseCodes);
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var caseChangesByOldCaseCodesTask = _staffingApiClient.GetCaseChangesByOldCaseCodes(lstOldCaseCodes);
            //get team size here
            var caseDataWithCortexTeamSizeTask = _staffingApiClient.GetCaseTeamSizeByOldCaseCodes(lstOldCaseCodes);


            await Task.WhenAll(caseSkuTermsTask, casesOnRollTask, caseRoleTypeListTask, caseChangesByOldCaseCodesTask, caseDataWithCortexTeamSizeTask);

            var caseSkuDemand = caseSkuTermsTask.Result;
            var casesOnRoll = casesOnRollTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var caseChanges = caseChangesByOldCaseCodesTask.Result;
            var caseDataWithCortexTeamSize = caseDataWithCortexTeamSizeTask.Result;

            caseChanges.Join(cases, (caseChange) => caseChange.OldCaseCode, (caseData) => caseData.OldCaseCode, (caseChange, caseData) =>
            {
                caseData.Notes = caseChange.Notes;
                caseData.CaseServedByRingfence = caseChange.CaseServedByRingfence;
                caseData.OriginalStaffingOfficeCode = caseData.StaffingOfficeCode;
                caseData.StaffingOfficeCode = caseChange.StaffingOfficeCode;

                return caseData;
            }).ToList();

            var employees = employeesIncludingTerminated.ToList();

            var newDemands = ConvertToCaseDataModel(cases, employees, skuTermLookup, null, offices, casesOnRoll, caseDataWithCortexTeamSize, caseSkuDemand);

            //TODO: remove the following line once new SKU logic is implemented
            //the line is added to prevent filtering of cases by SKU
            foreach (var newDemand in newDemands)
            {
                newDemand.SKUCaseTerms = null;
            };

            newDemands = newDemands
                .Where(x => (!x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.ManagingOfficeCode.ToString()))
                             || x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.StaffingOfficeCode.ToString())).ToList();

            newDemands = newDemands.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();

            return newDemands;
        }

        // TODO:Refactor logic to the best approach. Remove multiple if with same condition
        public async Task<IEnumerable<CaseData>> GetActiveCasesExceptNewDemandsAndAllocationsByOffices(DemandFilterCriteria filterObj, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesCases,
            IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply, string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(filterObj.OfficeCodes) || string.IsNullOrEmpty(filterObj.CaseTypeCodes))
                return Enumerable.Empty<CaseData>();

            var casesData = new List<CaseData>();
            //TODO: refactor this for better understanding. If only cases staffed by supply is selected then do not fetch cases basis demand filter
            if (filterObj.DemandTypes != Constants.DemandType.CasesStaffedBySupply)
            {
                var unfilteredCasesData = await _ccmApiClient.GetActiveCasesExceptNewDemandsByOffices(filterObj.OfficeCodes, filterObj.CaseTypeCodes, filterObj.StartDate, filterObj.EndDate, filterObj.ClientCodes);
                casesData = RemoveUserExceptionHideList(filterObj, unfilteredCasesData);
            }

            var lstOldCaseCodesFromSupply = (allocationsStaffedBySupply != null) ? string.Join(",", allocationsStaffedBySupply?.Select(x => x.OldCaseCode).Distinct()) : "";

            if (filterObj.ProjectStartIndex <= 1)
            {
                var selectedCases = await _ccmApiClient.GetCaseDataByCaseCodes((filterObj.CaseExceptionShowList + "," + lstOldCaseCodesFromSupply).Trim(','));
                var pinnedCasesTemp = selectedCases?.Where(c => c.Type == "ActiveCase" && filterObj.CaseExceptionShowList.Contains(c.OldCaseCode));
                var casesStaffedBySupplyTemp = selectedCases?.Where(c => c.Type == "ActiveCase" && lstOldCaseCodesFromSupply.Contains(c.OldCaseCode));

                casesData.InsertRange(0, pinnedCasesTemp);
                casesData.AddRange(casesStaffedBySupplyTemp);
                casesData = casesData.GroupBy(x => x.OldCaseCode).Select(g => g.FirstOrDefault()).ToList();
            }
            else
            {
                var casesStaffedBySupplyTemp = await _ccmApiClient.GetCaseDataByCaseCodes(lstOldCaseCodesFromSupply);
                casesData.AddRange(casesStaffedBySupplyTemp);
                casesData = casesData.GroupBy(x => x.OldCaseCode).Select(g => g.FirstOrDefault()).ToList();
            }

            var activeCasesExceptNewDemandAllocationAndSkuTerms = await GetCasesAllocationsAndSkuTermsData(casesData, filterObj.StartDate, filterObj.EndDate, employeesIncludingTerminated, offices,
                skuTerms, investmentCategories, lstCasesUpdatedInBOSSInDateRange, loggedInUser);
            var pinnedCases = activeCasesExceptNewDemandAllocationAndSkuTerms.Where(c => (bool)filterObj.CaseExceptionShowList?.Contains(c.OldCaseCode)).ToList();
            var casesStaffedBySupply = activeCasesExceptNewDemandAllocationAndSkuTerms.Where(c => (bool)lstOldCaseCodesFromSupply?.Contains(c.OldCaseCode)).ToList();

            activeCasesExceptNewDemandAllocationAndSkuTerms = activeCasesExceptNewDemandAllocationAndSkuTerms.Except(pinnedCases).ToList();
            activeCasesExceptNewDemandAllocationAndSkuTerms = activeCasesExceptNewDemandAllocationAndSkuTerms.Except(casesStaffedBySupply).ToList();

            activeCasesExceptNewDemandAllocationAndSkuTerms = GetCasesFilteredByPracticeArea(filterObj, activeCasesExceptNewDemandAllocationAndSkuTerms);

            if (filterObj.ProjectStartIndex <= 1)
            {
                var casesIncludingPinned = GetCasesFilteredByDemandTypes(filterObj, activeCasesExceptNewDemandAllocationAndSkuTerms);

                if (casesIncludingPinned.Count > 0)
                {
                    casesIncludingPinned = await FilterCasesByCaseAttributes(filterObj, casesIncludingPinned, revenueByServiceLinesCases);
                }

                casesIncludingPinned = casesIncludingPinned.Concat(casesStaffedBySupply).ToList();

                casesIncludingPinned = GetCasesFilteredByStaffFromSupply(filterObj, planningBoardOldCaseCodes, casesIncludingPinned);

                casesIncludingPinned = casesIncludingPinned.OrderBy(x => x.EndDate).ThenBy(y => y.ClientName).ToList();

                ((List<CaseData>)casesIncludingPinned).InsertRange(0, pinnedCases);
                return casesIncludingPinned;
            }

            activeCasesExceptNewDemandAllocationAndSkuTerms = GetCasesFilteredByDemandTypes(filterObj, activeCasesExceptNewDemandAllocationAndSkuTerms);

            if (activeCasesExceptNewDemandAllocationAndSkuTerms.Count > 0)
            {
                activeCasesExceptNewDemandAllocationAndSkuTerms = await FilterCasesByCaseAttributes(filterObj, activeCasesExceptNewDemandAllocationAndSkuTerms, revenueByServiceLinesCases);
            }

            activeCasesExceptNewDemandAllocationAndSkuTerms = activeCasesExceptNewDemandAllocationAndSkuTerms.Concat(casesStaffedBySupply).OrderBy(x => x.EndDate).ThenBy(y => y.ClientName).ToList();

            activeCasesExceptNewDemandAllocationAndSkuTerms = GetCasesFilteredByStaffFromSupply(filterObj, planningBoardOldCaseCodes, activeCasesExceptNewDemandAllocationAndSkuTerms);

            activeCasesExceptNewDemandAllocationAndSkuTerms = activeCasesExceptNewDemandAllocationAndSkuTerms.OrderBy(x => x.EndDate).ThenBy(y => y.ClientName).ToList();

            return activeCasesExceptNewDemandAllocationAndSkuTerms;
        }


        public async Task<IEnumerable<CaseData>> GetOnGoingCasesAndAllocationsByOfficesForNewStaffingtab(DemandFilterCriteria filterObj, IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesCases,
            string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(filterObj.CaseTypeCodes))
                return Enumerable.Empty<CaseData>();


            var casesDataTask = _ccmApiClient.GetNewDemandCasesByOffices(filterObj.OfficeCodes, filterObj.CaseTypeCodes, filterObj.StartDate, filterObj.EndDate, filterObj.ClientCodes);
           

            var lstOldCaseCodesFromSupply = (allocationsStaffedBySupply != null) ? string.Join(",", allocationsStaffedBySupply?.Select(x => x.OldCaseCode).Distinct()) : "";
            var casesStaffedBySupplyTask = _ccmApiClient.GetCaseDataByCaseCodes(lstOldCaseCodesFromSupply);

            await Task.WhenAll(casesDataTask, casesStaffedBySupplyTask);

            var casesStaffedBySupply = casesStaffedBySupplyTask.Result;
            var casesData = casesDataTask.Result;


            casesData = casesData.Concat(casesStaffedBySupply.Where(x => !casesData.Select(y => y.OldCaseCode).Contains(x.OldCaseCode))).ToList();

            var activeCasesExceptNewDemandAllocationAndSkuTerms = await GetCasesAllocationsAndSkuTermsData(casesData, filterObj.StartDate, filterObj.EndDate, employeesIncludingTerminated, offices,
                skuTerms, investmentCategories, lstCasesUpdatedInBOSSInDateRange, loggedInUser);

            var activeCasesStaffedBySupply = activeCasesExceptNewDemandAllocationAndSkuTerms.Where(c => (bool)lstOldCaseCodesFromSupply?.Contains(c.OldCaseCode)).ToList();
            activeCasesExceptNewDemandAllocationAndSkuTerms = activeCasesExceptNewDemandAllocationAndSkuTerms.Except(activeCasesStaffedBySupply).ToList();

            IList<CaseData> onGoingCases = await FilterCasesByFilterCriteria(filterObj, revenueByServiceLinesCases, planningBoardOldCaseCodes, activeCasesExceptNewDemandAllocationAndSkuTerms);
            onGoingCases = onGoingCases.OrderBy(x => x.EndDate).ThenBy(y => y.ClientName).ToList();

            onGoingCases = onGoingCases.Concat(activeCasesStaffedBySupply).ToList();

            onGoingCases = GetOngoingCasesFilteredByDateRange(filterObj, onGoingCases).ToList();


            return onGoingCases;
        }
        public async Task<IEnumerable<CaseData>> GetCasesForTypeahead(string searchString)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
                return Enumerable.Empty<CaseData>();

            var accessibleOffices = JWTHelper.GetAccessibleOffices(_httpContextAccessor.HttpContext);
            var officeCodes = accessibleOffices != null ? string.Join(",", accessibleOffices) : null;

            var cases = await _ccmApiClient.GetCasesForTypeahead(searchString, officeCodes);

            return cases ?? Enumerable.Empty<CaseData>();
        }

        public async Task<IEnumerable<CaseData>> GetCasesActiveAfterSpecifiedDate(DateTime? date)
        {
            var cases = await _ccmApiClient.GetCasesActiveAfterSpecifiedDate(date);

            return cases ?? Enumerable.Empty<CaseData>();
        }

        #region Private methods

        private static IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(
            IEnumerable<ResourceAssignmentViewModel> resourceAllocations,
            CaseDetails caseItem, IEnumerable<Resource> resources, IEnumerable<Office> offices,
            IEnumerable<InvestmentCategory> investmentCategories)
        {
            var allocations = (from resAlloc in resourceAllocations
                               join res in resources on resAlloc.EmployeeCode equals res.EmployeeCode into resAllocGroups
                               from resource in resAllocGroups.DefaultIfEmpty()
                               join o in offices on resAlloc.OperatingOfficeCode equals o.OfficeCode into resAllocOffices
                               from office in resAllocOffices.DefaultIfEmpty()
                               join ic in investmentCategories on resAlloc.InvestmentCode equals ic.InvestmentCode into resAllocInvestmentCat
                               from investmentCategory in resAllocInvestmentCat.DefaultIfEmpty()
                               select new ResourceAssignmentViewModel()
                               {
                                   Id = resAlloc.Id,
                                   OldCaseCode = caseItem.OldCaseCode,
                                   CaseName = caseItem.CaseName,
                                   ClientName = caseItem.ClientName,
                                   CaseTypeCode = (Constants.CaseType?)caseItem?.CaseTypeCode,
                                   EmployeeCode = resAlloc.EmployeeCode,
                                   EmployeeName = resource?.FullName,
                                   InternetAddress = resource?.InternetAddress,
                                   OperatingOfficeCode = resAlloc.OperatingOfficeCode,
                                   OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                   CurrentLevelGrade = resAlloc.CurrentLevelGrade,
                                   Allocation = resAlloc.Allocation,
                                   StartDate = resAlloc.StartDate,
                                   EndDate = resAlloc.EndDate,
                                   InvestmentCode = resAlloc.InvestmentCode,
                                   InvestmentName = investmentCategory?.InvestmentName,
                                   CaseRoleCode = resAlloc.CaseRoleCode,
                                   LastUpdatedBy = resAlloc.LastUpdatedBy,
                                   CaseStartDate = caseItem?.StartDate,
                                   CaseEndDate = caseItem?.EndDate
                               }).ToList();

            return allocations;
        }
        private static IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(
           IEnumerable<ResourceAssignmentViewModel> resourceAllocations, CaseData caseItem)
        {
            var allocations = resourceAllocations.Select(resAlloc => new ResourceAssignmentViewModel()
            {
                Id = resAlloc.Id,
                OldCaseCode = caseItem.OldCaseCode,
                CaseName = caseItem.CaseName,
                ClientName = caseItem.ClientName,
                CaseTypeCode = (Constants.CaseType?)caseItem?.CaseTypeCode,
                EmployeeCode = resAlloc.EmployeeCode,
                EmployeeName = resAlloc.EmployeeName,
                InternetAddress = resAlloc.InternetAddress,
                OperatingOfficeCode = resAlloc.OperatingOfficeCode,
                OperatingOfficeAbbreviation = resAlloc.OperatingOfficeAbbreviation,
                CurrentLevelGrade = resAlloc.CurrentLevelGrade,
                Allocation = resAlloc.Allocation,
                StartDate = resAlloc.StartDate,
                EndDate = resAlloc.EndDate,
                InvestmentCode = resAlloc.InvestmentCode,
                InvestmentName = resAlloc.InvestmentName,
                CaseRoleCode = resAlloc.CaseRoleCode,
                LastUpdatedBy = resAlloc.LastUpdatedBy,
                Notes = resAlloc.Notes,
                CaseStartDate = caseItem.StartDate,
                CaseEndDate = caseItem.EndDate,
                TerminationDate = resAlloc.TerminationDate,
                CaseRoleName = resAlloc?.CaseRoleName,
                ServiceLineCode = resAlloc.ServiceLineCode,
                ServiceLineName = resAlloc.ServiceLineName,
                CommitmentTypeCode = resAlloc.CommitmentTypeCode,
                CommitmentTypeName = resAlloc.CommitmentTypeName,
                IsPlaceholderAllocation = resAlloc.IsPlaceholderAllocation,
                PositionGroupCode = resAlloc?.PositionGroupCode
            });

            return allocations;
        }

        private static CaseDetails ConvertToCaseDetailsModel(CaseDetails caseData, Resource caseManagerData,
           Resource caseBillingPartnerData, CaseRoll caseRollData, CaseOppChanges caseChanges, CaseOppCortexTeamSize caseDataWithTeamSize, IEnumerable<CaseOppCommitmentViewModel> staCommitmentDetails)
        {
            var caseDetails = new CaseDetails
            {
                CaseCode = caseData.CaseCode,
                CaseName = caseData.CaseName,
                ClientCode = caseData.ClientCode,
                ClientName = caseData.ClientName,
                OldCaseCode = caseData.OldCaseCode,
                CaseManagerCode = caseData.CaseManagerCode,
                CaseManagerFullName = caseManagerData?.FullName,
                CaseManagerOfficeAbbreviation = caseManagerData?.Office.OfficeAbbreviation,
                CaseBillingPartnerCode = caseData.CaseManagerCode,
                CaseBillingPartnerFullName = caseBillingPartnerData?.FullName,
                CaseBillingPartnerOfficeAbbreviation = caseBillingPartnerData?.Office.OfficeAbbreviation,
                PegIndustryTerm = caseData.PegIndustryTerm,
                CaseTypeCode = caseData.CaseTypeCode,
                CaseType = caseData.CaseType,
                PrimaryIndustry = caseData.PrimaryIndustry,
                IndustryPracticeArea = caseData.IndustryPracticeArea,
                PrimaryCapability = caseData.PrimaryCapability,
                CapabilityPracticeArea = caseData.CapabilityPracticeArea,
                ManagingOfficeCode = caseData.ManagingOfficeCode,
                ManagingOfficeAbbreviation = caseData.ManagingOfficeAbbreviation,
                ManagingOfficeName = caseData.ManagingOfficeName,
                BillingOfficeCode = caseData.BillingOfficeCode,
                BillingOfficeAbbreviation = caseData.BillingOfficeAbbreviation,
                BillingOfficeName = caseData.BillingOfficeName,
                StartDate = caseData.StartDate,
                EndDate = caseData.EndDate,
                IsPrivateEquity = caseData.IsPrivateEquity,
                CaseAttributes = caseData.CaseAttributes,
                Type = caseData.Type,
                CaseRoll = caseRollData,
                CaseServedByRingfence = caseChanges?.CaseServedByRingfence ?? caseData.CaseServedByRingfence,
                EstimatedTeamSize = caseDataWithTeamSize?.EstimatedTeamSize,
                PricingTeamSize = caseDataWithTeamSize?.PricingTeamSize,
                PegOpportunityId = caseChanges?.PegOpportunityId,
                isSTACommitmentCreated = staCommitmentDetails?.Any(x => x.OldCaseCode == caseData.OldCaseCode) ?? false
                
            };

            return caseDetails;
        }

        private async Task<IList<CaseData>> FilterCasesByCaseAttributes(DemandFilterCriteria filterObj, IList<CaseData> casesAllocationAndSkuTerms, IList<Revenue> revenueByServiceLinesCases)
        {
            if (string.IsNullOrEmpty(filterObj.CaseAttributeNames))
            {
                return casesAllocationAndSkuTerms
                    .Where(x => !(x.IsPrivateEquity)).ToList();
            }
            var CasesAllocationAndSkuTermsFilteredByCaseAttributes = new List<CaseData>();
            bool isSelectedServicecontainsPEG = false;

            foreach (var selectedServiceLineCode in filterObj.CaseAttributeNames.Split(','))
            {
                if (selectedServiceLineCode == Constants.ServiceLineCodes.PEG || selectedServiceLineCode == Constants.ServiceLineCodes.PEG_SURGE)
                {
                    isSelectedServicecontainsPEG = true;
                    CasesAllocationAndSkuTermsFilteredByCaseAttributes
                        .AddRange(casesAllocationAndSkuTerms.Where(x => x.IsPrivateEquity || (x.CaseServedByRingfence ?? false)));
                }
                if (selectedServiceLineCode == Constants.ServiceLineCodes.AAG)
                {
                    CasesAllocationAndSkuTermsFilteredByCaseAttributes
                       .AddRange(casesAllocationAndSkuTerms.Where(x =>
                           x.CaseAttributes?.Split(",").ToList().Any(a =>
                               a.Contains(Constants.CaseAttribute.AAG, StringComparison.OrdinalIgnoreCase)) ?? false).ToList());
                }
                var caseWithSelectedServiceLine = revenueByServiceLinesCases?.Where(x => x.ServiceLineCode == selectedServiceLineCode).Select(y => (y.CaseCode, y.ClientCode)).ToList();

                if (caseWithSelectedServiceLine != null)
                {
                    CasesAllocationAndSkuTermsFilteredByCaseAttributes.AddRange(
                    casesAllocationAndSkuTerms.Where(x => caseWithSelectedServiceLine.Any(sl => sl.CaseCode == x.CaseCode && sl.ClientCode == x.ClientCode)
                    ).ToList());
                }
            }

            if (!isSelectedServicecontainsPEG)
            {
                CasesAllocationAndSkuTermsFilteredByCaseAttributes.RemoveAll(x => (x.CaseServedByRingfence ?? false) || x.IsPrivateEquity);
            }

            var caseAttributeNameList = CommonUtils.GetServiceLineCodeNames(filterObj.CaseAttributeNames);

            foreach (var caseData in casesAllocationAndSkuTerms)
            {
                var skuTerms = caseData.SKUCaseTerms?.SKUTerms?.Select(s => s.Name).ToList();
                var caseAssignedAttributes = caseData.CaseAttributes?.Split(",").ToList();
                if (caseAssignedAttributes != null && caseAssignedAttributes.Any(c =>
                        caseAttributeNameList.Any(f => c.Contains(f, StringComparison.OrdinalIgnoreCase)) ||
                        skuTerms?.Any(s =>
                            caseAttributeNameList.Any(f => s.StartsWith(f, StringComparison.OrdinalIgnoreCase))) == true))
                {
                    CasesAllocationAndSkuTermsFilteredByCaseAttributes.Add(caseData);
                }
            }

            var casesAllocationAndSkuTermsNotFilteredByCaseAttributes =
                casesAllocationAndSkuTerms.Except(CasesAllocationAndSkuTermsFilteredByCaseAttributes).ToList();

            var casesFilteredByResourceServiceLines = FilterCasesByResourceServiceLines(caseAttributeNameList, casesAllocationAndSkuTermsNotFilteredByCaseAttributes);

            var casesFilteredByCaseAttributesAndResourceServiceLines =
                CasesAllocationAndSkuTermsFilteredByCaseAttributes.Union(await casesFilteredByResourceServiceLines);

            return casesFilteredByCaseAttributesAndResourceServiceLines.ToList();
        }

        private async Task<IEnumerable<CaseData>> FilterCasesByResourceServiceLines(IList<string> caseAttributeNameList, IList<CaseData> cases)
        {
            if (cases.Count <= 0)
                return Enumerable.Empty<CaseData>();

            var caseAttributeNames = string.Join(",", caseAttributeNameList);
            var listCaseCodes = string.Join(",", cases.Select(x => x.OldCaseCode.ToString()));
            var taggedOldCaseCodes = await
                _staffingApiClient.GetCasesByResourceServiceLines(listCaseCodes, caseAttributeNames);
            var casesFilteredByResourceServiceLines = cases.Where(c => taggedOldCaseCodes.Contains(c.OldCaseCode));
            return casesFilteredByResourceServiceLines;
        }

        private static List<CaseData> GetCasesIncludingStaffedBySupplyAndUserPreferences(DemandFilterCriteria filterObj,
            IList<CaseData> unfilteredCasesData, IList<CaseData> pinnedCasesAndCasesStaffedBySupply, string lstOldCaseCodesFromSupply, string demandType)
        {
            //remove cases that were marked as hidden by user
            var filteredCases = unfilteredCasesData
                .Where(x => !(bool)filterObj.CaseExceptionHideList?.Contains(x.OldCaseCode)).ToList();

            //var newDemandsToShowAsPerUserPreferences = selectedCases?.Where(c => filterObj.CaseExceptionShowList.Contains(c.OldCaseCode)).ToList();
            //var casesStaffedBySupplyTemp = selectedCases?.Where(c => c.Type == "ActiveCase" && lstOldCaseCodesFromSupply.Contains(c.OldCaseCode));

            //add cases that were marked as pinned by user
            filteredCases.InsertRange(0, pinnedCasesAndCasesStaffedBySupply.Where(c => filterObj.CaseExceptionShowList.Contains(c.OldCaseCode) && c.Type.Contains(demandType)));
            filteredCases.AddRange(pinnedCasesAndCasesStaffedBySupply.Where(c => lstOldCaseCodesFromSupply.Contains(c.OldCaseCode) && c.Type.Contains(demandType)));

            filteredCases = filteredCases.GroupBy(x => x.OldCaseCode).Select(g => g.FirstOrDefault()).ToList();

            return filteredCases;
        }

        private static List<CaseData> RemoveUserExceptionHideList(DemandFilterCriteria filterObj, IList<CaseData> unfilteredCasesData)
        {
            var casesUserExceptionHideListRemoved = unfilteredCasesData.Where(x => !filterObj.CaseExceptionHideList.Contains(x.OldCaseCode)).ToList();
            return casesUserExceptionHideListRemoved;
        }

        private async Task<IList<CaseData>> GetCasesAllocationsAndSkuTermsData(IList<CaseData> casesData,
            DateTime startDate, DateTime endDate, IEnumerable<Resource> resources, IEnumerable<Office> offices,
            IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, string lstCasesUpdatedInBOSSInDateRange, string loggedInUser = null)
        {
            var listCaseCodes = string.Join(",", casesData.Select(x => x.OldCaseCode.ToString()));

            var caseSkuDemand =
                await _staffingApiClient.GetSKUTermForProjects(listCaseCodes, null, null);

            var caseSkuTerms =
                await _staffingApiClient.GetSKUTermsForCaseOrOpportunityForDuration(listCaseCodes, null, startDate,
                endDate);
            var resourcesAllocationDataTask = _staffingApiClient.GetResourceAllocationsByCaseCodes(listCaseCodes);
            var placeholderAllocationsDataTask = _staffingApiClient.GetPlaceholderAllocationsByCaseCodes(listCaseCodes);
            var casesOnRollTask = _staffingApiClient.GetCasesOnRollByCaseCodes(listCaseCodes);
            var staCommitmentDetailsTask = _staffingApiClient.GetProjectSTACommitmentDetails(listCaseCodes, null, null);
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var caseChangesByOldCaseCodesTask = _staffingApiClient.GetCaseChangesByOldCaseCodes(lstCasesUpdatedInBOSSInDateRange);
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();

            Task<IEnumerable<CaseViewNote>> casePlanningViewNotesTask;
            if (!string.IsNullOrEmpty(loggedInUser))
            {
                casePlanningViewNotesTask = _staffingApiClient.GetCaseViewNotesByOldCaseCodes(listCaseCodes, loggedInUser);
            }
            else
            {
                casePlanningViewNotesTask = Task.FromResult<IEnumerable<CaseViewNote>>(new List<CaseViewNote>());
            }

            await Task.WhenAll(resourcesAllocationDataTask, casesOnRollTask, staCommitmentDetailsTask, placeholderAllocationsDataTask, caseChangesByOldCaseCodesTask, commitmentTypeListTask);

            var resourceAllocations = resourcesAllocationDataTask.Result;
            var casesOnRoll = casesOnRollTask.Result;
            var staCommitmentDetails = staCommitmentDetailsTask.Result;
            var placeholderAllocations = placeholderAllocationsDataTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var caseChanges = caseChangesByOldCaseCodesTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;
            var casePlanningViewNotes = casePlanningViewNotesTask.Result;

            caseChanges.Join(casesData, (caseChange) => caseChange.OldCaseCode, (caseData) => caseData.OldCaseCode, (caseChange, caseData) =>
            {
                caseData.Notes = caseChange.Notes;
                caseData.CaseServedByRingfence = caseChange.CaseServedByRingfence;
                caseData.PegOpportunityId = caseChange.PegOpportunityId;
                return caseData;
            }).ToList();

            resourceAllocations = AttachEmployeeInfo(resourceAllocations, resources, offices, investmentCategories, caseRoleTypeList, commitmentTypeList);
            placeholderAllocations = AttachEmployeeInfo(placeholderAllocations, resources, offices, investmentCategories, caseRoleTypeList, commitmentTypeList);

            var cases = ConvertToCaseDataModelWithoutCortexTeamSize(casesData, resources, resourceAllocations, placeholderAllocations, casesOnRoll,staCommitmentDetails, skuTerms, caseSkuDemand,
                caseSkuTerms, startDate, casePlanningViewNotes);

            return cases.ToList();
        }

        private IEnumerable<CaseData> ConvertToCaseDataModelWithoutCortexTeamSize(IList<CaseData> casesData, IEnumerable<Resource> resources, IList<ResourceAssignmentViewModel> resourceAllocations,
            IList<ResourceAssignmentViewModel> placeholderAllocations, IEnumerable<CaseRoll> casesOnRoll, IEnumerable<CaseOppCommitmentViewModel> staCommitmentDetails, IEnumerable<SKUTerm> skuTerms,
            IEnumerable<SKUDemand> caseSkuDemand, IEnumerable<SKUCaseTerms> caseSkuTerms, DateTime startDate, IEnumerable<CaseViewNote> caseViewNotes)
        {
            var cases = casesData.Select(item => new CaseData
            {
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                CaseCode = item.CaseCode,
                CaseName = item.CaseName,
                OldCaseCode = item.OldCaseCode,
                CaseTypeCode = item.CaseTypeCode,
                CaseType = item.CaseType,
                CaseManagerCode = item.CaseManagerCode,
                CaseManagerName = resources.FirstOrDefault(x => x.EmployeeCode == item.CaseManagerCode)?.FullName,
                CaseManagerOfficeAbbreviation = resources.FirstOrDefault(x => x.EmployeeCode == item.CaseManagerCode)?.Office?.OfficeAbbreviation,
                ManagingOfficeCode = item.ManagingOfficeCode,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                ManagingOfficeName = item.ManagingOfficeName,
                BillingOfficeCode = item.BillingOfficeCode,
                BillingOfficeAbbreviation = item.BillingOfficeAbbreviation,
                BillingOfficeName = item.BillingOfficeName,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Type = item.Type,
                CaseRoll = casesOnRoll.FirstOrDefault(c => c.RolledFromOldCaseCode == item.OldCaseCode),
                isSTACommitmentCreated = staCommitmentDetails?.Any(c => c.OldCaseCode == item.OldCaseCode) ?? false,
                IsPrivateEquity = item.IsPrivateEquity,
                CaseServedByRingfence = item.CaseServedByRingfence,
                CaseAttributes = item.CaseAttributes,
                Notes = item.Notes,
                ClientPriority = item.ClientPriority,
                ClientPrioritySortOrder = item.ClientPrioritySortOrder,
                IndustryPracticeAreaCode = item.IndustryPracticeAreaCode,
                IndustryPracticeArea = item.IndustryPracticeArea,
                CapabilityPracticeAreaCode = item.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = item.CapabilityPracticeArea,
                AllocatedResources = ConvertToResourceAssignmentViewModel(resourceAllocations?.Where(x => x.OldCaseCode == item.OldCaseCode)?
                    .Where(y => (y.EndDate == DateTime.MinValue || y.EndDate >= item.StartDate)
                                && y.EndDate >= startDate), item), //get only active resources                
                PlaceholderAllocations = ConvertToResourceAssignmentViewModel(placeholderAllocations?.Where(x => x.OldCaseCode == item.OldCaseCode), item),
                SkuTerm = caseSkuDemand?.Where(x => x.OldCaseCode == item.OldCaseCode),
                SKUCaseTerms = caseSkuTerms?.Where(x => x.OldCaseCode == item.OldCaseCode)?.Select(x => new SKUCaseTermsViewModel
                {
                    Id = x.Id,
                    PipelineId = x.PipelineId,
                    OldCaseCode = x.OldCaseCode,
                    EffectiveDate = x.EffectiveDate,
                    LastUpdatedBy = x.LastUpdatedBy,
                    SKUTerms = x.SKUTermsCodes?.Split(',')?.Select(int.Parse).ToList().Join(skuTerms,
                        c => c,
                        s => s.Code,
                        (caseSku, skuTerm) => new SKUTerm
                        {
                            Code = skuTerm.Code,
                            Name = skuTerm.Name
                        })
                }).FirstOrDefault(),
                CasePlanningViewNotes = caseViewNotes.Where(x => x.OldCaseCode == item.OldCaseCode),
                PegOpportunityId = item.PegOpportunityId
            });

            return cases.ToList();
        }

        private IEnumerable<CaseData> ConvertToCaseDataModel(IEnumerable<CaseData> casesData, List<Resource> resources, IEnumerable<SKUTerm> skuTerms,
            IEnumerable<SKUCaseTerms> caseSkuTerms, IEnumerable<Office> offices, IEnumerable<CaseRoll> casesOnRoll, IEnumerable<CaseOppCortexTeamSize> caseDataWithCortexTeamSize, IEnumerable<SKUDemand> caseSkuDemand = null, IEnumerable<CaseViewNote> caseViewNotes = null)
        {
            var cases = (from caseData in casesData
                         join casesWithTeamSize in caseDataWithCortexTeamSize on caseData.OldCaseCode equals casesWithTeamSize.OldCaseCode into casesWithTS
                         from caseWithTSData in casesWithTS.DefaultIfEmpty()

                         select new CaseData()
                         {
                             ClientCode = caseData.ClientCode,
                             ClientName = caseData.ClientName,
                             CaseCode = caseData.CaseCode,
                             CaseName = caseData.CaseName,
                             OldCaseCode = caseData.OldCaseCode,
                             CaseTypeCode = caseData.CaseTypeCode,
                             CaseType = caseData.CaseType,
                             CaseManagerCode = caseData.CaseManagerCode,
                             CaseManagerName = resources.FirstOrDefault(x => x.EmployeeCode == caseData.CaseManagerCode)?.FullName,
                             CaseManagerOfficeAbbreviation = resources.FirstOrDefault(x => x.EmployeeCode == caseData.CaseManagerCode)?.Office?.OfficeAbbreviation,
                             ManagingOfficeCode = caseData.ManagingOfficeCode,
                             ManagingOfficeAbbreviation = caseData.ManagingOfficeAbbreviation,
                             ManagingOfficeName = caseData.ManagingOfficeName,
                             BillingOfficeCode = caseData.BillingOfficeCode,
                             BillingOfficeAbbreviation = caseData.BillingOfficeAbbreviation,
                             BillingOfficeName = caseData.BillingOfficeName,
                             StartDate = caseData.StartDate,
                             EndDate = caseData.EndDate,
                             Type = caseData.Type,
                             CaseRoll = casesOnRoll.FirstOrDefault(c => c.RolledFromOldCaseCode == caseData.OldCaseCode),
                             IsPrivateEquity = caseData.IsPrivateEquity,
                             CaseServedByRingfence = caseData.CaseServedByRingfence,
                             CaseAttributes = caseData.CaseAttributes,
                             Notes = caseData.Notes,
                             ClientPriority = caseData.ClientPriority,
                             ClientPrioritySortOrder = caseData.ClientPrioritySortOrder,
                             AllocatedResources = null,
                             PlaceholderAllocations = null,
                             SkuTerm = caseSkuDemand?.Where(x => x.OldCaseCode == caseData.OldCaseCode),
                             SKUCaseTerms = caseSkuTerms?.Where(x => x.OldCaseCode == caseData.OldCaseCode).Select(x => new SKUCaseTermsViewModel
                             {
                                 Id = x.Id,
                                 PipelineId = x.PipelineId,
                                 OldCaseCode = x.OldCaseCode,
                                 EffectiveDate = x.EffectiveDate,
                                 LastUpdatedBy = x.LastUpdatedBy,
                                 SKUTerms = x.SKUTermsCodes?.Split(',').Select(int.Parse).ToList().Join(skuTerms,
                                     c => c,
                                     s => s.Code,
                                     (caseSku, skuTerm) => new SKUTerm
                                     {
                                         Code = skuTerm.Code,
                                         Name = skuTerm.Name
                                     })
                             }).FirstOrDefault(), //TODO: remove the following line once new SKU logic is implemented
                             OriginalStaffingOfficeCode = caseData.OriginalStaffingOfficeCode,
                             StaffingOfficeCode = caseData.StaffingOfficeCode,
                             OriginalStaffingOfficeAbbreviation = offices.FirstOrDefault(x => x.OfficeCode == caseData.OriginalStaffingOfficeCode)?.OfficeAbbreviation,
                             StaffingOfficeAbbreviation = offices.FirstOrDefault(x => x.OfficeCode == caseData.StaffingOfficeCode)?.OfficeAbbreviation,
                             EstimatedTeamSize = caseWithTSData?.EstimatedTeamSize,
                             IsPlaceholderCreatedFromCortex = caseWithTSData?.IsPlaceholderCreatedFromCortex,
                             CasePlanningViewNotes = caseViewNotes.Where(x => x.OldCaseCode == caseData.OldCaseCode),
                             //LatestCaseViewNote = caseViewNotes.Where(x => x.OldCaseCode == caseData.OldCaseCode).FirstOrDefault()
                         }).ToList();

            return cases;
        }

        private IList<ResourceAssignmentViewModel> AttachEmployeeInfo(IEnumerable<ResourceAssignmentViewModel> resourceAllocations, IEnumerable<Resource> resources,
            IEnumerable<Office> offices, IEnumerable<InvestmentCategory> investmentCategories, IEnumerable<CaseRoleType> caseRoleTypeList = null, IEnumerable<CommitmentType> commitmentTypeList = null)
        {
            var resourceAllocationsWithResourceInfo = (from resAlloc in resourceAllocations
                                                       join res in resources on resAlloc.EmployeeCode equals res.EmployeeCode into resAllocGroups
                                                       from resource in resAllocGroups.DefaultIfEmpty()
                                                       join o in offices on resAlloc.OperatingOfficeCode equals o.OfficeCode into resAllocOffices
                                                       from office in resAllocOffices.DefaultIfEmpty()
                                                       join ic in investmentCategories on resAlloc.InvestmentCode equals ic.InvestmentCode into resAllocInvestmentCat
                                                       from invesmentCategory in resAllocInvestmentCat.DefaultIfEmpty()
                                                       join crt in caseRoleTypeList on resAlloc.CaseRoleCode equals crt.CaseRoleCode into resCaseRoleList
                                                       from caseRoleCode in resCaseRoleList.DefaultIfEmpty()
                                                       join ct in commitmentTypeList on resAlloc.CommitmentTypeCode equals ct.CommitmentTypeCode into resCommitmentTypeList
                                                       from commitmentType in resCommitmentTypeList.DefaultIfEmpty()
                                                       select new ResourceAssignmentViewModel()
                                                       {
                                                           Id = resAlloc.Id,
                                                           OldCaseCode = resAlloc.OldCaseCode,
                                                           EmployeeCode = resAlloc.EmployeeCode,
                                                           EmployeeName = resource?.FullName,
                                                           InternetAddress = resource?.InternetAddress,
                                                           OperatingOfficeCode = resAlloc.OperatingOfficeCode,
                                                           OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                                           CurrentLevelGrade = resAlloc.CurrentLevelGrade,
                                                           Allocation = resAlloc.Allocation,
                                                           StartDate = resAlloc.StartDate,
                                                           EndDate = resAlloc.EndDate,
                                                           InvestmentCode = resAlloc.InvestmentCode,
                                                           InvestmentName = invesmentCategory?.InvestmentName,
                                                           CaseRoleCode = resAlloc.CaseRoleCode,
                                                           LastUpdatedBy = resAlloc.LastUpdatedBy,
                                                           TerminationDate = resource?.TerminationDate,
                                                           CaseRoleName = caseRoleCode?.CaseRoleName,
                                                           ServiceLineCode = resAlloc.ServiceLineCode,
                                                           ServiceLineName = resAlloc.ServiceLineName,
                                                           IsPlaceholderAllocation = resAlloc.IsPlaceholderAllocation,
                                                           Notes = resAlloc.Notes,
                                                           CommitmentTypeCode = resAlloc.CommitmentTypeCode,
                                                           CommitmentTypeName = commitmentType?.CommitmentTypeName,
                                                           PositionGroupCode = resAlloc?.PositionGroupCode
                                                       }).ToList();

            return resourceAllocationsWithResourceInfo;
        }

        #endregion

    }
}