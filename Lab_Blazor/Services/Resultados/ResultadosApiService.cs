using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Resultados
{
    public class ResultadosApiService : BaseApiService, IResultadosApiService
    {
        public ResultadosApiService(IHttpClientFactory factory, ProtectedSessionStorage session)
            : base(factory, session) { }

        public async Task<List<ResultadoListadoDto>> GetResultadosAsync(ResultadoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(filtro.NumeroResultado))
                queryParams.Add($"numeroResultado={Uri.EscapeDataString(filtro.NumeroResultado)}");
            if (!string.IsNullOrWhiteSpace(filtro.Cedula))
                queryParams.Add($"cedula={Uri.EscapeDataString(filtro.Cedula)}");
            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                queryParams.Add($"nombre={Uri.EscapeDataString(filtro.Nombre)}");
            if (filtro.FechaDesde.HasValue)
                queryParams.Add($"fechaDesde={filtro.FechaDesde.Value:yyyy-MM-dd}");
            if (filtro.FechaHasta.HasValue)
                queryParams.Add($"fechaHasta={filtro.FechaHasta.Value:yyyy-MM-dd}");
            if (filtro.Anulado.HasValue)
                queryParams.Add($"anulado={filtro.Anulado.Value.ToString().ToLower()}");

            var url = "api/resultados";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ResultadoListadoDto>>() ?? new();
        }

        public async Task<ResultadoDetalleDto?> GetDetalleResultadoAsync(int idResultado)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/resultados/{idResultado}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ResultadoDetalleDto>();
        }

        public async Task<byte[]> ObtenerResultadosPdfAsync(List<int> ids)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            if (ids == null || !ids.Any())
                return Array.Empty<byte>();

            var queryString = string.Join("&", ids.Select(id => $"ids={id}"));
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/resultados/pdf-multiple?{queryString}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsByteArrayAsync()
                : Array.Empty<byte>();
        }

        public async Task<bool> AnularResultadoAsync(int idResultado)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/resultados/anular/{idResultado}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
