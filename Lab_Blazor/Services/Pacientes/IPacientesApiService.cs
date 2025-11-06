using Lab_Contracts.Pacientes;
using System.Net.Http;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Pacientes
{
    public interface IPacientesApiService
    {
        Task<List<PacienteDto>> ListarPacientesAsync();
        Task<PacienteDto?> ObtenerDetallePacienteAsync(int idPaciente);
        Task<List<PacienteDto>> ListarPacientesAsync(string criterio, string valor);
        Task<ResultadoPaginadoDto<PacienteDto>> ListarPacientesPaginadosAsync(PacienteFiltroDto filtro);

        Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> GuardarPacienteAsync(PacienteDto paciente);

        Task<HttpResponseMessage> GuardarPacienteAsync(int idPaciente, PacienteDto paciente);
        Task<HttpResponseMessage> AnularPacienteAsync(int idPaciente);
        Task<PacienteDto?> ObtenerPacientePorCedulaAsync(string cedula);

        Task<(bool Exito, string Mensaje)> ReenviarCredencialesTemporalesAsync(int idPaciente);
    }
}
