using System;
using System.Collections.Generic; 

namespace Staffing.HttpAggregator.Models
{
    public class NotesSharedWithGroupViewModel
    {
        public Guid? Id { get; set; }
        public IEnumerable<Employee> SharedWithEmployees{ get; set; } 
    }
}