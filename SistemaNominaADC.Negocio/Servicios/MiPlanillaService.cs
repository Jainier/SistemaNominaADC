using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class MiPlanillaService : IMiPlanillaService
{
    private readonly ApplicationDbContext _context;
    private readonly INominaService _nominaService;

    public MiPlanillaService(ApplicationDbContext context, INominaService nominaService)
    {
        _context = context;
        _nominaService = nominaService;
    }

    public async Task<List<MiPlanillaHistorialItemDTO>> HistorialPorEmpleadoAsync(int idEmpleado)
    {
        if (idEmpleado <= 0)
            throw new BusinessException("El empleado es invalido.");

        return await _context.PlanillasDetalle
            .AsNoTracking()
            .Where(d =>
                d.IdEmpleado == idEmpleado &&
                d.Planilla != null &&
                d.Planilla.Estado != null &&
                d.Planilla.Estado.Codigo == EstadoCodigosSistema.Aprobado)
            .Include(d => d.Planilla)
                .ThenInclude(p => p!.TipoPlanilla)
            .Include(d => d.Planilla)
                .ThenInclude(p => p!.Estado)
            .OrderByDescending(d => d.Planilla!.PeriodoFin)
            .ThenByDescending(d => d.Planilla!.IdPlanilla)
            .Select(d => new MiPlanillaHistorialItemDTO
            {
                IdPlanilla = d.IdPlanilla,
                PeriodoInicio = d.Planilla!.PeriodoInicio,
                PeriodoFin = d.Planilla!.PeriodoFin,
                FechaPago = d.Planilla!.FechaPago,
                TipoPlanilla = d.Planilla.TipoPlanilla != null
                    ? d.Planilla.TipoPlanilla.Nombre
                    : "N/D",
                EstadoPlanilla = d.Planilla.Estado != null
                    ? (d.Planilla.Estado.Nombre ?? "N/D")
                    : "N/D",
                SalarioBase = d.SalarioBase,
                SalarioBruto = d.SalarioBruto,
                TotalIngresos = d.TotalIngresos,
                TotalDeducciones = d.TotalDeducciones,
                SalarioNeto = d.SalarioNeto
            })
            .ToListAsync();
    }

    public async Task<MiPlanillaDetalleDTO> ObtenerDetallePorEmpleadoAsync(int idEmpleado, int idPlanilla)
    {
        if (idEmpleado <= 0)
            throw new BusinessException("El empleado es invalido.");
        if (idPlanilla <= 0)
            throw new BusinessException("La planilla es invalida.");

        var encabezado = await _context.PlanillasEncabezado
            .AsNoTracking()
            .Include(p => p.TipoPlanilla)
            .Include(p => p.Estado)
            .FirstOrDefaultAsync(p =>
                p.IdPlanilla == idPlanilla &&
                p.Estado != null &&
                p.Estado.Codigo == EstadoCodigosSistema.Aprobado)
            ?? throw new NotFoundException("Planilla no encontrada o no aprobada.");

        var resumen = await _nominaService.ObtenerResumenPlanilla(idPlanilla);
        var detalleEmpleado = resumen.Empleados.FirstOrDefault(e => e.IdEmpleado == idEmpleado)
            ?? throw new NotFoundException("No existe detalle de planilla para el empleado solicitado.");
        var empleado = await _context.Empleados
            .AsNoTracking()
            .Include(e => e.Puesto)
            .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado)
            ?? throw new NotFoundException("Empleado no encontrado.");

        var salarioBaseMensual = empleado.SalarioBase > 0m
            ? empleado.SalarioBase
            : (empleado.Puesto?.SalarioBase ?? 0m);

        return new MiPlanillaDetalleDTO
        {
            IdPlanilla = idPlanilla,
            IdEmpleado = idEmpleado,
            NombreEmpleado = detalleEmpleado.NombreEmpleado,
            Puesto = empleado.Puesto?.Nombre ?? "N/D",
            SalarioBaseMensual = salarioBaseMensual,
            PeriodoInicio = encabezado.PeriodoInicio,
            PeriodoFin = encabezado.PeriodoFin,
            FechaPago = encabezado.FechaPago,
            TipoPlanilla = encabezado.TipoPlanilla?.Nombre ?? "N/D",
            EstadoPlanilla = encabezado.Estado?.Nombre ?? "N/D",
            Detalle = detalleEmpleado
        };
    }
}
