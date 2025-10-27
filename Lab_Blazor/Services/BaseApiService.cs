using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;

namespace Lab_Blazor.Services
{
    public abstract class BaseApiService
    {
        private readonly ProtectedSessionStorage _session;
        protected readonly HttpClient _http;
        private string? _token;

        protected BaseApiService(IHttpClientFactory factory, ProtectedSessionStorage session, string clientName = "Api")
        {
            _http = factory.CreateClient(clientName);
            _session = session;
        }

        protected async Task<bool> SetAuthHeaderAsync()
        {
            if (!string.IsNullOrEmpty(_token))
                return true;

            var tokenResult = await _session.GetAsync<string>("jwt");
            if (!tokenResult.Success || string.IsNullOrWhiteSpace(tokenResult.Value))
                return false;

            _token = tokenResult.Value;
            return true;
        }

        protected void AddTokenHeader(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(_token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }
}
