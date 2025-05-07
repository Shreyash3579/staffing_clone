using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Models
{
    public class PracticeAffiliation
    {
        public Guid RecordId { get; set; }
        public string EmployeeCode { get; set; }
        public int RoleCode { get; set; }
        public string Role { get; set; }
        public Guid TagId { get; set; }
        public Guid GroupId { get; set; }
        public string Term { get; set; }
        public int OfficeEntityCode { get; set; }
        public char ContextTypeCode { get; set; }
        public string Entity { get; set; }
        public string PracticeArea { get; set; }
        public int AttributeCode { get; set; }
        public int PracticeAreaCode { get; set; }
        public string Abbreviation { get; set; }
        public string ContextTypeName { get; set; }
    }
}
