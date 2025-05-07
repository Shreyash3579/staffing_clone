using Hcpd.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hcpd.API.Contracts.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewViewModel>> GetReviewsByEmployeeCode(string employeeCode);
    }
}
