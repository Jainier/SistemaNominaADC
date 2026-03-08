using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Servicios;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Operaciones;

public partial class Planillas
{
    [Inject] private IPlanillaEncabezadoCliente PlanillaCliente { get; set; } = null!;
    [Inject] private ITipoPlanillaCliente TipoPlanillaCliente { get; set; } = null!;
    [Inject] private ApiErrorState ApiError { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private List<PlanillaEncabezado> listaPlanillas = new();
    private List<TipoPlanilla> listaTiposPlanilla = new();
    private readonly Dictionary<int, HashSet<string>> accionesPorPlanilla = [];
    private PlanillaEncabezado planillaActual = new();
    private NominaResumenPlanillaDTO? resumenActual;
    private NominaCalculoEmpleadoDTO? empleadoSeleccionado;
    private bool mostrarDetalleConceptoBase;
    private NominaConceptoAplicadoDTO? conceptoBaseSeleccionado;
    private bool mostrarFormulario;
    private bool mostrarResumen;
    private string tituloFormulario = "Nueva Planilla";
    private DateTime? filtroFechaDesde = FiltroFechasHelper.PrimerDiaMesActual();
    private DateTime? filtroFechaHasta = FiltroFechasHelper.UltimoDiaMesActual();
    private IEnumerable<PlanillaEncabezado> ListaPlanillasFiltrada =>
        listaPlanillas.Where(CumpleFiltroFechas);
    private decimal TotalIngresosResumen => resumenActual?.Empleados.Sum(x => x.TotalIngresos) ?? 0m;
    private decimal TotalDeduccionesResumen => resumenActual?.Empleados.Sum(x => x.TotalDeducciones) ?? 0m;
    private decimal TotalNetoResumen => resumenActual?.Empleados.Sum(x => x.SalarioNeto) ?? 0m;
    private IEnumerable<NominaConceptoAplicadoDTO> ConceptosBaseCcss =>
        empleadoSeleccionado?.Conceptos
            .Where(x => x.AfectaCcss && !x.EsSalarioBruto)
        ?? Enumerable.Empty<NominaConceptoAplicadoDTO>();

    private IEnumerable<NominaConceptoAplicadoDTO> ConceptosLiquidacion =>
        empleadoSeleccionado?.Conceptos
            .Where(x =>
                (!x.AfectaCcss && !EsConceptoTecnicoVisual(x)) || x.EsSalarioBruto)
        ?? Enumerable.Empty<NominaConceptoAplicadoDTO>();

    private decimal TotalIngresosDesglose => ConceptosLiquidacion.Where(x => !x.EsDeduccion).Sum(x => x.Monto);
    private decimal TotalDeduccionesDesglose => ConceptosLiquidacion.Where(x => x.EsDeduccion).Sum(x => x.Monto);
    private decimal SalarioAPagarDesglose => Math.Max(0m, TotalIngresosDesglose - TotalDeduccionesDesglose);
    private decimal TotalBaseCcssIngresos => ConceptosBaseCcss.Where(x => !x.EsDeduccion).Sum(x => x.Monto);
    private decimal TotalBaseCcssDeducciones => ConceptosBaseCcss.Where(x => x.EsDeduccion).Sum(x => x.Monto);
    private decimal TotalBaseCcssBruto => Math.Max(0m, TotalBaseCcssIngresos - TotalBaseCcssDeducciones);

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
        await CargarTiposPlanilla();
    }

    private async Task CargarDatos()
    {
        listaPlanillas = await PlanillaCliente.Lista();
        await CargarAccionesDisponibles();
    }

    private async Task CargarTiposPlanilla()
    {
        listaTiposPlanilla = (await TipoPlanillaCliente.Lista(soloActivos: true))
            .ToList();
    }

    private async Task CargarAccionesDisponibles()
    {
        accionesPorPlanilla.Clear();

        var tareas = listaPlanillas.Select(async p =>
        {
            var acciones = await PlanillaCliente.AccionesDisponibles(p.IdPlanilla);
            accionesPorPlanilla[p.IdPlanilla] = acciones
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        });

        await Task.WhenAll(tareas);
    }

    private void Crear()
    {
        planillaActual = new PlanillaEncabezado
        {
            PeriodoInicio = DateTime.Today,
            PeriodoFin = DateTime.Today,
            FechaPago = DateTime.Today,
            IdEstado = 0
        };
        tituloFormulario = "Nueva Planilla";
        mostrarFormulario = true;
    }

    private void Editar(PlanillaEncabezado item)
    {
        planillaActual = new PlanillaEncabezado
        {
            IdPlanilla = item.IdPlanilla,
            PeriodoInicio = item.PeriodoInicio,
            PeriodoFin = item.PeriodoFin,
            FechaPago = item.FechaPago,
            PeriodoAguinaldo = item.PeriodoAguinaldo,
            IdTipoPlanilla = item.IdTipoPlanilla,
            IdEstado = item.IdEstado
        };
        tituloFormulario = "Editar Planilla";
        mostrarFormulario = true;
    }

    private async Task Guardar()
    {
        ApiError.Clear();
        if (await PlanillaCliente.Guardar(planillaActual))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private async Task Desactivar()
    {
        ApiError.Clear();
        if (planillaActual.IdPlanilla > 0 && await PlanillaCliente.Desactivar(planillaActual.IdPlanilla))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private async Task VerResumen(PlanillaEncabezado planilla)
    {
        ApiError.Clear();
        var resumen = await PlanillaCliente.Resumen(planilla.IdPlanilla);
        if (resumen is null) return;

        resumenActual = resumen;
        empleadoSeleccionado = null;
        conceptoBaseSeleccionado = null;
        mostrarDetalleConceptoBase = false;
        mostrarResumen = true;
    }

    private void SeleccionarEmpleado(NominaCalculoEmpleadoDTO empleado)
    {
        empleadoSeleccionado = empleado;
        conceptoBaseSeleccionado = null;
        mostrarDetalleConceptoBase = false;
    }

    private void SeleccionarConceptoBase(NominaConceptoAplicadoDTO concepto)
    {
        conceptoBaseSeleccionado = concepto;
        mostrarDetalleConceptoBase = true;
    }

    private void CerrarResumen()
    {
        mostrarResumen = false;
        resumenActual = null;
        empleadoSeleccionado = null;
        conceptoBaseSeleccionado = null;
        mostrarDetalleConceptoBase = false;
    }

    private bool TieneAccion(PlanillaEncabezado item, string accion)
        => accionesPorPlanilla.TryGetValue(item.IdPlanilla, out var acciones)
        && acciones.Contains(accion);

    private IEnumerable<string> AccionesWorkflow(PlanillaEncabezado item)
    {
        if (!accionesPorPlanilla.TryGetValue(item.IdPlanilla, out var acciones) || acciones.Count == 0)
            return Enumerable.Empty<string>();

        var ordenPreferido = new[] { "Editar", "Calcular", "Recalcular", "Aprobar", "Rechazar", "Desactivar" };
        var setPreferido = new HashSet<string>(ordenPreferido, StringComparer.OrdinalIgnoreCase);

        var ordenadas = ordenPreferido.Where(a => acciones.Contains(a));
        var otras = acciones
            .Where(a => !setPreferido.Contains(a))
            .OrderBy(a => a, StringComparer.OrdinalIgnoreCase);

        return ordenadas.Concat(otras);
    }

    private static string ClaseBotonAccion(string accion) => accion.Trim().ToUpperInvariant() switch
    {
        "EDITAR" => "btn-outline-secondary",
        "CALCULAR" => "btn-success",
        "RECALCULAR" => "btn-warning",
        "APROBAR" => "btn-dark",
        "RECHAZAR" => "btn-danger",
        "DESACTIVAR" => "btn-outline-danger",
        _ => "btn-outline-secondary"
    };

    private async Task EjecutarAccionWorkflow(PlanillaEncabezado planilla, string accion)
    {
        var accionNorm = (accion ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(accionNorm))
            return;

        if (EsAccionDescargarColillas(accionNorm))
        {
            await DescargarColillasPlanilla(planilla.IdPlanilla);
            return;
        }

        if (string.Equals(accionNorm, WorkflowAcciones.Editar, StringComparison.OrdinalIgnoreCase))
        {
            Editar(planilla);
            return;
        }

        ApiError.Clear();
        var resumen = await PlanillaCliente.EjecutarAccion(planilla.IdPlanilla, accionNorm);
        if (!string.IsNullOrWhiteSpace(ApiError.Message))
            return;

        if (resumen is not null)
        {
            resumenActual = resumen;
            empleadoSeleccionado = null;
            conceptoBaseSeleccionado = null;
            mostrarDetalleConceptoBase = false;
            mostrarResumen = true;
        }

        await CargarDatos();
    }

    private async Task DescargarColillasPlanilla(int idPlanilla)
    {
        ApiError.Clear();
        var archivo = await PlanillaCliente.DescargarComprobantesZip(idPlanilla);
        if (archivo is null)
            return;

        var base64 = Convert.ToBase64String(archivo.Value.contenido);
        await JS.InvokeVoidAsync(
            "adcArchivos.descargarDesdeBase64",
            archivo.Value.nombreArchivo,
            archivo.Value.contentType,
            base64);
    }

    private static bool EsMontoNegativo(NominaConceptoAplicadoDTO concepto) => concepto.EsDeduccion;

    private static string CantidadTexto(NominaConceptoAplicadoDTO concepto)
    {
        if (!concepto.Cantidad.HasValue)
            return "-";

        var unidad = string.IsNullOrWhiteSpace(concepto.UnidadCantidad) ? string.Empty : $" {concepto.UnidadCantidad}";
        return $"{concepto.Cantidad.Value:0.##}{unidad}";
    }

    private static bool EsConceptoTecnicoVisual(NominaConceptoAplicadoDTO concepto)
    {
        var codigo = (concepto.CodigoConcepto ?? string.Empty).Trim().ToUpperInvariant();
        return codigo is "TI" or "TD" or "NETO";
    }

    private void CerrarDetalleConceptoBase()
    {
        mostrarDetalleConceptoBase = false;
    }

    private static bool EsAccionDescargarColillas(string accion)
    {
        var normalizada = (accion ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("_", string.Empty);
        return string.Equals(normalizada, "DESCARGARCOLILLAS", StringComparison.OrdinalIgnoreCase);
    }

    private void LimpiarFiltrosFecha()
    {
        filtroFechaDesde = FiltroFechasHelper.PrimerDiaMesActual();
        filtroFechaHasta = FiltroFechasHelper.UltimoDiaMesActual();
    }

    private bool CumpleFiltroFechas(PlanillaEncabezado planilla)
    {
        var desde = filtroFechaDesde?.Date;
        var hasta = filtroFechaHasta?.Date;

        if (!desde.HasValue && !hasta.HasValue)
            return true;

        var inicio = planilla.PeriodoInicio.Date;
        var fin = planilla.PeriodoFin.Date;

        if (desde.HasValue && fin < desde.Value)
            return false;

        if (hasta.HasValue && inicio > hasta.Value)
            return false;

        return true;
    }

    private void Cancelar() => mostrarFormulario = false;
}
