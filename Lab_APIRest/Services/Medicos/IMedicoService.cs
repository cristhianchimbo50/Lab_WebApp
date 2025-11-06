using Lab_Contracts.Medicos;
using Lab_Contracts.Common;

namespace Lab_APIRest.Services.Medicos
{
    public interface IMedicoService
    {
        Task<List<MedicoDto>> ListarMedicosAsync();
        Task<MedicoDto?> ObtenerDetalleMedicoAsync(int idMedico);
        Task<MedicoDto?> ObtenerMedicoPorCorreoAsync(string correo);
        Task<List<MedicoDto>> ListarMedicosPorNombreAsync(string nombre);
        Task<List<MedicoDto>> ListarMedicosPorEspecialidadAsync(string especialidad);
        Task<List<MedicoDto>> ListarMedicosPorCorreoAsync(string correo);
        Task<MedicoDto> GuardarMedicoAsync(MedicoDto medicoDto);
        Task<bool> GuardarMedicoAsync(int idMedico, MedicoDto medicoDto);
        Task<bool> AnularMedicoAsync(int idMedico);
        Task<ResultadoPaginadoDto<MedicoDto>> ListarMedicosPaginadosAsync(MedicoFiltroDto filtro);
    }
}
