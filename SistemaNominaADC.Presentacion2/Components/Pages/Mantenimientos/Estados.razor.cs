using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos
{
    public partial class Estados
    {
        [Inject] private IEstadoService EstadoService { get; set; } = null!;
        [Inject] private IGrupoEstadoService GrupoService { get; set; } = null!;

        // Variables de estado para la vista
        private List<Estado>? listaEstados;
        private Estado estadoActual = new();
        private bool mostrarFormulario = false;
        private string tituloFormulario = "Nuevo Estado";

        //Variables de grupos para la vista.
        private List<GrupoEstado>? listaGrupos;
        private List<int> gruposSeleccionados = new();

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            listaEstados = await EstadoService.Lista();
            listaGrupos = await GrupoService.Lista(); 
        }

        private void AlternarGrupo(int idGrupo, object? valor)
        {
            bool seleccionado = (bool)(valor ?? false);
            if (seleccionado)
            {
                if (!gruposSeleccionados.Contains(idGrupo)) gruposSeleccionados.Add(idGrupo);
            }
            else
            {
                gruposSeleccionados.Remove(idGrupo);
            }
        }

        private void Crear()
        {
            estadoActual = new Estado {EstadoActivo = true }; 
            tituloFormulario = "Nuevo Estado";
            mostrarFormulario = true;
        }

        private async Task Editar(Estado item)
        {
            estadoActual = item;
            gruposSeleccionados = await EstadoService.ObtenerIdsGruposAsociados(item.IdEstado);
            mostrarFormulario = true;
        }
        private async Task Guardar()
        {
            if (await EstadoService.Guardar(estadoActual, gruposSeleccionados))
            {
                mostrarFormulario = false;
                gruposSeleccionados.Clear(); 
                await CargarDatos();
            }
        }
        private void Cancelar()
        {
            mostrarFormulario = false;
        }

    }
}