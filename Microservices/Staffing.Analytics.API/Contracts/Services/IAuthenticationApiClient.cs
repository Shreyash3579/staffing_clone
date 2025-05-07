using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface IAuthenticationApiClient
    {
        Task<String> GetToken(string appName, string appSecret);
    }
}
