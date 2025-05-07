namespace Staffing.API.Core.Helpers
{
    public static class Constants
    {
        
        public static class Policy
        {
            public const string StaffingAllAccess = "StaffingAllAccess";
            public const string ResourceAllocationRead = "ResourceAllocationReadAccess";
            public const string CRRatioRead = "CRRatioReadAccess";
            public const string StaffingApiLookupRead = "StaffingApiLookupReadAccess";
            public const string CommitmentRead = "CommitmentReadAccess";
            public const string PlanningCardDetailsRead = "PlanningCardDetailsAccess";

        }

        public static class Role
        {
            public const string ResourceAllocationReadAccess = "ResourceAllocationReadAccess";
            public const string CRRatioReadAccess = "CRRatioReadAccess";
            public const string BackgroundPollingApiAccess = "BackgroundPollingApi";
            public const string StaffingApiLookupReadAccess = "StaffingApiLookupReadAccess";
            public const string CommitmentReadAccess = "CommitmentReadAccess";
            
        }

        public static class ApiEndPoints
        {
            public const string GetResourceAllocationsBySelectedValues = "GetResourceAllocationsBySelectedValues";
            public const string GetCommitmentBySelectedValues = "GetCommitmentBySelectedValues";
        }

        public static class ClientProperties
        {
            public const string ClientName = "ClientName";
            public const string MandatoryParams = "MandatoryParams";
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
