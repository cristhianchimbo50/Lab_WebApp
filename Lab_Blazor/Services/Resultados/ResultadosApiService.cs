using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Resultados
{
    public class ResultadosApiService : BaseApiService, IResultadosApiService
    {
        public ResultadosApiService(IHttpClientFactory Factory, ProtectedSessionStorage Sesion, IJSRuntime Js)
            : base(Factory, Sesion, Js) { }

        public async Task<List<ResultadoListadoDto>> ObtenerResultadosAsync(ResultadoFiltroDto Filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var ParametrosConsulta = new List<string>();

            if (!string.IsNullOrWhiteSpace(Filtro.NumeroResultado))
                ParametrosConsulta.Add($"numeroResultado={Uri.EscapeDataString(Filtro.NumeroResultado)}");
            if (!string.IsNullOrWhiteSpace(Filtro.NumeroOrden))
                ParametrosConsulta.Add($"numeroOrden={Uri.EscapeDataString(Filtro.NumeroOrden)}");
            if (!string.IsNullOrWhiteSpace(Filtro.Cedula))
                ParametrosConsulta.Add($"cedula={Uri.EscapeDataString(Filtro.Cedula)}");
            if (!string.IsNullOrWhiteSpace(Filtro.Nombre))
                ParametrosConsulta.Add($"nombre={Uri.EscapeDataString(Filtro.Nombre)}");
            if (Filtro.FechaDesde.HasValue)
                ParametrosConsulta.Add($"fechaDesde={Filtro.FechaDesde.Value:yyyy-MM-dd}");
            if (Filtro.FechaHasta.HasValue)
                ParametrosConsulta.Add($"fechaHasta={Filtro.FechaHasta.Value:yyyy-MM-dd}");
            if (Filtro.Anulado.HasValue)
                ParametrosConsulta.Add($"anulado={Filtro.Anulado.Value.ToString().ToLower()}");

            var Url = "api/resultados";
            if (ParametrosConsulta.Any())
                Url += "?" + string.Join("&", ParametrosConsulta);

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, Url);
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<List<ResultadoListadoDto>>() ?? new();
        }

        public async Task<ResultadoPaginadoDto<ResultadoListadoDto>> BuscarResultadosPaginadosAsync(ResultadoFiltroDto Filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/resultados/buscar")
            {
                Content = JsonContent.Create(Filtro)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<ResultadoListadoDto>>()
                ?? new ResultadoPaginadoDto<ResultadoListadoDto> { Items = new List<ResultadoListadoDto>(), PageNumber = Filtro.PageNumber, PageSize = Filtro.PageSize };
        }

        public async Task<ResultadoDetalleDto?> ObtenerDetalleResultadoAsync(int IdResultado)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/resultados/{IdResultado}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<ResultadoDetalleDto>();
        }

        public async Task<byte[]> ObtenerResultadosPdfAsync(List<int> IdsResultados)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            if (IdsResultados == null || !IdsResultados.Any())
                return Array.Empty<byte>();

            var CadenaConsulta = string.Join("&", IdsResultados.Select(Id => $"ids={Id}"));
            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/resultados/pdf-multiple?{CadenaConsulta}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            return Respuesta.IsSuccessStatusCode
                ? await Respuesta.Content.ReadAsByteArrayAsync()
                : Array.Empty<byte>();
        }

        public async Task<bool> AnularResultadoAsync(int IdResultado)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/resultados/anular/{IdResultado}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            return Respuesta.IsSuccessStatusCode;
        }

        public async Task<List<ResultadoListadoDto>> ObtenerResultadosPacienteAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, "api/resultados/mis-resultados");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<List<ResultadoListadoDto>>() ?? new();
        }

        public async Task<ResultadoDetalleDto?> ObtenerDetalleResultadoPacienteAsync(int IdResultado)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/resultados/mi-detalle/{IdResultado}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<ResultadoDetalleDto>();
        }
    }
}
