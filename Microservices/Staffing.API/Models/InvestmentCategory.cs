namespace Staffing.API.Models
{
    public class InvestmentCategory
    {
        public int InvestmentCode { get; set; }
        public string InvestmentName { get; set; }
        public string InvestmentDescription { get; set; }
        public int Precedence { get; set; }

        public string ApplicableCaseTypeCodes { get; set; }
    }
}
