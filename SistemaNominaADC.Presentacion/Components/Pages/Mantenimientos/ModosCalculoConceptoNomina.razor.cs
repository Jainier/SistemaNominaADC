using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class ModosCalculoConceptoNomina
{
    [Inject] private IModoCalculoConceptoNominaCliente ModoCliente { get; set; } = null!;

    private List<ModoCalculoConceptoNomina> listaModos = new();
    private ModoCalculoConceptoNomina modoActual = new();
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Modo de Cálculo";

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos() => listaModos = await ModoCliente.Lista();

    private void Crear()
    {
        modoActual = new ModoCalculoConceptoNomina();
        tituloFormulario = "Nuevo Modo de Cálculo";
        mostrarFormulario = true;
    }

    private void Editar(ModoCalculoConceptoNomina item)
    {
        modoActual = new ModoCalculoConceptoNomina
        {
            IdModoCalculoConceptoNomina = item.IdModoCalculoConceptoNomina,
            Nombre = item.Nombre,
            Descripcion = item.Descripcion,
            IdEstado = item.IdEstado
        };
        tituloFormulario = "Editar Modo de Cálculo";
        mostrarFormulario = true;
    }

    private async Task Guardar()
    {
        if (await ModoCliente.Guardar(modoActual))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private async Task Desactivar()
    {
        if (modoActual.IdModoCalculoConceptoNomina > 0 && await ModoCliente.Desactivar(modoActual.IdModoCalculoConceptoNomina))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private void Cancelar() => mostrarFormulario = false;
}
