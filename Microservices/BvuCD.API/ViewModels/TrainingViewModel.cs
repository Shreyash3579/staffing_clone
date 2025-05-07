using System;
namespace BvuCD.API.ViewModels
{
    public class TrainingViewModel
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string Role { get; set; }
        public string TrainingName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
