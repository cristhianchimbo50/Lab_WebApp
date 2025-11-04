using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Lab_Blazor.Services.Auth;

namespace Lab_Blazor.Services.Auth
{
    public class SesionVerificacionService
    {
        private readonly IAuthApiService _authApi;
        private readonly IJSRuntime _js;
        private readonly NavigationManager _nav;

        public SesionVerificacionService(IAuthApiService authApi, IJSRuntime js, NavigationManager nav)
        {
            _authApi = authApi;
            _js = js;
            _nav = nav;
        }

        public async Task<bool> VerificarOSalirAsync()
        {
            bool sesionValida = await _authApi.VerificarSesionAsync();
            if (!sesionValida)
            {
                var currentUrl = _nav.ToBaseRelativePath(_nav.Uri);
                await _js.InvokeVoidAsync("localStorage.setItem", "redirectAfterLogin", currentUrl);

                await _authApi.LogoutAsync();

                _nav.NavigateTo("/login", forceLoad: true);
            }
            return sesionValida;
        }
    }
}
