using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class BasisPollingService : IBasisPollingService
    {
        private readonly IBasisApiClient _basisApiClient;
        private readonly IBasisPollingRepository _basisPollingRepository;
        private readonly IWorkdayPollingService _workdayPollingService;
        public BasisPollingService(IBasisApiClient basisApiClient, IBasisPollingRepository basisPollingRepository, 
            IWorkdayPollingService workdayPollingService)
        {
            _basisApiClient = basisApiClient;
            _basisPollingRepository = basisPollingRepository;
            _workdayPollingService = workdayPollingService;
        }

        public async Task InsertPracticeAreaLookUpData()
        {
            var practiceAreas = await _basisApiClient.GetAllPracticeAreas();
            var dataTable = ConvertToPracticeAreaDataTable(practiceAreas.ToList());
            await _basisPollingRepository.InsertPracticeAreaLookUpData(dataTable);
        }

        public async Task UpsertPracticeAffiliations()
        {
            var affiliations = await _basisApiClient.GetAllPracticeAffiliation();
            var dataTable = ConvertToPracticeAffiliationDataTable(affiliations);
            await _basisPollingRepository.UpsertPracticeAffiliations(dataTable);
        }

        public async Task InsertMonthlySnapshotForPracticeAffiliations()
        {
            await _basisPollingRepository.InsertMonthlySnapshotForPracticeAffiliations();
            await _workdayPollingService.UpdateCapacityAnalysisMonthlyForChangesinEmployeePracticeAffiliations();
        }

        private DataTable ConvertToPracticeAreaDataTable(IList<PracticeArea> practiceAreas)
        {
            var practiceAreaDataTable = new DataTable();
            practiceAreaDataTable.Columns.Add("practiceAreaCode", typeof(int));
            practiceAreaDataTable.Columns.Add("practiceAreaName", typeof(string));
            practiceAreaDataTable.Columns.Add("practiceAreaAbbreviation", typeof(string));
            practiceAreaDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var practiceArea in practiceAreas)
            {
                var row = practiceAreaDataTable.NewRow();

                row["practiceAreaCode"] = practiceArea.PracticeAreaCode;
                row["practiceAreaName"] = practiceArea.PracticeAreaName;
                row["practiceAreaAbbreviation"] = (object)practiceArea.PracticeAreaAbbreviation ?? DBNull.Value;
                row["lastUpdatedBy"] = "Auto-PollingJob";

                practiceAreaDataTable.Rows.Add(row);
            }

            return practiceAreaDataTable;
        }

        private DataTable ConvertToPracticeAffiliationDataTable(IEnumerable<PracticeAffiliation> affiliations)
        {
            var practiceAffiliationDataTable = new DataTable();
            practiceAffiliationDataTable.Columns.Add("recordId", typeof(string));
            practiceAffiliationDataTable.Columns.Add("employeeCode", typeof(string));
            practiceAffiliationDataTable.Columns.Add("roleCode", typeof(int));
            practiceAffiliationDataTable.Columns.Add("role", typeof(string));
            practiceAffiliationDataTable.Columns.Add("tagId", typeof(string));
            practiceAffiliationDataTable.Columns.Add("groupId", typeof(string));
            practiceAffiliationDataTable.Columns.Add("term", typeof(string));
            practiceAffiliationDataTable.Columns.Add("officeEntityCode", typeof(int));
            practiceAffiliationDataTable.Columns.Add("contextTypeCode", typeof(string));
            practiceAffiliationDataTable.Columns.Add("entity", typeof(string));
            practiceAffiliationDataTable.Columns.Add("practiceArea", typeof(string));
            practiceAffiliationDataTable.Columns.Add("attributeCode", typeof(int));
            practiceAffiliationDataTable.Columns.Add("abbreviation", typeof(string));
            practiceAffiliationDataTable.Columns.Add("contextTypeName", typeof(string));
            practiceAffiliationDataTable.Columns.Add("practiceAreaCode", typeof(int));

            foreach (var affiliation in affiliations)
            {
                var row = practiceAffiliationDataTable.NewRow();

                row["recordId"] = (object)affiliation.RecordId ?? DBNull.Value;
                row["employeeCode"] = (object)affiliation.EmployeeCode ?? DBNull.Value;
                row["roleCode"] = (object)affiliation.RoleCode ?? DBNull.Value;
                row["role"] = (object)affiliation.Role ?? DBNull.Value;
                row["tagId"] = (object)affiliation.TagId ?? DBNull.Value;
                row["groupId"] = (object)affiliation.GroupId ?? DBNull.Value;
                row["term"] = (object)affiliation.Term ?? DBNull.Value;
                row["officeEntityCode"] = (object)affiliation.OfficeEntityCode ?? DBNull.Value;
                row["contextTypeCode"] = (object)affiliation.ContextTypeCode ?? DBNull.Value;
                row["entity"] = (object)affiliation.Entity ?? DBNull.Value;
                row["practiceArea"] = (object)affiliation.PracticeArea ?? DBNull.Value;
                row["attributeCode"] = (object)affiliation.AttributeCode ?? DBNull.Value;
                row["abbreviation"] = (object)affiliation.Abbreviation ?? DBNull.Value;
                row["contextTypeName"] = (object)affiliation.ContextTypeName ?? DBNull.Value;
                row["practiceAreaCode"] = (object)affiliation.PracticeAreaCode ?? DBNull.Value;

                practiceAffiliationDataTable.Rows.Add(row);
            }

            return practiceAffiliationDataTable;
        }
    }
}
