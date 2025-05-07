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
    public class IrisPollingService : IIrisPollingService
    {
        public IIrisPollingRepository _irisPollingRepository;
        public IIrisApiClient _irisApiClient;

        public IrisPollingService(IIrisApiClient irisApiClient, IIrisPollingRepository irisPollingRepository)
        {
            _irisApiClient = irisApiClient;
            _irisPollingRepository = irisPollingRepository;
        }

        public async Task UpsertWorkAndSchoolHistoryForAllActiveEmployeesFromIris()
        {
            int pageNumber = 1;
            int pageCount = 5000;
            bool includeAlumni = false;
            int totalCount = 0;
            var workAndSchoolHistoryToUpsert = new List <EmployeeWorkAndSchoolHistory>();

            do
            {
                var workAndSchoolHistory = await _irisApiClient.GetWorkSchoolHistoryForAll(pageNumber, pageCount, includeAlumni);
                totalCount = workAndSchoolHistory.TotalCount;

                if (workAndSchoolHistory.Items?.Count > 0)
                {
                    workAndSchoolHistoryToUpsert.AddRange(workAndSchoolHistory.Items);
                }

                pageNumber++;
            } while (workAndSchoolHistoryToUpsert.Count < totalCount);

            var schoolHistoryDataTable = ConvertToSchoolHistoryDataTable(workAndSchoolHistoryToUpsert);
            var workHistoryDataTable = ConvertToWorkHistoryDataTable(workAndSchoolHistoryToUpsert);
            await _irisPollingRepository.UpsertWorkAndSchoolHistory(workHistoryDataTable, schoolHistoryDataTable);
        }

        public async Task<string> InsertWorkAndEducationHistoryAfterLastModifiedDateForActiveEmployeesFromIris(DateTime? lastModifiedDateAdter)
        {
            bool includeAlumni = false;
            string updatedEmployeeCodes = string.Empty;
            DateTime lastPolledTime;
            if(lastModifiedDateAdter == null || lastModifiedDateAdter == DateTime.MinValue)
            {
                lastPolledTime = DateTime.Now;
            }else
            {
                lastPolledTime = lastModifiedDateAdter.Value;
            }
            var workAndSchoolHistoryToUpsert = await _irisApiClient.GetWorkSchoolHistoryByModifiedDate(lastPolledTime, includeAlumni);
            if (workAndSchoolHistoryToUpsert.Items.Count > 0)
            {
                updatedEmployeeCodes = string.Join(",", workAndSchoolHistoryToUpsert.Items.Select(x => x.Ecode).Distinct());
                var schoolHistoryDataTable = ConvertToSchoolHistoryDataTable(workAndSchoolHistoryToUpsert.Items);
                var workHistoryDataTable = ConvertToWorkHistoryDataTable(workAndSchoolHistoryToUpsert.Items);
                await _irisPollingRepository.UpsertWorkAndSchoolHistory(workHistoryDataTable, schoolHistoryDataTable);
            }

            return "Success for :" + updatedEmployeeCodes;

        }

        private DataTable ConvertToSchoolHistoryDataTable(IEnumerable<EmployeeWorkAndSchoolHistory> employeesWorkAndSchoolHistory)
        {
            var schoolHistoryDataTable = new DataTable();
            schoolHistoryDataTable.Columns.Add("employeeCode", typeof(string));
            schoolHistoryDataTable.Columns.Add("schoolName", typeof(string));
            schoolHistoryDataTable.Columns.Add("fieldOfStudy", typeof(string));
            schoolHistoryDataTable.Columns.Add("degree", typeof(string));
            schoolHistoryDataTable.Columns.Add("endDate", typeof(DateTime));
            schoolHistoryDataTable.Columns.Add("lastUpdatedBy", typeof(string));


            foreach (var employeeWorkAndSchoolHistory in employeesWorkAndSchoolHistory)
            {
                foreach(var employeeSchoolHistory in employeeWorkAndSchoolHistory.EducationHistory)
                {
                    var row = schoolHistoryDataTable.NewRow();

                    row["employeeCode"] = employeeWorkAndSchoolHistory.Ecode;
                    row["schoolName"] = (object)employeeSchoolHistory.SchoolName ?? DBNull.Value;
                    row["fieldOfStudy"] = (object)employeeSchoolHistory.FieldOfStudy ?? DBNull.Value;
                    row["degree"] = (object)employeeSchoolHistory.Degree ?? DBNull.Value;
                    row["endDate"] = (object)employeeSchoolHistory.EndDate ?? DBNull.Value;
                    row["lastUpdatedBy"] = "Auto-Iris-API";

                    schoolHistoryDataTable.Rows.Add(row);
                }
                
            }

            return schoolHistoryDataTable;
        }

        private DataTable ConvertToWorkHistoryDataTable(IEnumerable<EmployeeWorkAndSchoolHistory> employeesWorkAndSchoolHistory)
        {
            var workHistoryDataTable = new DataTable();
            workHistoryDataTable.Columns.Add("employeeCode", typeof(string));
            workHistoryDataTable.Columns.Add("companyName", typeof(string));
            workHistoryDataTable.Columns.Add("industry", typeof(string));
            workHistoryDataTable.Columns.Add("startDate", typeof(DateTime));
            workHistoryDataTable.Columns.Add("endDate", typeof(DateTime));
            workHistoryDataTable.Columns.Add("jobTitle", typeof(string));
            workHistoryDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var employeeWorkAndSchoolHistory in employeesWorkAndSchoolHistory)
            {
                foreach (var employeeWorkHistory in employeeWorkAndSchoolHistory.EmploymentHistory)
                {
                    var row = workHistoryDataTable.NewRow();
                    row["employeeCode"] = employeeWorkAndSchoolHistory.Ecode;
                    row["companyName"] = (object)employeeWorkHistory.CompanyName ?? DBNull.Value;
                    row["industry"] = (object)employeeWorkHistory.Industry ?? DBNull.Value;
                    row["startDate"] = (object)employeeWorkHistory.StartDate ?? DBNull.Value;
                    row["endDate"] = (object)employeeWorkHistory.EndDate ?? DBNull.Value;
                    row["jobTitle"] = (object)employeeWorkHistory.JobTitle ?? DBNull.Value;
                    row["lastUpdatedBy"] = "Auto-Iris-API";

                    workHistoryDataTable.Rows.Add(row);
                }
            }
            return workHistoryDataTable;
        }

    }
}
