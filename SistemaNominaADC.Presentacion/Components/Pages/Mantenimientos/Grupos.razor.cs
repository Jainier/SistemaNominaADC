using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos
{
    public partial class Grupos
    {
        [Inject] private IGrupoEstadoCliente GrupoCliente { get; set; } = null!;

        private List<GrupoEstado>? listaGrupos;
        private GrupoEstado grupoActual = new();
        private bool mostrarFormulario = false;
        private string tituloFormulario = "";

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            listaGrupos = await GrupoCliente.Lista();
        }

        private void Crear()
        {
            grupoActual = new GrupoEstado();
            tituloFormulario = "Crear Nuevo Grupo de Estados";
            mostrarFormulario = true;
        }

        private void Editar(GrupoEstado item)
        {
            grupoActual = item;
            tituloFormulario = "Editar Grupo";
            mostrarFormulario = true;
        }

        private async Task Guardar()
        {
            if (await GrupoCliente.Guardar(grupoActual))
            {
                mostrarFormulario = false;
                await CargarDatos();
            }
        }

        private void Cancelar() => mostrarFormulario = false;
    }
}
