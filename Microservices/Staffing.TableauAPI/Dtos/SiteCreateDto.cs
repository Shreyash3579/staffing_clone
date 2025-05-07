using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.TableauAPI.Dtos
{
    public class SiteCreateDto
    {
        [Required]
        public string Name { get; set; }
        public string Type { get; set; }
        public int SiteType { get; set; }
        public DateTime Created { get; set; }
    }
}
