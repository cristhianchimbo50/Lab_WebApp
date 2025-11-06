using Lab_Contracts.Reactivos;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Reactivos
{
    public class ReactivosApiService : BaseApiService, IReactivosApiService
    {
        public ReactivosApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<List<ReactivoDto>> ListarReactivosAsync()
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Get, "api/reactivos");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<ReactivoDto>>() ?? new();
        }

        public async Task<ReactivoDto?> ObtenerDetalleReactivoAsync(int idReactivo)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/reactivos/{idReactivo}");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ReactivoDto>();
        }

        public async Task<HttpResponseMessage> GuardarReactivoAsync(ReactivoDto reactivo)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/reactivos") { Content = JsonContent.Create(reactivo) };
            AddTokenHeader(req);
            return await _http.SendAsync(req);
        }

        public async Task<HttpResponseMessage> GuardarReactivoAsync(int idReactivo, ReactivoDto reactivo)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/reactivos/{idReactivo}") { Content = JsonContent.Create(reactivo) };
            AddTokenHeader(req);
            return await _http.SendAsync(req);
        }

        public async Task<HttpResponseMessage> AnularReactivoAsync(int idReactivo)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/reactivos/anular/{idReactivo}");
            AddTokenHeader(req);
            return await _http.SendAsync(req);
        }

        public async Task<HttpResponseMessage> RegistrarIngresosReactivosAsync(IEnumerable<MovimientoReactivoIngresoDto> ingresos)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/reactivos/ingresos") { Content = JsonContent.Create(ingresos) };
            AddTokenHeader(req);
            return await _http.SendAsync(req);
        }

        public async Task<HttpResponseMessage> RegistrarEgresosReactivosAsync(IEnumerable<MovimientoReactivoEgresoDto> egresos)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/reactivos/egresos") { Content = JsonContent.Create(egresos) };
            AddTokenHeader(req);
            return await _http.SendAsync(req);
        }

        public async Task<ResultadoPaginadoDto<ReactivoDto>> ListarReactivosPaginadosAsync(ReactivoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/reactivos/buscar") { Content = JsonContent.Create(filtro) };
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ResultadoPaginadoDto<ReactivoDto>>()
                ?? new ResultadoPaginadoDto<ReactivoDto> { Items = new List<ReactivoDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
        }
    }
}
