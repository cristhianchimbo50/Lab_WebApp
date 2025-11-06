using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_Contracts.Common;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Pagos
{
    public class PagosApiService : BaseApiService, IPagosApiService
    {
        public PagosApiService(IHttpClientFactory Factory, ProtectedSessionStorage Session, IJSRuntime Js)
            : base(Factory, Session, Js) { }

        public async Task<PagoDto?> RegistrarPagoAsync(PagoDto Dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/pagos")
            {
                Content = JsonContent.Create(Dto)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<PagoDto>();
        }

        public async Task<List<PagoDto>> ListarPagosPorOrdenAsync(int IdOrden)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/pagos/orden/{IdOrden}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<List<PagoDto>>() ?? new();
        }

        public async Task<IEnumerable<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto Filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/pagos/cuentasporcobrar/listar")
            {
                Content = JsonContent.Create(Filtro)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<IEnumerable<OrdenDto>>() ?? new List<OrdenDto>();
        }

        public async Task<ResultadoPaginadoDto<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro, int pagina, int tamano)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            filtro.PageNumber = pagina;
            filtro.PageSize = tamano;

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/pagos/cuentasporcobrar/listar-paginado")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<OrdenDto>>() ?? new ResultadoPaginadoDto<OrdenDto>();
        }

        public async Task<PagoDto?> RegistrarCobroCuentaPorCobrarAsync(PagoDto Pago)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/pagos/cuentasporcobrar/registrar")
            {
                Content = JsonContent.Create(Pago)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<PagoDto>();
        }
    }
}
