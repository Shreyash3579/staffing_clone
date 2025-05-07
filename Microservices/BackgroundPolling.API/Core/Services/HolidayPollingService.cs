using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class HolidayPollingService : IHolidayPollingService
    {
        public IHolidayPollingRepository _holidayPollingRepository;
        public IStaffingApiClient _staffingApiClient;
        public IBasisApiClient _basisApiClient;

        public HolidayPollingService(IHolidayPollingRepository holidayPollingRepository, IStaffingApiClient staffingApiClient, IBasisApiClient basisApiClient)
        {
            _holidayPollingRepository = holidayPollingRepository;
            _staffingApiClient = staffingApiClient;
            _basisApiClient = basisApiClient;
        }

        public async Task InsertHolidays()
        {
            var holidays = await _basisApiClient.GetHolidays();
            if (holidays.Count > 0)
            {
                var holidayDataTable = ConvertToHolidayDataTable(holidays);
                await _holidayPollingRepository.InsertHolidays(holidayDataTable);
            }
        }

        private DataTable ConvertToHolidayDataTable(IList<Holiday> holidays)
        {
            var holidayDataTable = new DataTable();
            holidayDataTable.Columns.Add("officeCode", typeof(int));
            holidayDataTable.Columns.Add("startDate", typeof(DateTime));
            holidayDataTable.Columns.Add("endDate", typeof(DateTime));
            holidayDataTable.Columns.Add("notes", typeof(string));
            holidayDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            holidayDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var holiday in holidays)
            {
                var row = holidayDataTable.NewRow();

                row["officeCode"] = (object)holiday.OfficeCode ?? DBNull.Value;
                row["startDate"] = (object)holiday.StartDate ?? DBNull.Value;
                row["endDate"] = (object)holiday.EndDate ?? DBNull.Value;
                row["notes"] = (object)holiday.Description ?? DBNull.Value;
                row["lastUpdated"] = (object)holiday.LastUpdated ?? DBNull.Value;
                row["lastUpdatedBy"] = (object)holiday.LastUpdatedBy ?? DBNull.Value;

                holidayDataTable.Rows.Add(row);
            }

            return holidayDataTable;
        }
    }
}
