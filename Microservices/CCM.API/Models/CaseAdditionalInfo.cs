namespace CCM.API.Models
{
    public class CaseAdditionalInfo
    {
        public int CaseCode { get; set; }
        public string CaseName { get; set; }
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public string OldCaseCode { get; set; }
        public int CaseTypeCode { get; set; }
        public string CaseTypeName { get; set; }
        public int ClientGroupCode { get; set; }
        public string ClientGroupName { get; set; }
        public string PrimaryIndustry { get; set; }
        public string PrimaryCapability { get; set; }
        public int PracticeAreaIndustryCode { get; set; }
        public string PracticeAreaIndustryAbbreviation { get; set; }
        public string PracticeAreaIndustry { get; set; }
        public int PracticeAreaCapabilityCode { get; set; }
        public string PracticeAreaCapabilityAbbreviation { get; set; }
        public string PracticeAreaCapability { get; set; }
        public bool PegCase { get; set; }
        public string CaseManagerCode { get; set; }
        public string CaseManagerName { get; set; }
        public int ManagingOfficeCode { get; set; }
        public string ManagingOfficeName { get; set; }
        public string ManagingOfficeAbbreviation { get; set; }
        public int BillingOfficeCode { get; set; }
        public string BillingOfficeAbbreviation { get; set; }
        public string BillingOfficeName { get; set; }
        public bool IsPegCaseClass { get; set; }
        public int PegIndustryTermCode { get; set; }
        public string PegIndustryTerm { get; set; }
        public string PegIndustryAbbreviation { get; set; }

    }
}
