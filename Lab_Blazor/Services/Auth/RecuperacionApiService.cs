using Lab_Contracts.Auth;
using Lab_Contracts.Shared;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Auth
{
    public class RecuperacionApiService : IRecuperacionApiService
    {
        private readonly HttpClient _http;

        public RecuperacionApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<RespuestaMensajeDto> SolicitarRecuperacionAsync(OlvideContraseniaDto dto)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/recuperacion/solicitar", dto);
                if (resp.IsSuccessStatusCode)
                    return await resp.Content.ReadFromJsonAsync<RespuestaMensajeDto>() ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Respuesta vacía del servidor." };

                var error = await resp.Content.ReadFromJsonAsync<RespuestaMensajeDto>();
                return error ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Error desconocido del servidor." };
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
                var resp = await _http.PostAsJsonAsync("api/recuperacion/restablecer", dto);
                if (resp.IsSuccessStatusCode)
                    return await resp.Content.ReadFromJsonAsync<RespuestaMensajeDto>() ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Respuesta vacía del servidor." };

                var error = await resp.Content.ReadFromJsonAsync<RespuestaMensajeDto>();
                return error ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Error desconocido del servidor." };
            }
            catch (Exception)
            {
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Error al conectarse con el servidor." };
            }
        }
    }
}
