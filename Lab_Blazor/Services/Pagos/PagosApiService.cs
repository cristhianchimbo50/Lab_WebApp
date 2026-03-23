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

        public async Task<PagoDto?> GuardarPagoAsync(PagoDto pago)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/pagos") { Content = JsonContent.Create(pago) };
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                throw new Exception(await ExtraerMensajeErrorAsync(resp, "Error al registrar el pago."));
            return await resp.Content.ReadFromJsonAsync<PagoDto>();
        }

        public async Task<List<PagoDto>> ListarPagosAsync(int idOrden)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/pagos/orden/{idOrden}");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<PagoDto>>() ?? new();
        }

        public async Task<IEnumerable<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/pagos/cuentasporcobrar/listar") { Content = JsonContent.Create(filtro) };
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<IEnumerable<OrdenDto>>() ?? new List<OrdenDto>();
        }

        public async Task<ResultadoPaginadoDto<OrdenDto>> ListarCuentasPorCobrarPaginadoAsync(PagoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/pagos/cuentasporcobrar/listar-paginado") { Content = JsonContent.Create(filtro) };
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ResultadoPaginadoDto<OrdenDto>>() ?? new ResultadoPaginadoDto<OrdenDto>();
        }

        public async Task<PagoDto?> GuardarCobroCuentaPorCobrarAsync(PagoDto pago)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/pagos/cuentasporcobrar/registrar") { Content = JsonContent.Create(pago) };
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<PagoDto>();
        }

        private static async Task<string> ExtraerMensajeErrorAsync(HttpResponseMessage resp, string predeterminado)
        {
            var contenido = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(contenido)) return predeterminado;

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(contenido);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("Mensaje", out var m)) return m.GetString() ?? predeterminado;
                    if (doc.RootElement.TryGetProperty("message", out var mm)) return mm.GetString() ?? predeterminado;
                    if (doc.RootElement.TryGetProperty("title", out var t)) return t.GetString() ?? predeterminado;
                }
            }
            catch { }

            return contenido;
        }
    }
}
