using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Contracts.Services
{
    public interface IAuthenticationApiClient
    {
        Task<string> GetToken(string appName, string appSecret);
    }
}
