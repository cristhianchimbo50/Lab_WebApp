using Lab_Contracts.Common;
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
        public OrdenesApiService(IHttpClientFactory Factory, ProtectedSessionStorage Session, IJSRuntime Js)
            : base(Factory, Session, Js) { }

        public async Task<List<OrdenDto>> ListarOrdenesAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Get, "api/ordenes");
            AddTokenHeader(solicitud);

            var respuesta = await _http.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content.ReadFromJsonAsync<List<OrdenDto>>() ?? new();
        }

        public async Task<ResultadoPaginadoDto<OrdenDto>> ListarOrdenesPaginadosAsync(OrdenFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Post, "api/ordenes/buscar")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(solicitud);

            var respuesta = await _http.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<OrdenDto>>()
                   ?? new ResultadoPaginadoDto<OrdenDto> { Items = new List<OrdenDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
        }

        public async Task<OrdenDto?> ObtenerOrdenAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/{idOrden}");
            AddTokenHeader(solicitud);

            var respuesta = await _http.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content.ReadFromJsonAsync<OrdenDto?>();
        }

        public async Task<OrdenRespuestaDto?> GuardarOrdenAsync(OrdenCompletaDto orden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Post, "api/ordenes")
            {
                Content = JsonContent.Create(orden)
            };
            AddTokenHeader(solicitud);

            var respuesta = await _http.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content.ReadFromJsonAsync<OrdenRespuestaDto>();
        }

        public async Task<HttpResponseMessage> GuardarOrdenHttpAsync(OrdenCompletaDto orden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Post, "api/ordenes")
            {
                Content = JsonContent.Create(orden)
            };
            AddTokenHeader(solicitud);

            return await _http.SendAsync(solicitud);
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/detalle/{idOrden}");
            AddTokenHeader(solicitud);

            var respuesta = await _http.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content.ReadFromJsonAsync<OrdenDetalleDto>();
        }

        public async Task<HttpResponseMessage> AnularOrdenAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/ordenes/anular/{idOrden}");
            AddTokenHeader(solicitud);

            return await _http.SendAsync(solicitud);
        }

        public async Task<byte[]> GenerarOrdenTicketPdfAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/{idOrden}/ticket-pdf");
            AddTokenHeader(solicitud);

            var respuesta = await _http.SendAsync(solicitud);
            return respuesta.IsSuccessStatusCode
                ? await respuesta.Content.ReadAsByteArrayAsync()
                : Array.Empty<byte>();
        }

        public async Task<HttpResponseMessage> GuardarResultadosAsync(ResultadoGuardarDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Post, "api/ordenes/ingresar-resultado")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(solicitud);

            return await _http.SendAsync(solicitud);
        }

        public async Task<List<ExamenDto>> ListarExamenesPorOrdenAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/{idOrden}/examenes");
            AddTokenHeader(solicitud);

            var respuesta = await _http.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> AnularOrdenCompletaAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/ordenes/anular-completo/{idOrden}");
            AddTokenHeader(solicitud);

            return await _http.SendAsync(solicitud);
        }

        public async Task<List<OrdenDto>> ListarOrdenesPorPacienteAsync(int idPaciente)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/paciente/{idPaciente}");
            AddTokenHeader(solicitud);

            var respuesta = await _http.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content.ReadFromJsonAsync<List<OrdenDto>>() ?? new();
        }

        public async Task<(OrdenDetalleDto? Detalle, bool TieneSaldoPendiente)> ObtenerDetalleOrdenPorPacienteAsync(int idPaciente, int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/paciente/{idPaciente}/detalle/{idOrden}");
            AddTokenHeader(solicitud);

            var respuesta = await _http.SendAsync(solicitud);
            if (!respuesta.IsSuccessStatusCode)
                return (null, false);

            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            using var documento = await JsonDocument.ParseAsync(await respuesta.Content.ReadAsStreamAsync());
            var raiz = documento.RootElement;

            var detalle = raiz.TryGetProperty("detalleOrden", out var elementoDetalle)
                ? JsonSerializer.Deserialize<OrdenDetalleDto>(elementoDetalle.GetRawText(), opciones)
                : null;
            var tieneSaldo = raiz.TryGetProperty("tieneSaldoPendiente", out var elementoSaldo) && elementoSaldo.GetBoolean();
            return (detalle, tieneSaldo);
        }

        public async Task<HttpResponseMessage> VerificarNotificacionResultadosOrdenAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Post, $"api/ordenes/{idOrden}/verificar-notificacion");
            AddTokenHeader(solicitud);
            return await _http.SendAsync(solicitud);
        }
    }
}
