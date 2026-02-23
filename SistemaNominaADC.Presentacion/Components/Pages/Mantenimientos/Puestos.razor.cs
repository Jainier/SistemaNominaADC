using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class Puestos
{
    [Inject] private IPuestoCliente PuestoCliente { get; set; } = null!;
    [Inject] private IDepartamentoCliente DepartamentoCliente { get; set; } = null!;

    private List<Puesto> listaPuestos = new();
    private List<Departamento> listaDepartamentos = new();
    private Puesto puestoActual = new();
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Puesto";

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(CargarPuestos(), CargarDepartamentos());
    }

    private async Task CargarPuestos() => listaPuestos = await PuestoCliente.Lista();
    private async Task CargarDepartamentos() => listaDepartamentos = await DepartamentoCliente.Lista();
    private void Crear() { puestoActual = new Puesto(); tituloFormulario = "Nuevo Puesto"; mostrarFormulario = true; }
    private void Editar(Puesto item) { puestoActual = item; tituloFormulario = "Editar Puesto"; mostrarFormulario = true; }
    private async Task Guardar() { if (await PuestoCliente.Guardar(puestoActual)) { mostrarFormulario = false; await CargarPuestos(); } }
    private async Task Desactivar() { if (puestoActual.IdPuesto > 0 && await PuestoCliente.Desactivar(puestoActual.IdPuesto)) { mostrarFormulario = false; await CargarPuestos(); } }
    private void Cancelar() => mostrarFormulario = false;
}
