using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Layout
{
    public partial class NavMenu : IDisposable
    {
        [Inject] private IObjetoSistemaCliente ObjetoCliente { get; set; } = null!;

        private string? currentUrl;
        private List<ObjetoSistema>? objetosMenu;
        private bool expandSubMenu = false;

        protected override async Task OnInitializedAsync()
        {
            currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            NavigationManager.LocationChanged += OnLocationChanged;
            
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
            currentUrl = NavigationManager.ToBaseRelativePath(e.Location);
            StateHasChanged();
        }
        public void Dispose() => NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
