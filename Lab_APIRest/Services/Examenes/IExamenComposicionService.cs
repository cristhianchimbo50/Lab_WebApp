using Lab_Contracts.Examenes;

namespace Lab_APIRest.Services.Examenes
{
    public interface IExamenComposicionService
    {
        Task<List<ExamenComposicionDto>> ListarComposicionesPorPadreAsync(int idExamenPadre);
        Task<List<ExamenComposicionDto>> ListarComposicionesPorHijoAsync(int idExamenHijo);
        Task<bool> GuardarComposicionAsync(ExamenComposicionDto composicionDto);
        Task<bool> EliminarComposicionAsync(int idExamenPadre, int idExamenHijo);
    }
}
