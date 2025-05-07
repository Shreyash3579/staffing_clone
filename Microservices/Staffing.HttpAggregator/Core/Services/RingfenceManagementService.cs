using Staffing.HttpAggregator.Contracts;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class RingfenceManagementService : IRingfenceManagementService
    {
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly ICCMApiClient _ccmApiClient;

        public RingfenceManagementService(IResourceApiClient resourceApiClient, IStaffingApiClient staffingApiClient, ICCMApiClient ccmApiClient)
        {
            _staffingApiClient = staffingApiClient;
            _resourceApiClient = resourceApiClient;
            _ccmApiClient = ccmApiClient;
        }
       
        public async Task<IEnumerable<RingfenceManagementViewModel>> GetTotalResourcesByOfficesAndRingfences(string officeCodes, string commitmentTypeCodes)
        {
            if (string.IsNullOrEmpty(officeCodes) || string.IsNullOrEmpty(commitmentTypeCodes))
                throw new ArgumentException("Office Codes & Ringfence Codes are required");

            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var ringfencesAllocationsDataTask = _staffingApiClient.GetCommitmentBySelectedValues(commitmentTypeCodes, null, DateTime.Today, null, true);
            var ringfencesDetailsDataTask = _staffingApiClient.GetRingfencesDetailsByOfficesAndCommitmentCodes(officeCodes, commitmentTypeCodes);
            var commitmentTypesDataTask = _staffingApiClient.GetCommitmentTypeList();
            var officesDataTask = _ccmApiClient.GetOfficeList();
            await Task.WhenAll(resourcesDataTask, ringfencesAllocationsDataTask, ringfencesDetailsDataTask, commitmentTypesDataTask, officesDataTask);

            var allResources = resourcesDataTask.Result;
            var ringfencesAllocations = ringfencesAllocationsDataTask.Result;
            var ringfencesManagementDetails = ringfencesDetailsDataTask.Result;
            var commitmentTypes = commitmentTypesDataTask.Result;
            var offices = officesDataTask.Result;

            var filteredResourcesByOffices = allResources.Where(x => officeCodes.IndexOf(x.SchedulingOffice.OfficeCode.ToString()) > -1);

            var resourcesGrpByOfficeAndRingfence = ringfencesAllocations.Join(
                                    filteredResourcesByOffices,
                                    ringfenceAllocation => ringfenceAllocation.EmployeeCode,
                                    resource => resource.EmployeeCode,
                                    (ringfencesAllocation, resource) => new
                                    {
                                        EmployeeCode = ringfencesAllocation.EmployeeCode,
                                        OperatingOfficeCode = resource.SchedulingOffice.OfficeCode,
                                        CommitmentTypeCode = ringfencesAllocation.CommitmentTypeCode,
                                    }
                                ).GroupBy(x => new { x.OperatingOfficeCode, x.CommitmentTypeCode}).Select(grp => new {
                                    OperatingOfficeCode = grp.Key.OperatingOfficeCode.ToString(),
                                    CommitmentTypeCode = grp.Key.CommitmentTypeCode,
                                    TotalRFResources = grp.Count()
                                }).ToList();

            var ringfenceData = (
                    from officeCode in officeCodes.Split(",")
                    from commitmentTypeCode in commitmentTypeCodes.Split(",")
                    join office in offices on officeCode equals office.OfficeCode.ToString()
                    join commitment in commitmentTypes on commitmentTypeCode equals commitment.CommitmentTypeCode
                    join gc in resourcesGrpByOfficeAndRingfence on new { officeCode = officeCode, commitmentTypeCode = commitmentTypeCode } equals new { officeCode = gc.OperatingOfficeCode.ToString(), commitmentTypeCode = gc.CommitmentTypeCode } into groupedData
                        from groupedRfData in groupedData.DefaultIfEmpty()
                    join rmd in ringfencesManagementDetails on new { officeCode = officeCode, commitmentTypeCode = commitmentTypeCode } equals new { officeCode = rmd.OfficeCode.ToString(), commitmentTypeCode = rmd.CommitmentTypeCode } into rfManagement
                        from rfManagementData in rfManagement.DefaultIfEmpty()
                    join r in allResources on rfManagementData?.LastUpdatedBy equals r.EmployeeCode into resource
                        from lastUpdatedResource in resource.DefaultIfEmpty()
                    select new RingfenceManagementViewModel
                    {
                        Id = rfManagementData?.Id,
                        OfficeCode = Convert.ToInt16(officeCode),
                        OfficeName = office.OfficeName,
                        CommitmentTypeCode = commitmentTypeCode,
                        CommitmentTypeName = commitment.CommitmentTypeName,
                        RfTeamsOwed = rfManagementData?.RfTeamsOwed,
                        TotalRFResources = groupedRfData?.TotalRFResources ?? 0,
                        EffectiveDate = rfManagementData?.EffectiveDate,
                        LastUpdatedBy = rfManagementData?.LastUpdatedBy,
                        LastUpdatedByName = lastUpdatedResource?.FullName
                    }).ToList();

            return ringfenceData?.OrderBy(x => x.OfficeName).ThenBy(y => y.CommitmentTypeName) ?? Enumerable.Empty<RingfenceManagementViewModel>();
        }

        public async Task<IEnumerable<RingfenceManagementViewModel>> GetRingfenceAuditLogByOfficeAndCommitmentCode(string officeCode, string commitmentTypeCode)
        {
            if (string.IsNullOrEmpty(officeCode) || string.IsNullOrEmpty(commitmentTypeCode))
                throw new ArgumentException("Office Code & Ringfence Code are required");

            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var auditDataTask = _staffingApiClient.GetRingfenceAuditLogByOfficeAndCommitmentCode(officeCode, commitmentTypeCode);
            await Task.WhenAll(resourcesDataTask, auditDataTask);

            var allResources = resourcesDataTask.Result;
            var auditData = auditDataTask.Result;

            var auditLogViewModel = (
                                        from log in auditData
                                        join resource in allResources on log.LastUpdatedBy equals resource.EmployeeCode
                                        select new RingfenceManagementViewModel
                                        {
                                            Id = log.Id,
                                            OfficeCode = log.OfficeCode,
                                            RfTeamsOwed = log.RfTeamsOwed,
                                            CommitmentTypeCode = log.CommitmentTypeCode,
                                            EffectiveDate = log.EffectiveDate,
                                            LastUpdatedBy = log.LastUpdatedBy,
                                            LastUpdatedByName = resource.FullName
                                        }
                                    ).ToList();

            return auditLogViewModel ?? Enumerable.Empty<RingfenceManagementViewModel>();
        }

        #region Private methods

        #endregion
    }
}
