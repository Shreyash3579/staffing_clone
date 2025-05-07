namespace Staffing.API.Models
{
    public class CasePlanningBoardBucket
    {
        public int Id { get; set; }
        public string BucketName { get; set; }
        public short SortOrder { get; set; }
        public bool IncludeInDemand { get; set; }
        public bool IsPartiallyChecked { get; set; }
    }
}
