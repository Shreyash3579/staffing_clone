using System;

namespace Vacation.API.Models
{
    public class VacationRequest
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string ReplicationServer { get; set; }
    }
}
