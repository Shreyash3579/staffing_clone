using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class AnalyticsAuditService : IAnalyticsAuditService
    {
        private readonly IStaffingAnalyticsRepository _staffingAnalyticsRepository;
        private readonly AppSettingsConfiguration _appSettings;
        private readonly IStaffingApiClient _staffingApiClient;

        public AnalyticsAuditService(IStaffingAnalyticsRepository staffingAnalyticsRepository,
            IOptionsSnapshot<AppSettingsConfiguration> appSettings, IStaffingApiClient staffingApiClient)
        {
           _staffingAnalyticsRepository = staffingAnalyticsRepository;
           _appSettings = appSettings.Value;
           _staffingApiClient = staffingApiClient;
        }

        public async Task<IEnumerable<CADMismatchLog>> GetAnalyticsRecordsNotSyncedWithCAD()
        {
            var data = await _staffingAnalyticsRepository.GetAnalyticsRecordsNotSyncedWithCAD();
            var staffingSyncData = await _staffingApiClient.GetStaffingTablesRecordsForSync();
            var filteredstaffingAnalyticsData = data.ToList().Where(x => x.SourceTable == "ScheduleMaster" || x.SourceTable == "CommitmentMaster" || x.SourceTable == "CaseOppChanges" || x.SourceTable == "InvestmentCategory" || x.SourceTable == "CaseRoleType").ToList();

            var filterStaffingAndStaffingAnalyticsSyncData = GetDataFilteredFromStaffingAndStaffingAnalytics(filteredstaffingAnalyticsData, staffingSyncData.ToList());
            var dataSmdRa = GetSmdRaFilteredData(data.ToList());

            var emailRecipients = _appSettings.DebugEmail;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (Environments.Production == environment)
            {
                EmailHelper.SendEmailForCADMismatchAudit(emailRecipients, dataSmdRa.ToList(), filterStaffingAndStaffingAnalyticsSyncData);
            } 
            var combinedData = dataSmdRa.Concat(filterStaffingAndStaffingAnalyticsSyncData);
            return combinedData;
        }

        public List<CADMismatchLog> GetDataFilteredFromStaffingAndStaffingAnalytics(IList<CADMismatchLog> analyticsobjectList, IList<CADMismatchLog> staffingobjectList)
        {
            var result = new List<CADMismatchLog>();

            foreach (var staffingObject in staffingobjectList)
            {
                var matchingAnalyticsObject = analyticsobjectList.FirstOrDefault(
                    obj => obj.SourceTable == staffingObject.SourceTable);

                if (matchingAnalyticsObject != null)
                {
                    var timeDifference = (staffingObject.LastUpdated - matchingAnalyticsObject.LastUpdated).TotalMinutes;
                    var countMismatchDifference = staffingObject.CountMismatch - matchingAnalyticsObject.CountMismatch;

                    // Check if the time difference is greater than 5 minutes and return only those values that have count difference
                    if (timeDifference > 5 && countMismatchDifference>0)
                    {
                        var mismatchLog = new CADMismatchLog
                        {
                            SourceTable = staffingObject.SourceTable,
                            CountMismatch = countMismatchDifference,
                            LastUpdated = matchingAnalyticsObject.LastUpdated,
                            Remarks = matchingAnalyticsObject.Remarks
                        };

                    result.Add(mismatchLog);

                    }
                }
            }

            return result.ToList();
        }

        public List<CADMismatchLog> GetSmdRaFilteredData(IList<CADMismatchLog> data)
        {
            var dataSmdRa= data.Where(x => x.SourceTable == "SMD" || x.SourceTable == "RA");
            var result = new List<CADMismatchLog>();

            foreach (var smdRaObject in dataSmdRa)
            {
                // setting a threshold i.e. if LastUpdated is null, then set countmismatch null too
                var countMismatch = smdRaObject.LastUpdated == DateTime.MinValue ? null : smdRaObject.CountMismatch;

                var mismatchLog = new CADMismatchLog
                {
                    SourceTable = smdRaObject.SourceTable,
                    LastUpdated = smdRaObject.LastUpdated,
                    Remarks = smdRaObject.Remarks,
                    CountMismatch = countMismatch
                };

                result.Add(mismatchLog);
            }

            return result;
        }
    }   
}
