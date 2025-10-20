using Lab_Contracts.Ordenes;

namespace Lab_APIRest.Services.Ordenes
{
    public interface IOrdenService
    {
        Task<List<OrdenDto>> ListarOrdenesAsync();
        Task<OrdenDto?> ObtenerOrdenPorIdAsync(int idOrden);
        Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto dto);
        Task<bool> AnularOrdenCompletaAsync(int idOrden);
    }


}
