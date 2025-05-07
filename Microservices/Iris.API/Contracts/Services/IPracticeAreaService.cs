using Iris.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iris.API.Contracts.Services
{
    public interface IPracticeAreaService
    {
        Task<IEnumerable<PracticeArea>> GetAllIndustryPracticeArea();
        Task<IEnumerable<PracticeArea>> GetAllCapabilityPracticeArea();
    }
}
