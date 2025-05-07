namespace BackgroundPolling.API.Models
{
    public class Office
    {
        public int OfficeCode { get; set; }
        public string OfficeName { get; set; }
        public string OfficeAbbreviation { get; set; }
        public string OfficeCluster { get; set; }

        public string OfficeRegion { get; set; }
        public int? OfficeRegionCode { get; set; }

        public string OfficeSubRegion { get; set; }
    }
}
