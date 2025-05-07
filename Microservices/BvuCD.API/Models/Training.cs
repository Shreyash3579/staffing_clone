using System;

namespace BvuCD.API.Models
{
    public class Training
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string AttendeeRole { get; set; }
        public string TrainingName { get; set; }
        public string TrainingDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SessionName { get; set; }
        public string SessionDescription { get; set; }
        public string TrainingLocation { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }

    }

}
