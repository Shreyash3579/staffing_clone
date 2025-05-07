using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IBvuApiClient
    {
        public Task<IList<Training>> GetTrainings(DateTime? lastPolledDateTime);
    }
}
