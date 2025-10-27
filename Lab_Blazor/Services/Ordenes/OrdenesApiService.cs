using Lab_Contracts.Examenes;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Ordenes
{
    public class OrdenesApiService : BaseApiService, IOrdenesApiService
    {
        public OrdenesApiService(IHttpClientFactory factory, ProtectedSessionStorage session)
            : base(factory, session) { }

        public async Task<List<OrdenDto>> GetOrdenesAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/ordenes");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<OrdenDto>>() ?? new();
        }

        public async Task<OrdenDto?> GetOrdenPorIdAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/{id}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<OrdenDto>();
        }

        public async Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto orden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/ordenes")
            {
                Content = JsonContent.Create(orden)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<OrdenRespuestaDto>();
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/detalle/{idOrden}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<OrdenDetalleDto>();
        }

        public async Task<HttpResponseMessage> AnularOrdenAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/ordenes/anular/{idOrden}");
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> CrearOrdenHttpResponseAsync(OrdenCompletaDto orden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/ordenes")
            {
                Content = JsonContent.Create(orden)
            };
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<byte[]> ObtenerTicketOrdenPdfAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/{idOrden}/ticket-pdf");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsByteArrayAsync()
                : Array.Empty<byte>();
        }

        public async Task<HttpResponseMessage> GuardarResultadosAsync(ResultadoGuardarDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/ordenes/ingresar-resultado")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<List<ExamenDto>> ObtenerExamenesPorOrdenAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/{idOrden}/examenes");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> AnularOrdenCompletaAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/ordenes/anular-completo/{idOrden}");
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }
    }
}
