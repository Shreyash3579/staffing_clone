using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.TableauAPI.Dtos
{
    public class SiteUpdateDto
    {
        public string Name { get; set; }
        public int SiteType { get; set; }
        public string Type { get; set; }
        public DateTime Created { get; set; }
    }
}
