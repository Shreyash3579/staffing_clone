using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class EmailUtilityService : IEmailUtilityService
    {
        private readonly IStaffingAnalyticsRepository _staffingAnalyticsRepository;
        private readonly AppSettingsConfiguration _appSettings;
        private readonly ICcmApiClient _ccmApiClient;
        private readonly IHttpAggregatorClient _httpAggregatorClient;

        public EmailUtilityService(IStaffingAnalyticsRepository staffingAnalyticsRepository,
            ICcmApiClient ccmApiClient,
            IOptionsSnapshot<AppSettingsConfiguration> appSettings, IHttpAggregatorClient httpAggregatorClient)
        {
            _staffingAnalyticsRepository = staffingAnalyticsRepository;
            _appSettings = appSettings.Value;
            _ccmApiClient = ccmApiClient;
            _httpAggregatorClient = httpAggregatorClient;
        }

        public async Task TestEmailWithO365()
        {
            var emailRecipients = _appSettings.DebugEmail;
            EmailHelper.TestEmailWithO365(emailRecipients);
        }

        public async Task SendEmailForAuditsOfAllocationByStaffingUser(string staffingUserECode, DateTime auditLogsFromDate) 
        {
            var auditLogs = await _staffingAnalyticsRepository.GetAuditLogsForSelectedUserAndDate(staffingUserECode, auditLogsFromDate);
            var emailRecipients = _appSettings.DebugEmail;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (Environments.Production == environment)
            {
                emailRecipients = _appSettings.BCNAuditLogEmail;
            }
            EmailHelper.SendEmailForAuditsOfAllocationByStaffingUser(emailRecipients, auditLogs.ToList(), auditLogsFromDate);
        }

        public async Task SendEmailsForCasesServedByRingfenceByOfficeAndCaseType(string officeCodes, string caseTypeCodes)
        {
            var EMEARegionCode = Convert.ToInt32(ConfigurationUtility.GetValue("RegionCodes:EMEARegionCode"));

            var EMEAOfficeCodes = await _ccmApiClient.GetOfficesFlatListByRegionOrCluster(EMEARegionCode);
            if (string.IsNullOrEmpty(officeCodes))
            {
                officeCodes = string.Join(",", EMEAOfficeCodes.Select(x => x.OfficeCode));
            }
            if (string.IsNullOrEmpty(caseTypeCodes))
            {
                caseTypeCodes = "1"; // Billable
            }
            var casesServedByRingfence = await _staffingAnalyticsRepository.GetCasesServedByRingfenceByOfficeAndCaseType(officeCodes, caseTypeCodes);
            var emailRecipients = _appSettings.DebugEmail;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (Environments.Production == environment)
            {
                emailRecipients = _appSettings.EmeaPEGStaffingUserEmail;
            }

            var casesServedByRingfenceWithSelectedProprties = casesServedByRingfence.Select(x => new
            {
                x.OldCaseCode,
                x.StartDate,
                x.EndDate,
                x.ManagingOfficeCode,
                x.ManagingOfficeName
            });
            EmailHelper.SendEmailToEmeaPegStaffingOfficer(emailRecipients, casesServedByRingfenceWithSelectedProprties.ToList());
        }

        public async Task<string> SendMonthlyStaffingAllocationsEmailToExperts(string employeeCodes)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (environment == Environments.Production || environment == Environments.Development)
            {
                var responseMessage = await _httpAggregatorClient.SendMonthlyStaffingAllocationsEmailToExperts(employeeCodes);
                return responseMessage;
            }
            else
            {
                return "Auto-Job scheduled for Development & Prod Only";
            }

        }

        public async Task<string> SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(string employeeCodes)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (environment == Environments.Production || environment == Environments.Development)
            {
                var responseMessage = await _httpAggregatorClient.SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(employeeCodes);
                return responseMessage;
            }
            else
            {
                return "Auto-Job scheduled for Development & Prod Only";
            }

        }
    }
}
