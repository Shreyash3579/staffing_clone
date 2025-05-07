using CCM.API.Models;
using System.Collections.Generic;

namespace CCM.API.ViewModels
{
    public class CaseMasterViewModel
    {
        public IList<CaseMaster> CaseMaster { get; set; }
        public IList<CaseMasterHistory> CaseMasterHistory { get; set; }
    }
}
