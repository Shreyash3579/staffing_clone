using System;

namespace Staffing.TableauAPI.Dtos
{
    public class SiteDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int SiteType { get; set; }
        public DateTime Created { get; set; }
    }
}
