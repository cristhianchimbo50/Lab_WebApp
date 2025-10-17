using Lab_Contracts.Resultados;

namespace Lab_APIRest.Services.Resultados
{
    public interface IResultadoService
    {
        Task<bool> GuardarResultadosAsync(ResultadoGuardarDto dto);
    }
}
