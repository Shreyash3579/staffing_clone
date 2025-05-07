using System;

namespace Staffing.API.ViewModels
{
    public class UserNotificationViewModel
    {
        public Guid NotificationId { get; set; }
        public string Description { get; set; }
        public DateTime NotificationDate { get; set; }
        public DateTime NotificationDueDate { get; set; }
        public char NotificationStatus { get; set; }
        public string pipelineId { get; set; } = null;
        public string OldCaseCode { get; set; } = null;
        public string EmployeeCode { get; set; } = null;
    }
}