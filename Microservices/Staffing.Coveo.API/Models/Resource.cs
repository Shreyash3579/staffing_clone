using Staffing.Coveo.API.ViewModels;
using System;
using System.Collections.Generic;

namespace Staffing.Coveo.API.Models
{
    public class Resource
    {
        public string EmployeeCode { get; set; }
        public string EmployeeType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string LevelGrade { get; set; }
        public string LevelName { get; set; }
        public decimal BillCode { get; set; }
        public bool Status { get; set; }
        public string ActiveStatus { get; set; }
        public string MentorName { get; set; }
        public string MentorEcode { get; set; }
        public string InternetAddress { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string ProfileImageUrl { get; set; }
        public Office Office { get; set; }
        public Office SchedulingOffice { get; set; }
        public decimal FTE { get; set; }
        public ServiceLine ServiceLine { get; set; }
        public Position Position { get; set; }
        public bool IsTerminated { get; set; }
        public string Source { get; set; }
        public string Uri { get; set; }
        public string UriHash { get; set; }
        public string SysCollection { get; set; }
        public Guid SearchUid { get; set; }
        public int RequestDuration { get; set; }
        public string Title { get; set; }
    }

    public class ResourceResponse
    {
        public int TotalCount { get; set; }
        public Guid SearchUid { get; set; }
        public int Duration { get; set; }
        public List<ResourceResult> Results { get; set; }
    }
    public class ResourceResult
    {
        public CommonSourceViewModel Raw { get; set; }
    }
}
