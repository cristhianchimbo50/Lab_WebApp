using Lab_Contracts.Auth;
using Lab_Contracts.Shared;

namespace Lab_Blazor.Services.Auth
{
    public interface IRecuperacionApiService
    {
        Task<RespuestaMensajeDto> SolicitarRecuperacionAsync(OlvideContraseniaDto Dto);
        Task<RespuestaMensajeDto> RestablecerContraseniaAsync(RestablecerContraseniaDto Dto);
    }
}
