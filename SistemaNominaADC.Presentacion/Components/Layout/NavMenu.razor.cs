using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Security;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Layout
{
    public partial class NavMenu : IDisposable
    {
        [Inject] private CustomAuthStateProvider AuthStateProvider { get; set; } = null!;
        [Inject] private SessionService SessionService { get; set; } = null!;
        [Inject] private IObjetoSistemaCliente ObjetoSistemaCliente { get; set; } = null!;

        private string? currentUrl;
        private List<MenuItem> menuOperaciones = new();
        private List<MenuItem> menuMantenimientos = new();
        private List<MenuItem> menuMantenimientosPlanilla = new();
        private List<MenuItem> menuConfiguracion = new();
        private bool expandOperacionesMenu = true;
        private bool expandMantenimientosMenu;
        private bool expandMantenimientosPlanillaMenu = true;
        private bool expandConfiguracionMenu;
        private bool _disposed;

        private static readonly HashSet<string> MantenimientosPlanilla = new(StringComparer.OrdinalIgnoreCase)
        {
            "ModoCalculoConceptoNomina",
            "TipoConceptoNomina",
            "TipoPlanilla",
            "TipoPlanillaConcepto",
            "TramoRentaSalario",
            "EmpleadoConceptoNomina"
        };

        protected override async Task OnInitializedAsync()
        {
            currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            NavigationManager.LocationChanged += OnLocationChanged;
            AuthStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;

            if (!SessionService.IsAuthenticated)
            {
                menuOperaciones = new();
                menuMantenimientos = new();
                menuMantenimientosPlanilla = new();
                menuConfiguracion = new();
                return;
            }

            await CargarMenuAsync();
        }

        private void ToggleOperacionesMenu()
        {
            expandOperacionesMenu = !expandOperacionesMenu;
        }

        private void ToggleMantenimientosMenu()
        {
            expandMantenimientosMenu = !expandMantenimientosMenu;
        }

        private void ToggleConfiguracionMenu()
        {
            expandConfiguracionMenu = !expandConfiguracionMenu;
        }

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
            AuthStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }

        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            if (!SessionService.IsAuthenticated)
            {
                menuOperaciones = new();
                menuMantenimientos = new();
                menuMantenimientosPlanilla = new();
                menuConfiguracion = new();
                expandOperacionesMenu = false;
                expandMantenimientosMenu = false;
                expandMantenimientosPlanillaMenu = false;
                expandConfiguracionMenu = false;
            }
            else
            {
                expandOperacionesMenu = true;
                await CargarMenuAsync();
            }

            await InvokeAsync(StateHasChanged);
        }

        private async void Logout()
        {
            await SessionService.ClearAsync();
            AuthStateProvider.NotifyUserLogout();
            NavigationManager.NavigateTo("/login");
        }

        private async Task CargarMenuAsync()
        {
            menuOperaciones = new();
            menuMantenimientos = new();
            menuMantenimientosPlanilla = new();
            menuConfiguracion = new();

            var objetos = await ObjetoSistemaCliente.ListaParaMenu();
            var visibles = new HashSet<string>(
                objetos.Select(o => ObjetoSistemaCatalogo.Canonicalize(o.NombreEntidad)),
                StringComparer.OrdinalIgnoreCase);

            if (visibles.Count == 0 && RolesSistema.EsAdministrador(SessionService.Roles))
            {
                foreach (var def in ObjetoSistemaCatalogo.Items)
                {
                    visibles.Add(def.NombreEntidad);
                }
            }

            var rutasAgregadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var def in ObjetoSistemaCatalogo.Items)
            {
                if (!visibles.Contains(def.NombreEntidad))
                    continue;

                if (!rutasAgregadas.Add(def.Ruta))
                    continue;

                var item = new MenuItem(def.Etiqueta, def.Ruta, def.Icono);
                if (string.Equals(def.Seccion, "Operaciones", StringComparison.OrdinalIgnoreCase))
                {
                    menuOperaciones.Add(item);
                    continue;
                }

                if (string.Equals(def.Seccion, "Mantenimientos", StringComparison.OrdinalIgnoreCase))
                {
                    if (MantenimientosPlanilla.Contains(def.NombreEntidad))
                        menuMantenimientosPlanilla.Add(item);
                    else
                        menuMantenimientos.Add(item);
                    continue;
                }

                if (string.Equals(def.Seccion, "Configuracion", StringComparison.OrdinalIgnoreCase))
                {
                    menuConfiguracion.Add(item);
                }
            }
        }

        private record MenuItem(string Nombre, string Ruta, string Icono);
    }
}
