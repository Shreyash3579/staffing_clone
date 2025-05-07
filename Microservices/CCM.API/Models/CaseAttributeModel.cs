using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCM.API.Models
{
    public class CaseAttributeModel
    {
        public int clientCode { get; set; }
        public int caseCode { get; set; }
        public string oldCaseCode { get; set; }
        public int caseAttributeCode { get; set; }
        public string caseAttributeName { get; set; }
        public DateTime lastUpdated { get; set; }
        public string lastUpdatedBy { get; set; }
    }
}
