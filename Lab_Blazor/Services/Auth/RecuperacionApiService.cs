using Lab_Contracts.Auth;
using Lab_Contracts.Shared;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Auth
{
    public class RecuperacionApiService : BaseApiService, IRecuperacionApiService
    {
        public RecuperacionApiService(IHttpClientFactory Factory, ProtectedSessionStorage Session, IJSRuntime Js)
            : base(Factory, Session, Js) { }

        public async Task<RespuestaMensajeDto> SolicitarRecuperacionAsync(OlvideContraseniaDto Dto)
        {
            try
            {
                var Response = await _http.PostAsJsonAsync("api/recuperacion/solicitar", Dto);
                if (Response.IsSuccessStatusCode)
                    return await Response.Content.ReadFromJsonAsync<RespuestaMensajeDto>()
                           ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Respuesta vacía del servidor." };

                var ErrorResponse = await Response.Content.ReadFromJsonAsync<RespuestaMensajeDto>();
                return ErrorResponse ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Error desconocido del servidor." };
            }
            catch (Exception)
            {
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Error al conectarse con el servidor." };
            }
        }

        public async Task<RespuestaMensajeDto> RestablecerContraseniaAsync(RestablecerContraseniaDto Dto)
        {
            try
            {
                var Response = await _http.PostAsJsonAsync("api/recuperacion/restablecer", Dto);
                if (Response.IsSuccessStatusCode)
                    return await Response.Content.ReadFromJsonAsync<RespuestaMensajeDto>()
                           ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Respuesta vacía del servidor." };

                var ErrorResponse = await Response.Content.ReadFromJsonAsync<RespuestaMensajeDto>();
                return ErrorResponse ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Error desconocido del servidor." };
            }
            catch (Exception)
            {
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Error al conectarse con el servidor." };
            }
        }
    }
}
