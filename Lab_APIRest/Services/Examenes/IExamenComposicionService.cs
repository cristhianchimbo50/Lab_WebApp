using Lab_Contracts.Examenes;

namespace Lab_APIRest.Services.Examenes
{
    public interface IExamenComposicionService
    {
        Task<List<ExamenComposicionDto>> GetComposicionesPorExamenPadreAsync(int idExamenPadre);
        Task<List<ExamenComposicionDto>> GetComposicionesPorExamenHijoAsync(int idExamenHijo);
        Task<bool> CrearComposicionAsync(ExamenComposicionDto dto);
        Task<bool> EliminarComposicionAsync(int idExamenPadre, int idExamenHijo);
    }
}
