namespace Staffing.API.Models
{
    public class RevOffice
    {
        public int OfficeLevel { get; set; }
        public int OfficeCode { get; set; }
        public string OfficeName { get; set; }
        public int ParentOfficeCode { get; set; }
        public string CurrencyCode { get; set; }
        public string OfficeHead { get; set; }
        public string EntityTypeCode { get; set; }
        public string OfficeAbbreviation { get; set; }
        public string OfficeStatus { get; set; }
        public string LastUpdatedBy { get; set; }   
    }
}
