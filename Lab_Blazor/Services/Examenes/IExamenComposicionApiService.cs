using Lab_Contracts.Examenes;

namespace Lab_Blazor.Services.Examenes
{
    public interface IExamenComposicionApiService
    {
        Task<List<ExamenComposicionDto>> ListarComposicionesPorPadreAsync(int idExamenPadre);
        Task<List<ExamenComposicionDto>> ListarComposicionesPorHijoAsync(int idExamenHijo);
        Task<HttpResponseMessage> GuardarComposicionAsync(ExamenComposicionDto composicion);
        Task<HttpResponseMessage> EliminarComposicionAsync(int idExamenPadre, int idExamenHijo);
    }
}
