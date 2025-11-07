using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Auth
{
    public class RecuperacionApiService : BaseApiService, IRecuperacionApiService
    {
        public RecuperacionApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<RespuestaMensajeDto> SolicitarRecuperacionContraseniaAsync(OlvideContraseniaDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/recuperacion/solicitar", dto);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<RespuestaMensajeDto>()
                           ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Respuesta vacía del servidor." };

                var errorResponse = await response.Content.ReadFromJsonAsync<RespuestaMensajeDto>();
                return errorResponse ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Error desconocido del servidor." };
            }
            catch (Exception)
            {
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Error al conectarse con el servidor." };
            }
        }

        public async Task<RespuestaMensajeDto> RestablecerContraseniaAsync(RestablecerContraseniaDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/recuperacion/restablecer", dto);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<RespuestaMensajeDto>()
                           ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Respuesta vacía del servidor." };

                var errorResponse = await response.Content.ReadFromJsonAsync<RespuestaMensajeDto>();
                return errorResponse ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Error desconocido del servidor." };
            }
            catch (Exception)
            {
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Error al conectarse con el servidor." };
            }
        }
    }
}
