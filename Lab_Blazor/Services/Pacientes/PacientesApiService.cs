using Lab_Contracts.Pacientes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Pacientes
{
    public class PacientesApiService : BaseApiService, IPacientesApiService
    {
        public PacientesApiService(IHttpClientFactory Factory, ProtectedSessionStorage Session, IJSRuntime Js)
            : base(Factory, Session, Js) { }

        public async Task<List<PacienteDto>> ObtenerPacientesAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, "api/pacientes");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<List<PacienteDto>>() ?? new();
        }

        public async Task<PacienteDto?> ObtenerPacientePorIdAsync(int Id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/pacientes/{Id}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<PacienteDto>();
        }

        public async Task<List<PacienteDto>> BuscarPacientesAsync(string Campo, string Valor)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var CampoQuery = Uri.EscapeDataString(Campo ?? string.Empty);
            var ValorQuery = Uri.EscapeDataString(Valor ?? string.Empty);

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/pacientes/buscar?CampoBusqueda={CampoQuery}&ValorBusqueda={ValorQuery}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<List<PacienteDto>>() ?? new();
        }

        public async Task<ResultadoPaginadoDto<PacienteDto>> BuscarPacientesAsync(PacienteFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/pacientes/buscar")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<ResultadoPaginadoDto<PacienteDto>>()
                ?? new ResultadoPaginadoDto<PacienteDto> { Items = new List<PacienteDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
        }

        public async Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> CrearPacienteAsync(PacienteDto Paciente)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/pacientes")
            {
                Content = JsonContent.Create(Paciente)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            if (Respuesta.IsSuccessStatusCode)
            {
                try
                {
                    var Contenido = await Respuesta.Content.ReadFromJsonAsync<JsonElement>();
                    var Dto = new PacienteDto();

                    if (Contenido.TryGetProperty("idPaciente", out var Id))
                        Dto.IdPaciente = Id.GetInt32();
                    if (Contenido.TryGetProperty("nombrePaciente", out var Nombre))
                        Dto.NombrePaciente = Nombre.GetString() ?? "";
                    if (Contenido.TryGetProperty("correoElectronicoPaciente", out var Correo))
                        Dto.CorreoElectronicoPaciente = Correo.GetString() ?? "";
                    if (Contenido.TryGetProperty("contraseniaTemporal", out var Temporal))
                        Dto.ContraseniaTemporal = Temporal.GetString();

                    string Mensaje = Contenido.TryGetProperty("mensaje", out var Msg)
                        ? Msg.GetString() ?? "Paciente registrado correctamente."
                        : "Paciente registrado correctamente.";

                    return (true, Mensaje, Dto);
                }
                catch
                {
                    return (true, "Paciente registrado correctamente.", null);
                }
            }

            string ErrorMsg = Respuesta.StatusCode == System.Net.HttpStatusCode.Conflict
                ? await Respuesta.Content.ReadAsStringAsync()
                : $"Error {Respuesta.StatusCode}: {await Respuesta.Content.ReadAsStringAsync()}";

            return (false, ErrorMsg, null);
        }

        public async Task<HttpResponseMessage> EditarPacienteAsync(int Id, PacienteDto Paciente)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/pacientes/{Id}")
            {
                Content = JsonContent.Create(Paciente)
            };
            AddTokenHeader(Solicitud);

            return await _http.SendAsync(Solicitud);
        }

        public async Task<HttpResponseMessage> AnularPacienteAsync(int Id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/pacientes/anular/{Id}");
            AddTokenHeader(Solicitud);

            return await _http.SendAsync(Solicitud);
        }

        public async Task<PacienteDto?> ObtenerPacientePorCedulaAsync(string Cedula)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var CedulaQuery = Uri.EscapeDataString(Cedula ?? string.Empty);
            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/pacientes/buscar?CampoBusqueda=cedula&ValorBusqueda={CedulaQuery}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            var Pacientes = await Respuesta.Content.ReadFromJsonAsync<List<PacienteDto>>();
            return Pacientes?.FirstOrDefault();
        }

        public async Task<(bool Exito, string Mensaje)> ReenviarTemporalAsync(int IdPaciente)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, $"api/pacientes/{IdPaciente}/reenviar-temporal");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            if (Respuesta.IsSuccessStatusCode)
                return (true, "Se envió una nueva contraseña temporal al correo del paciente.");

            var Error = await Respuesta.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(Error) ? "No se pudo reenviar la contraseña temporal." : Error);
        }
    }
}
