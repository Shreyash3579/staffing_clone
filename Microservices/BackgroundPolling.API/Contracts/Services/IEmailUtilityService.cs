using BackgroundPolling.API.Core.Helpers;
using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IEmailUtilityService
    {
        public Task SendEmailForAuditsOfAllocationByStaffingUser(string staffingUserECode,
            DateTime auditLogsFromDate);

        public Task SendEmailsForCasesServedByRingfenceByOfficeAndCaseType(string officeCodes, string caseTypeCodes);
        public Task TestEmailWithO365();
        [SkipWhenPreviousJobIsRunning]
        public Task<string> SendMonthlyStaffingAllocationsEmailToExperts(string employeeCodes);
        [SkipWhenPreviousJobIsRunning]
        public Task<string> SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(string employeeCodes);
    }

}
