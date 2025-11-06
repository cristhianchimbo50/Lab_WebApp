using Lab_Contracts.Convenios;
using Lab_Contracts.Common;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Convenios
{
    public class ConveniosApiService : BaseApiService, IConveniosApiService
    {
        public ConveniosApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<List<ConvenioDto>> ListarConveniosAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/convenios");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ConvenioDto>>() ?? new();
        }

        public async Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(ConvenioFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Post, "api/convenios/buscar")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(solicitud);

            var respuesta = await _http.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<ConvenioDto>>()
                   ?? new ResultadoPaginadoDto<ConvenioDto> { Items = new List<ConvenioDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
        }

        public async Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(string? criterio, string? valor, DateOnly? desde, DateOnly? hasta, int page, int pageSize)
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

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ResultadoPaginadoDto<ConvenioDto>>()
                ?? new ResultadoPaginadoDto<ConvenioDto> { Items = new List<ConvenioDto>(), PageNumber = page, PageSize = pageSize };
        }

        public async Task<ConvenioDetalleDto?> ObtenerDetalleConvenioAsync(int idConvenio)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/convenios/{idConvenio}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ConvenioDetalleDto>();
        }

        public async Task<List<OrdenDisponibleDto>> ListarOrdenesDisponiblesPorMedicoAsync(int idMedico)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/convenios/ordenes-disponibles/{idMedico}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<OrdenDisponibleDto>>() ?? new();
        }

        public async Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto registroConvenio)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/convenios")
            {
                Content = JsonContent.Create(registroConvenio)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AnularConvenioAsync(int idConvenio)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/convenios/{idConvenio}/anular");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
