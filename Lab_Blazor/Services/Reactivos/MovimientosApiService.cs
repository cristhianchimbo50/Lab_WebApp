using Lab_Contracts.Reactivos;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Reactivos
{
    public class MovimientosApiService : BaseApiService, IMovimientosApiService
    {
        public MovimientosApiService(IHttpClientFactory factory, ProtectedSessionStorage session)
            : base(factory, session) { }

        public async Task<List<MovimientoReactivoDto>> FiltrarMovimientosAsync(MovimientoReactivoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/movimientos/filtrar")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<MovimientoReactivoDto>>() ?? new();
        }
    }
}
