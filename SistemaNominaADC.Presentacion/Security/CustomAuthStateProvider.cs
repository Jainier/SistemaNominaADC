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

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!oSessionService.IsAuthenticated)
            {
                var oAnonymous = new ClaimsPrincipal(
                    new ClaimsIdentity()
                );

                return Task.FromResult(
                    new AuthenticationState(oAnonymous)
                );
            }

            var oClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, oSessionService.UserName ?? string.Empty)
        };

            foreach (var sRol in oSessionService.Roles)
            {
                oClaims.Add(new Claim(ClaimTypes.Role, sRol));
            }

            var oIdentity = new ClaimsIdentity(oClaims, "jwt");

            var oUser = new ClaimsPrincipal(oIdentity);

            return Task.FromResult(
                new AuthenticationState(oUser)
            );
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
