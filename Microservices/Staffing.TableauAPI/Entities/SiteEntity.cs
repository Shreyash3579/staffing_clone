using System;

namespace Staffing.TableauAPI.Entities
{
    public class SiteEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int SiteType { get; set; }
        public DateTime Created { get; set; }
    }
}
