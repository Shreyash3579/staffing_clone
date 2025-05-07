namespace Staffing.Coveo.API.Core.Helpers
{
    public static class Constants
    {
        public static class DemandType
        {
            public const string Opportunity = "Opportunity";
            public const string NewDemand = "NewDemand";
            public const string ActiveCase = "ActiveCase";
        }
        public enum ProjectStatus { Active, Inactive };
        public static class Source
        {
            public const string Resource = "resource";
            public const string Project = "project";
            public const string Everything = "everything";
        }
    }
}