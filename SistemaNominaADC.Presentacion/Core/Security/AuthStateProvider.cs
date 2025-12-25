namespace SistemaNominaADC.Presentacion_Old.Core.Security
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Components.Authorization;

    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthService _authService;
        private UsuarioSesion? _usuarioActual;

        public AuthStateProvider(AuthService authService)
        {
            authService.OnAuthStateChanged += () => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // En un entorno real, buscarías un token en localStorage/cookies
            var identity = new ClaimsIdentity();

            if (_usuarioActual != null)
            {
                // Crear la identidad del usuario a partir del UsuarioSesion simulado
                identity = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, _usuarioActual.sUsuario),
                new Claim(ClaimTypes.PrimarySid, _usuarioActual.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, _usuarioActual.sRol)
            }, "CustomAuth");
            }

            var user = new ClaimsPrincipal(identity);
            return await Task.FromResult(new AuthenticationState(user));
        }

        // Método para ser llamado cuando el usuario inicia sesión
        public void NotifyUserLogin(UsuarioSesion sesion)
        {
            _usuarioActual = sesion; // Almacenar el usuario de sesión
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        // Método para ser llamado cuando el usuario cierra sesión
        public void NotifyUserLogout()
        {
            _usuarioActual = null; // Limpiar el usuario de sesión
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        // El método invocado por el evento del AuthService
        private void StateChanged()
        {
            // Aquí podrías añadir lógica para re-evaluar el estado si el AuthService
            // cambia el estado interno (como al llamar Logout)
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

}
