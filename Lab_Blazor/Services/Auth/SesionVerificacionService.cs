using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Lab_Blazor.Services.Auth;

namespace Lab_Blazor.Services.Auth
{
    public class SesionVerificacionService
    {
        private readonly IAuthApiService AuthService;
        private readonly IJSRuntime JsRuntime;
        private readonly NavigationManager Navegacion;

        public SesionVerificacionService(IAuthApiService AuthService, IJSRuntime JsRuntime, NavigationManager Navegacion)
        {
            this.AuthService = AuthService;
            this.JsRuntime = JsRuntime;
            this.Navegacion = Navegacion;
        }

        public async Task<bool> VerificarOSalirAsync()
        {
            bool IsSessionValid = await AuthService.VerificarSesionAsync();
            if (!IsSessionValid)
            {
                var CurrentUrl = Navegacion.ToBaseRelativePath(Navegacion.Uri);
                await JsRuntime.InvokeVoidAsync("localStorage.setItem", "redirectAfterLogin", CurrentUrl);

                await AuthService.LogoutAsync();

                Navegacion.NavigateTo("/login", forceLoad: true);
            }
            return IsSessionValid;
        }
    }
}
