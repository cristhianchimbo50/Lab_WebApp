using Lab_Contracts.Reactivos;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Reactivos
{
    public class MovimientosApiService : BaseApiService, IMovimientosApiService
    {
        public MovimientosApiService(IHttpClientFactory Factory, ProtectedSessionStorage Sesion, IJSRuntime Js)
            : base(Factory, Sesion, Js) { }

        public async Task<List<MovimientoReactivoDto>> FiltrarMovimientosAsync(MovimientoReactivoFiltroDto Filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/movimientos/filtrar")
            {
                Content = JsonContent.Create(Filtro)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<List<MovimientoReactivoDto>>() ?? new();
        }

        public async Task<ResultadoPaginadoDto<MovimientoReactivoDto>> BuscarMovimientosAsync(MovimientoReactivoFiltroDto Filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/movimientos/buscar")
            {
                Content = JsonContent.Create(Filtro)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<MovimientoReactivoDto>>()
                ?? new ResultadoPaginadoDto<MovimientoReactivoDto> { Items = new List<MovimientoReactivoDto>(), PageNumber = Filtro.PageNumber, PageSize = Filtro.PageSize };
        }
    }
}
