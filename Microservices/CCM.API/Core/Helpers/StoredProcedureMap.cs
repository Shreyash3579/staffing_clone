namespace CCM.API.Core.Helpers
{
    public static class StoredProcedureMap
    {
        //Case Controller
        public const string GetActiveCasesExceptNewDemandsByOffices = "staffingGetActiveCasesExceptNewDemandCasesByOffices";
        public const string GetCaseDataByCaseCodes = "staffingGetCaseDataByCaseCodes";
        public const string GetCaseDetailsByCaseCodes = "staffingGetCaseDetailsByCaseCodes";
        public const string GetCasesEndingBySpecificDate = "staffingGetCasesEndingBySpecificDate";
        public const string GetCasesForTypeahead = "staffingGetCasesForTypeahead";
        public const string GetCasesWithTaxonomiesByCaseCodes = "staffingGetCasesWithTaxonomiesByCaseCodes";
        public const string GetNewDemandCasesByOffices = "staffingGetNewDemandCasesByOffices";
        public const string GetCasesActiveAfterSpecifiedDate = "staffingGetCasesActiveAfterSpecifiedDate";
        public const string GetCasesWithStartOrEndDateUpdatedInCCM = "staffingGetCasesWithStartOrEndDateUpdatedInCCM";
        public const string GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled = "staffingGetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled";
        public const string GetCaseAttributeByLastUpdatedDate = "staffingGetCaseAttributeByLastUpdatedDate";
        public const string GetCaseAdditionalInfo = "staffingGetCaseAdditionalInfo";
        //CaseType Controller
        public const string GetCaseTypeList = "staffingGetCaseTypeList";

        // Lookup Controller
        public const string GetCaseAttributeLookupList = "staffingGetCCMCaseAttributeLookupList";

        // CaseOpportunityMap controller
        public const string GetCasesByPipelineIds = "staffingGetCasesByPipelineIds";
    }
}
