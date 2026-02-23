using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace SistemaNominaADC.Presentacion.Security
{
    public class DummyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public DummyAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Marca como autenticado a nivel de servidor para evitar challenges del middleware.
            // La autorización real de la UI se maneja en CustomAuthStateProvider.
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "dummy-user")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Dummy");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Dummy");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            // No redirigir ni forzar 401 aquí: el enrutado Blazor maneja la UI de login.
            return Task.CompletedTask;
        }
    }
}
