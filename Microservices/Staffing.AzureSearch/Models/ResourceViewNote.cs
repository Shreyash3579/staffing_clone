namespace Staffing.AzureSearch.Models
{
    public class ResourceViewNote
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string Note { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
