using Lab_Contracts.Examenes;

namespace Lab_APIRest.Services.Examenes
{
    public interface IExamenComposicionService
    {
        Task<List<ExamenComposicionDto>> ObtenerPorPadre(int IdExamenPadre);
        Task<List<ExamenComposicionDto>> ObtenerPorHijo(int IdExamenHijo);
        Task<bool> Crear(ExamenComposicionDto ComposicionDto);
        Task<bool> Eliminar(int IdExamenPadre, int IdExamenHijo);
    }
}
