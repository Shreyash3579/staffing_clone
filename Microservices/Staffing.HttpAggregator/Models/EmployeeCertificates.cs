using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
{
    public class EmployeeCertificates
    {
        public string EmployeeCode { get; set; }
        public List<Certification> Certifications { get; set; }
    }

    public class Certification
    {
        public DateTime? IssuedDate { get; set; }
        public string Name { get; set; }
    }
}
