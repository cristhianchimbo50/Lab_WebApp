using Lab_Contracts.Reactivos;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Reactivos
{
    public class ReactivosApiService : BaseApiService, IReactivosApiService
    {
        public ReactivosApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<List<ReactivoDto>> GetReactivosAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/reactivos");
            AddTokenHeader(request);
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ReactivoDto>>() ?? new();
        }

        public async Task<ReactivoDto?> GetReactivoPorIdAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/reactivos/{id}");
            AddTokenHeader(request);
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ReactivoDto>();
        }

        public async Task<HttpResponseMessage> CrearReactivoAsync(ReactivoDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/reactivos")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);
            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> EditarReactivoAsync(int id, ReactivoDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/reactivos/{id}")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);
            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> AnularReactivoAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/reactivos/anular/{id}");
            AddTokenHeader(request);
            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> RegistrarIngresosAsync(IEnumerable<MovimientoReactivoIngresoDto> ingresos)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/reactivos/ingresos")
            {
                Content = JsonContent.Create(ingresos)
            };
            AddTokenHeader(request);
            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> RegistrarEgresosAsync(IEnumerable<MovimientoReactivoEgresoDto> egresos)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/reactivos/egresos")
            {
                Content = JsonContent.Create(egresos)
            };
            AddTokenHeader(request);
            return await _http.SendAsync(request);
        }
    }
}
