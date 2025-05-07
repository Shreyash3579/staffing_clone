using System;

namespace Staffing.Coveo.API.Models
{
    public class ServiceLine
    {
        public Guid ServiceLineId { get; set; }
        public string ServiceLineHierarchyCode { get; set; }
        public string ServiceLineHierarchyName { get; set; }
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public int EmployeeCount { get; set; }
        public bool InActive { get; set; }

        public override bool Equals(object obj)
        {
            return ((ServiceLine)obj)?.ServiceLineCode == ServiceLineCode;
        }
        public override int GetHashCode()
        {
            return ServiceLineCode.GetHashCode();
        }
    }
}
