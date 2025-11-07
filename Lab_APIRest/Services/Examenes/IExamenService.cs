using Lab_Contracts.Examenes;
using Lab_Contracts.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Lab_APIRest.Services.Examenes
{
    public interface IExamenService
    {
        Task<List<ExamenDto>> ListarExamenesAsync();
        Task<ExamenDto?> ObtenerDetalleExamenAsync(int idExamen);
        Task<List<ExamenDto>> ListarExamenesPorNombreAsync(string nombre);
        Task<ExamenDto> GuardarExamenAsync(ExamenDto datosExamen);
        Task<bool> GuardarExamenAsync(int idExamen, ExamenDto datosExamen);
        Task<bool> AnularExamenAsync(int idExamen);
        Task<List<ExamenDto>> ListarExamenesHijosAsync(int idExamenPadre);
        Task<bool> AsignarExamenHijoAsync(int idExamenPadre, int idExamenHijo);
        Task<bool> EliminarExamenHijoAsync(int idExamenPadre, int idExamenHijo);
        Task<ResultadoPaginadoDto<ExamenDto>> ListarExamenesPaginadosAsync(ExamenFiltroDto filtro);
    }
}
