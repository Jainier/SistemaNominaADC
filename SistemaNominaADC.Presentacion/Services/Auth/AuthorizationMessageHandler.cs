using SistemaNominaADC.Presentacion.Services.Auth;
using System.Net.Http.Headers;

namespace SistemaNominaADC.Presentacion.Security
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly SessionService _sessionService;

        public AuthorizationMessageHandler(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!_sessionService.IsAuthenticated)
            {
                var bRestored = await _sessionService.RestoreSessionAsync();
                Console.WriteLine($"[AuthHandler] RestoreSessionAsync => {bRestored}");
            }

            Console.WriteLine($"[AuthHandler] IsAuthenticated={_sessionService.IsAuthenticated}, TokenEmpty={string.IsNullOrWhiteSpace(_sessionService.Token)}");

            if (!string.IsNullOrWhiteSpace(_sessionService.Token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _sessionService.Token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
