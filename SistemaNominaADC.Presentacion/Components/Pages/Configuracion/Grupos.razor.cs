using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Configuracion
{
    public partial class Grupos
    {
        [Inject] private IGrupoEstadoCliente GrupoCliente { get; set; } = null!;
        [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;

        private List<GrupoEstado>? listaGrupos;
        private List<Estado> listaEstados = new();
        private List<int> estadosSeleccionados = new();
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
            listaEstados = await EstadoCliente.Lista();
        }

        private void Crear()
        {
            grupoActual = new GrupoEstado();
            estadosSeleccionados.Clear();
            tituloFormulario = "Crear Nuevo Grupo de Estados";
            mostrarFormulario = true;
        }

        private async Task Editar(GrupoEstado item)
        {
            grupoActual = item;
            estadosSeleccionados = await GrupoCliente.ObtenerIdsEstadosAsociados(item.IdGrupoEstado);
            tituloFormulario = "Editar Grupo";
            mostrarFormulario = true;
        }

        private async Task Guardar()
        {
            if (await GrupoCliente.Guardar(grupoActual, estadosSeleccionados))
            {
                mostrarFormulario = false;
                estadosSeleccionados.Clear();
                await CargarDatos();
            }
        }

        private async Task Desactivar()
        {
            if (grupoActual.IdGrupoEstado <= 0) return;

            if (await GrupoCliente.Eliminar(grupoActual.IdGrupoEstado))
            {
                mostrarFormulario = false;
                estadosSeleccionados.Clear();
                await CargarDatos();
            }
        }

        private void AlternarEstado(int idEstado, object? valor)
        {
            bool seleccionado = valor is bool b && b;
            if (seleccionado)
            {
                if (!estadosSeleccionados.Contains(idEstado))
                    estadosSeleccionados.Add(idEstado);
                return;
            }

            estadosSeleccionados.Remove(idEstado);
        }

        private void Cancelar() => mostrarFormulario = false;
    }
}

