using Lab_Contracts.Ajustes.Perfil;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Perfil
{
    public class PerfilApiService : BaseApiService, IPerfilApiService
    {
        public PerfilApiService(IHttpClientFactory FactoryHttp, ProtectedSessionStorage AlmacenamientoSesion, IJSRuntime JsRuntime)
            : base(FactoryHttp, AlmacenamientoSesion, JsRuntime) { }

        public async Task<PerfilResponseDto?> ObtenerDetallePerfilAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var solicitud = new HttpRequestMessage(HttpMethod.Get, "api/ajustes/perfil");
            AddTokenHeader(solicitud);
            var respuesta = await _http.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<PerfilResponseDto>();
        }
    }
}
