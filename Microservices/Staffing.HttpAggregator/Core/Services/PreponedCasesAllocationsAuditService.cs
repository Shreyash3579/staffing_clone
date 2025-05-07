using Staffing.HttpAggregator.Contracts.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;

namespace Staffing.HttpAggregator.Core.Services
{
    public class PreponedCasesAllocationsAuditService : IPreponedCasesAllocationsAuditService
    {
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly ICCMApiClient _ccmApiClient;

        public PreponedCasesAllocationsAuditService(IStaffingApiClient staffingApiClient,
            IResourceApiClient resourceApiClient, ICCMApiClient ccmApiClient)
        {
            _staffingApiClient = staffingApiClient;
            _resourceApiClient = resourceApiClient;
            _ccmApiClient = ccmApiClient;
        }

        public async Task<IEnumerable<PreponedCasesAllocationsAuditViewModel>> GetPreponedCaseAllocationsAudit(string serviceLineCodes, string officeCodes,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(serviceLineCodes) || string.IsNullOrEmpty(officeCodes))
                return Enumerable.Empty<PreponedCasesAllocationsAuditViewModel>();

            var auditDataTask = _staffingApiClient.GetPreponedCaseAllocationsAudit(serviceLineCodes, officeCodes, startDate, endDate);
            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(auditDataTask, resourcesTask);

            var auditData = auditDataTask.Result;
            var resources = resourcesTask.Result;

            if (!auditData.Any())
                return Enumerable.Empty<PreponedCasesAllocationsAuditViewModel>();

            var listCaseCodes = auditData.Select(x => x.OldCaseCode).Distinct();

            var casesData = await _ccmApiClient.GetCaseDataByCaseCodes(string.Join(",", listCaseCodes));

            var casesAllocationsAuditsRaw = auditData.GroupBy(d => new { d.OldCaseCode, d.CaseLastUpdated }).Select(g =>
            {
                var preponedCaseDetail = g.FirstOrDefault();
                var commonCaseDetails = new PreponedCasesAllocationsAuditViewModel()
                {
                    Id = preponedCaseDetail.Id,
                    CaseCode = preponedCaseDetail.CaseCode,
                    ClientCode = preponedCaseDetail.ClientCode,
                    OldCaseCode = preponedCaseDetail.OldCaseCode,
                    OriginalCaseStartDate = preponedCaseDetail.OriginalCaseStartDate,
                    UpdatedCaseStartDate = preponedCaseDetail.UpdatedCaseStartDate,
                    OriginalCaseEndDate = preponedCaseDetail.OriginalCaseEndDate,
                    UpdatedCaseEndDate = preponedCaseDetail.UpdatedCaseEndDate,
                    CaseLastUpdatedBy = preponedCaseDetail.CaseLastUpdatedBy,
                    CaseLastUpdated = preponedCaseDetail.CaseLastUpdated,
                    LastUpdatedBy = preponedCaseDetail.LastUpdatedBy,
                    EmployeeCodes = string.Join(",", g.Select(x => x.EmployeeCode).Distinct())
                };

                return commonCaseDetails;
            });

            var preponedCasesAllocationsAudits = ConvertToPreponedCasesAllocationsAuditViewModel(casesAllocationsAuditsRaw, casesData, resources);

            return preponedCasesAllocationsAudits;
        }

        #region private methods
        private static IEnumerable<PreponedCasesAllocationsAuditViewModel> ConvertToPreponedCasesAllocationsAuditViewModel(IEnumerable<PreponedCasesAllocationsAuditViewModel> auditData,
            IEnumerable<CaseData> cases, IEnumerable<Resource> resources)
        {
            var resourceAllocationsViewModel = (from data in auditData
                                                join caseData in cases on data.OldCaseCode equals caseData.OldCaseCode into auditDataGroups
                                                from caseItem in auditDataGroups.DefaultIfEmpty()
                                                select new PreponedCasesAllocationsAuditViewModel
                                                {
                                                    Id = data.Id,
                                                    CaseCode = data.CaseCode,
                                                    ClientCode = data.ClientCode,
                                                    OldCaseCode = data.OldCaseCode,
                                                    CaseName = caseItem?.CaseName,
                                                    ClientName = caseItem?.ClientName,
                                                    OriginalCaseStartDate = data.OriginalCaseStartDate,
                                                    UpdatedCaseStartDate = data.UpdatedCaseStartDate,
                                                    OriginalCaseEndDate = data.OriginalCaseEndDate,
                                                    UpdatedCaseEndDate = data.UpdatedCaseEndDate,
                                                    EmployeeCodes = data.EmployeeCodes,
                                                    EmployeeNames = GetResourceNameByEmployeeCodes(data.EmployeeCodes, resources),
                                                    CaseLastUpdatedBy = data.CaseLastUpdatedBy,
                                                    CaseLastUpdatedByName = resources.FirstOrDefault(r => r.EmployeeCode == data.CaseLastUpdatedBy)?.FullName,
                                                    CaseLastUpdated = data.CaseLastUpdated,
                                                    LastUpdatedBy = data.LastUpdatedBy
                                                }).ToList();

            return resourceAllocationsViewModel;
        }

        private static string GetResourceNameByEmployeeCodes(string employeeCodes, IEnumerable<Resource> resources)
        {
            string employeeNames = string.Join(" | ", resources.Where(x => employeeCodes.Contains(x.EmployeeCode)).Select(z => z.FullName));
            return employeeNames;
        }
        #endregion
    }
}
