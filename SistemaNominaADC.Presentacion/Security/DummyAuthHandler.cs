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
            // No autentica a nadie: la autenticación real la maneja tu CustomAuthStateProvider.
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            // En vez de responder 401, redirige al login
            Response.StatusCode = StatusCodes.Status302Found;
            Response.Headers.Location = "/login";
            return Task.CompletedTask;
        }
    }
}
