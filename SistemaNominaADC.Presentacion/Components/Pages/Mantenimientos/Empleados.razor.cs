using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class Empleados
{
    [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = null!;
    private List<Empleado> listaEmpleados = new();
    private Empleado empleadoActual = new() { Estado = true, FechaIngreso = DateTime.Today };
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Empleado";

    protected override async Task OnInitializedAsync() => await Cargar();

    private async Task Cargar() => listaEmpleados = await EmpleadoCliente.Lista();
    private void Crear() { empleadoActual = new Empleado { Estado = true, FechaIngreso = DateTime.Today }; tituloFormulario = "Nuevo Empleado"; mostrarFormulario = true; }
    private void Editar(Empleado item) { empleadoActual = item; tituloFormulario = "Editar Empleado"; mostrarFormulario = true; }
    private async Task Guardar() { if (await EmpleadoCliente.Guardar(empleadoActual)) { mostrarFormulario = false; await Cargar(); } }
    private async Task Desactivar() { if (empleadoActual.IdEmpleado > 0 && await EmpleadoCliente.Desactivar(empleadoActual.IdEmpleado)) { mostrarFormulario = false; await Cargar(); } }
    private void Cancelar() => mostrarFormulario = false;
}
