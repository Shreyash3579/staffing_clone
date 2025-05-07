using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Helpers
{
    public class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccesor;
        private readonly IAuthenticationApiClient _authenticationApiClient;

        public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccesor, IAuthenticationApiClient authenticationApiClient)
        {
            _httpContextAccesor = httpContextAccesor;
            _authenticationApiClient = authenticationApiClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Create Default HttpContext for Hangfire
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
            var appSecret = ConfigurationUtility.GetValue("Token:AppSecret");
            var token = await _authenticationApiClient.GetToken("Staffing", appSecret);
            return token;
        }
    }
}
