using Lab_Contracts.Auth;
using Lab_Contracts.Resultados;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Resultados
{
    public class ResultadosApiService : IResultadosApiService
    {
        private readonly HttpClient _http;

        public ResultadosApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<ResultadoListadoDto>> GetResultadosAsync(ResultadoFiltroDto filtro)
        {
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

            return await _http.GetFromJsonAsync<List<ResultadoListadoDto>>(url) ?? new();
        }

        public async Task<ResultadoDetalleDto?> GetDetalleResultadoAsync(int idResultado)
        {
            return await _http.GetFromJsonAsync<ResultadoDetalleDto>($"api/resultados/{idResultado}");
        }

        public async Task<byte[]> ObtenerResultadosPdfAsync(List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Array.Empty<byte>();

            var queryString = string.Join("&", ids.Select(id => $"ids={id}"));
            var url = $"api/resultados/pdf-multiple?{queryString}";

            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return Array.Empty<byte>();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<bool> AnularResultadoAsync(int idResultado)
        {
            var response = await _http.PutAsync($"api/resultados/anular/{idResultado}", null);
            return response.IsSuccessStatusCode;
        }

    }
}
