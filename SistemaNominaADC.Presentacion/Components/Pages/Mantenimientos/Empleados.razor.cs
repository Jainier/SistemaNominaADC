using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class Empleados
{
    [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = null!;
    [Inject] private IPuestoCliente PuestoCliente { get; set; } = null!;
    private List<Empleado> listaEmpleados = new();
    private List<Puesto> listaPuestos = new();
    private Empleado empleadoActual = new() { FechaIngreso = DateTime.Today };
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Empleado";

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(CargarEmpleados(), CargarPuestos());
    }

    private async Task CargarEmpleados() => listaEmpleados = await EmpleadoCliente.Lista();
    private async Task CargarPuestos() => listaPuestos = await PuestoCliente.Lista();
    private void Crear() { empleadoActual = new Empleado { FechaIngreso = DateTime.Today }; tituloFormulario = "Nuevo Empleado"; mostrarFormulario = true; }
    private void Editar(Empleado item) { empleadoActual = item; tituloFormulario = "Editar Empleado"; mostrarFormulario = true; }
    private async Task Guardar() { if (await EmpleadoCliente.Guardar(empleadoActual)) { mostrarFormulario = false; await CargarEmpleados(); } }
    private async Task Desactivar() { if (empleadoActual.IdEmpleado > 0 && await EmpleadoCliente.Desactivar(empleadoActual.IdEmpleado)) { mostrarFormulario = false; await CargarEmpleados(); } }
    private void Cancelar() => mostrarFormulario = false;
}
