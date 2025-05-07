using System;

namespace Staffing.API.Models
{
    public class NoteSharedWithGroup
    {
        public Guid? Id { get; set; }

        public string SharedWithEmployeeCode { get; set; }
    }
}
