using Lab_Contracts.Medicos;

namespace Lab_APIRest.Services.Medicos
{
    public interface IMedicoService
    {
        Task<List<MedicoDto>> ObtenerMedicosAsync();
        Task<MedicoDto?> ObtenerMedicoPorIdAsync(int IdMedico);
        Task<MedicoDto?> ObtenerMedicoPorCorreoAsync(string Correo);
        Task<List<MedicoDto>> ObtenerMedicosPorNombreAsync(string Nombre);
        Task<List<MedicoDto>> ObtenerMedicosPorEspecialidadAsync(string Especialidad);
        Task<List<MedicoDto>> ObtenerMedicosPorCorreoAsync(string Correo);
        Task<MedicoDto> RegistrarMedicoAsync(MedicoDto DatosMedico);
        Task<bool> EditarMedicoAsync(int IdMedico, MedicoDto DatosMedico);
        Task<bool> AnularMedicoAsync(int IdMedico);
        Task<List<MedicoDto>> ListarMedicosAsync();
    }
}
