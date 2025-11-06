using Lab_Contracts.Ajustes.Perfil;

namespace Lab_APIRest.Services.Perfil
{
    public interface IPerfilService
    {
        Task<PerfilResponseDto?> ObtenerDetallePerfilAsync(int idUsuario, CancellationToken ct);
    }
}
