using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class AuditTrailService : IAuditTrailService
    {
        private readonly IPipelineApiClient _pipelineApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly ICCMApiClient _ccmApiClient;
        public AuditTrailService(IPipelineApiClient pipelineApiClient, IStaffingApiClient staffingApiClient,
            IResourceApiClient resourceApiClient, ICCMApiClient ccmApiClient)
        {
            _pipelineApiClient = pipelineApiClient;
            _staffingApiClient = staffingApiClient;
            _resourceApiClient = resourceApiClient;
            _ccmApiClient = ccmApiClient;
        }
        public async Task<IEnumerable<AuditTrailViewModel>> GetAuditTrailForCaseOrOpportunity(string oldCaseCode, string pipelineId, int? limit, int? offset)
        {
            var auditCasesDataTask = _staffingApiClient.GetAuditTrailForCaseOrOpportunity(oldCaseCode, pipelineId, limit, offset);

            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(auditCasesDataTask, resourcesDataTask);

            var auditCaseHistories = auditCasesDataTask.Result;
            var resources = resourcesDataTask.Result;

            var auditTrails = (from audit in auditCaseHistories
                               join res in resources on audit.UpdatedBy equals res.EmployeeCode into resAudits
                               from resource in resAudits.DefaultIfEmpty()
                               select new AuditTrailViewModel
                               {
                                   Date = DateTime.SpecifyKind(audit.Date, DateTimeKind.Utc),
                                   EventDescription = audit.EventDescription.Replace("[resource]", resources.FirstOrDefault(r => String.Equals(r.EmployeeCode, audit.Employee, StringComparison.CurrentCultureIgnoreCase))?.FullName.ToString()),
                                   New = audit.New,
                                   Old = audit.Old,
                                   UpdatedBy = resource?.FullName ?? audit.UpdatedBy

                               }).ToList();

            return auditTrails;
        }

        public async Task<IEnumerable<AuditTrailViewModel>> GetAuditTrailForEmployee(string employeeCode, int? limit, int? offset)
        {
            var auditEmployeeHistories = await _staffingApiClient.GetAuditTrailForEmployee(employeeCode, limit, offset);

            var pipelineIds = auditEmployeeHistories.Where(x => x.PipelineId != Guid.Empty).Select(x => x.PipelineId.ToString()).Distinct().ToList();
            pipelineIds.AddRange(auditEmployeeHistories.Where(x => x.NewPipelineId != Guid.Empty).Select(x => x.NewPipelineId.ToString()).Distinct().ToList());

            var listPipelineId = string.Join(",", pipelineIds);

            var oldCaseCodes = auditEmployeeHistories.Where(x => x.OldCaseCode != null).Select(x => x.OldCaseCode).Distinct().ToList();
            oldCaseCodes.AddRange(auditEmployeeHistories.Where(x => x.NewOldCaseCode != null).Select(x => x.NewOldCaseCode).Distinct().ToList());
            
            string listOldCaseCode = string.Join(",", oldCaseCodes);

            var opportunitiesDataTask = _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(listPipelineId);

            var casesDataTask = _ccmApiClient.GetCaseDataByCaseCodes(listOldCaseCode);

            var resourceDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(opportunitiesDataTask, casesDataTask, resourceDataTask);

            var opportunities = opportunitiesDataTask.Result;
            var cases = casesDataTask.Result;
            var resources = resourceDataTask.Result;

            var auditTrails = (from audit in auditEmployeeHistories
                               join res in resources on audit.UpdatedBy equals res.EmployeeCode into resAudit
                               from resource in resAudit.DefaultIfEmpty()
                               select new AuditTrailViewModel
                               {
                                   Date = DateTime.SpecifyKind(audit.Date, DateTimeKind.Utc),
                                   EventDescription = audit.EventDescription.Contains("[case]")
                                        ? audit.EventDescription.Replace("[case]", $"{audit.OldCaseCode}: {cases.FirstOrDefault(c => c.OldCaseCode == audit.OldCaseCode)?.CaseName}")
                                        : audit.EventDescription.Replace("[opportunity]", opportunities.FirstOrDefault(o => o.PipelineId == audit.PipelineId)?.OpportunityName),
                                   New = audit.New != null && audit.New.Contains("[newCase]")
                                        ? audit.New.Replace("[newCase]", cases.FirstOrDefault(c => c.OldCaseCode == audit.NewOldCaseCode)?.CaseName)
                                        : (audit.New != null && audit.New.Contains("[newOpportunity]")
                                            ? audit.New.Replace("[newOpportunity]", opportunities.FirstOrDefault(o => o.PipelineId == audit.NewPipelineId)?.OpportunityName)
                                            : audit.New),
                                   Old = audit.Old != null && audit.Old.Contains("[case]")
                                            ? audit.Old.Replace("[case]", cases.FirstOrDefault(c => c.OldCaseCode == audit.OldCaseCode)?.CaseName)
                                            : (audit.Old != null && audit.Old.Contains("[opportunity]")
                                                ? audit.Old.Replace("[opportunity]", opportunities.FirstOrDefault(o => o.PipelineId == audit.PipelineId)?.OpportunityName)
                                                : audit.Old),
                                   UpdatedBy = resource?.FullName ?? audit.UpdatedBy
                               }).ToList();


            return auditTrails;
        }
    }
}
