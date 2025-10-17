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
            return await _http.GetFromJsonAsync<List<PagoDto>>($"api/pagos/orden/{idOrden}") ?? new();
        }
    }
}
