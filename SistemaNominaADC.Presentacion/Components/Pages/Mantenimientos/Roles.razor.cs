using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos
{
    public partial class Roles
    {
        private List<RolDTO>? listaRoles;
        private bool mostrarModal = false;
        private string nuevoRolNombre = "";

        protected override async Task OnInitializedAsync()
        {
            await CargarRoles();
        }

        private async Task CargarRoles()
        {
            listaRoles = await RolCliente.GetRoles();
        }

        private void AbrirModalCrear()
        {
            nuevoRolNombre = "";
            mostrarModal = true;
        }

        private async Task GuardarRol()
        {
            if (string.IsNullOrWhiteSpace(nuevoRolNombre)) return;

            var exito = await RolCliente.CrearRol(nuevoRolNombre);
            if (exito)
            {
                mostrarModal = false;
                await CargarRoles();
            }
        }

        private async Task ConfirmarInactivar(RolDTO rol)
        {
            var confirmado = await JS.InvokeAsync<bool>("confirm", new object[] { $"¿Está seguro que desea inactivar el rol {rol.Nombre}?" });
            if (confirmado && rol.Id != null)
            {
                var exito = await RolCliente.InactivarRol(rol.Id);
                if (exito)
                {
                    await CargarRoles();
                }
            }
        }
    }
}
