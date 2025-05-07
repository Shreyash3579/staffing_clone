using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OcelotAPIGateway.Helpers
{
    public class GetToPostHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get)
            {
                request.Method = new HttpMethod("POST");
                var queryParams = request.RequestUri.OriginalString.Substring(request.RequestUri.OriginalString.IndexOf('?') + 1);
                if (!string.IsNullOrEmpty(queryParams))
                {
                    var payload = new Dictionary<string, string>();
                    var keyValuePair = queryParams.Split('&');
                    foreach (var kvp in keyValuePair)
                    {
                        var key = kvp.Substring(0, kvp.IndexOf('='));
                        var value = kvp.Substring(kvp.IndexOf('=') + 1);
                        payload.Add(key, value);
                    }

                    var requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.RequestUri = new Uri(request.RequestUri.OriginalString.Substring(0, request.RequestUri.OriginalString.IndexOf('?')));
                }

            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
