namespace Basis.API.Core.Helpers
{
    public static class StoredProcedureMap
    {
        //PracticeArea Controller
        public const string GetAllPracticeArea = "staffingGetAllPracticeArea";
        public const string GetIndustryPracticeAreaLookupList = "staffingGetIndustryPracticeAreaLookupList";
        public const string GetCapabilityPracticeAreaLookupList = "staffingGetCapabilityPracticeAreaLookupList";
        public const string GetAffiliationsByEmployeeCodesAndPracticeAreaCodes = "staffingGetAffiliationsByEmployeeCodesAndPracticeAreaCodes";
        public const string GetCurrencyRatesByEffectiveDate = "staffingGetCurrencyRatesByEffectiveDate";
        public const string GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate = "staffingGetCurrencyRatesByCurrencyCodesBetweenEffectiveDate";
        public const string GetAllPracticeAffiliation = "staffingGetPracticeAffiliations";
        public const string GetAffiliationRoleList = "staffingGetAffiliationRoleList";

        //Holiday Controller
        public const string GetOfficeHolidaysByEmployee = "staffingGetOfficeHolidaysByEmployee";
        public const string GetOfficeHolidaysWithinDateRangeByEmployees = "staffingGetOfficeHolidaysWithinDateRangeByEmployees";
        public const string GetOfficeHolidaysWithinDateRangeByOffices = "staffingGetOfficeHolidaysWithinDateRangeByOffices";
        public const string GetHolidays = "staffingGetHolidays";
    }
}
