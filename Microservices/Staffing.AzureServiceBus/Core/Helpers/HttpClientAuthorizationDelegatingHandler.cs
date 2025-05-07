using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Staffing.AzureServiceBus.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Core.Helpers
{
    public class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccesor;
        private readonly IAuthenticationApiClient _authenticationApiClient;
        private readonly IMemoryCache _cache;

        public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccesor, IAuthenticationApiClient authenticationApiClient, IMemoryCache cache)
        {
            _httpContextAccesor = httpContextAccesor;
            _authenticationApiClient = authenticationApiClient;
            _cache = cache;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Create Default HttpContext for Service Bus
            if (_httpContextAccesor.HttpContext == null)
            {
                _httpContextAccesor.HttpContext = new DefaultHttpContext();
            }

            var authorizationHeader = _httpContextAccesor.HttpContext
                .Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                request.Headers.Add("Authorization", new List<string>() { authorizationHeader });
            }

            var token = await GetToken();

            if (token == null) return await base.SendAsync(request, cancellationToken);

            token = token.Substring(token.IndexOf("Bearer", StringComparison.Ordinal) + "Bearer".Length);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<string> GetToken()
        {
            var token = await _cache.GetOrCreateAsync("staffingToken", async cacheEntry =>
            {
                cacheEntry.AbsoluteExpiration = DateTime.Now.AddHours(6);
                cacheEntry.Priority = CacheItemPriority.Normal;
                var appSecret = ConfigurationUtility.GetValue("Token:AppSecret");
                var tokenData = await _authenticationApiClient.GetToken("Staffing", appSecret);

                return tokenData;
            });

            return token;
        }
        
    }
}
