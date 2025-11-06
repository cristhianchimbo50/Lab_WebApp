using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Lab_Blazor.Services.Auth;

namespace Lab_Blazor.Services.Auth
{
    public class SesionVerificacionService
    {
        private readonly IAuthApiService _authService;
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navegacion;

        public SesionVerificacionService(IAuthApiService authService, IJSRuntime jsRuntime, NavigationManager navegacion)
        {
            _authService = authService;
            _jsRuntime = jsRuntime;
            _navegacion = navegacion;
        }

        public async Task<bool> VerificarOSalirAsync()
        {
            bool isSessionValid = await _authService.VerificarSesionAsync();
            if (!isSessionValid)
            {
                var currentUrl = _navegacion.ToBaseRelativePath(_navegacion.Uri);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "redirectAfterLogin", currentUrl);

                await _authService.LogoutAsync();

                _navegacion.NavigateTo("/login", forceLoad: true);
            }
            return isSessionValid;
        }
    }
}
