using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Lab_Contracts.Common;
using System.Linq;

namespace Lab_Blazor.Services.Resultados
{
    public class ResultadosApiService : BaseApiService, IResultadosApiService
    {
        public ResultadosApiService(IHttpClientFactory factory, ProtectedSessionStorage sesion, IJSRuntime js)
            : base(factory, sesion, js) { }

        public async Task<List<ResultadoListadoDto>> ListarResultadosAsync(ResultadoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var parametros = new List<string>();
            if (!string.IsNullOrWhiteSpace(filtro.NumeroResultado)) parametros.Add($"numeroResultado={Uri.EscapeDataString(filtro.NumeroResultado)}");
            if (!string.IsNullOrWhiteSpace(filtro.NumeroOrden)) parametros.Add($"numeroOrden={Uri.EscapeDataString(filtro.NumeroOrden)}");
            if (!string.IsNullOrWhiteSpace(filtro.Cedula)) parametros.Add($"cedula={Uri.EscapeDataString(filtro.Cedula)}");
            if (!string.IsNullOrWhiteSpace(filtro.Nombre)) parametros.Add($"nombre={Uri.EscapeDataString(filtro.Nombre)}");
            if (filtro.FechaDesde.HasValue) parametros.Add($"fechaDesde={filtro.FechaDesde.Value:yyyy-MM-dd}");
            if (filtro.FechaHasta.HasValue) parametros.Add($"fechaHasta={filtro.FechaHasta.Value:yyyy-MM-dd}");
            if (filtro.Anulado.HasValue) parametros.Add($"anulado={filtro.Anulado.Value.ToString().ToLower()}");
            var url = "api/resultados" + (parametros.Any() ? "?" + string.Join("&", parametros) : string.Empty);
            var req = new HttpRequestMessage(HttpMethod.Get, url); AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<ResultadoListadoDto>>() ?? new();
        }

        public async Task<ResultadoPaginadoDto<ResultadoListadoDto>> ListarResultadosPaginadosAsync(ResultadoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/resultados/buscar") { Content = JsonContent.Create(filtro) }; AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ResultadoPaginadoDto<ResultadoListadoDto>>()
                ?? new ResultadoPaginadoDto<ResultadoListadoDto> { Items = new List<ResultadoListadoDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
        }

        public async Task<ResultadoDetalleDto?> ObtenerDetalleResultadoAsync(int idResultado)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/resultados/{idResultado}"); AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ResultadoDetalleDto>();
        }

        public async Task<byte[]> GenerarResultadosPdfAsync(List<int> idsResultados)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            if (idsResultados == null || !idsResultados.Any()) return Array.Empty<byte>();
            var qs = string.Join("&", idsResultados.Select(id => $"ids={id}"));
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/resultados/pdf-multiple?{qs}"); AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : Array.Empty<byte>();
        }

        public async Task<bool> AnularResultadoAsync(int idResultado)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/resultados/anular/{idResultado}"); AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }

        public async Task<List<ResultadoListadoDto>> ListarResultadosPacienteAsync()
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Get, "api/resultados/mis-resultados"); AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<ResultadoListadoDto>>() ?? new();
        }

        public async Task<ResultadoDetalleDto?> ObtenerDetalleResultadoPacienteAsync(int idResultado)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/resultados/mi-detalle/{idResultado}"); AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ResultadoDetalleDto>();
        }

        public async Task<bool> RevisarResultadoAsync(int idResultado, ResultadoRevisionDto revision)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/resultados/{idResultado}/revision")
            {
                Content = JsonContent.Create(revision)
            };
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarResultadoAsync(ResultadoActualizarDto dto)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesin expirada.");
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/resultados/{dto.IdResultado}")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }
    }
}
