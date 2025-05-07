using Staffing.Coveo.API.Contracts.Services;

namespace Staffing.Coveo.API.Models
{
    public class AppSettings
    {
        public Coveo Coveo { get; set; }
    }

    public class Coveo
    {
        public string ApiKey { get; set; }
        public string OrganizationId { get; set; }
        public string SearchURL { get; set; }
        public Analytics Analytics { get; set; }
        public string Query { get; set; }
        public string AdvancedQuery { get; set; }
        public string SearchHub { get; set; }
        public string DefaultQueryPatameters { get; set; }
        public string Pipeline { get; set; }
        public Resourcesource ResourceSource { get; set; }
        public CaseSource CaseSource { get; set; }
        public OpportunitySource OpportunitySource { get; set; }
        public AllocationSource AllocationSource { get; set; }
        public CommitmentSource CommitmentSource { get; set; }
        public TimeOffSource TimeOffSource { get; set; }
        public TrainingsSource TrainingsSource { get; set; }
        public TransactionsSource TransactionsSource { get; set; }
        public WorkdayLOATransactionSource WorkdayLOATransactionSource { get; set; }
    }

    public class Resourcesource : ISource
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string SortCriteria { get; set; }
        public string FirstResult { get; set; }
        public int NumberOfResults { get; set; }
        public NestedQuery NestedQuery { get; set; }
    }

    public class CaseSource : ISource
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string SortCriteria { get; set; }
        public string FirstResult { get; set; }
        public int NumberOfResults { get; set; }
        public NestedQuery NestedQuery { get; set; }
    }

    public class OpportunitySource : ISource
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string SortCriteria { get; set; }
        public string FirstResult { get; set; }
        public int NumberOfResults { get; set; }
        public NestedQuery NestedQuery { get; set; }
    }

    public class AllocationSource : ISource
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string SortCriteria { get; set; }
        public string FirstResult { get; set; }
        public int NumberOfResults { get; set; }
        public NestedQuery NestedQuery { get; set; }
    }

    public class CommitmentSource : ISource
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string SortCriteria { get; set; }
        public string FirstResult { get; set; }
        public int NumberOfResults { get; set; }
        public NestedQuery NestedQuery { get; set; }
    }

    public class TimeOffSource : ISource
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string SortCriteria { get; set; }
        public string FirstResult { get; set; }
        public int NumberOfResults { get; set; }
        public NestedQuery NestedQuery { get; set; }
    }

    public class TrainingsSource : ISource
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string SortCriteria { get; set; }
        public string FirstResult { get; set; }
        public int NumberOfResults { get; set; }
        public NestedQuery NestedQuery { get; set; }
    }

    public class TransactionsSource : ISource
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string SortCriteria { get; set; }
        public string FirstResult { get; set; }
        public int NumberOfResults { get; set; }
        public NestedQuery NestedQuery { get; set; }
    }

    public class WorkdayLOATransactionSource : ISource
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string SortCriteria { get; set; }
        public string FirstResult { get; set; }
        public int NumberOfResults { get; set; }
        public NestedQuery NestedQuery { get; set; }
    }

    public class NestedQuery
    {
        public bool IsRequired { get; set; }
        public string Sources { get; set; }
        public string InField { get; set; }
        public string OutField { get; set; }
    }
}
