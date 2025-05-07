using System;

namespace CaseIntake.API.Models
{
    public class MessageObject
    {
        public string Name { get; set; }    // The name from opportunity, planning card, or case details
        public string Id { get; set; }      // The ID which could be opportunityId, planningCardId, or oldCaseCode
        public DateTime LastUpdated { get; set; }  // The time when the message was created/updated
        public string LastUpdatedBy { get; set; }      // The name of the person who last updated the item
    }

}
