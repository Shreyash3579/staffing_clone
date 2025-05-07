namespace Staffing.Analytics.API.Core.Helpers
{
    public class Constants
    {
        public const string ExternalCommitments = "ExternalCommitments";
        public static class Policy
        {
            public const string StaffingUserOnly = "StaffingUserOnly";
            public const string StaffingAnalyticsRead = "StaffingAnalyticsReadAccess";
        }

        public static class Role
        {            
            public const string BackgroundPollingApiAccess = "BackgroundPollingApi";
            public const string StaffingAnalyticsReadAccess = "StaffingAnalyticsReadAccess";
        }
        public static class EffectiveCostReasons
        {
            public const string HISTORICAL_RATE_NA = "Historical Bill Rate NA";
        }

        public enum EmployeeStatus
        {
            Terminated = 1,
            Transition = 2,
            LOAPaid = 3,
            LOAUnpaid = 4,
            LOABOSS = 5,
            Active = 6
        }
    }
}
