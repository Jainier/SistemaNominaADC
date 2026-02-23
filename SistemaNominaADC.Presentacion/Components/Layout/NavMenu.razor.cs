using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;
using Microsoft.AspNetCore.Components.Authorization;
using SistemaNominaADC.Presentacion.Security;
using System.Net;


namespace SistemaNominaADC.Presentacion.Components.Layout
{
    public partial class NavMenu : IDisposable
    {
        [Inject] private CustomAuthStateProvider AuthStateProvider { get; set; } = null!;

        [Inject] private IObjetoSistemaCliente ObjetoCliente { get; set; } = null!;
        [Inject] private SessionService SessionService { get; set; } = null!;

        private string? currentUrl;
        private List<ObjetoSistemaDetalleDTO>? objetosMenu;
        private List<MenuItem> menuMantenimientos = new();
        private bool expandSubMenu;
        private bool _disposed;
        private bool _menuCargado;
        private bool _cargandoMenu;


        protected override async Task OnInitializedAsync()
        {
            currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            NavigationManager.LocationChanged += OnLocationChanged;

            AuthStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;

            if (!SessionService.IsAuthenticated)
            {
                objetosMenu = new();
                _menuCargado = false;
                return;
            }
            await CargarMenuAsync();
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
            AuthStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            if (!SessionService.IsAuthenticated)
            {
                objetosMenu = new();
                menuMantenimientos = new();
                expandSubMenu = false;
                _menuCargado = false;
            }
            else
            {
                await CargarMenuAsync();
            }

            await InvokeAsync(StateHasChanged);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_disposed || _menuCargado || _cargandoMenu || !SessionService.IsAuthenticated)
                return;

            await CargarMenuAsync();
            await InvokeAsync(StateHasChanged);
        }
        private async void Logout()
        {
            await SessionService.ClearAsync();
            AuthStateProvider.NotifyUserLogout();
            NavigationManager.NavigateTo("/login");
        }

        private void ConstruirMenuMantenimientos()
        {
            var lista = new List<MenuItem>();
            var objetos = objetosMenu ?? new();

            foreach (var obj in objetos)
            {
                var nombre = obj.NombreEntidad;
                var ruta = BuildRuta(nombre);
                if (lista.All(i => !string.Equals(i.Nombre, nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    lista.Add(new MenuItem(nombre, ruta));
                }
            }

            menuMantenimientos = lista;
        }

        private async Task CargarMenuAsync()
        {
            if (_cargandoMenu)
                return;

            _cargandoMenu = true;
            try
            {
                await CargarMenuConReintentoAsync();
            }
            catch
            {
                objetosMenu = new();
                ConstruirMenuMantenimientos();
                _menuCargado = true;
            }
            finally
            {
                _cargandoMenu = false;
            }
        }

        private async Task CargarMenuConReintentoAsync()
        {
            const int maxIntentos = 2;

            for (var intento = 1; intento <= maxIntentos; intento++)
            {
                try
                {
                    objetosMenu = await ObjetoCliente.ListaParaMenu();
                    ConstruirMenuMantenimientos();
                    _menuCargado = true;
                    return;
                }
                catch (TaskCanceledException) when (intento < maxIntentos)
                {
                    await Task.Delay(200);
                }
                catch (HttpRequestException ex) when (
                    (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
                    && intento < maxIntentos)
                {
                    await Task.Delay(200);
                }
                catch (TaskCanceledException)
                {
                    _menuCargado = false;
                    objetosMenu ??= new();
                    ConstruirMenuMantenimientos();
                    return;
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    _menuCargado = false;
                    objetosMenu ??= new();
                    ConstruirMenuMantenimientos();
                    return;
                }
            }
        }

        private static string BuildRuta(string nombreEntidad)
        {
            return $"/mantenimientos/{nombreEntidad.ToLower()}s";
        }

        private record MenuItem(string Nombre, string Ruta);
    }
}
