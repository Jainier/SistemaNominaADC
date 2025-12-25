using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos
{
    public partial class Estados
    {
        [Inject] private IEstadoService EstadoService { get; set; } = null!;

        // Variables de estado para la vista
        private List<Estado>? listaEstados;
        private Estado estadoActual = new();
        private bool mostrarFormulario = false;
        private string tituloFormulario = "Nuevo Estado";

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            listaEstados = await EstadoService.Lista();
        }

        private void Crear()
        {
            estadoActual = new Estado {EstadoActivo = true }; // Valor por defecto
            tituloFormulario = "Nuevo Estado";
            mostrarFormulario = true;
        }

        private void Editar(Estado item)
        {
            estadoActual = item;
            tituloFormulario = $"Editar Estado: {item.Nombre}";
            mostrarFormulario = true;
        }

        private async Task Guardar()
        {
            var resultado = await EstadoService.Guardar(estadoActual);
            if (resultado)
            {
                mostrarFormulario = false;
                await CargarDatos();
            }
            else
            {
                // Aquí podrías implementar una notificación de error
                Console.WriteLine("Error al intentar guardar el estado.");
            }
        }

        private void Cancelar()
        {
            mostrarFormulario = false;
        }

    }
}