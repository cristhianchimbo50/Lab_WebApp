using Lab_Contracts.Ajustes.Perfil;

namespace Lab_APIRest.Services.Perfil
{
    public interface IPerfilService
    {
        Task<PerfilResponseDto?> ObtenerPerfilAsync(int idUsuario, CancellationToken ct);
    }
}
