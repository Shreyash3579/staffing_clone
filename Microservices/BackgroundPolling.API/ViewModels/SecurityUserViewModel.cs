namespace BackgroundPolling.API.ViewModels
{
    public class SecurityUserViewModel
    {
        public string EmployeeCode { get; set; }
        public string RoleCodes { get; set; }
        public bool IsBossSystemUser { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Source { get; set; }
    }
}

