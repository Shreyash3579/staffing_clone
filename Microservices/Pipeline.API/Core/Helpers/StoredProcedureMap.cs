namespace Pipeline.API.Core.Helpers
{
    public static class StoredProcedureMap
    {
        public const string GetOpportunitiesByOffices = "staffingGetOpportunitiesByOffices";
        public const string GetOpportunityByPipelineIds = "staffingGetOpportunityByPipelineIds";
        public const string GetOpportunitiesWithTaxonomiesByPipelineIds = "staffingGetOpportunitiesWithTaxonomiesByPipelineIds";
        public const string GetOpportunitiesForTypeahead = "staffingGetopportunitiesForTypeahead";
        public const string GetOpportunityStatusTypeList = "staffingGetOpportunityStatusTypeList";
        public const string GetOpportunityMasterChangesSinceLastPolled = "staffingGetOpportunityMasterChangesSinceLastPolled";
        public const string GetOpportunityByCortexIds = "BOSS.GetOpportunityByCortexIds";
        public const string GetOpportunitiesFlatData = "staffingGetOpportunitiesFlatData";

    }
}
