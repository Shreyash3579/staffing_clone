using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Hcpd.API.Models
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        public string FormItemTypeCode { get; set; }
        public string FormItemTypeName { get; set; }
        public Int16 FormItemValueCode { get; set; }
        public string FormItemValue { get; set; }
        public string DocTitle { get; set; }
        public string Document { get; set; }
        public string ReviewStatusCode { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime ReviewStatusDate { get; set; }
    }
}
