using Lab_Contracts.Convenios;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Convenios
{
    public class ConveniosApiService : BaseApiService, IConveniosApiService
    {
        public ConveniosApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<List<ConvenioDto>> ObtenerConveniosAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/convenios");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ConvenioDto>>() ?? new();
        }

        public async Task<ConvenioDetalleDto?> ObtenerDetalleAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/convenios/{id}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ConvenioDetalleDto>();
        }

        public async Task<List<OrdenDisponibleDto>> ObtenerOrdenesDisponiblesAsync(int idMedico)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/convenios/ordenes-disponibles/{idMedico}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<OrdenDisponibleDto>>() ?? new();
        }

        public async Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/convenios")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AnularConvenioAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/convenios/{id}/anular");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
