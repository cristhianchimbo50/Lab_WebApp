using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace Lab_Blazor.Services
{
    public abstract class BaseApiService
    {
        private readonly ProtectedSessionStorage _session;
        private readonly IJSRuntime _js;
        protected readonly HttpClient _http;
        private string? _token;

        protected BaseApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js, string clientName = "Api")
        {
            _http = factory.CreateClient(clientName);
            _session = session;
            _js = js;
        }

        protected async Task<bool> SetAuthHeaderAsync()
        {
            if (!string.IsNullOrEmpty(_token))
                return true;

            var tokenResult = await _session.GetAsync<string>("jwt");
            if (tokenResult.Success && !string.IsNullOrWhiteSpace(tokenResult.Value))
            {
                _token = tokenResult.Value;
                return true;
            }

            var token = await _js.InvokeAsync<string>("localStorage.getItem", "jwt");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _token = token;
                return true;
            }

            return false;
        }

        protected void AddTokenHeader(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(_token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }
}
