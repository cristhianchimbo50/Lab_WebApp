using Lab_Contracts.Examenes;

namespace Lab_APIRest.Services.Examenes
{
    public interface IExamenService
    {
        Task<List<ExamenDto>> ObtenerExamenesAsync();
        Task<ExamenDto?> ObtenerExamenPorIdAsync(int IdExamen);
        Task<List<ExamenDto>> BuscarExamenesPorNombreAsync(string Nombre);
        Task<ExamenDto> RegistrarExamenAsync(ExamenDto DatosExamen);
        Task<bool> EditarExamenAsync(int IdExamen, ExamenDto DatosExamen);
        Task<bool> AnularExamenAsync(int IdExamen);

        //Para examenes compuestos
        Task<List<ExamenDto>> ObtenerHijosDeExamenAsync(int IdExamenPadre);
        Task<bool> AgregarExamenHijoAsync(int IdExamenPadre, int IdExamenHijo);
        Task<bool> EliminarExamenHijoAsync(int IdExamenPadre, int IdExamenHijo);
    }
}
