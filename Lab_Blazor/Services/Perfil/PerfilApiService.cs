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

        public async Task<PerfilResponseDto?> ObtenerPerfilAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, "api/ajustes/perfil");
            AddTokenHeader(Solicitud);

            var RespuestaHttp = await _http.SendAsync(Solicitud);
            RespuestaHttp.EnsureSuccessStatusCode();

            return await RespuestaHttp.Content.ReadFromJsonAsync<PerfilResponseDto>();
        }
    }
}
