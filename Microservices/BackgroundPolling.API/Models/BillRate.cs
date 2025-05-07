using System;

namespace BackgroundPolling.API.Models
{
    public class BillRate
    {
        public Guid Id { get; set; }
        public string OfficeCode { get; set; }
        public string LevelGrade { get; set; }
        public decimal BillCode { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
        public decimal Rate { get; set; }
        public string Breakdown { get; set; }
        public DateTime _startDate { get; set; }
        public DateTime StartDate
        {
            get => _startDate;
            set => _startDate = value.Date;
        }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string LastUpdated { get; set; }
    }
}
