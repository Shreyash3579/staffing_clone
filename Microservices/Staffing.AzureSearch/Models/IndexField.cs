namespace Staffing.AzureSearch.Models
{
    public class IndexField
    {
        public string FieldName { get; set; }
        public string Description { get; set; }

        public IndexField(string fieldName, string description)
        {
            FieldName = fieldName;
            Description = description;
        }
    }
}
