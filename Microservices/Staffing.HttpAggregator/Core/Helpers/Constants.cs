using System.Collections.Generic;
using System.Linq;

namespace Staffing.HttpAggregator.Core.Helpers
{
    public static class Constants
    {
        public static class StaffingTag
        {
            public const string AAG = "AAG";
            public const string PEG = "PEG";
            public const string PEG_SURGE = "PEG-Surge";
            public const string FRWD = "FRWD";
            public const string ADAPT = "ADAPT";
        }

        public static class CaseAttribute
        {
            public const string AAG = "Advanced Analytics";
        }

        public static class EmailStatus
        {
            public const string INPROGRESS = "InProgress";
            public const string FAILED = "Failed";
            public const string SUCCESS = "Success";
        }

        public static class EmailType
        {
            public const string EXPERTS = "1";
            public const string INNOVATIONANDDESIGN = "2";
        }

        public static class DemandType
        {
            public const string Opportunity = "Opportunity";
            public const string NewDemand = "NewDemand";
            public const string StaffedCase = "StaffedCase";
            public const string CaseEnding = "CaseEnding";
            public const string ActiveCase = "ActiveCase";
            public const string CasesStaffedBySupply = "CasesStaffedBySupply";
            public const string PlanningCards = "PlanningCards";
        }

        public enum CaseType
        {
            Billable = 1,
            Adminstrative = 2,
            ClientDevelopment = 4,
            ProBono = 5
        }

        public static class SortBy
        {
            public const string Name = "fullName";
            public const string AvailabilityDate = "dateFirstAvailable";
            public const string AvailabilityPercent = "percentAvailable";
            public const string LevelGrade = "levelGrade";
            public const string Office = "office";
        }

        public static class ServiceLineCodes
        {
            public const string GeneralConsulting = "SL0001";
            public const string AAG = "SL0022";
            public const string PEG = "P";
            public const string PEG_SURGE = "PS";
            public const string FRWD = "SL0011";
            public const string InnovationAndDesign = "SL0006";
            public const string EXPERTS = "SL0002";
        }

        public static class StaffingPreferenceType
        {
            public const string Industry = "I";
            public const string Capability = "C";
        }

        public static class CommitmentTypes
        {
            public const string Allocation = "allocation";
            public const string NamedPlaceholderAllocation = "namedPlaceholderAllocations";
            public const string Holiday = "holiday";
            public const string LOA = "loa";
            public const string Transfer = "transfer";
            public const string Transition = "transition";
            public const string Vacation = "vacation";
            public const string Training = "training";
            public const string Termination = "termination";
            public const string PlaceholderAndPlanningCard = "placeholderAndPlanningCard";
        }

        public static class EmployeeStatus
        {
            public const string LOA = "loa";
            public const string Transition = "transition";
            public const string Active = "active";
            public const string NotYetStarted = "notYetStarted";
        }
        public static class CasePlanningBoardColumns
        {
            public const string NewDemandsColumn = "New Demands";
            public const string ProjectsToBeIncludedInDemandMetrics = "Projects To Be Included In Demand Metrics";
        }
    }
    public enum ProjectStatus { Active, Inactive };

    public static class CommonUtils
    {
        public static IList<string> GetServiceLineCodeNames(string attributeCodes)
        {
            List<string> caseOppAttributeCodes = attributeCodes.Split(',').ToList();
            string caseAttributeNames = string.Empty;

            caseAttributeNames += caseOppAttributeCodes.Contains(Constants.ServiceLineCodes.PEG) ? Constants.StaffingTag.PEG + "," : string.Empty;
            caseAttributeNames += caseOppAttributeCodes.Contains(Constants.ServiceLineCodes.PEG_SURGE) ? Constants.StaffingTag.PEG_SURGE + "," : string.Empty;
            caseAttributeNames += caseOppAttributeCodes.Contains(Constants.ServiceLineCodes.AAG) ? Constants.StaffingTag.AAG + "," : string.Empty;
            caseAttributeNames += caseOppAttributeCodes.Contains(Constants.ServiceLineCodes.FRWD) ? Constants.StaffingTag.FRWD + "," : string.Empty;
            caseAttributeNames += caseOppAttributeCodes.Contains(Constants.ServiceLineCodes.InnovationAndDesign) ? Constants.StaffingTag.ADAPT + "," : string.Empty;
            var list = caseAttributeNames.Split(',').ToList();
            list.RemoveAll(x => x == "");
            return list;
        }
    }
}