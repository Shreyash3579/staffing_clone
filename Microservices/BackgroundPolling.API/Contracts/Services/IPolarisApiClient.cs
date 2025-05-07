using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IPolarisApiClient
    {
        public Task<IList<PolarisSecurityUser>> GetRevSecurityUsersWithGeography();
    }
}
