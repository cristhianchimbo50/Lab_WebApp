using Lab_Contracts.Examenes;

namespace Lab_APIRest.Services.Examenes
{
    public interface IExamenService
    {
        Task<List<ExamenDto>> GetExamenesAsync();
        Task<ExamenDto?> GetExamenByIdAsync(int id);
        Task<List<ExamenDto>> BuscarExamenesPorNombreAsync(string nombre);
        Task<ExamenDto> CrearExamenAsync(ExamenDto dto);
        Task<bool> EditarExamenAsync(int id, ExamenDto dto);
        Task<bool> AnularExamenAsync(int id);

        //Para examenes compuestos
        Task<List<ExamenDto>> ObtenerHijosDeExamenAsync(int idExamenPadre);
        Task<bool> AgregarExamenHijoAsync(int idExamenPadre, int idExamenHijo);
        Task<bool> EliminarExamenHijoAsync(int idExamenPadre, int idExamenHijo);
    }
}
