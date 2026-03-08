using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class TiposPlanilla
{
    [Inject] private ITipoPlanillaCliente TipoCliente { get; set; } = null!;

    private List<TipoPlanilla> listaTipos = new();
    private TipoPlanilla tipoActual = new();
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Tipo de Planilla";

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos() => listaTipos = await TipoCliente.Lista();

    private void Crear()
    {
        tipoActual = new TipoPlanilla
        {
            ModoCalculo = "Regular",
            AportaBaseCcss = true,
            AportaBaseRentaMensual = true
        };
        tituloFormulario = "Nuevo Tipo de Planilla";
        mostrarFormulario = true;
    }

    private void Editar(TipoPlanilla item)
    {
        tipoActual = new TipoPlanilla
        {
            IdTipoPlanilla = item.IdTipoPlanilla,
            Nombre = item.Nombre,
            Descripcion = item.Descripcion,
            ModoCalculo = item.ModoCalculo,
            AportaBaseCcss = item.AportaBaseCcss,
            AportaBaseRentaMensual = item.AportaBaseRentaMensual,
            IdEstado = item.IdEstado
        };
        tituloFormulario = "Editar Tipo de Planilla";
        mostrarFormulario = true;
    }

    private async Task Guardar()
    {
        if (await TipoCliente.Guardar(tipoActual))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private async Task Desactivar()
    {
        if (tipoActual.IdTipoPlanilla > 0 && await TipoCliente.Desactivar(tipoActual.IdTipoPlanilla))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private void Cancelar() => mostrarFormulario = false;
}
