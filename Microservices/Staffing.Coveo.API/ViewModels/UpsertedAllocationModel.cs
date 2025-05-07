using System;

namespace Staffing.Coveo.API.ViewModels
{
    public class UpsertedAllocationModel
    {
        public Guid? id { get; set; }
        public int clientCode { get; set; }
        public int? caseCode { get; set; }
        public string oldCaseCode { get; set; }
        public string employeeCode { get; set; }
        public string serviceLineCode { get; set; }
        public string serviceLineName { get; set; }
        public int operatingOfficeCode { get; set; }
        public string currentlevelGrade { get; set; }
        public int allocationPercent { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public Guid? pipelineId { get; set; }
        public int? investmentCode { get; set; }
        public string investmentName { get; set; }
        public string caseRoleCode { get; set; }
        public string caseRoleName { get; set; }
        public string notes { get; set; }
        public DateTime lastUpdated { get; set; }
        public string lastUpdatedBy { get; set; }
        public string documentId { get; set; }
        public string data { get; set; }
    }
}
