using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos
{
    public partial class Roles
    {
        [Inject] private IRolCliente RolCliente { get; set; } = default!;

        private List<RolDTO> listaRoles = new();

        private RolCreateUpdateDTO rolActual = new();
        private string? rolIdActual;
        private string tituloFormulario = string.Empty;
        private bool mostrarFormulario;
        private bool esRolSistema;
        private bool rolActivo = true;
        private bool rolActivoOriginal = true;
        private bool rolesCargados;

        protected override async Task OnInitializedAsync()
        {
            await Task.CompletedTask;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender || rolesCargados || SessionService.IsRestoring)
            {
                return;
            }

            if (!SessionService.IsAuthenticated)
            {
                await SessionService.RestoreSessionAsync();
            }

            if (!SessionService.IsAuthenticated)
            {
                return;
            }

            await CargarRoles();
            rolesCargados = true;
            StateHasChanged();
        }
        private async Task CargarRoles()
        {
            listaRoles = await RolCliente.GetRoles();
        }

        private void Crear()
        {
            rolActual = new RolCreateUpdateDTO();
            rolIdActual = null;
            esRolSistema = false;
            rolActivo = true;
            rolActivoOriginal = true;
            tituloFormulario = "Nuevo Rol";
            mostrarFormulario = true;
        }

        private void Editar(RolDTO rol)
        {
            rolIdActual = rol.Id;
            rolActual = new RolCreateUpdateDTO
            {
                Nombre = rol.Nombre
            };

            esRolSistema = rol.EsSistema;
            rolActivo = rol.Activo;
            rolActivoOriginal = rol.Activo;
            tituloFormulario = "Editar Rol";
            mostrarFormulario = true;
        }

        private async Task Guardar()
        {
            bool resultado;

            if (rolIdActual == null)
            {
                resultado = await RolCliente.CrearRol(rolActual.Nombre);
            }
            else
            {
                var rolActualizar = new RolDTO
                {
                    Id = rolIdActual,
                    Nombre = rolActual.Nombre,
                    EsSistema = esRolSistema,
                    Activo = rolActivo
                };

                resultado = await RolCliente.ActualizarRol(rolActualizar);
            }

            if (resultado)
            {
                if (rolIdActual != null && !esRolSistema && rolActivo != rolActivoOriginal)
                {
                    var cambioEstadoOk = rolActivo
                        ? await RolCliente.ActivarRol(rolIdActual)
                        : await RolCliente.InactivarRol(rolIdActual);

                    if (!cambioEstadoOk)
                        return;
                }

                mostrarFormulario = false;
                await CargarRoles();
            }
        }

        private void Cancelar()
        {
            mostrarFormulario = false;
        }

        private async Task Inactivar()
        {
            if (string.IsNullOrWhiteSpace(rolIdActual) || esRolSistema)
                return;

            if (await RolCliente.InactivarRol(rolIdActual))
            {
                mostrarFormulario = false;
                await CargarRoles();
            }
        }
    }
}
