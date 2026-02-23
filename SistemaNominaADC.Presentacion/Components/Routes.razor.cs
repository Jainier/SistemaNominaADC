using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            SessionService.EnableBrowserStorageInterop();

            if (SessionService.IsAuthenticated)
            {
                SessionService.MarkInitialRestoreCompleted();
                AuthStateProvider.NotifyUserAuthentication();
                StateHasChanged();
                return;
            }

            if (SessionService.IsRestoring)
                return;

            await TryRestoreSessionAsync();
            SessionService.MarkInitialRestoreCompleted();
            StateHasChanged();
        }

        private async Task TryRestoreSessionAsync()
        {
            if (SessionService.IsRestoring || SessionService.RestoreAttempted)
            {
                if (SessionService.IsAuthenticated)
                {
                    AuthStateProvider.NotifyUserAuthentication();
                }
                return;
            }

            var restored = await SessionService.RestoreSessionAsync();

            if (restored || SessionService.IsAuthenticated)
            {
                AuthStateProvider.NotifyUserAuthentication();
            }
        }

        protected async Task OnNavigateAsync(NavigationContext context)
        {
            await Task.CompletedTask;
        }
    }
}
