using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IAuthenticationApiClient
    {
        Task<string> GetToken(string appName, string appSecret);
    }
}
