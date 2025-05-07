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
    public class TrainingPollingService : ITrainingPollingService
    {
        public ITrainingPollingRepository _trainingPollingRepository;
        public IPollMasterRepository _pollMasterRepository;
        public IBvuApiClient _bvuApiClient;
        public TrainingPollingService(ITrainingPollingRepository trainingPollingRepository,
            IPollMasterRepository pollMasterRepository, IBvuApiClient bvuApiClient)
        {
            _trainingPollingRepository = trainingPollingRepository;
            _pollMasterRepository = pollMasterRepository;
            _bvuApiClient = bvuApiClient;

        }
        public async Task upsertTrainings()
        {
            var trainings = await _bvuApiClient.GetTrainings(null);
            if (trainings.Count > 0)
            {
                var trainingDataTable = ConvertToTrainingDataTable(trainings);
                await _trainingPollingRepository.UpsertTrainings(trainingDataTable);
            }
        }

        private DataTable ConvertToTrainingDataTable(IList<Training> trainings)
        {
            var trainingDataTable = new DataTable();
            trainingDataTable.Columns.Add("id", typeof(Guid));
            trainingDataTable.Columns.Add("employeeCode", typeof(string));
            trainingDataTable.Columns.Add("startDate", typeof(DateTime));
            trainingDataTable.Columns.Add("endDate", typeof(DateTime));
            trainingDataTable.Columns.Add("role", typeof(string));
            trainingDataTable.Columns.Add("trainingName", typeof(string));
            trainingDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            trainingDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var training in trainings)
            {
                var row = trainingDataTable.NewRow();

                row["id"] = (object)training.Id ?? DBNull.Value;
                row["employeeCode"] = (object)training.EmployeeCode ?? DBNull.Value;
                row["startDate"] = (object)training.StartDate ?? DBNull.Value;
                row["endDate"] = (object)training.EndDate ?? DBNull.Value;
                row["role"] = (object)training.Role ?? DBNull.Value;
                row["trainingName"] = (object)training.TrainingName ?? DBNull.Value;
                row["lastUpdated"] = (object)training.LastUpdated ?? DBNull.Value;
                row["lastUpdatedBy"] = (object)training.LastUpdatedBy ?? DBNull.Value;

                trainingDataTable.Rows.Add(row);
            }

            return trainingDataTable;
        }
    }
}
