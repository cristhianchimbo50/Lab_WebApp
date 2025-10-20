using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Pagos
{
    public class PagosApiService : IPagosApiService
    {
        private readonly HttpClient _http;

        public PagosApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<PagoDto?> RegistrarPagoAsync(PagoDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/pagos", dto);
            return await resp.Content.ReadFromJsonAsync<PagoDto>();
        }

        public async Task<List<PagoDto>> ListarPagosPorOrdenAsync(int idOrden)
        {
            var response = await _http.GetAsync($"api/pagos/orden/{idOrden}");
            if (!response.IsSuccessStatusCode)
                return new List<PagoDto>();

            return await response.Content.ReadFromJsonAsync<List<PagoDto>>() ?? new List<PagoDto>();
        }


        public async Task<IEnumerable<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro)
        {
            var response = await _http.PostAsJsonAsync("api/pagos/cuentasporcobrar/listar", filtro);

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<OrdenDto>();

            return await response.Content.ReadFromJsonAsync<IEnumerable<OrdenDto>>() ?? [];
        }

        public async Task<PagoDto?> RegistrarCobroCuentaPorCobrarAsync(PagoDto pago)
        {
            var response = await _http.PostAsJsonAsync("api/pagos/cuentasporcobrar/registrar", pago);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<PagoDto>();
        }
    }
}
