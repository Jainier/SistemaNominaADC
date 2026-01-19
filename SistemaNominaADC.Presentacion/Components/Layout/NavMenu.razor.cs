using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Layout
{
    public partial class NavMenu : IDisposable
    {
        [Inject] private IObjetoSistemaCliente ObjetoCliente { get; set; } = null!;
        [Inject] private SessionService SessionService { get; set; } = null!;

        private string? currentUrl;
        private List<ObjetoSistema>? objetosMenu;
        private bool expandSubMenu;
        private bool _disposed;

        protected override async Task OnInitializedAsync()
        {
            currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            NavigationManager.LocationChanged += OnLocationChanged;

            if (!SessionService.IsAuthenticated)
            {
                objetosMenu = new();
                return;
            }

            try
            {
                objetosMenu = await ObjetoCliente.ListaParaMenu();
            }
            catch
            {
                objetosMenu = new();
            }
        }

        private void ToggleSubMenu() => expandSubMenu = !expandSubMenu;

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            if (_disposed)
                return;

            currentUrl = NavigationManager.ToBaseRelativePath(e.Location);

            _ = InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            _disposed = true;
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
    }
}
