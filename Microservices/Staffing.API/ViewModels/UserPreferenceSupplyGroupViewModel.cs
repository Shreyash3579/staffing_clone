using Staffing.API.Models;
using System.Collections.Generic;
using System;

namespace Staffing.API.ViewModels
{
    public class UserPreferenceSupplyGroupViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDefaultForResourcesTab { get; set; }
        public bool IsShared { get; set; }
        public string GroupMemberCodes { get; set; }
        public string SortBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public IEnumerable<UserPreferenceGroupFilters> FilterBy { get; set; }
        public IEnumerable<UserPreferenceGroupSharedInfo> SharedWith { get; set; }

    }
}
