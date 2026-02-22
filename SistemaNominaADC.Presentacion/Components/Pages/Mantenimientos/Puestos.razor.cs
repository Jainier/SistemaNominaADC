using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class Puestos
{
    [Inject] private IPuestoCliente PuestoCliente { get; set; } = null!;

    private List<Puesto> listaPuestos = new();
    private Puesto puestoActual = new() { Estado = true };
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Puesto";

    protected override async Task OnInitializedAsync() => await Cargar();

    private async Task Cargar() => listaPuestos = await PuestoCliente.Lista();
    private void Crear() { puestoActual = new Puesto { Estado = true }; tituloFormulario = "Nuevo Puesto"; mostrarFormulario = true; }
    private void Editar(Puesto item) { puestoActual = item; tituloFormulario = "Editar Puesto"; mostrarFormulario = true; }
    private async Task Guardar() { if (await PuestoCliente.Guardar(puestoActual)) { mostrarFormulario = false; await Cargar(); } }
    private async Task Desactivar() { if (puestoActual.IdPuesto > 0 && await PuestoCliente.Desactivar(puestoActual.IdPuesto)) { mostrarFormulario = false; await Cargar(); } }
    private void Cancelar() => mostrarFormulario = false;
}
