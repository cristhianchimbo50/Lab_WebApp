using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;

namespace Lab_APIRest.Services.Pagos
{
    public interface IPagoService
    {
        Task<PagoDto?> RegistrarPagoAsync(PagoDto dto);
        Task<List<PagoDto>> ListarPagosPorOrdenAsync(int idOrden);
    }
}
