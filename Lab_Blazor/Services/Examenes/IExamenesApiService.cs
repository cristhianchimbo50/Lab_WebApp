using Lab_Contracts.Examenes;

namespace Lab_Blazor.Services.Examenes
{
    public interface IExamenesApiService
    {
        Task<List<ExamenDto>> GetExamenesAsync();
        Task<ExamenDto?> GetExamenPorIdAsync(int id);
        Task<List<ExamenDto>> BuscarExamenesPorNombreAsync(string nombre);
        Task<HttpResponseMessage> CrearExamenAsync(ExamenDto examen);
        Task<HttpResponseMessage> EditarExamenAsync(int id, ExamenDto examen);
        Task<HttpResponseMessage> AnularExamenAsync(int id);

        //Para composiciones

        Task<List<ExamenDto>> ObtenerHijosDeExamenAsync(int idPadre);
        Task<HttpResponseMessage> AgregarExamenHijoAsync(int idPadre, int idHijo);
        Task<HttpResponseMessage> EliminarExamenHijoAsync(int idPadre, int idHijo);
    }
}
