using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Operaciones;

public partial class MisPlanillas
{
    [Inject] private IMiPlanillaCliente MiPlanillaCliente { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private ApiErrorState ApiError { get; set; } = null!;

    private List<MiPlanillaHistorialItemDTO> historial = new();
    private MiPlanillaDetalleDTO? detalleActual;
    private IEnumerable<NominaConceptoAplicadoDTO> conceptosBaseCcss = Enumerable.Empty<NominaConceptoAplicadoDTO>();
    private IEnumerable<NominaConceptoAplicadoDTO> otrosIngresos = Enumerable.Empty<NominaConceptoAplicadoDTO>();
    private IEnumerable<NominaConceptoAplicadoDTO> otrasDeducciones = Enumerable.Empty<NominaConceptoAplicadoDTO>();

    protected override async Task OnInitializedAsync()
    {
        await CargarHistorial();
    }

    private async Task CargarHistorial()
    {
        historial = await MiPlanillaCliente.Historial();
    }

    private async Task VerDetalle(int idPlanilla)
    {
        var detalle = await MiPlanillaCliente.Detalle(idPlanilla);
        if (detalle is null)
            return;

        detalleActual = detalle;
        var conceptos = detalleActual.Detalle.Conceptos ?? new List<NominaConceptoAplicadoDTO>();

        conceptosBaseCcss = conceptos
            .Where(x => x.AfectaCcss && !x.EsSalarioBruto)
            .OrderBy(OrdenConceptoCalculoBruto)
            .ThenBy(x => x.NombreConcepto)
            .ToList();

        otrosIngresos = conceptos
            .Where(x => !x.AfectaCcss && x.EsIngreso && !EsConceptoTecnicoVisual(x))
            .OrderBy(x => x.NombreConcepto)
            .ToList();

        otrasDeducciones = conceptos
            .Where(x => !x.AfectaCcss && x.EsDeduccion && !EsConceptoTecnicoVisual(x))
            .OrderBy(x => x.NombreConcepto)
            .ToList();
    }

    private async Task DescargarPdf(int idPlanilla)
    {
        var archivo = await MiPlanillaCliente.DescargarPdf(idPlanilla);
        if (!archivo.HasValue)
            return;

        var base64 = Convert.ToBase64String(archivo.Value.contenido);
        await JS.InvokeVoidAsync(
            "adcArchivos.abrirDesdeBase64",
            archivo.Value.nombreArchivo,
            archivo.Value.contentType,
            base64);
    }

    private void CerrarDetalle()
    {
        detalleActual = null;
        conceptosBaseCcss = Enumerable.Empty<NominaConceptoAplicadoDTO>();
        otrosIngresos = Enumerable.Empty<NominaConceptoAplicadoDTO>();
        otrasDeducciones = Enumerable.Empty<NominaConceptoAplicadoDTO>();
    }

    private static bool EsConceptoTecnicoVisual(NominaConceptoAplicadoDTO concepto)
    {
        var codigo = (concepto.CodigoConcepto ?? string.Empty).Trim().ToUpperInvariant();
        if (codigo is "TI" or "TD" or "NETO")
            return true;

        var nombre = (concepto.NombreConcepto ?? string.Empty).Trim().ToUpperInvariant();
        return nombre is "TOTAL INGRESOS" or "TOTAL DEDUCCIONES" or "SALARIO NETO";
    }

    private static int OrdenConceptoCalculoBruto(NominaConceptoAplicadoDTO concepto)
    {
        var codigo = (concepto.CodigoConcepto ?? string.Empty).Trim().ToUpperInvariant();
        if (codigo == "SB")
            return 0;

        return concepto.EsDeduccion ? 2 : 1;
    }
}
