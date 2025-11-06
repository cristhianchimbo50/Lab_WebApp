using Lab_Contracts.Examenes;

namespace Lab_Blazor.Services.Examenes
{
    public interface IExamenComposicionApiService
    {
        Task<List<ExamenComposicionDto>> ObtenerComposicionesPorExamenPadreAsync(int IdExamenPadre);
        Task<List<ExamenComposicionDto>> ObtenerComposicionesPorExamenHijoAsync(int IdExamenHijo);
        Task<HttpResponseMessage> CrearComposicionAsync(ExamenComposicionDto Composicion);
        Task<HttpResponseMessage> EliminarComposicionAsync(int IdExamenPadre, int IdExamenHijo);
    }
}
