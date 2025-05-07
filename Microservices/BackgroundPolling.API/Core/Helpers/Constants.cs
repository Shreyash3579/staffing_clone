namespace BackgroundPolling.API.Core.Helpers
{
    public static class Constants
    {
        public enum EmployeeStatus
        {
            Terminated = 1,
            Transition = 2,
            LOAPaid = 3,
            LOAUnpaid = 4,
            LOABOSS = 5,
            Active = 6
        }
        public enum CaseType
        {
            Billable = 1,
            Adminstrative = 2,
            ClientDevelopment = 4,
            ProBono = 5
        }
        public enum RoleCodes
        { 
            Rev = 6,
            Hr=5,
            PracticeStaffing = 9
        }

        public enum InvestmentTypeCodes
        {
            PrePost = 4
        }

        public static class Source
        {
            public const string HR = "HR";
            public const string Boss = "Boss";
            public const string Workday = "Workday";
            public const string Polaris = "Polaris";
            public const string Rev = "Rev";
            public const string PracticeStaffing = "Practices";

        }

        public const string WorkdayStaffingTransactionsToBeEffective = "WorkdayStaffingTransactionsToBeEffective";
        public const string LoAUpdatedRecentlyInworkday = "LoAUpdatedRecentlyInworkday";
        public const string CaseMasterAndCaseMasterHistoryUpdatedRecentlyInBasis = "CaseMasterAndCaseMasterHistoryUpdatedRecentlyInBasis";
        public const string CasesPreponedRecentlyInCCM = "CasesPreponedRecentlyInCCM";
        public const string CurrencyRateUpdatedInCCM = "CurrencyRateUpdatedInCCM";
        public const string CaseEndDateUpdatedInCCM = "CaseEndDateUpdatedInCCM";
        public const string CaseRollProcessedRecentlyInStaffing = "CaseRollProcessedRecentlyInStaffing";
        public const string EndDateColumn = "enddate";
        public const string CaseAdditionalInfo = "CaseAdditionalInfo";
    }
}
