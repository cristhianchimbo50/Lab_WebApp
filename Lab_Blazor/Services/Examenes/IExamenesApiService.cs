using Lab_Contracts.Examenes;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Lab_Blazor.Services.Examenes
{
    public interface IExamenesApiService
    {
        Task<List<ExamenDto>> ListarExamenesAsync();
        Task<List<ExamenDto>> ListarExamenesAsync(string filtro);
        Task<ExamenDto?> ObtenerDetalleExamenAsync(int id);
        Task<List<ExamenDto>> ListarExamenesPorNombreAsync(string nombre);
        Task<HttpResponseMessage> GuardarExamenAsync(ExamenDto examen);
        Task<HttpResponseMessage> GuardarExamenAsync(int id, ExamenDto examen);
        Task<HttpResponseMessage> AnularExamenAsync(int id);
        Task<List<ExamenDto>> ListarExamenesHijosAsync(int idPadre);
        Task<HttpResponseMessage> AsignarExamenHijoAsync(int idPadre, int idHijo);
        Task<HttpResponseMessage> EliminarExamenHijoAsync(int idPadre, int idHijo);
    }
}
