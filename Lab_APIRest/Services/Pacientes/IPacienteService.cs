using Lab_Contracts.Pacientes;
using Lab_Contracts.Common;

namespace Lab_APIRest.Services.Pacientes
{
    public interface IPacienteService
    {
        Task<List<PacienteDto>> ListarPacientesAsync();
        Task<PacienteDto?> ObtenerDetallePacienteAsync(int idPaciente);
        Task<List<PacienteDto>?> ListarPacientesAsync(string criterio, string valor);
        Task<ResultadoPaginadoDto<PacienteDto>> ListarPacientesPaginadosAsync(PacienteFiltroDto filtro);
        Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> GuardarPacienteAsync(PacienteDto dto);
        Task<bool> GuardarPacienteAsync(int idPaciente, PacienteDto dto);
        Task<bool> AnularPacienteAsync(int idPaciente);
        Task<(bool Exito, string Mensaje, string? NuevaTemporal)> ReenviarCredencialesTemporalesPacienteAsync(int idPaciente);
    }
}
