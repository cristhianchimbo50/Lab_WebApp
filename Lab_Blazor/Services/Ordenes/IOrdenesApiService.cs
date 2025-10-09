using Lab_Contracts.Ordenes;

namespace Lab_Blazor.Services.Ordenes
{
    public interface IOrdenesApiService
    {
        Task<List<OrdenDto>> GetOrdenesAsync();
        Task<OrdenDto?> GetOrdenPorIdAsync(int id);
        Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto orden);
    }

}