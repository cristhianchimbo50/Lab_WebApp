using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;

namespace Lab_APIRest.Services.Pagos
{
    public interface IPagoService
    {
        Task<PagoDto?> RegistrarPago(PagoDto PagoDto);
        Task<List<PagoDto>> ListarPagosPorOrden(int IdOrden);
        Task<PagoDto?> RegistrarCobroCuentaPorCobrar(PagoDto PagoDto);
        Task<List<OrdenDto>> ListarCuentasPorCobrar(PagoFiltroDto Filtro);
    }
}
