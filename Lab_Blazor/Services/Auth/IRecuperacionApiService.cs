using Lab_Contracts.Auth;

namespace Lab_Blazor.Services.Auth
{
    public interface IRecuperacionApiService
    {
        Task<RespuestaMensajeDto> SolicitarRecuperacionContraseniaAsync(OlvideContraseniaDto dto);
        Task<RespuestaMensajeDto> RestablecerContraseniaAsync(RestablecerContraseniaDto dto);
    }
}
