using Lab_Contracts.Auth;

namespace Lab_APIRest.Services.Auth
{
    public interface IRecuperacionService
    {
        Task<RespuestaMensajeDto> SolicitarRecuperacionAsync(OlvideContraseniaDto dto, CancellationToken ct);
        Task<RespuestaMensajeDto> RestablecerContraseniaAsync(RestablecerContraseniaDto dto, CancellationToken ct);
    }
}
