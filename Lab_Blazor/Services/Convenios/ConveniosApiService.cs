using Lab_Contracts.Convenios;
using Lab_Contracts.Common;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Convenios
{
    public class ConveniosApiService : BaseApiService, IConveniosApiService
    {
        public ConveniosApiService(IHttpClientFactory Factory, ProtectedSessionStorage Session, IJSRuntime Js)
            : base(Factory, Session, Js) { }

        public async Task<List<ConvenioDto>> ObtenerConveniosAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, "api/convenios");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();

            return await Response.Content.ReadFromJsonAsync<List<ConvenioDto>>() ?? new();
        }

        public async Task<ResultadoPaginadoDto<ConvenioDto>> BuscarConveniosAsync(ConvenioFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/convenios/buscar")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<ConvenioDto>>()
                   ?? new ResultadoPaginadoDto<ConvenioDto> { Items = new List<ConvenioDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
        }

        public async Task<ResultadoPaginadoDto<ConvenioDto>> BuscarConveniosAsync(string? criterio, string? valor, DateOnly? desde, DateOnly? hasta, int page, int pageSize)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(criterio)) query["criterio"] = criterio;
            if (!string.IsNullOrWhiteSpace(valor)) query["valor"] = valor;
            if (desde.HasValue) query["desde"] = desde.Value.ToString("yyyy-MM-dd");
            if (hasta.HasValue) query["hasta"] = hasta.Value.ToString("yyyy-MM-dd");
            query["page"] = page.ToString();
            query["pageSize"] = pageSize.ToString();
            var url = $"api/convenios/buscar?{query}";

            var Request = new HttpRequestMessage(HttpMethod.Get, url);
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();

            return await Response.Content.ReadFromJsonAsync<ResultadoPaginadoDto<ConvenioDto>>()
                ?? new ResultadoPaginadoDto<ConvenioDto> { Items = new List<ConvenioDto>(), PageNumber = page, PageSize = pageSize };
        }

        public async Task<ConvenioDetalleDto?> ObtenerDetalleAsync(int Id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/convenios/{Id}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();

            return await Response.Content.ReadFromJsonAsync<ConvenioDetalleDto>();
        }

        public async Task<List<OrdenDisponibleDto>> ObtenerOrdenesDisponiblesAsync(int IdMedico)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/convenios/ordenes-disponibles/{IdMedico}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();

            return await Response.Content.ReadFromJsonAsync<List<OrdenDisponibleDto>>() ?? new();
        }

        public async Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto Dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Post, "api/convenios")
            {
                Content = JsonContent.Create(Dto)
            };
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            return Response.IsSuccessStatusCode;
        }

        public async Task<bool> AnularConvenioAsync(int Id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Put, $"api/convenios/{Id}/anular");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            return Response.IsSuccessStatusCode;
        }
    }
}
