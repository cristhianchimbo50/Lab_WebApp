using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;

namespace Lab_Blazor.Services.Pagos
{
    public interface IPagosApiService
    {
        Task<PagoDto?> RegistrarPagoAsync(PagoDto dto);
        Task<List<PagoDto>> ListarPagosPorOrdenAsync(int idOrden);

        Task<IEnumerable<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro);

        Task<PagoDto?> RegistrarCobroCuentaPorCobrarAsync(PagoDto pago);
    }
}
