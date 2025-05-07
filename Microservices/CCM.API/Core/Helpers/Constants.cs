namespace CCM.API.Core.Helpers
{
    public static class Constants
    {
        public const string NewDemand = "NewDemand";
        public const string ActiveCase = "ActiveCase";
        public const string OfficeClusterEntityTypeCode = "OC";
        public const string OfficeRegionEntityTypeCode = "RG";
        public const string OfficeSubRegionEntityTypeCode = "SR";

        public static class Policy
        {
            public const string CCMAllAccess = "CCMAllAccess";
            public const string OfficeLookupReadAccess = "OfficeLookupReadAccess";
            public const string CaseInfoReadAccess = "CaseInfoReadAccess";
            public const string CaseTypeLookupReadAccess = "CaseTypeLookupReadAccess";
        }

        public static class Role
        {
            public const string BackgroundPollingApiAccess = "BackgroundPollingApi";

        }

    }

}
