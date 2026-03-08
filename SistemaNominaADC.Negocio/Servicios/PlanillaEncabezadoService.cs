using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class PlanillaEncabezadoService : IPlanillaEncabezadoService
{
    private readonly ApplicationDbContext _context;
    private readonly IFlujoEstadoService _flujoEstadoService;

    public PlanillaEncabezadoService(ApplicationDbContext context, IFlujoEstadoService flujoEstadoService)
    {
        _context = context;
        _flujoEstadoService = flujoEstadoService;
    }

    public Task<List<PlanillaEncabezado>> Lista() =>
        _context.PlanillasEncabezado
            .Include(x => x.TipoPlanilla)
            .Include(x => x.Estado)
            .OrderByDescending(x => x.PeriodoInicio)
            .ToListAsync();

    public async Task<PlanillaEncabezado> Obtener(int id) =>
        await _context.PlanillasEncabezado
            .Include(x => x.TipoPlanilla)
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.IdPlanilla == id)
        ?? throw new NotFoundException("Planilla no encontrada.");

    public async Task<PlanillaEncabezado> Crear(PlanillaEncabezado modelo)
    {
        await Validar(modelo, 0);
        var idEstadoNulo = await EstadoSistemaHelper.ObtenerIdEstadoPorCodigoAsync(_context, EstadoCodigosSistema.Nulo);
        modelo.IdEstado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.PlanillaEncabezado,
            idEstadoNulo,
            WorkflowAcciones.Crear);
        _context.PlanillasEncabezado.Add(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task<bool> Actualizar(PlanillaEncabezado modelo)
    {
        var actual = await _context.PlanillasEncabezado
            .FirstOrDefaultAsync(x => x.IdPlanilla == modelo.IdPlanilla)
            ?? throw new NotFoundException("Planilla no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.PlanillaEncabezado, actual.IdEstado, WorkflowAcciones.Editar);
        await Validar(modelo, modelo.IdPlanilla);

        actual.PeriodoInicio = modelo.PeriodoInicio;
        actual.PeriodoFin = modelo.PeriodoFin;
        actual.PeriodoAguinaldo = modelo.PeriodoAguinaldo;
        actual.FechaPago = modelo.FechaPago;
        actual.IdTipoPlanilla = modelo.IdTipoPlanilla;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Desactivar(int id)
    {
        var actual = await _context.PlanillasEncabezado
            .FirstOrDefaultAsync(x => x.IdPlanilla == id)
            ?? throw new NotFoundException("Planilla no encontrada.");

        actual.IdEstado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.PlanillaEncabezado, actual.IdEstado, WorkflowAcciones.Desactivar);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EjecutarAccionAsync(int idPlanilla, string accion, IEnumerable<string>? roles)
    {
        if (string.IsNullOrWhiteSpace(accion))
            throw new BusinessException("La accion es obligatoria.");

        var planilla = await _context.PlanillasEncabezado
            .FirstOrDefaultAsync(x => x.IdPlanilla == idPlanilla)
            ?? throw new NotFoundException("Planilla no encontrada.");

        planilla.IdEstado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.PlanillaEncabezado,
            planilla.IdEstado,
            accion.Trim(),
            roles);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<string>> ObtenerAccionesDisponibles(int idPlanilla, IEnumerable<string>? roles)
    {
        var planilla = await _context.PlanillasEncabezado
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdPlanilla == idPlanilla)
            ?? throw new NotFoundException("Planilla no encontrada.");

        return await _flujoEstadoService.ObtenerAccionesDisponiblesAsync(WorkflowEntidades.PlanillaEncabezado, planilla.IdEstado, roles);
    }

    private async Task Validar(PlanillaEncabezado modelo, int id)
    {
        if (modelo.PeriodoInicio == default) throw new BusinessException("El periodo inicio es obligatorio.");
        if (modelo.PeriodoFin == default) throw new BusinessException("El periodo fin es obligatorio.");
        if (modelo.FechaPago == default) throw new BusinessException("La fecha de pago es obligatoria.");
        if (modelo.PeriodoFin < modelo.PeriodoInicio) throw new BusinessException("El periodo fin no puede ser menor que el periodo inicio.");
        if (modelo.IdTipoPlanilla <= 0) throw new BusinessException("El tipo de planilla es obligatorio.");
        if (modelo.PeriodoAguinaldo.HasValue && modelo.PeriodoAguinaldo.Value < 0) throw new BusinessException("El periodo aguinaldo no puede ser negativo.");

        if (!await _context.TiposPlanilla.AnyAsync(x => x.IdTipoPlanilla == modelo.IdTipoPlanilla))
            throw new NotFoundException("Tipo de planilla no encontrado.");

        var idsEstadosRechazados = await _context.FlujosEstado
            .Where(f =>
                f.Entidad == WorkflowEntidades.PlanillaEncabezado &&
                f.Accion == WorkflowAcciones.Rechazar)
            .Select(f => f.IdEstadoDestino)
            .Distinct()
            .ToListAsync();

        var existe = await _context.PlanillasEncabezado.AnyAsync(x =>
            x.IdPlanilla != id &&
            x.IdTipoPlanilla == modelo.IdTipoPlanilla &&
            x.PeriodoInicio == modelo.PeriodoInicio &&
            x.PeriodoFin == modelo.PeriodoFin &&
            !idsEstadosRechazados.Contains(x.IdEstado));

        if (existe) throw new BusinessException("Ya existe una planilla para ese periodo y tipo de planilla.");

    }
}
