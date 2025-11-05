using Lab_Contracts.Examenes;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace Lab_Blazor.Services.Ordenes
{
    public class OrdenesApiService : BaseApiService, IOrdenesApiService
    {
        public OrdenesApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

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

        public async Task<PagedResultDto<OrdenDto>> BuscarOrdenesAsync(OrdenFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/ordenes/buscar")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PagedResultDto<OrdenDto>>()
                   ?? new PagedResultDto<OrdenDto> { Items = new List<OrdenDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
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

        public async Task<List<OrdenDto>> GetOrdenesPacienteAsync(int idPaciente)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/paciente/{idPaciente}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<OrdenDto>>() ?? new();
        }

        public async Task<(OrdenDetalleDto? Detalle, bool TieneSaldoPendiente)> ObtenerDetalleOrdenPacienteAsync(int idPaciente, int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/paciente/{idPaciente}/detalle/{idOrden}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return (null, false);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;

            var detalle = root.TryGetProperty("detalleOrden", out var detEl)
                ? JsonSerializer.Deserialize<OrdenDetalleDto>(detEl.GetRawText(), options)
                : null;
            var tieneSaldo = root.TryGetProperty("tieneSaldoPendiente", out var saldoEl) && saldoEl.GetBoolean();
            return (detalle, tieneSaldo);
        }

        public async Task<HttpResponseMessage> VerificarNotificacionResultadosAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/ordenes/{idOrden}/verificar-notificacion");
            AddTokenHeader(request);
            return await _http.SendAsync(request);
        }


    }
}
