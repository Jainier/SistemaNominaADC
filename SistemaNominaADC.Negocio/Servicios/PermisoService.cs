using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class PermisoService : IPermisoService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificacionService _notificacionService;
    private readonly IFlujoEstadoService _flujoEstadoService;

    public PermisoService(
        ApplicationDbContext context,
        INotificacionService notificacionService,
        IFlujoEstadoService flujoEstadoService)
    {
        _context = context;
        _notificacionService = notificacionService;
        _flujoEstadoService = flujoEstadoService;
    }

    public async Task<List<Permiso>> HistorialAsync(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null)
    {
        var query = _context.Set<Permiso>()
            .Include(x => x.Empleado)
            .Include(x => x.TipoPermiso)
            .Include(x => x.Estado)
            .AsNoTracking()
            .AsQueryable();

        if (idEmpleado.HasValue && idEmpleado.Value > 0)
            query = query.Where(x => x.IdEmpleado == idEmpleado.Value);

        if (fechaDesde.HasValue)
            query = query.Where(x => x.FechaInicio >= fechaDesde.Value.Date);

        if (fechaHasta.HasValue)
            query = query.Where(x => x.FechaFin <= fechaHasta.Value.Date);

        if (idEstado.HasValue && idEstado.Value > 0)
            query = query.Where(x => x.IdEstado == idEstado.Value);

        return await query
            .OrderByDescending(x => x.IdPermiso)
            .ToListAsync();
    }

    public async Task<List<Permiso>> HistorialPorEmpleadosAsync(IReadOnlyCollection<int> idsEmpleados, int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null)
    {
        if (idsEmpleados is null || idsEmpleados.Count == 0)
            return new List<Permiso>();

        var query = _context.Set<Permiso>()
            .Include(x => x.Empleado)
            .Include(x => x.TipoPermiso)
            .Include(x => x.Estado)
            .AsNoTracking()
            .Where(x => x.IdEmpleado.HasValue && idsEmpleados.Contains(x.IdEmpleado.Value))
            .AsQueryable();

        if (idEmpleado.HasValue && idEmpleado.Value > 0)
            query = query.Where(x => x.IdEmpleado == idEmpleado.Value);

        if (fechaDesde.HasValue)
            query = query.Where(x => x.FechaInicio >= fechaDesde.Value.Date);

        if (fechaHasta.HasValue)
            query = query.Where(x => x.FechaFin <= fechaHasta.Value.Date);

        if (idEstado.HasValue && idEstado.Value > 0)
            query = query.Where(x => x.IdEstado == idEstado.Value);

        return await query
            .OrderByDescending(x => x.IdPermiso)
            .ToListAsync();
    }

    public async Task<List<string>> ObtenerAccionesDisponibles(int idPermiso, IEnumerable<string>? roles)
    {
        var permiso = await _context.Set<Permiso>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdPermiso == idPermiso)
            ?? throw new NotFoundException("Permiso no encontrado.");

        if (!permiso.IdEstado.HasValue || permiso.IdEstado.Value <= 0)
            return new List<string>();

        return await _flujoEstadoService.ObtenerAccionesDisponiblesAsync(
            WorkflowEntidades.Permiso,
            permiso.IdEstado.Value,
            roles);
    }

    public async Task<Permiso> CrearAsync(PermisoCreateDTO dto, string? actorUserId)
    {
        if (dto.IdEmpleado <= 0) throw new BusinessException("El empleado es obligatorio.");
        if (dto.IdTipoPermiso <= 0) throw new BusinessException("El tipo de permiso es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Motivo)) throw new BusinessException("El motivo es obligatorio.");

        SolicitudesWorkflowHelper.ValidarRangoFechas(dto.FechaInicio, dto.FechaFin, "Permiso");

        var empleado = await _context.Empleados
            .Include(e => e.Puesto)
            .FirstOrDefaultAsync(e => e.IdEmpleado == dto.IdEmpleado)
            ?? throw new NotFoundException("Empleado no encontrado.");

        _ = await _context.TipoPermisos.FirstOrDefaultAsync(t => t.IdTipoPermiso == dto.IdTipoPermiso)
            ?? throw new NotFoundException("Tipo de permiso no encontrado.");

        await ValidarSolapamientoConSolicitudesVacacionesAsync(dto.IdEmpleado, dto.FechaInicio, dto.FechaFin);
        await ValidarSolapamientoConPermisosAsync(dto.IdEmpleado, dto.FechaInicio, dto.FechaFin);
        await SolicitudesConflictosService.ValidarSinConflictoConIncapacidadAsync(
            _context,
            dto.IdEmpleado,
            dto.FechaInicio,
            dto.FechaFin,
            "El permiso");

        var idEstadoPendiente = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.Permiso, null, WorkflowAcciones.Crear);

        var entidad = new Permiso
        {
            IdEmpleado = dto.IdEmpleado,
            IdTipoPermiso = dto.IdTipoPermiso,
            FechaInicio = dto.FechaInicio.Date,
            FechaFin = dto.FechaFin.Date,
            Motivo = dto.Motivo.Trim(),
            IdEstado = idEstadoPendiente
        };

        _context.Set<Permiso>().Add(entidad);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "PERMISO_CREADO",
            $"Permiso #{entidad.IdPermiso} creado para empleado #{empleado.IdEmpleado}. Motivo: {entidad.Motivo}",
            idEstadoPendiente,
            actorUserId);

        await NotificarAsync(
            entidad.IdPermiso,
            empleado.IdEmpleado,
            empleado.IdentityUserId,
            "Nueva solicitud de permiso",
            $"Se registro la solicitud de permiso #{entidad.IdPermiso} en estado pendiente.");

        return await ObtenerConRelacionesAsync(entidad.IdPermiso);
    }

    public async Task<Permiso> ActualizarPendienteAsync(int idPermiso, PermisoCreateDTO dto, string? actorUserId)
    {
        if (dto.IdEmpleado <= 0) throw new BusinessException("El empleado es obligatorio.");
        if (dto.IdTipoPermiso <= 0) throw new BusinessException("El tipo de permiso es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Motivo)) throw new BusinessException("El motivo es obligatorio.");

        var permiso = await _context.Set<Permiso>().FirstOrDefaultAsync(x => x.IdPermiso == idPermiso)
            ?? throw new NotFoundException("Permiso no encontrado.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.Permiso, permiso.IdEstado, WorkflowAcciones.Editar);
        if (!permiso.IdEmpleado.HasValue || permiso.IdEmpleado.Value <= 0)
            throw new BusinessException("El permiso no tiene empleado asociado.");
        if (dto.IdEmpleado != permiso.IdEmpleado.Value)
            throw new BusinessException("No se permite cambiar el empleado en un permiso existente.");

        SolicitudesWorkflowHelper.ValidarRangoFechas(dto.FechaInicio, dto.FechaFin, "Permiso");

        _ = await _context.TipoPermisos.FirstOrDefaultAsync(t => t.IdTipoPermiso == dto.IdTipoPermiso)
            ?? throw new NotFoundException("Tipo de permiso no encontrado.");

        await ValidarSolapamientoConSolicitudesVacacionesAsync(dto.IdEmpleado, dto.FechaInicio, dto.FechaFin);
        await ValidarSolapamientoConPermisosAsync(dto.IdEmpleado, dto.FechaInicio, dto.FechaFin, idPermiso);
        await SolicitudesConflictosService.ValidarSinConflictoConIncapacidadAsync(
            _context,
            dto.IdEmpleado,
            dto.FechaInicio,
            dto.FechaFin,
            "El permiso");

        permiso.IdTipoPermiso = dto.IdTipoPermiso;
        permiso.FechaInicio = dto.FechaInicio.Date;
        permiso.FechaFin = dto.FechaFin.Date;
        permiso.Motivo = dto.Motivo.Trim();
        permiso.ComentarioAprobacion = null;
        permiso.IdentityUserIdDecision = null;
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "PERMISO_ACTUALIZADO",
            $"Permiso #{permiso.IdPermiso} actualizado. Tipo: #{permiso.IdTipoPermiso}.",
            permiso.IdEstado,
            actorUserId);

        return await ObtenerConRelacionesAsync(permiso.IdPermiso);
    }

    public async Task<Permiso> EjecutarAccionAsync(int idPermiso, string accion, string? comentario, string? actorUserId, IEnumerable<string>? roles)
    {
        if (string.IsNullOrWhiteSpace(accion))
            throw new BusinessException("La accion es obligatoria.");

        if (string.Equals(accion.Trim(), WorkflowAcciones.Editar, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("La accion Editar se realiza desde la actualizacion del registro.");

        if (string.Equals(accion.Trim(), WorkflowAcciones.Aprobar, StringComparison.OrdinalIgnoreCase))
            return await AprobarAsync(idPermiso, actorUserId, comentario);

        if (string.Equals(accion.Trim(), WorkflowAcciones.Rechazar, StringComparison.OrdinalIgnoreCase))
            return await RechazarAsync(idPermiso, comentario ?? string.Empty, actorUserId);

        var permiso = await _context.Set<Permiso>().FirstOrDefaultAsync(x => x.IdPermiso == idPermiso)
            ?? throw new NotFoundException("Permiso no encontrado.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.Permiso, permiso.IdEstado, accion, roles);
        var idDestino = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.Permiso, permiso.IdEstado, accion, roles);

        permiso.IdEstado = idDestino;
        permiso.ComentarioAprobacion = string.IsNullOrWhiteSpace(comentario) ? permiso.ComentarioAprobacion : comentario.Trim();
        permiso.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "PERMISO_ACCION",
            $"Permiso #{permiso.IdPermiso} ejecuta accion '{accion.Trim()}'.",
            idDestino,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == permiso.IdEmpleado);
        await NotificarAsync(
            permiso.IdPermiso,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Solicitud de permiso actualizada",
            $"La solicitud de permiso #{permiso.IdPermiso} cambio con la accion '{accion.Trim()}'.");

        return await ObtenerConRelacionesAsync(permiso.IdPermiso);
    }

    public async Task<Permiso> AprobarAsync(int idPermiso, string? actorUserId, string? comentario)
    {
        var permiso = await _context.Set<Permiso>().FirstOrDefaultAsync(x => x.IdPermiso == idPermiso)
            ?? throw new NotFoundException("Permiso no encontrado.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.Permiso, permiso.IdEstado, WorkflowAcciones.Aprobar);
        var idAprobado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.Permiso, permiso.IdEstado, WorkflowAcciones.Aprobar);
        permiso.IdEstado = idAprobado;
        permiso.ComentarioAprobacion = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
        permiso.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "PERMISO_APROBADO",
            $"Permiso #{permiso.IdPermiso} aprobado. {(string.IsNullOrWhiteSpace(comentario) ? string.Empty : $"Comentario: {comentario.Trim()}")}",
            idAprobado,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == permiso.IdEmpleado);
        await NotificarAsync(
            permiso.IdPermiso,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Solicitud de permiso aprobada",
            $"La solicitud de permiso #{permiso.IdPermiso} fue aprobada.");

        return await ObtenerConRelacionesAsync(permiso.IdPermiso);
    }

    public async Task<Permiso> RechazarAsync(int idPermiso, string motivoRechazo, string? actorUserId)
    {
        if (string.IsNullOrWhiteSpace(motivoRechazo))
            throw new BusinessException("El motivo de rechazo es obligatorio.");

        var permiso = await _context.Set<Permiso>().FirstOrDefaultAsync(x => x.IdPermiso == idPermiso)
            ?? throw new NotFoundException("Permiso no encontrado.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.Permiso, permiso.IdEstado, WorkflowAcciones.Rechazar);
        var idRechazado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.Permiso, permiso.IdEstado, WorkflowAcciones.Rechazar);
        permiso.IdEstado = idRechazado;
        permiso.ComentarioAprobacion = motivoRechazo.Trim();
        permiso.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "PERMISO_RECHAZADO",
            $"Permiso #{permiso.IdPermiso} rechazado. Motivo: {motivoRechazo.Trim()}",
            idRechazado,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == permiso.IdEmpleado);
        await NotificarAsync(
            permiso.IdPermiso,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Solicitud de permiso rechazada",
            $"La solicitud de permiso #{permiso.IdPermiso} fue rechazada. Motivo: {motivoRechazo.Trim()}");

        return await ObtenerConRelacionesAsync(permiso.IdPermiso);
    }

    private async Task NotificarAsync(int idPermiso, int? idEmpleadoSolicitante, string? solicitanteUserId, string titulo, string mensaje)
    {
        var destinatarios = await _notificacionService.ObtenerUserIdsPorRolesAsync(RolesSistema.RolesAprobadorGlobal);

        if (idEmpleadoSolicitante.HasValue && idEmpleadoSolicitante.Value > 0)
        {
            var idDepartamento = await _context.Empleados
                .Where(e => e.IdEmpleado == idEmpleadoSolicitante.Value)
                .Select(e => (int?)e.Puesto!.IdDepartamento)
                .FirstOrDefaultAsync();

            if (idDepartamento.HasValue && idDepartamento.Value > 0)
            {
                var jefaturasDepto = await _notificacionService.ObtenerUserIdsJefaturaDepartamentoAsync(idDepartamento.Value);
                destinatarios.AddRange(jefaturasDepto);
            }
        }

        if (!string.IsNullOrWhiteSpace(solicitanteUserId))
            destinatarios.Add(solicitanteUserId);

        await _notificacionService.EnviarAsync(
            destinatarios,
            titulo,
            mensaje,
            $"/operaciones/permisos?id={idPermiso}");
    }

    private async Task<Permiso> ObtenerConRelacionesAsync(int idPermiso)
    {
        return await _context.Set<Permiso>()
            .Include(x => x.Empleado)
            .Include(x => x.TipoPermiso)
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.IdPermiso == idPermiso)
            ?? throw new NotFoundException("Permiso no encontrado.");
    }

    private async Task ValidarSolapamientoConPermisosAsync(int idEmpleado, DateTime fechaInicio, DateTime fechaFin, int? idPermisoExcluir = null)
    {
        var idRechazado = await SolicitudesWorkflowHelper.ObtenerEstadoRechazadoAsync(_context);

        var existeSolapamiento = await _context.Set<Permiso>()
            .AnyAsync(x =>
                x.IdEmpleado == idEmpleado &&
                (!idPermisoExcluir.HasValue || x.IdPermiso != idPermisoExcluir.Value) &&
                x.IdEstado != idRechazado &&
                x.FechaInicio != null &&
                x.FechaFin != null &&
                x.FechaInicio.Value.Date <= fechaFin.Date &&
                x.FechaFin.Value.Date >= fechaInicio.Date);

        if (existeSolapamiento)
            throw new BusinessException("Ya existe un permiso en ese rango de fechas.");
    }

    private async Task ValidarSolapamientoConSolicitudesVacacionesAsync(int idEmpleado, DateTime fechaInicio, DateTime fechaFin)
    {
        var idRechazado = await SolicitudesWorkflowHelper.ObtenerEstadoRechazadoAsync(_context);

        var existeSolapamiento = await _context.Set<SolicitudVacaciones>()
            .AnyAsync(x =>
                x.IdEmpleado == idEmpleado &&
                x.IdEstado != idRechazado &&
                x.FechaInicio != null &&
                x.FechaFin != null &&
                x.FechaInicio.Value.Date <= fechaFin.Date &&
                x.FechaFin.Value.Date >= fechaInicio.Date);

        if (existeSolapamiento)
            throw new BusinessException("El permiso se solapa con una solicitud de vacaciones existente.");
    }
}
