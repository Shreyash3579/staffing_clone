using CCM.API.Models;
using CCM.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCM.API.Contracts.Services
{
    public interface ICaseAttributeService
    {
        Task<IEnumerable<CaseAttributeModel>> GetCaseAttributesByLastUpdatedDate(DateTime? lastUpdatedDate);
    }
}
