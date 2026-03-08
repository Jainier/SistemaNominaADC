using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Text;

namespace SistemaNominaADC.Api.Reports;

public class ComprobantePlanillaService : IComprobantePlanillaService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ComprobantePlanillaService(
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task GenerarYGuardarComprobantesPlanillaAsync(int idPlanilla)
    {
        if (idPlanilla <= 0)
            throw new BusinessException("El id de planilla es invalido.");

        var encabezado = await _context.PlanillasEncabezado
            .AsNoTracking()
            .Include(p => p.TipoPlanilla)
            .Include(p => p.Estado)
            .FirstOrDefaultAsync(p => p.IdPlanilla == idPlanilla)
            ?? throw new NotFoundException("Planilla no encontrada.");

        var detalles = await _context.PlanillasDetalle
            .Where(d => d.IdPlanilla == idPlanilla)
            .Include(d => d.Empleado)
                .ThenInclude(e => e!.Puesto)
            .ToListAsync();

        if (detalles.Count == 0)
            throw new BusinessException("No existen detalles de planilla para generar comprobantes.");

        var idsDetalle = detalles.Select(d => d.IdPlanillaDetalle).ToList();
        var conceptosDb = await _context.PlanillasDetalleConcepto
            .AsNoTracking()
            .Where(c => idsDetalle.Contains(c.IdPlanillaDetalle))
            .Include(c => c.TipoConceptoNomina)
            .OrderBy(c => c.IdDetalleConcepto)
            .ToListAsync();

        var logo = CargarLogoEmpresa();
        var ahora = DateTime.Now;

        foreach (var detalle in detalles)
        {
            var conceptos = conceptosDb
                .Where(c => c.IdPlanillaDetalle == detalle.IdPlanillaDetalle)
                .Select(c => new NominaConceptoAplicadoDTO
                {
                    IdConceptoNomina = c.IdConceptoNomina,
                    CodigoConcepto = c.TipoConceptoNomina?.CodigoConcepto ?? string.Empty,
                    NombreConcepto = c.TipoConceptoNomina?.Nombre ?? $"Concepto {c.IdConceptoNomina}",
                    PorcentajeAplicado = c.TipoConceptoNomina?.ValorPorcentaje,
                    Monto = c.Monto,
                    EsIngreso = c.TipoConceptoNomina?.EsIngreso ?? false,
                    EsDeduccion = c.TipoConceptoNomina?.EsDeduccion ?? false,
                    AfectaCcss = c.TipoConceptoNomina?.AfectaCcss ?? false,
                    EsSalarioBruto = EsSalarioBruto(c.TipoConceptoNomina?.CodigoFormula, c.TipoConceptoNomina?.CodigoConcepto),
                    EsAjusteNoLaborado = EsAjusteNoLaborado(c.TipoConceptoNomina?.CodigoFormula, c.TipoConceptoNomina?.CodigoConcepto)
                })
                .ToList();

            var empleado = detalle.Empleado;
            var salarioBaseMensual = (empleado?.SalarioBase ?? 0m) > 0m
                ? empleado!.SalarioBase
                : (empleado?.Puesto?.SalarioBase ?? 0m);

            var data = new MiPlanillaDetalleDTO
            {
                IdPlanilla = encabezado.IdPlanilla,
                IdEmpleado = detalle.IdEmpleado,
                NombreEmpleado = empleado?.NombreCompleto ?? $"Empleado {detalle.IdEmpleado}",
                Puesto = empleado?.Puesto?.Nombre ?? "N/D",
                SalarioBaseMensual = salarioBaseMensual,
                PeriodoInicio = encabezado.PeriodoInicio,
                PeriodoFin = encabezado.PeriodoFin,
                FechaPago = encabezado.FechaPago,
                TipoPlanilla = encabezado.TipoPlanilla?.Nombre ?? "N/D",
                EstadoPlanilla = encabezado.Estado?.Nombre ?? "N/D",
                Detalle = new NominaCalculoEmpleadoDTO
                {
                    IdEmpleado = detalle.IdEmpleado,
                    NombreEmpleado = empleado?.NombreCompleto ?? $"Empleado {detalle.IdEmpleado}",
                    SalarioBase = detalle.SalarioBase,
                    SalarioBruto = detalle.SalarioBruto,
                    TotalIngresos = detalle.TotalIngresos,
                    TotalDeducciones = detalle.TotalDeducciones,
                    SalarioNeto = detalle.SalarioNeto,
                    Conceptos = conceptos
                }
            };

            var pdf = MiPlanillaPdfBuilder.Generar(data, logo);
            var nombre = ConstruirNombreComprobanteEmpleado(data.NombreEmpleado, data.FechaPago);

            detalle.ComprobantePdf = pdf;
            detalle.NombreComprobantePdf = nombre;
            detalle.HashComprobantePdf = CalcularHashSha256(pdf);
            detalle.FechaGeneracionComprobantePdf = ahora;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<(byte[] contenidoZip, string nombreArchivoZip)> GenerarZipComprobantesPlanillaAsync(int idPlanilla)
    {
        if (idPlanilla <= 0)
            throw new BusinessException("El id de planilla es invalido.");

        var encabezado = await _context.PlanillasEncabezado
            .AsNoTracking()
            .Include(p => p.TipoPlanilla)
            .FirstOrDefaultAsync(p => p.IdPlanilla == idPlanilla)
            ?? throw new NotFoundException("Planilla no encontrada.");

        var detalles = await _context.PlanillasDetalle
            .AsNoTracking()
            .Where(d => d.IdPlanilla == idPlanilla)
            .Include(d => d.Empleado)
            .ToListAsync();

        if (detalles.Count == 0)
            throw new BusinessException("No existen detalles de planilla para generar comprobantes.");

        if (detalles.Any(d => d.ComprobantePdf is null || d.ComprobantePdf.Length == 0))
        {
            await GenerarYGuardarComprobantesPlanillaAsync(idPlanilla);
            detalles = await _context.PlanillasDetalle
                .AsNoTracking()
                .Where(d => d.IdPlanilla == idPlanilla)
                .Include(d => d.Empleado)
                .ToListAsync();
        }

        var detallesConPdf = detalles
            .Where(d => d.ComprobantePdf is { Length: > 0 })
            .ToList();

        if (detallesConPdf.Count == 0)
            throw new BusinessException("No se encontraron comprobantes PDF para esta planilla.");

        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var nombresUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var detalle in detallesConPdf)
            {
                var nombreBase = string.IsNullOrWhiteSpace(detalle.NombreComprobantePdf)
                    ? ConstruirNombreComprobanteEmpleado(detalle.Empleado?.NombreCompleto, encabezado.FechaPago)
                    : AsegurarExtensionPdf(SanitizarNombreArchivo(detalle.NombreComprobantePdf));

                var nombreUnico = ObtenerNombreUnicoZip(nombreBase, nombresUsados);
                var entry = zip.CreateEntry(nombreUnico, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(detalle.ComprobantePdf!);
            }
        }

        var nombreZip = ConstruirNombreZipPlanilla(encabezado.TipoPlanilla?.Nombre, encabezado.FechaPago);
        return (ms.ToArray(), nombreZip);
    }

    private byte[]? CargarLogoEmpresa()
    {
        try
        {
            var pathActual = _environment.ContentRootPath;
            var candidatos = new List<string>();
            for (var i = 0; i < 6; i++)
            {
                candidatos.Add(Path.Combine(pathActual, "wwwroot", "images", "logos", "ADCNewLogo1.png"));
                candidatos.Add(Path.Combine(pathActual, "SistemaNominaADC.Presentacion", "wwwroot", "images", "logos", "ADCNewLogo1.png"));
                var parent = Directory.GetParent(pathActual);
                if (parent is null)
                    break;
                pathActual = parent.FullName;
            }

            var logoPath = candidatos.FirstOrDefault(System.IO.File.Exists);
            return logoPath is null ? null : System.IO.File.ReadAllBytes(logoPath);
        }
        catch
        {
            return null;
        }
    }

    private static string CalcularHashSha256(byte[] contenido)
    {
        var hash = SHA256.HashData(contenido);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    private static bool EsSalarioBruto(string? codigoFormula, string? codigoConcepto) =>
        string.Equals((codigoFormula ?? codigoConcepto ?? string.Empty).Trim(), "BR", StringComparison.OrdinalIgnoreCase);

    private static bool EsAjusteNoLaborado(string? codigoFormula, string? codigoConcepto)
    {
        var token = (codigoFormula ?? codigoConcepto ?? string.Empty).Trim();
        return string.Equals(token, "AUSENCIA", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(token, "PERMISO_SIN_GOCE", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(token, "INCAPACIDAD", StringComparison.OrdinalIgnoreCase);
    }

    private static string ConstruirNombreComprobanteEmpleado(string? nombreEmpleado, DateTime fechaPago)
    {
        var empleado = SanitizarNombreArchivo(nombreEmpleado);
        if (string.IsNullOrWhiteSpace(empleado))
            empleado = "Empleado";

        return AsegurarExtensionPdf($"{empleado} - {fechaPago:dd-MM-yyyy}.pdf");
    }

    private static string ConstruirNombreZipPlanilla(string? tipoPlanilla, DateTime fechaPago)
    {
        var tipo = SanitizarNombreArchivo(tipoPlanilla);
        if (string.IsNullOrWhiteSpace(tipo))
            tipo = "Planilla";

        return $"{SanitizarNombreArchivo($"Colillas Nómina {tipo} - {fechaPago:dd-MM-yyyy}")}.zip";
    }

    private static string ObtenerNombreUnicoZip(string nombreArchivo, HashSet<string> usados)
    {
        var nombreLimpio = SanitizarNombreArchivo(nombreArchivo);
        nombreLimpio = AsegurarExtensionPdf(nombreLimpio);

        if (usados.Add(nombreLimpio))
            return nombreLimpio;

        var baseName = Path.GetFileNameWithoutExtension(nombreLimpio);
        var extension = Path.GetExtension(nombreLimpio);

        var i = 2;
        while (true)
        {
            var candidato = $"{baseName} ({i}){extension}";
            if (usados.Add(candidato))
                return candidato;
            i++;
        }
    }

    private static string AsegurarExtensionPdf(string nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo))
            return "Comprobante.pdf";

        return nombreArchivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? nombreArchivo
            : $"{nombreArchivo}.pdf";
    }

    private static string SanitizarNombreArchivo(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return string.Empty;

        var invalidos = Path.GetInvalidFileNameChars();
        var chars = valor.Trim()
            .Select(c => invalidos.Contains(c) ? '-' : c)
            .ToArray();

        var limpio = new string(chars);
        while (limpio.Contains("  "))
            limpio = limpio.Replace("  ", " ");

        return limpio.Trim(' ', '.', '-');
    }
}
