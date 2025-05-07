using System;

namespace BackgroundPolling.API.Models
{
    public class LOATransactionProcess
    {
        public DateTime? LastDayOfWork { get; set; }
        public DateTime FirstDayOfLeave { get; set; }
        public DateTime? EstimatedLastDayOfLeave { get; set; }
        public DateTime? ActualLastDayOfLeave { get; set; }
        public DateTime? FirstDayBackAtWork { get; set; }
        public DateTime? FirstDayAtWork { get; set; }
    }
}
