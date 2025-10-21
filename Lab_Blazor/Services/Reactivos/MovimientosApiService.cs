using Lab_Contracts.Reactivos;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Reactivos
{
    public class MovimientosApiService : IMovimientosApiService
    {
        private readonly HttpClient _http;

        public MovimientosApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<MovimientoReactivoDto>> FiltrarMovimientosAsync(MovimientoReactivoFiltroDto filtro)
        {
            var response = await _http.PostAsJsonAsync("api/movimientos/filtrar", filtro);

            if (!response.IsSuccessStatusCode)
                return new();

            var data = await response.Content.ReadFromJsonAsync<List<MovimientoReactivoDto>>();
            return data ?? new();
        }
    }
}
