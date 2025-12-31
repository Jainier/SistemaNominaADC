using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos
{
    public partial class Departamentos
    {
        [Inject] private IDepartamentoCliente DepartamentoCliente { get; set; } = null!;

        // Variables para la vista
        private List<Departamento>? listaDepartamentos;
        private List<Estado> estadosDisponibles = new();
        private Departamento departamentoActual = new();
        private bool mostrarFormulario = false;
        private string tituloFormulario = "Nuevo Departamento";

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            listaDepartamentos = await DepartamentoCliente.Lista();
        }

        // Acción: Botón "Nuevo"
        private void Crear()
        {
            departamentoActual = new Departamento();
            tituloFormulario = "Nuevo Departamento";
            mostrarFormulario = true;
        }

        // Acción: Clic en una fila (Editar)
        private void Editar(Departamento item)
        {
            departamentoActual = item;
            tituloFormulario = $"Editar: {item.Nombre}";
            mostrarFormulario = true;
        }

        // Acción: Botón "Guardar" del formulario
        private async Task Guardar()
        {
            var resultado = await DepartamentoCliente.Guardar(departamentoActual);
            if (resultado)
            {
                mostrarFormulario = false;
                await CargarDatos(); // Recargar la lista para ver cambios
            }
            else
            {
                // Aquí podrías mostrar una alerta de error
                Console.WriteLine("Error al guardar");
            }
        }

        // Acción: Botón "Cancelar"
        private void Cancelar()
        {
            mostrarFormulario = false;
            // Opcional: Recargar datos por si se modificó algo en memoria sin guardar en BD
            _ = CargarDatos();
        }
    }
}