using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Models
{
    public class CaseAttribute
    {
        public int clientCode { get; set; }
        public int caseCode { get; set; }
        public string oldCaseCode { get; set; }
        public int caseAttributeCode { get; set; }
        public string caseAttributeName { get; set; }
    }
}
