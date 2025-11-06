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

        public async Task<List<OrdenDto>> ObtenerOrdenesAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, "api/ordenes");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<List<OrdenDto>>() ?? new();
        }

        public async Task<ResultadoPaginadoDto<OrdenDto>> BuscarOrdenesAsync(OrdenFiltroDto Filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/ordenes/buscar")
            {
                Content = JsonContent.Create(Filtro)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<OrdenDto>>()
                   ?? new ResultadoPaginadoDto<OrdenDto> { Items = new List<OrdenDto>(), PageNumber = Filtro.PageNumber, PageSize = Filtro.PageSize };
        }

        public async Task<OrdenDto?> ObtenerOrdenPorIdAsync(int Id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/{Id}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<OrdenDto?>();
        }

        public async Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto Orden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/ordenes")
            {
                Content = JsonContent.Create(Orden)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<OrdenRespuestaDto>();
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenAsync(int IdOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/detalle/{IdOrden}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<OrdenDetalleDto>();
        }

        public async Task<HttpResponseMessage> AnularOrdenAsync(int IdOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/ordenes/anular/{IdOrden}");
            AddTokenHeader(Solicitud);

            return await _http.SendAsync(Solicitud);
        }

        public async Task<HttpResponseMessage> CrearOrdenHttpResponseAsync(OrdenCompletaDto Orden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/ordenes")
            {
                Content = JsonContent.Create(Orden)
            };
            AddTokenHeader(Solicitud);

            return await _http.SendAsync(Solicitud);
        }

        public async Task<byte[]> ObtenerTicketOrdenPdfAsync(int IdOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/{IdOrden}/ticket-pdf");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            return Respuesta.IsSuccessStatusCode
                ? await Respuesta.Content.ReadAsByteArrayAsync()
                : Array.Empty<byte>();
        }

        public async Task<HttpResponseMessage> GuardarResultadosAsync(ResultadoGuardarDto Dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/ordenes/ingresar-resultado")
            {
                Content = JsonContent.Create(Dto)
            };
            AddTokenHeader(Solicitud);

            return await _http.SendAsync(Solicitud);
        }

        public async Task<List<ExamenDto>> ObtenerExamenesPorOrdenAsync(int IdOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/{IdOrden}/examenes");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> AnularOrdenCompletaAsync(int IdOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/ordenes/anular-completo/{IdOrden}");
            AddTokenHeader(Solicitud);

            return await _http.SendAsync(Solicitud);
        }

        public async Task<List<OrdenDto>> ObtenerOrdenesPacienteAsync(int IdPaciente)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/paciente/{IdPaciente}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<List<OrdenDto>>() ?? new();
        }

        public async Task<(OrdenDetalleDto? Detalle, bool TieneSaldoPendiente)> ObtenerDetalleOrdenPacienteAsync(int IdPaciente, int IdOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/ordenes/paciente/{IdPaciente}/detalle/{IdOrden}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            if (!Respuesta.IsSuccessStatusCode)
                return (null, false);

            var Opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            using var Documento = await JsonDocument.ParseAsync(await Respuesta.Content.ReadAsStreamAsync());
            var Raiz = Documento.RootElement;

            var Detalle = Raiz.TryGetProperty("detalleOrden", out var ElementoDetalle)
                ? JsonSerializer.Deserialize<OrdenDetalleDto>(ElementoDetalle.GetRawText(), Opciones)
                : null;
            var TieneSaldo = Raiz.TryGetProperty("tieneSaldoPendiente", out var ElementoSaldo) && ElementoSaldo.GetBoolean();
            return (Detalle, TieneSaldo);
        }

        public async Task<HttpResponseMessage> VerificarNotificacionResultadosAsync(int IdOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, $"api/ordenes/{IdOrden}/verificar-notificacion");
            AddTokenHeader(Solicitud);
            return await _http.SendAsync(Solicitud);
        }

    }
}
