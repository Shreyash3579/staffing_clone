using System.Collections.Generic;

namespace BackgroundPolling.API.Models
{
    public class EmployeeLanguages
    {
        public string EmployeeCode { get; set; }
        public IList<Language> Languages { get; set; }
    }

    public class Language
    {
        public string Name { get; set; }
        public int ProficiencyCode { get; set; }
        public string ProficiencyName { get; set; }
    }
}
