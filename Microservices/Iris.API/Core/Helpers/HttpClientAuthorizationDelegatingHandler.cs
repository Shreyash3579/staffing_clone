using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Iris.API.Core.Helpers
{
    public class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccesor;

        public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccesor)
        {
            _httpContextAccesor = httpContextAccesor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authorizationApiKey = ConfigurationUtility.GetValue("Authorization:ApiKey");
            var authorizationApiValue = ConfigurationUtility.GetValue("Authorization:Value");

            var authorizationApiKeyIntegrations = ConfigurationUtility.GetValue("Authorization:ApiKeyIntegrations");
            var authorizationApiValueIntegrations = ConfigurationUtility.GetValue("Authorization:ValueIntegrations");

            var authorizationHeader = _httpContextAccesor.HttpContext
                .Request.Headers[authorizationApiKey];

            var authorizationHeader2 = _httpContextAccesor.HttpContext
                .Request.Headers[authorizationApiKeyIntegrations];

            if (string.IsNullOrEmpty(authorizationHeader))
            {
                request.Headers.Add(authorizationApiKey, authorizationApiValue);
            }

            if (string.IsNullOrEmpty(authorizationHeader2))
            {
                request.Headers.Add(authorizationApiKeyIntegrations, authorizationApiValueIntegrations);
            }

            return await base.SendAsync(request, cancellationToken);
        }       
    }
}
