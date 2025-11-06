using Lab_Contracts.Ajustes.Perfil;

namespace Lab_Blazor.Services.Perfil
{
    public interface IPerfilApiService
    {
        Task<PerfilResponseDto?> ObtenerDetallePerfilAsync();
    }
}
