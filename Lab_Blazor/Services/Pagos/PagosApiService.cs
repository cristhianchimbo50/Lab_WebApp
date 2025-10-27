using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Pagos
{
    public class PagosApiService : BaseApiService, IPagosApiService
    {
        public PagosApiService(IHttpClientFactory factory, ProtectedSessionStorage session)
            : base(factory, session) { }

        public async Task<PagoDto?> RegistrarPagoAsync(PagoDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/pagos")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PagoDto>();
        }

        public async Task<List<PagoDto>> ListarPagosPorOrdenAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/pagos/orden/{idOrden}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<PagoDto>>() ?? new();
        }

        public async Task<IEnumerable<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/pagos/cuentasporcobrar/listar")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<OrdenDto>>() ?? [];
        }

        public async Task<PagoDto?> RegistrarCobroCuentaPorCobrarAsync(PagoDto pago)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/pagos/cuentasporcobrar/registrar")
            {
                Content = JsonContent.Create(pago)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PagoDto>();
        }
    }
}
