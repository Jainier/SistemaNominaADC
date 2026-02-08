using Microsoft.AspNetCore.Components.Authorization;
using SistemaNominaADC.Presentacion.Services.Auth;
using System.Security.Claims;


namespace SistemaNominaADC.Presentacion.Security
{

    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly SessionService oSessionService;


        public CustomAuthStateProvider(SessionService sessionService)
        {
            oSessionService = sessionService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!oSessionService.IsAuthenticated)
            {
                await oSessionService.RestoreSessionAsync();
            }

            if (!oSessionService.IsAuthenticated)
            {
                return new AuthenticationState(
                    new ClaimsPrincipal(new ClaimsIdentity())
                );
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, oSessionService.UserName ?? string.Empty)
    };

            foreach (var rol in oSessionService.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public void NotifyUserAuthentication()
        {
            NotifyAuthenticationStateChanged(
                GetAuthenticationStateAsync()
            );
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(
                Task.FromResult(
                    new AuthenticationState(
                        new ClaimsPrincipal(new ClaimsIdentity())
                    )
                )
            );
        }

    }

}
