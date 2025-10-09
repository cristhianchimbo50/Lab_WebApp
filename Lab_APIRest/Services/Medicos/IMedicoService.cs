using Lab_Contracts.Medicos;

namespace Lab_APIRest.Services.Medicos
{
    public interface IMedicoService
    {
        Task<List<MedicoDto>> GetMedicosAsync();
        Task<MedicoDto?> GetMedicoByIdAsync(int id);
        Task<MedicoDto?> GetMedicoPorCorreoAsync(string correo);
        Task<List<MedicoDto>> GetMedicosPorNombreAsync(string nombre);
        Task<List<MedicoDto>> GetMedicosPorEspecialidadAsync(string especialidad);
        Task<List<MedicoDto>> GetMedicosPorCorreoAsync(string correo);
        Task<MedicoDto> CrearMedicoAsync(MedicoDto dto);
        Task<bool> EditarMedicoAsync(int id, MedicoDto dto);
        Task<bool> AnularMedicoAsync(int id);
    }
}
