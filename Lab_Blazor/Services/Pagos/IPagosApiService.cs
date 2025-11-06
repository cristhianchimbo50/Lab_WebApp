using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Pagos
{
    public interface IPagosApiService
    {
        Task<PagoDto?> GuardarPagoAsync(PagoDto pago);
        Task<List<PagoDto>> ListarPagosAsync(int idOrden);
        Task<IEnumerable<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro);
        Task<ResultadoPaginadoDto<OrdenDto>> ListarCuentasPorCobrarPaginadoAsync(PagoFiltroDto filtro);
        Task<PagoDto?> GuardarCobroCuentaPorCobrarAsync(PagoDto pago);
    }
}
