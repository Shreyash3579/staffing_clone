using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class VacationPollingService : IVacationPollingService
    {
        public IVacationPollingRepository _vacationPollingRepository;
        public IPollMasterRepository _pollMasterRepository;
        public IVacationApiClient _vacationApiClient;
        public VacationPollingService(IVacationPollingRepository vacationPollingRepository,
            IPollMasterRepository pollMasterRepository, IVacationApiClient vacationApiClient)
        {
            _vacationPollingRepository = vacationPollingRepository;
            _pollMasterRepository = pollMasterRepository;
            _vacationApiClient = vacationApiClient;

        }
        public async Task upsertVacations()
        {
            var vacations = await _vacationApiClient.GetVacationsSinceLastPolled(null);
            if (vacations.Count > 0)
            {
                var vacationDataTable = ConvertToVacationDataTable(vacations);
                await _vacationPollingRepository.UpsertVacations(vacationDataTable);
            }
        }

        private DataTable ConvertToVacationDataTable(IList<Vacation> vacations)
        {
            var vacationDataTable = new DataTable();
            vacationDataTable.Columns.Add("id", typeof(Guid));
            vacationDataTable.Columns.Add("employeeCode", typeof(string));
            vacationDataTable.Columns.Add("startDate", typeof(DateTime));
            vacationDataTable.Columns.Add("endDate", typeof(DateTime));
            vacationDataTable.Columns.Add("notes", typeof(string));
            vacationDataTable.Columns.Add("statusCode", typeof(string));
            vacationDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            vacationDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            vacationDataTable.Columns.Add("replicationServer", typeof(string));

            foreach (var vacation in vacations)
            {
                var row = vacationDataTable.NewRow();

                row["id"] = (object)vacation.Id ?? DBNull.Value;
                row["employeeCode"] = (object)vacation.EmployeeCode ?? DBNull.Value;
                row["startDate"] = (object)vacation.StartDate ?? DBNull.Value;
                row["endDate"] = (object)vacation.EndDate ?? DBNull.Value;
                row["notes"] = (object)vacation.Description ?? DBNull.Value;
                row["statusCode"] = (object)vacation.Status ?? DBNull.Value;
                row["lastUpdated"] = (object)vacation.LastUpdated ?? DBNull.Value;
                row["lastUpdatedBy"] = (object)vacation.LastUpdatedBy ?? DBNull.Value;
                row["replicationServer"] = (object)vacation.ReplicationServer ?? DBNull.Value;

                vacationDataTable.Rows.Add(row);
            }

            return vacationDataTable;
        }
    }
}
