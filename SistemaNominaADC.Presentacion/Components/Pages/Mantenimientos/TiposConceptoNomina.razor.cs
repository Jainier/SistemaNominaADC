using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class TiposConceptoNomina
{
    [Inject] private ITipoConceptoNominaCliente TipoCliente { get; set; } = null!;
    [Inject] private IModoCalculoConceptoNominaCliente ModoCliente { get; set; } = null!;
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;

    private List<TipoConceptoNomina> listaTipos = new();
    private List<ModoCalculoConceptoNomina> listaModos = new();
    private TipoConceptoNomina tipoActual = new();
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Concepto de Nómina";

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
        await CargarModos();
    }

    private async Task CargarDatos() => listaTipos = await TipoCliente.Lista();
    private async Task CargarModos()
    {
        var idsActivos = EstadoActivoFiltro.ObtenerIdsActivos(await EstadoCliente.Lista());
        listaModos = (await ModoCliente.Lista())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .ToList();
    }

    private void Crear()
    {
        tipoActual = new TipoConceptoNomina();
        tituloFormulario = "Nuevo Concepto de Nómina";
        mostrarFormulario = true;
    }

    private void Editar(TipoConceptoNomina item)
    {
        tipoActual = new TipoConceptoNomina
        {
            IdConceptoNomina = item.IdConceptoNomina,
            CodigoConcepto = item.CodigoConcepto,
            Nombre = item.Nombre,
            IdModoCalculo = item.IdModoCalculo,
            FormulaCalculo = item.FormulaCalculo,
            CodigoFormula = item.CodigoFormula,
            ValorPorcentaje = item.ValorPorcentaje,
            ValorFijo = item.ValorFijo,
            OrdenCalculo = item.OrdenCalculo,
            EsIngreso = item.EsIngreso,
            EsDeduccion = item.EsDeduccion,
            AfectaCcss = item.AfectaCcss,
            AfectaRenta = item.AfectaRenta,
            IdEstado = item.IdEstado
        };
        tituloFormulario = "Editar Concepto de Nómina";
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
        if (tipoActual.IdConceptoNomina > 0 && await TipoCliente.Desactivar(tipoActual.IdConceptoNomina))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private void OnEsIngresoChanged(ChangeEventArgs e)
    {
        var marcado = e.Value is bool b && b;
        tipoActual.EsIngreso = marcado;
        if (marcado)
            tipoActual.EsDeduccion = false;
    }

    private void OnEsDeduccionChanged(ChangeEventArgs e)
    {
        var marcado = e.Value is bool b && b;
        tipoActual.EsDeduccion = marcado;
        if (marcado)
            tipoActual.EsIngreso = false;
    }

    private void Cancelar() => mostrarFormulario = false;
}
