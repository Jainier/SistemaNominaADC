using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SistemaNominaADC.Presentacion.Components.Layout;
using SistemaNominaADC.Presentacion.Components.Pages;
using SistemaNominaADC.Presentacion.Security;
using SistemaNominaADC.Presentacion.Services.Auth;

namespace SistemaNominaADC.Presentacion.Components
{
    public partial class RoutesBase : ComponentBase
    {
        [Inject] protected SessionService SessionService { get; set; } = null!;
        [Inject] protected CustomAuthStateProvider AuthStateProvider { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            var restored = await SessionService.RestoreSessionAsync();

            if (restored)
            {
                AuthStateProvider.NotifyUserAuthentication();
            }
        }
    }
}
