using Lab_Contracts.Examenes;

namespace Lab_Blazor.Services.Examenes
{
    public interface IExamenComposicionApiService
    {
        Task<List<ExamenComposicionDto>> GetComposicionesPorExamenPadreAsync(int idExamenPadre);
        Task<List<ExamenComposicionDto>> GetComposicionesPorExamenHijoAsync(int idExamenHijo);
        Task<HttpResponseMessage> CrearComposicionAsync(ExamenComposicionDto dto);
        Task<HttpResponseMessage> EliminarComposicionAsync(int idExamenPadre, int idExamenHijo);
    }
}
