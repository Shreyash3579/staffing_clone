namespace Pipeline.API.Core.Helpers
{
    public class Constants
    {
        public static class Policy
        {
            public const string PipelineAllAccess = "PipelineAllAccess";
            public const string OpportunityDetailsReadAccess = "OpportunityDetailsReadAccess";
            public const string OpportunityLookupReadAccess = "OpportunityLookupReadAccess";
        }

        public static class Role
        {
            public const string BackgroundPollingApiAccess = "BackgroundPollingApi";
            public const string PipelineApiAllAccess = "PipelineApiAllAccess";

        }
    }
}
