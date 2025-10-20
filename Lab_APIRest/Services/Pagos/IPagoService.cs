using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;

namespace Lab_APIRest.Services.Pagos
{
    public interface IPagoService
    {
        Task<PagoDto?> RegistrarPagoAsync(PagoDto dto);
        Task<List<PagoDto>> ListarPagosPorOrdenAsync(int idOrden);

        Task<PagoDto?> RegistrarCobroCuentaPorCobrarAsync(PagoDto dto);
        Task<List<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro);
    }
}
