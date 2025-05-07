using Iris.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iris.API.Contracts.RepositoryInterfaces
{
    public interface IPracticeAreaRepository
    {
        Task<IEnumerable<PracticeArea>> GetAllIndustryPracticeArea();
        Task<IEnumerable<PracticeArea>> GetAllCapabilityPracticeArea();
    }
}
