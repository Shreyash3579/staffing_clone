using System;

namespace BackgroundPolling.API.Models
{
    public class Notification
    {
        public Guid? Id { get; set; }
        public int NotificationTypeCode { get; set; }
        public int OfficeCode { get; set; }
        public string EmployeeCode { get; set; }
        public string OldCaseCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public char NotificationStatus { get; set; }
        public string NotificationNotes { get; set; }
        public string LastUpdatedBy { get; set; }
    }

    public enum NotificationType
    {
        CaseEnd = 1,
        CaseRoll,
        BackFill,
        Training,
        LOA
    }
}
