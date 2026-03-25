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
        private readonly SemaphoreSlim _mutex = new(1, 1);
        private static readonly TimeSpan VentanaVerificacion = TimeSpan.FromSeconds(30);

        private DateTime _ultimaVerificacionUtc = DateTime.MinValue;
        private bool _sesionCerrada;

        public SesionVerificacionService(IAuthApiService authService, IJSRuntime jsRuntime, NavigationManager navegacion)
        {
            _authService = authService;
            _jsRuntime = jsRuntime;
            _navegacion = navegacion;
        }

        public async Task<bool> VerificarOSalirAsync(bool forzar = false)
        {
            if (_sesionCerrada) return false;

            if (!forzar && DateTime.UtcNow - _ultimaVerificacionUtc < VentanaVerificacion)
                return true;

            await _mutex.WaitAsync();
            try
            {
                if (_sesionCerrada) return false;
                if (!forzar && DateTime.UtcNow - _ultimaVerificacionUtc < VentanaVerificacion)
                    return true;

                var isSessionValid = await _authService.VerificarSesionAsync();
                _ultimaVerificacionUtc = DateTime.UtcNow;

                if (!isSessionValid)
                {
                    _sesionCerrada = true;
                    await CerrarSesionYRedirigirAsync();
                    return false;
                }

                return true;
            }
            finally
            {
                _mutex.Release();
            }
        }

        public async Task EjecutarConSesionAsync(Func<Task> accion, bool forzarVerificacion = false)
        {
            if (!await VerificarOSalirAsync(forzarVerificacion))
                return;

            await accion();
        }

        public async Task<T?> EjecutarConSesionAsync<T>(Func<Task<T>> accion, bool forzarVerificacion = false, T? fallback = default)
        {
            if (!await VerificarOSalirAsync(forzarVerificacion))
                return fallback;

            return await accion();
        }

        private async Task CerrarSesionYRedirigirAsync()
        {
            var currentUrl = _navegacion.ToBaseRelativePath(_navegacion.Uri);
            if (!string.IsNullOrWhiteSpace(currentUrl) && !currentUrl.StartsWith("login", StringComparison.OrdinalIgnoreCase))
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "redirectAfterLogin", currentUrl);
            }

            await _authService.LogoutAsync();
            _navegacion.NavigateTo("/login", forceLoad: true);
        }
    }
}
