using System;

namespace Staffing.API.Models
{
    public class UserNotification
    {
        public Guid NotificationId { get; set; }
        public string NotificationType { get; set; }
        public char NotificationStatus { get; set; }
        public int? OfficeCode { get; set; }
        public string OldCaseCode { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string NotificationNotes { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}