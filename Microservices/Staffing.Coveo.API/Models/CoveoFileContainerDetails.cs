using System;

namespace Staffing.Coveo.API.Models
{
    public class CoveoFileContainerDetails
    {
        public string UploadUri { get; set; }
        public Guid FileId { get; set; }
        public dynamic RequiredHeaders { get; set; }
    }
}
