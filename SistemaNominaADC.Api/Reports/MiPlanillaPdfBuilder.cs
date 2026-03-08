using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Api.Reports;

public static class MiPlanillaPdfBuilder
{
    static MiPlanillaPdfBuilder()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] Generar(MiPlanillaDetalleDTO data, byte[]? logoBytes)
    {
        var baseCcss = data.Detalle.Conceptos
            .Where(x => x.AfectaCcss && !EsConceptoTecnicoVisual(x))
            .OrderBy(OrdenConceptoCalculoBruto)
            .ThenBy(x => x.NombreConcepto)
            .ToList();

        var ingresosNoCcss = data.Detalle.Conceptos
            .Where(x => !x.AfectaCcss && x.EsIngreso && !EsConceptoTecnicoVisual(x))
            .OrderBy(x => x.NombreConcepto)
            .ToList();

        var deduccionesNoCcss = data.Detalle.Conceptos
            .Where(x => !x.AfectaCcss && x.EsDeduccion && !EsConceptoTecnicoVisual(x))
            .OrderBy(x => x.NombreConcepto)
            .ToList();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(24);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Comprobante de Planilla").FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Planilla #{data.IdPlanilla}").FontSize(11);
                        });

                        row.ConstantItem(120).AlignRight().Height(55).Element(e =>
                        {
                            if (logoBytes is { Length: > 0 })
                                e.Image(logoBytes).FitArea();
                        });
                    });

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Element(e => RenderInfoGeneral(e, data));
                    col.Item().Element(e => RenderTablaConceptos(e, "Calculo Salario Bruto", baseCcss));
                    col.Item().Element(e => RenderTablaConceptos(e, "Otros Ingresos", ingresosNoCcss));
                    col.Item().Element(e => RenderTablaConceptos(e, "Otras Deducciones", deduccionesNoCcss));
                    col.Item().Element(e => RenderResumenFinal(e, data));
                });

                page.Footer()
                    .AlignCenter()
                    .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1))
                    .Text(x =>
                    {
                        x.Span("Documento generado por Sistema Nomina ADC - ");
                        x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    });
            });
        });

        return pdf.GeneratePdf();
    }

    private static void RenderInfoGeneral(IContainer container, MiPlanillaDetalleDTO data)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
        {
            col.Spacing(4);
            col.Item().Text($"Nombre: {data.NombreEmpleado}").SemiBold();
            col.Item().Text($"Puesto: {data.Puesto}");
            col.Item().Text($"Salario Base Mensual: {data.SalarioBaseMensual:N2}");
            col.Item().Text($"Periodo: {data.PeriodoInicio:dd/MM/yyyy} - {data.PeriodoFin:dd/MM/yyyy}");
            col.Item().Text($"Tipo de planilla: {data.TipoPlanilla}");
        });
    }

    private static void RenderTablaConceptos(IContainer container, string titulo, IReadOnlyList<NominaConceptoAplicadoDTO> conceptos)
    {
        container.Column(col =>
        {
            col.Spacing(4);
            col.Item().Text(titulo).FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(1.2f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Codigo");
                    header.Cell().Element(CellHeader).Text("Concepto");
                    header.Cell().Element(CellHeader).AlignRight().Text("Cant.");
                    header.Cell().Element(CellHeader).AlignRight().Text("%");
                    header.Cell().Element(CellHeader).AlignRight().Text("Monto");
                });

                foreach (var concepto in conceptos)
                {
                    table.Cell().Element(CellBody).Text(concepto.CodigoConcepto);
                    table.Cell().Element(CellBody).Text(concepto.NombreConcepto);
                    table.Cell().Element(CellBody).AlignRight().Text(
                        concepto.Cantidad.HasValue
                            ? $"{concepto.Cantidad:0.##}{(string.IsNullOrWhiteSpace(concepto.UnidadCantidad) ? string.Empty : $" {concepto.UnidadCantidad}")}"
                            : "-");
                    table.Cell().Element(CellBody).AlignRight().Text(
                        concepto.PorcentajeAplicado.HasValue ? $"{(concepto.PorcentajeAplicado.Value * 100m):0.####}%" : "-");
                    table.Cell().Element(CellBody).AlignRight().Text($"{(concepto.EsDeduccion ? "-" : "+")}{concepto.Monto:N2}");
                }

                var total = conceptos.Where(c => !c.EsDeduccion).Sum(c => c.Monto) - conceptos.Where(c => c.EsDeduccion).Sum(c => c.Monto);
                table.Cell().ColumnSpan(4).Element(CellFooter).AlignRight().Text("Total");
                table.Cell().Element(CellFooter).AlignRight().Text($"{total:N2}");
            });
        });
    }

    private static void RenderResumenFinal(IContainer container, MiPlanillaDetalleDTO data)
    {
        container.Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
        {
            col.Spacing(4);
            col.Item().Text("Resumen del Pago").FontSize(12).SemiBold();
            col.Item().Text($"Salario Base: {data.Detalle.SalarioBase:N2}");
            col.Item().Text($"Salario Bruto: {data.Detalle.SalarioBruto:N2}");
            col.Item().Text($"Total Ingresos: {data.Detalle.TotalIngresos:N2}");
            col.Item().Text($"Total Deducciones: {data.Detalle.TotalDeducciones:N2}");
            col.Item().Text($"Salario Neto: {data.Detalle.SalarioNeto:N2}").SemiBold().FontSize(12);
        });
    }

    private static IContainer CellHeader(IContainer container) =>
        container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4)
            .PaddingHorizontal(4)
            .DefaultTextStyle(x => x.SemiBold());

    private static IContainer CellBody(IContainer container) =>
        container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten4)
            .PaddingVertical(3)
            .PaddingHorizontal(4);

    private static IContainer CellFooter(IContainer container) =>
        container
            .BorderTop(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingTop(5)
            .PaddingHorizontal(4)
            .DefaultTextStyle(x => x.SemiBold());

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
