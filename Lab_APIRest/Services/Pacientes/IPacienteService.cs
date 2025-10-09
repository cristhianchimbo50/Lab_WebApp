using Lab_Contracts.Pacientes;

namespace Lab_APIRest.Services.Pacientes
{
    public interface IPacienteService
    {
        Task<List<PacienteDto>> GetPacientesAsync();
        Task<PacienteDto?> GetPacienteByIdAsync(int id);
        Task<PacienteDto?> GetPacienteByCedulaAsync(string cedula);
        Task<List<PacienteDto>> GetPacientesPorNombreAsync(string nombre);
        Task<List<PacienteDto>> GetPacientesPorCorreoAsync(string correo);
        Task<PacienteDto> CrearPacienteAsync(PacienteDto dto, int usuarioId);
        Task<bool> EditarPacienteAsync(int id, PacienteDto dto);
        Task<bool> AnularPacienteAsync(int id);
    }
}
