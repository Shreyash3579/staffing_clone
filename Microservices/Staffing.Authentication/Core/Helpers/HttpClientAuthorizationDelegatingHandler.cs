using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Staffing.Authentication.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Staffing.Authentication.Core.Helpers
{
    public class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccesor;
        private readonly IServiceProvider _serviceProvider;

        public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccesor,
            IServiceProvider serviceProvider)
        {
            _httpContextAccesor = httpContextAccesor;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var authorizationHeader = _httpContextAccesor.HttpContext
                .Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorizationHeader))
                request.Headers.Add("Authorization", new List<string> { authorizationHeader });

            var token = await GetToken();

            if (token == null) return await base.SendAsync(request, cancellationToken);

            token = token.Substring(token.IndexOf("Bearer", StringComparison.Ordinal) + "Bearer".Length);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<string> GetToken()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var securityUserService = scope.ServiceProvider.GetRequiredService<ISecurityUserService>();
                var token = Task.Run(() => securityUserService.AuthenticateApp("Staffing",
                    "ImOivLnEiP/jaTt+LlMj5ef0KgogNLiSPqqHPOb5doM="));
                return token.Result;
            }
        }
    }
}