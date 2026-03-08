using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class NominaService : INominaService
{
    private readonly ApplicationDbContext _context;
    private readonly INominaCalculator _nominaCalculator;
    private readonly IFlujoEstadoService _flujoEstadoService;
    private readonly IComprobantePlanillaService _comprobantePlanillaService;
    private readonly ILogger<NominaService> _logger;

    public NominaService(
        ApplicationDbContext context,
        INominaCalculator nominaCalculator,
        IFlujoEstadoService flujoEstadoService,
        IComprobantePlanillaService comprobantePlanillaService,
        ILogger<NominaService> logger)
    {
        _context = context;
        _nominaCalculator = nominaCalculator;
        _flujoEstadoService = flujoEstadoService;
        _comprobantePlanillaService = comprobantePlanillaService;
        _logger = logger;
    }

    public Task<NominaResumenPlanillaDTO> CalcularPlanilla(int idPlanilla, string? actorUserId, IEnumerable<string>? roles = null) =>
        ProcesarPlanillaInterno(idPlanilla, WorkflowAcciones.Calcular, actorUserId, roles);

    public Task<NominaResumenPlanillaDTO> RecalcularPlanilla(int idPlanilla, string? actorUserId, IEnumerable<string>? roles = null) =>
        ProcesarPlanillaInterno(idPlanilla, WorkflowAcciones.Recalcular, actorUserId, roles);

    public async Task<bool> AprobarPlanilla(int idPlanilla, string? actorUserId, IEnumerable<string>? roles = null)
    {
        if (idPlanilla <= 0) throw new BusinessException("El id de planilla es invalido.");

        using var tx = await _context.Database.BeginTransactionAsync();

        var planilla = await _context.PlanillasEncabezado
            .FirstOrDefaultAsync(x => x.IdPlanilla == idPlanilla)
            ?? throw new NotFoundException("Planilla no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.PlanillaEncabezado, planilla.IdEstado, WorkflowAcciones.Aprobar, roles);
        planilla.IdEstado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.PlanillaEncabezado, planilla.IdEstado, WorkflowAcciones.Aprobar, roles);
        planilla.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);

        await _context.SaveChangesAsync();
        await _comprobantePlanillaService.GenerarYGuardarComprobantesPlanillaAsync(idPlanilla);

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "APROBAR_PLANILLA",
            $"Planilla {idPlanilla} aprobada.",
            planilla.IdEstado,
            actorUserId);

        await tx.CommitAsync();

        return true;
    }

    public async Task<bool> RechazarPlanilla(int idPlanilla, string? actorUserId, IEnumerable<string>? roles = null)
    {
        if (idPlanilla <= 0) throw new BusinessException("El id de planilla es invalido.");

        var planilla = await _context.PlanillasEncabezado
            .FirstOrDefaultAsync(x => x.IdPlanilla == idPlanilla)
            ?? throw new NotFoundException("Planilla no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.PlanillaEncabezado, planilla.IdEstado, WorkflowAcciones.Rechazar, roles);
        planilla.IdEstado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.PlanillaEncabezado, planilla.IdEstado, WorkflowAcciones.Rechazar, roles);
        planilla.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);

        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "RECHAZAR_PLANILLA",
            $"Planilla {idPlanilla} rechazada.",
            planilla.IdEstado,
            actorUserId);

        return true;
    }

    public async Task<NominaResumenPlanillaDTO> ObtenerResumenPlanilla(int idPlanilla)
    {
        if (idPlanilla <= 0) throw new BusinessException("El id de planilla es invalido.");

        var planilla = await _context.PlanillasEncabezado
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdPlanilla == idPlanilla)
            ?? throw new NotFoundException("Planilla no encontrada.");

        var detalles = await _context.PlanillasDetalle
            .AsNoTracking()
            .Where(x => x.IdPlanilla == idPlanilla)
            .Include(x => x.Empleado)
            .OrderBy(x => x.IdPlanillaDetalle)
            .ToListAsync();

        var idsDetalle = detalles.Select(x => x.IdPlanillaDetalle).ToList();
        var conceptos = await _context.PlanillasDetalleConcepto
            .AsNoTracking()
            .Where(x => idsDetalle.Contains(x.IdPlanillaDetalle))
            .Include(x => x.TipoConceptoNomina)
            .ToListAsync();

        var idsEmpleado = detalles.Select(d => d.IdEmpleado).Distinct().ToList();
        var asignacionesPorcentuales = await _context.EmpleadosConceptoNomina
            .AsNoTracking()
            .Where(x =>
                x.Activo &&
                x.Porcentaje.HasValue &&
                idsEmpleado.Contains(x.IdEmpleado) &&
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde.Value.Date <= planilla.PeriodoFin.Date) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value.Date >= planilla.PeriodoInicio.Date))
            .ToListAsync();

        var porcentajePorEmpleadoConcepto = asignacionesPorcentuales
            .GroupBy(x => new { x.IdEmpleado, x.IdConceptoNomina })
            .ToDictionary(
                g => (g.Key.IdEmpleado, g.Key.IdConceptoNomina),
                g => g.OrderBy(a => a.Prioridad).First().Porcentaje);

        var empleados = detalles
            .Select(detalle => new NominaCalculoEmpleadoDTO
            {
                IdEmpleado = detalle.IdEmpleado,
                NombreEmpleado = detalle.Empleado?.NombreCompleto ?? $"Empleado {detalle.IdEmpleado}",
                SalarioBase = detalle.SalarioBase,
                TotalIngresos = detalle.TotalIngresos,
                SalarioBruto = detalle.SalarioBruto,
                TotalDeducciones = detalle.TotalDeducciones,
                SalarioNeto = detalle.SalarioNeto,
                Conceptos = conceptos
                    .Where(c => c.IdPlanillaDetalle == detalle.IdPlanillaDetalle)
                    .OrderBy(c => c.IdDetalleConcepto)
                    .Select(c => new NominaConceptoAplicadoDTO
                    {
                        IdConceptoNomina = c.IdConceptoNomina,
                        CodigoConcepto = c.TipoConceptoNomina?.CodigoConcepto ?? string.Empty,
                        NombreConcepto = c.TipoConceptoNomina?.Nombre ?? $"Concepto {c.IdConceptoNomina}",
                        PorcentajeAplicado = porcentajePorEmpleadoConcepto.TryGetValue((detalle.IdEmpleado, c.IdConceptoNomina), out var porcentajeEmpleado)
                            ? porcentajeEmpleado
                            : c.TipoConceptoNomina?.ValorPorcentaje,
                        Monto = c.Monto,
                        EsIngreso = c.TipoConceptoNomina?.EsIngreso ?? false,
                        EsDeduccion = c.TipoConceptoNomina?.EsDeduccion ?? false,
                        AfectaCcss = c.TipoConceptoNomina?.AfectaCcss ?? false,
                        EsSalarioBruto = EsSalarioBruto(c.TipoConceptoNomina?.CodigoFormula, c.TipoConceptoNomina?.CodigoConcepto),
                        EsAjusteNoLaborado = EsAjusteNoLaborado(c.TipoConceptoNomina?.CodigoFormula, c.TipoConceptoNomina?.CodigoConcepto)
                    })
                    .ToList()
            })
            .ToList();

        if (empleados.Count > 0)
        {
            var idEstadoActivo = await SolicitudesWorkflowHelper.ObtenerEstadoActivoAsync(_context);
            var idEstadoAprobadoSolicitud = await SolicitudesWorkflowHelper.ObtenerEstadoAprobadoAsync(_context);

            foreach (var empleado in empleados)
            {
                var calculoConDetalle = await _nominaCalculator.CalcularEmpleado(
                    idPlanilla,
                    empleado.IdEmpleado,
                    planilla.PeriodoInicio.Date,
                    planilla.PeriodoFin.Date,
                    idEstadoActivo,
                    idEstadoAprobadoSolicitud,
                    false);

                var detallePorConcepto = calculoConDetalle.Conceptos
                    .GroupBy(c => c.IdConceptoNomina)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var concepto in empleado.Conceptos)
                {
                    if (!detallePorConcepto.TryGetValue(concepto.IdConceptoNomina, out var detalle))
                        continue;

                    concepto.Cantidad = detalle.Cantidad;
                    concepto.UnidadCantidad = detalle.UnidadCantidad;
                    concepto.Detalles = detalle.Detalles;
                }
            }
        }

        if (empleados.Count == 0)
        {
            var idEstadoActivo = await SolicitudesWorkflowHelper.ObtenerEstadoActivoAsync(_context);
            empleados = await _context.Empleados
                .AsNoTracking()
                .Where(x =>
                    x.IdEstado == idEstadoActivo &&
                    x.FechaIngreso.Date <= planilla.PeriodoFin.Date &&
                    (!x.FechaSalida.HasValue || x.FechaSalida.Value.Date >= planilla.PeriodoInicio.Date))
                .OrderBy(x => x.IdEmpleado)
                .Select(x => new NominaCalculoEmpleadoDTO
                {
                    IdEmpleado = x.IdEmpleado,
                    NombreEmpleado = x.NombreCompleto,
                    SalarioBase = 0m,
                    TotalIngresos = 0m,
                    SalarioBruto = 0m,
                    TotalDeducciones = 0m,
                    SalarioNeto = 0m
                })
                .ToListAsync();

            foreach (var emp in empleados)
                emp.Conceptos = [];
        }

        return new NominaResumenPlanillaDTO
        {
            IdPlanilla = planilla.IdPlanilla,
            Accion = "RESUMEN_PLANILLA",
            FechaProceso = DateTime.Now,
            EmpleadosProcesados = empleados.Count,
            TotalBrutoPlanilla = Redondear(empleados.Sum(x => x.SalarioBruto)),
            TotalDeduccionesPlanilla = Redondear(empleados.Sum(x => x.TotalDeducciones)),
            TotalNetoPlanilla = Redondear(empleados.Sum(x => x.SalarioNeto)),
            Empleados = empleados
        };
    }

    private async Task<NominaResumenPlanillaDTO> ProcesarPlanillaInterno(
        int idPlanilla,
        string accionFlujo,
        string? actorUserId,
        IEnumerable<string>? roles)
    {
        if (idPlanilla <= 0) throw new BusinessException("El id de planilla es invalido.");

        var planilla = await _context.PlanillasEncabezado
            .FirstOrDefaultAsync(x => x.IdPlanilla == idPlanilla)
            ?? throw new NotFoundException("Planilla no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.PlanillaEncabezado, planilla.IdEstado, accionFlujo, roles);

        if (planilla.PeriodoFin.Date < planilla.PeriodoInicio.Date)
            throw new BusinessException("El periodo de la planilla es invalido.");

        var idEstadoActivo = await SolicitudesWorkflowHelper.ObtenerEstadoActivoAsync(_context);
        var idEstadoAprobadoSolicitud = await SolicitudesWorkflowHelper.ObtenerEstadoAprobadoAsync(_context);

        var empleados = await _context.Empleados
            .AsNoTracking()
            .Where(x =>
                x.IdEstado == idEstadoActivo &&
                x.FechaIngreso.Date <= planilla.PeriodoFin.Date &&
                (!x.FechaSalida.HasValue || x.FechaSalida.Value.Date >= planilla.PeriodoInicio.Date))
            .OrderBy(x => x.IdEmpleado)
            .ToListAsync();

        if (empleados.Count == 0)
            throw new BusinessException("No hay empleados activos para procesar en el periodo.");

        using var tx = await _context.Database.BeginTransactionAsync();

        var detallesExistentes = await _context.PlanillasDetalle
            .Where(x => x.IdPlanilla == idPlanilla)
            .ToListAsync();

        if (detallesExistentes.Count > 0)
        {
            var idsDetalle = detallesExistentes.Select(x => x.IdPlanillaDetalle).ToList();
            var conceptosExistentes = await _context.PlanillasDetalleConcepto
                .Where(x => idsDetalle.Contains(x.IdPlanillaDetalle))
                .ToListAsync();

            if (conceptosExistentes.Count > 0)
                _context.PlanillasDetalleConcepto.RemoveRange(conceptosExistentes);

            _context.PlanillasDetalle.RemoveRange(detallesExistentes);
            await _context.SaveChangesAsync();
        }

        var resultados = new List<NominaCalculoEmpleadoDTO>();
        var trazaDetallada = string.Equals(accionFlujo, WorkflowAcciones.Recalcular, StringComparison.OrdinalIgnoreCase);

        if (trazaDetallada)
        {
            _logger.LogInformation(
                "{Mensaje}",
                $"\n########## RECALCULO PLANILLA INICIO ##########\nPlanilla: {idPlanilla}\nPeriodo : {planilla.PeriodoInicio.Date:yyyy-MM-dd} .. {planilla.PeriodoFin.Date:yyyy-MM-dd}\nEmpleados a procesar: {empleados.Count}\n###############################################");
        }

        foreach (var empleado in empleados)
        {
            var resultado = await _nominaCalculator.CalcularEmpleado(
                idPlanilla,
                empleado.IdEmpleado,
                planilla.PeriodoInicio.Date,
                planilla.PeriodoFin.Date,
                idEstadoActivo,
                idEstadoAprobadoSolicitud,
                trazaDetallada);

            var detalle = new PlanillaDetalle
            {
                IdPlanilla = idPlanilla,
                IdEmpleado = empleado.IdEmpleado,
                SalarioBase = resultado.SalarioBase,
                TotalIngresos = resultado.TotalIngresos,
                SalarioBruto = resultado.SalarioBruto,
                TotalDeducciones = resultado.TotalDeducciones,
                SalarioNeto = resultado.SalarioNeto,
                IdEstado = idEstadoActivo
            };

            _context.PlanillasDetalle.Add(detalle);
            await _context.SaveChangesAsync();

            if (resultado.Conceptos.Count > 0)
            {
                var conceptos = resultado.Conceptos.Select(c => new PlanillaDetalleConcepto
                {
                    IdPlanillaDetalle = detalle.IdPlanillaDetalle,
                    IdConceptoNomina = c.IdConceptoNomina,
                    Monto = c.Monto,
                    IdEstado = idEstadoActivo
                });

                _context.PlanillasDetalleConcepto.AddRange(conceptos);
                await _context.SaveChangesAsync();
            }

            resultados.Add(resultado);

            if (trazaDetallada)
            {
                _logger.LogInformation(
                    "[NOMINA-RESUMEN] Empleado={EmpleadoId} '{Nombre}' => SB={SB:N2}, BR={BR:N2}, TI={TI:N2}, TD={TD:N2}, NETO={NETO:N2}, Conceptos={Conceptos}",
                    resultado.IdEmpleado,
                    resultado.NombreEmpleado,
                    resultado.SalarioBase,
                    resultado.SalarioBruto,
                    resultado.TotalIngresos,
                    resultado.TotalDeducciones,
                    resultado.SalarioNeto,
                    resultado.Conceptos.Count);
            }
        }

        var idEstadoDestino = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.PlanillaEncabezado, planilla.IdEstado, accionFlujo, roles);
        planilla.IdEstado = idEstadoDestino;
        await _context.SaveChangesAsync();

        var nombreAccionBitacora = $"{accionFlujo}_PLANILLA";
        var resumen = new NominaResumenPlanillaDTO
        {
            IdPlanilla = idPlanilla,
            Accion = nombreAccionBitacora,
            FechaProceso = DateTime.Now,
            EmpleadosProcesados = resultados.Count,
            TotalBrutoPlanilla = Redondear(resultados.Sum(x => x.SalarioBruto)),
            TotalDeduccionesPlanilla = Redondear(resultados.Sum(x => x.TotalDeducciones)),
            TotalNetoPlanilla = Redondear(resultados.Sum(x => x.SalarioNeto)),
            Empleados = resultados
        };

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            nombreAccionBitacora,
            $"Planilla {idPlanilla}: empleados={resumen.EmpleadosProcesados}, bruto={resumen.TotalBrutoPlanilla}, deducciones={resumen.TotalDeduccionesPlanilla}, neto={resumen.TotalNetoPlanilla}",
            idEstadoDestino,
            actorUserId);

        await tx.CommitAsync();

        if (trazaDetallada)
        {
            _logger.LogInformation(
                "{Mensaje}",
                $"\n########### RECALCULO PLANILLA FIN ###########\nPlanilla: {idPlanilla}\nTotal Bruto      : {resumen.TotalBrutoPlanilla:N2}\nTotal Deducciones: {resumen.TotalDeduccionesPlanilla:N2}\nTotal Neto       : {resumen.TotalNetoPlanilla:N2}\n##############################################");
        }

        return resumen;
    }

    private static decimal Redondear(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    private static bool EsSalarioBruto(string? codigoFormula, string? codigoConcepto) =>
        string.Equals((codigoFormula ?? codigoConcepto ?? string.Empty).Trim(), "BR", StringComparison.OrdinalIgnoreCase);

    private static bool EsAjusteNoLaborado(string? codigoFormula, string? codigoConcepto)
    {
        var token = (codigoFormula ?? codigoConcepto ?? string.Empty).Trim();
        return string.Equals(token, "AUSENCIA", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(token, "PERMISO_SIN_GOCE", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(token, "INCAPACIDAD", StringComparison.OrdinalIgnoreCase);
    }
}

