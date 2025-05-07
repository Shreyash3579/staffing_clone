using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IVacationApiClient
    {
        public Task<IList<Vacation>> GetVacationsSinceLastPolled(DateTime? lastPolledDateTime);
    }
}
