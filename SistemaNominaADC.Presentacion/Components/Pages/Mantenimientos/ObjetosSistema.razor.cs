using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos
{
    public partial class ObjetosSistema
    {
        [Inject] private IObjetoSistemaCliente ObjetoCliente { get; set; } = null!;
        [Inject] private IGrupoEstadoCliente GrupoCliente { get; set; } = null!;
        [Inject] private IRolCliente RolCliente { get; set; } = null!;

        private List<ObjetoSistemaDetalleDTO>? listaObjetos;
        private List<GrupoEstado>? listaGrupos;
        private List<RolDTO> listaRoles = new();
        private ObjetoSistemaCreateUpdateDTO objetoActual = new();
        private List<string> rolesSeleccionados = new();
        private bool objetoActivo = true;
        private bool mostrarFormulario = false;
        private string tituloFormulario = "";

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            listaObjetos = await ObjetoCliente.Lista();
            listaGrupos = await GrupoCliente.Lista();
            listaRoles = await RolCliente.GetRoles();
        }

        private void Crear()
        {
            objetoActual = new ObjetoSistemaCreateUpdateDTO();
            rolesSeleccionados = new List<string>();
            objetoActivo = true;
            tituloFormulario = "Configurar Nueva Entidad";
            mostrarFormulario = true;
        }

        private void Editar(ObjetoSistemaDetalleDTO item)
        {
            objetoActual = new ObjetoSistemaCreateUpdateDTO
            {
                IdObjeto = item.IdObjeto,
                NombreEntidad = item.NombreEntidad,
                IdGrupoEstado = item.IdGrupoEstado,
                Roles = item.Roles.ToList()
            };
            rolesSeleccionados = item.Roles.ToList();
            objetoActivo = rolesSeleccionados.Count > 0;
            tituloFormulario = $"Editando Configuración: {item.NombreEntidad}";
            mostrarFormulario = true;
        }

        private async Task Guardar()
        {
            objetoActual.Roles = objetoActivo ? rolesSeleccionados.ToList() : new List<string>();
            if (await ObjetoCliente.Guardar(objetoActual))
            {
                mostrarFormulario = false;
                await CargarDatos();
            }
        }

        private void Cancelar() => mostrarFormulario = false;

        private async Task Inactivar()
        {
            if (objetoActual.IdObjeto <= 0)
                return;

            if (await ObjetoCliente.Inactivar(objetoActual.IdObjeto))
            {
                mostrarFormulario = false;
                await CargarDatos();
            }
        }

        private void AlternarRol(string rol, object? value)
        {
            var isChecked = value is bool b && b;
            if (isChecked)
            {
                if (!rolesSeleccionados.Contains(rol))
                    rolesSeleccionados.Add(rol);
            }
            else
            {
                rolesSeleccionados.Remove(rol);
            }
        }
    }
}
