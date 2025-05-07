namespace Staffing.Analytics.API.Models.Workday
{
    public class JobProfile
    {
        public string DefaultJobTitle { get; set; }
        public string PositionCode { get; set; }
        public string PositionGroupName { get; set; }
        public string PositionGroupCode { get; set; }
        public string JobCategoryId { get; set; }
        public string JobCategory { get; set; }
        public bool InActive { get; set; }
    }
}
