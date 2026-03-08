using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class TiposPlanillaConcepto
{
    [Inject] private ITipoPlanillaConceptoCliente Cliente { get; set; } = null!;
    [Inject] private ITipoPlanillaCliente TipoPlanillaCliente { get; set; } = null!;
    [Inject] private ITipoConceptoNominaCliente TipoConceptoCliente { get; set; } = null!;
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;
    [Inject] private ApiErrorState ApiError { get; set; } = null!;

    private List<TipoPlanillaConcepto> lista = [];
    private List<TipoPlanilla> tiposPlanilla = [];
    private List<TipoConceptoNomina> conceptosNomina = [];
    private TipoPlanillaConcepto actual = new();
    private bool mostrarFormulario;
    private bool esEdicion;
    private string tituloFormulario = "Nuevo Concepto por Tipo de Planilla";

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos()
    {
        var idsActivos = EstadoActivoFiltro.ObtenerIdsActivos(await EstadoCliente.Lista());
        lista = await Cliente.Lista();
        tiposPlanilla = (await TipoPlanillaCliente.Lista())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .OrderBy(x => x.Nombre)
            .ToList();
        conceptosNomina = (await TipoConceptoCliente.Lista())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .OrderBy(x => x.Nombre)
            .ToList();
    }

    private void Crear()
    {
        actual = new TipoPlanillaConcepto
        {
            Activo = true,
            Prioridad = 0
        };
        esEdicion = false;
        tituloFormulario = "Nuevo Concepto por Tipo de Planilla";
        mostrarFormulario = true;
    }

    private void Editar(TipoPlanillaConcepto item)
    {
        actual = new TipoPlanillaConcepto
        {
            IdTipoPlanilla = item.IdTipoPlanilla,
            IdConceptoNomina = item.IdConceptoNomina,
            Activo = item.Activo,
            Obligatorio = item.Obligatorio,
            PermiteMontoManual = item.PermiteMontoManual,
            Prioridad = item.Prioridad
        };

        esEdicion = true;
        tituloFormulario = "Editar Concepto por Tipo de Planilla";
        mostrarFormulario = true;
    }

    private async Task Guardar()
    {
        ApiError.Clear();
        if (await Cliente.Guardar(actual))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private async Task Desactivar()
    {
        ApiError.Clear();
        if (esEdicion && await Cliente.Desactivar(actual.IdTipoPlanilla, actual.IdConceptoNomina))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private void Cancelar() => mostrarFormulario = false;
}

