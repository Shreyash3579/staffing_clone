using Hcpd.API.Contracts.RepositoryInterfaces;
using Hcpd.API.Core.Helpers;
using Hcpd.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hcpd.API.Core.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IBaseRepository<Review> _baseRepository;

        public ReviewRepository(IBaseRepository<Review> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<IEnumerable<Review>> GetReviewsByEmployeeCode(string employeeCode)
        {
            var result = await
                _baseRepository.GetAllAsync(new { employeeCode },
                    StoredProcedureMap.GetEmployeeReviews);

            return result;
        }
    }
}
