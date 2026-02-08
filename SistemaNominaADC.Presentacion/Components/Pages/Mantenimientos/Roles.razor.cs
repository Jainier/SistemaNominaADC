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

        protected override async Task OnInitializedAsync()
        {
            if (!SessionService.IsAuthenticated)
                await SessionService.RestoreSessionAsync();

            await CargarRoles();
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
                    EsSistema = esRolSistema
                };

                resultado = await RolCliente.ActualizarRol(rolActualizar);
            }

            if (resultado)
            {
                mostrarFormulario = false;
                await CargarRoles();
            }
        }

        private void Cancelar()
        {
            mostrarFormulario = false;
        }
    }
}
