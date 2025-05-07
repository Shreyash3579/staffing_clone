using Staffing.HttpAggregator.Models;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class EmployeeInfoWithGxcAffiliationsViewModel
    {
        public Resource employee { get; set; }
        public IEnumerable<EmployeePracticeAreaViewModel> affiliationsByEmployeeCodesAndPracticeAreaCodes { get; set; }
        public AdvisorViewModel employeeAdvisor { get; set; }
        public IEnumerable<MenteeViewModel> employeeMentees { get; set; }
        public IEnumerable<ResourceTimeInLevel> employeeTimeInLevel { get; set; }
    }
}
