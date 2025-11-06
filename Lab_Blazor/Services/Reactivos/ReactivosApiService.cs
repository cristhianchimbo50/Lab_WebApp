using Lab_Contracts.Reactivos;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Reactivos
{
    public class ReactivosApiService : BaseApiService, IReactivosApiService
    {
        public ReactivosApiService(IHttpClientFactory Factory, ProtectedSessionStorage Session, IJSRuntime Js)
            : base(Factory, Session, Js) { }

        public async Task<List<ReactivoDto>> ObtenerReactivosAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, "api/reactivos");
            AddTokenHeader(Solicitud);
            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();
            return await Respuesta.Content.ReadFromJsonAsync<List<ReactivoDto>>() ?? new();
        }

        public async Task<ReactivoDto?> ObtenerReactivoPorIdAsync(int IdReactivo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/reactivos/{IdReactivo}");
            AddTokenHeader(Solicitud);
            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();
            return await Respuesta.Content.ReadFromJsonAsync<ReactivoDto>();
        }

        public async Task<HttpResponseMessage> CrearReactivoAsync(ReactivoDto Reactivo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/reactivos")
            {
                Content = JsonContent.Create(Reactivo)
            };
            AddTokenHeader(Solicitud);
            return await _http.SendAsync(Solicitud);
        }

        public async Task<HttpResponseMessage> EditarReactivoAsync(int IdReactivo, ReactivoDto Reactivo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/reactivos/{IdReactivo}")
            {
                Content = JsonContent.Create(Reactivo)
            };
            AddTokenHeader(Solicitud);
            return await _http.SendAsync(Solicitud);
        }

        public async Task<HttpResponseMessage> AnularReactivoAsync(int IdReactivo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/reactivos/anular/{IdReactivo}");
            AddTokenHeader(Solicitud);
            return await _http.SendAsync(Solicitud);
        }

        public async Task<HttpResponseMessage> RegistrarIngresosAsync(IEnumerable<MovimientoReactivoIngresoDto> Ingresos)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/reactivos/ingresos")
            {
                Content = JsonContent.Create(Ingresos)
            };
            AddTokenHeader(Solicitud);
            return await _http.SendAsync(Solicitud);
        }

        public async Task<HttpResponseMessage> RegistrarEgresosAsync(IEnumerable<MovimientoReactivoEgresoDto> Egresos)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/reactivos/egresos")
            {
                Content = JsonContent.Create(Egresos)
            };
            AddTokenHeader(Solicitud);
            return await _http.SendAsync(Solicitud);
        }

        public async Task<ResultadoPaginadoDto<ReactivoDto>> BuscarReactivosAsync(ReactivoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/reactivos/buscar")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(Solicitud);
            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();
            return await Respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<ReactivoDto>>()
                ?? new ResultadoPaginadoDto<ReactivoDto> { Items = new List<ReactivoDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
        }
    }
}
