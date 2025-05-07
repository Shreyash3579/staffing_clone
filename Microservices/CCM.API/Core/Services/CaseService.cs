using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Contracts.Services;
using CCM.API.Core.Helpers;
using CCM.API.Models;
using CCM.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCM.API.Core.Services
{
    public class CaseService : ICaseService
    {
        private readonly ICaseRepository _caseRepository;

        public CaseService(ICaseRepository caseRepository)
        {
            _caseRepository = caseRepository;
        }

        public async Task<IEnumerable<CaseViewModelBasic>> GetNewDemandCasesByOffices(DateTime startDate, DateTime endDate, string officeCodes, string caseTypeCodes, string clientCodes)
        {
            if (startDate > endDate)
                throw new ArgumentException("endDate should be greater than or equal to startDate");
            if (string.IsNullOrEmpty(officeCodes))
                throw new ArgumentException("Office Code can not be null");
            if (string.IsNullOrEmpty(caseTypeCodes))
                throw new ArgumentException("Case Type Code can not be null");

            var casesData = await
                _caseRepository.GetNewDemandCasesByOffices(startDate, endDate, officeCodes, caseTypeCodes, clientCodes);

            return ConvertToCaseViewModelBasic(casesData, startDate);
        }

        public async Task<IEnumerable<CaseViewModelBasic>> GetActiveCasesExceptNewDemandsByOffices(DateTime startDate, DateTime endDate, string officeCodes, string caseTypeCodes, string clientCodes)
        {
            if (startDate > endDate)
                throw new ArgumentException("endDate should be greater than or equal to startDate");
            if (string.IsNullOrEmpty(officeCodes))
                throw new ArgumentException("Office Code can not be null");
            if (string.IsNullOrEmpty(caseTypeCodes))
                throw new ArgumentException("Case Type Code can not be null");

            var casesData = await
                _caseRepository.GetActiveCasesExceptNewDemandsByOffices(startDate, endDate, officeCodes, caseTypeCodes, clientCodes);

            return ConvertToCaseViewModelBasic(casesData, startDate);
        }

        public async Task<IEnumerable<CaseViewModel>> GetCaseDetailsByCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
            {
                throw new ArgumentException("Case Codes cannot be empty");
            }

            var result = await _caseRepository.GetCaseDetailsByCaseCodes(oldCaseCodes);

            return ConvertToCaseViewModel(result);
        }

        public async Task<IEnumerable<CaseViewModelBasic>> GetCaseDataByCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
            {
                throw new ArgumentException("Case Code cannot be empty");
            }

            var result = await _caseRepository.GetCaseDataByCaseCodes(oldCaseCodes);

            return ConvertToCaseViewModelBasic(result);

        }

        public async Task<IEnumerable<CaseViewModelBasic>> GetCasesForTypeahead(string searchString)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
            {
                return Enumerable.Empty<CaseViewModelBasic>();
            }

            var result = await _caseRepository.GetCasesForTypeahead(searchString);

            return ConvertToCaseViewModelBasic(result);
        }

        public async Task<IEnumerable<CaseViewModelBasic>> GetCasesEndingBySpecificDate(int caseEndsBeforeNumberOfDays)
        {
            var result = await _caseRepository.GetCasesEndingBySpecificDate(caseEndsBeforeNumberOfDays);
            return ConvertToCaseViewModelBasic(result);
        }

        public async Task<IEnumerable<CaseViewModelBasic>> GetCasesWithTaxonomiesByCaseCodes(string oldCaseCodeList)
        {
            var result = await _caseRepository.GetCasesWithTaxonomiesByCaseCodes(oldCaseCodeList);

            return ConvertToCaseViewModelBasic(result);
        }



        public async Task<IEnumerable<CaseViewModel>> GetCasesActiveAfterSpecifiedDate(DateTime? date)
        {
            var dateValue = date ?? DateTime.Today.AddYears(-1);

            var result = await _caseRepository.GetCasesActiveAfterSpecifiedDate(dateValue.Date);

            return ConvertToCaseViewModelDetailed(result);
        }

        public async Task<IEnumerable<CaseViewModel>> GetCasesWithStartOrEndDateUpdatedInCCM(string columnName, DateTime? lastPollDateTime)
        {
            if (string.IsNullOrEmpty(columnName)) 
                return Enumerable.Empty<CaseViewModel>();
            
            var result = await _caseRepository.GetCasesWithStartOrEndDateUpdatedInCCM(columnName, lastPollDateTime);

            return ConvertToCaseViewModelDetailed(result);
        }

        public async Task<CaseMasterViewModel> GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled(DateTime? lastPolledDateTime)
        {
            if (lastPolledDateTime == DateTime.MinValue)
                lastPolledDateTime = null;
            var caseMasterAndCaseMasterHistoryDataChanges = await _caseRepository.GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled(lastPolledDateTime);

            return caseMasterAndCaseMasterHistoryDataChanges;
        }

        public async Task<IEnumerable<CaseAdditionalInfo>> GetCaseAdditionalInfo(DateTime? lastUpdated)
        {
            if (lastUpdated == DateTime.MinValue)
                lastUpdated = null;

            var caseAdditionalInfo = await _caseRepository.GetCaseAdditionalInfo(lastUpdated);

            return caseAdditionalInfo;
        }

        //*-------------Helper Functions-----------------*//

        private static IEnumerable<CaseViewModelBasic> ConvertToCaseViewModelBasic(IEnumerable<Case> result, DateTime? startDate = null)
        {
            var startDateForNewDemand = startDate.HasValue ? startDate : DateTime.Now.Date;

            var caseData = result.Select(r => new CaseViewModelBasic
            {
                CaseCode = r.CaseCode,
                CaseName = r.CaseName,
                CaseManagerCode = r.CaseManagerCode ?? string.Empty,
                CaseManagerName = r.CaseManagerName ?? string.Empty,
                ClientCode = r.ClientCode,
                ClientName = r.ClientName,
                OldCaseCode = r.OldCaseCode,
                CaseType = r.CaseType,
                CaseTypeCode = r.CaseTypeCode,
                ManagingOfficeCode = r.ManagingOfficeCode,
                ManagingOfficeAbbreviation = r.ManagingOfficeAbbreviation,
                ManagingOfficeName = r.ManagingOfficeName,
                BillingOfficeCode = r.BillingOfficeCode,
                BillingOfficeAbbreviation = r.BillingOfficeAbbreviation,
                BillingOfficeName = r.BillingOfficeName,
                StartDate = r.StartDate,
                EndDate = r.EndDate ?? r.ProjectedEndDate,
                PrimaryIndustryTermCode = r.PrimaryIndustryTermCode,
                PrimaryIndustryTagId = r.PrimaryIndustryTagId,
                PrimaryIndustry = r.PrimaryIndustry,
                PrimaryCapabilityTermCode = r.PrimaryCapabilityTermCode,
                PrimaryCapabilityTagId = r.PrimaryCapabilityTagId,
                PrimaryCapability = r.PrimaryCapability,
                IsPrivateEquity = r.IsPrivateEquity,
                CaseAttributes = r.CaseAttributes,
                Type = r.StartDate >= startDateForNewDemand ? Constants.NewDemand : Constants.ActiveCase,
                CaseServedByRingfence = r.CaseServedByRingfence,
                ClientPriority = r.ClientPriority,
                ClientPrioritySortOrder = r.ClientPrioritySortOrder,
                IndustryPracticeAreaCode = r.IndustryPracticeAreaCode,
                IndustryPracticeArea = r.IndustryPracticeArea,
                CapabilityPracticeAreaCode = r.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = r.CapabilityPracticeArea,
                PegIndustryTerm = r.PegIndustryTerm
            });

            return caseData;
        }

        private static IEnumerable<CaseViewModel> ConvertToCaseViewModel(IEnumerable<Case> results)
        {
            var caseDetails = results.Select(result =>  new CaseViewModel
            {
                CaseCode = result.CaseCode,
                CaseName = result.CaseName,
                ClientCode = result.ClientCode,
                ClientName = result.ClientName,
                ClientGroupCode = result.ClientGroupCode,
                ClientGroupName = result.ClientGroupName,
                OldCaseCode = result.OldCaseCode,
                CaseManagerCode = result.CaseManagerCode,
                CaseBillingPartnerCode = result.CaseBillingPartnerCode,
                CaseType = result.CaseType,
                CaseTypeCode = result.CaseTypeCode,
                PrimaryIndustryTermCode = result.PrimaryIndustryTermCode,
                PrimaryIndustryTagId = result.PrimaryIndustryTagId,
                PrimaryIndustry = result.PrimaryIndustry,
                IndustryPracticeAreaCode = result.PrimaryIndustryTermCode,
                IndustryPracticeArea = result.IndustryPracticeArea,
                PegIndustryTerm = result.PegIndustryTerm,
                PrimaryCapabilityTermCode = result.PrimaryCapabilityTermCode,
                PrimaryCapabilityTagId = result.PrimaryCapabilityTagId,
                PrimaryCapability = result.PrimaryCapability,
                CapabilityPracticeAreaCode = result.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = result.CapabilityPracticeArea,
                ManagingOfficeCode = result.ManagingOfficeCode,
                ManagingOfficeAbbreviation = result.ManagingOfficeAbbreviation,
                ManagingOfficeName = result.ManagingOfficeName,
                BillingOfficeCode = result.BillingOfficeCode,
                BillingOfficeAbbreviation = result.BillingOfficeAbbreviation,
                BillingOfficeName = result.BillingOfficeName,
                StartDate = result.StartDate,
                EndDate = result.EndDate ?? result.ProjectedEndDate,
                IsPrivateEquity = result.IsPrivateEquity,
                CaseAttributes = result.CaseAttributes,
                Type = result.StartDate >= DateTime.Today ? Constants.NewDemand : Constants.ActiveCase,
                CaseServedByRingfence = result.CaseServedByRingfence
            });

            return caseDetails;
        }

        private static IEnumerable<CaseViewModel> ConvertToCaseViewModelDetailed(IEnumerable<Case> results)
        {
            var caseDetails = results.Select(result => new CaseViewModel
            {
                CaseCode = result.CaseCode,
                CaseName = result.CaseName,
                ClientCode = result.ClientCode,
                ClientName = result.ClientName,
                OldCaseCode = result.OldCaseCode,
                CaseManagerCode = result.CaseManagerCode,
                CaseBillingPartnerCode = result.CaseBillingPartnerCode,
                CaseType = result.CaseType,
                PrimaryIndustryTermCode = result.PrimaryIndustryTermCode,
                PrimaryIndustryTagId = result.PrimaryIndustryTagId,
                PrimaryIndustry = result.PrimaryIndustry,
                IndustryPracticeArea = result.IndustryPracticeArea,
                PrimaryCapabilityTermCode = result.PrimaryCapabilityTermCode,
                PrimaryCapabilityTagId = result.PrimaryCapabilityTagId,
                PrimaryCapability = result.PrimaryCapability,
                CapabilityPracticeArea = result.CapabilityPracticeArea,
                ManagingOfficeCode = result.ManagingOfficeCode,
                ManagingOfficeAbbreviation = result.ManagingOfficeAbbreviation,
                ManagingOfficeName = result.ManagingOfficeName,
                BillingOfficeCode = result.BillingOfficeCode,
                BillingOfficeAbbreviation = result.BillingOfficeAbbreviation,
                BillingOfficeName = result.BillingOfficeName,
                OriginalStartDate = result.OriginalStartDate,
                StartDate = result.StartDate,
                OriginalEndDate = result.OriginalEndDate ?? result.OriginalProjectedEndDate,
                EndDate = result.EndDate ?? result.ProjectedEndDate,
                IsPrivateEquity = result.IsPrivateEquity,
                CaseAttributes = result.CaseAttributes,
                Type = result.StartDate >= DateTime.Today ? Constants.NewDemand : Constants.ActiveCase,
                CaseServedByRingfence = result.CaseServedByRingfence,
                LastUpdatedBy = result.LastUpdatedBy,
                LastUpdated = result.LastUpdated
            });

            return caseDetails;
        }
    }
}
