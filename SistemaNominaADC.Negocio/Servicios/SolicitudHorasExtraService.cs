using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class SolicitudHorasExtraService : ISolicitudHorasExtraService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificacionService _notificacionService;
    private readonly IFlujoEstadoService _flujoEstadoService;

    public SolicitudHorasExtraService(
        ApplicationDbContext context,
        INotificacionService notificacionService,
        IFlujoEstadoService flujoEstadoService)
    {
        _context = context;
        _notificacionService = notificacionService;
        _flujoEstadoService = flujoEstadoService;
    }

    public async Task<List<SolicitudHorasExtra>> HistorialAsync(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null)
    {
        var query = _context.Set<SolicitudHorasExtra>()
            .Include(x => x.Empleado)
            .ThenInclude(e => e!.Puesto)
            .Include(x => x.TipoHoraExtra)
            .Include(x => x.Estado)
            .AsQueryable();

        if (idEmpleado.HasValue && idEmpleado.Value > 0)
            query = query.Where(x => x.IdEmpleado == idEmpleado.Value);

        if (fechaDesde.HasValue)
            query = query.Where(x => x.Fecha >= fechaDesde.Value.Date);

        if (fechaHasta.HasValue)
            query = query.Where(x => x.Fecha <= fechaHasta.Value.Date);

        if (idEstado.HasValue && idEstado.Value > 0)
            query = query.Where(x => x.IdEstado == idEstado.Value);

        return await query.OrderByDescending(x => x.IdSolicitudHorasExtra).ToListAsync();
    }

    public async Task<List<string>> ObtenerAccionesDisponibles(int idSolicitud, IEnumerable<string>? roles)
    {
        var solicitud = await _context.Set<SolicitudHorasExtra>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdSolicitudHorasExtra == idSolicitud)
            ?? throw new NotFoundException("Solicitud de horas extra no encontrada.");

        if (!solicitud.IdEstado.HasValue || solicitud.IdEstado.Value <= 0)
            return new List<string>();

        return await _flujoEstadoService.ObtenerAccionesDisponiblesAsync(
            WorkflowEntidades.SolicitudHorasExtra,
            solicitud.IdEstado.Value,
            roles);
    }

    public async Task<SolicitudHorasExtra> CrearAsync(SolicitudHorasExtraCreateDTO dto, string? actorUserId)
    {
        if (dto.IdEmpleado <= 0) throw new BusinessException("El empleado es obligatorio.");
        if (dto.IdTipoHoraExtra <= 0) throw new BusinessException("El tipo de hora extra es obligatorio.");
        if (dto.CantidadHoras <= 0 || dto.CantidadHoras > 24) throw new BusinessException("La cantidad de horas es invalida.");
        if (string.IsNullOrWhiteSpace(dto.Motivo)) throw new BusinessException("El motivo es obligatorio.");

        var fecha = dto.Fecha.Date;
        var hoy = DateTime.Today;
        if (fecha > hoy.AddDays(7) || fecha < hoy.AddMonths(-2))
            throw new BusinessException("La fecha de la hora extra esta fuera del rango permitido.");

        var empleado = await _context.Empleados
            .Include(e => e.Puesto)
            .FirstOrDefaultAsync(e => e.IdEmpleado == dto.IdEmpleado)
            ?? throw new NotFoundException("Empleado no encontrado.");

        _ = await _context.TipoHoraExtras.FirstOrDefaultAsync(t => t.IdTipoHoraExtra == dto.IdTipoHoraExtra)
            ?? throw new NotFoundException("Tipo de hora extra no encontrado.");

        var idRechazado = await SolicitudesWorkflowHelper.ObtenerEstadoRechazadoAsync(_context);
        var existeDuplicado = await _context.Set<SolicitudHorasExtra>()
            .AnyAsync(x =>
                x.IdEmpleado == dto.IdEmpleado &&
                x.Fecha == fecha &&
                x.IdTipoHoraExtra == dto.IdTipoHoraExtra &&
                x.IdEstado != idRechazado);

        if (existeDuplicado)
            throw new BusinessException("Ya existe una solicitud de horas extra para ese empleado, fecha y tipo.");

        await SolicitudesConflictosService.ValidarSinConflictoConIncapacidadAsync(
            _context,
            dto.IdEmpleado,
            fecha,
            fecha,
            "La solicitud de horas extra");

        var idPendiente = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.SolicitudHorasExtra,
            null,
            WorkflowAcciones.Crear);
        var entidad = new SolicitudHorasExtra
        {
            IdEmpleado = dto.IdEmpleado,
            Fecha = fecha,
            CantidadHoras = dto.CantidadHoras,
            IdTipoHoraExtra = dto.IdTipoHoraExtra,
            Motivo = dto.Motivo.Trim(),
            IdEstado = idPendiente
        };

        _context.Set<SolicitudHorasExtra>().Add(entidad);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "HORAS_EXTRA_SOLICITADAS",
            $"SolicitudHorasExtra #{entidad.IdSolicitudHorasExtra} creada. Motivo: {dto.Motivo.Trim()}",
            idPendiente,
            actorUserId);

        await NotificarAsync(
            entidad.IdSolicitudHorasExtra,
            empleado.IdEmpleado,
            empleado.IdentityUserId,
            "Nueva solicitud de horas extra",
            $"Se registro la solicitud de horas extra #{entidad.IdSolicitudHorasExtra} en estado pendiente.");

        return await ObtenerConRelacionesAsync(entidad.IdSolicitudHorasExtra);
    }

    public async Task<SolicitudHorasExtra> ActualizarPendienteAsync(int idSolicitud, SolicitudHorasExtraCreateDTO dto, string? actorUserId)
    {
        if (dto.IdEmpleado <= 0) throw new BusinessException("El empleado es obligatorio.");
        if (dto.IdTipoHoraExtra <= 0) throw new BusinessException("El tipo de hora extra es obligatorio.");
        if (dto.CantidadHoras <= 0 || dto.CantidadHoras > 24) throw new BusinessException("La cantidad de horas es invalida.");
        if (string.IsNullOrWhiteSpace(dto.Motivo)) throw new BusinessException("El motivo es obligatorio.");

        var solicitud = await _context.Set<SolicitudHorasExtra>().FirstOrDefaultAsync(x => x.IdSolicitudHorasExtra == idSolicitud)
            ?? throw new NotFoundException("Solicitud de horas extra no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(
            WorkflowEntidades.SolicitudHorasExtra,
            solicitud.IdEstado,
            WorkflowAcciones.Editar);
        if (!solicitud.IdEmpleado.HasValue || solicitud.IdEmpleado.Value <= 0)
            throw new BusinessException("La solicitud no tiene empleado asociado.");
        if (dto.IdEmpleado != solicitud.IdEmpleado.Value)
            throw new BusinessException("No se permite cambiar el empleado en una solicitud existente.");

        var fecha = dto.Fecha.Date;
        var hoy = DateTime.Today;
        if (fecha > hoy.AddDays(7) || fecha < hoy.AddMonths(-2))
            throw new BusinessException("La fecha de la hora extra esta fuera del rango permitido.");

        _ = await _context.TipoHoraExtras.FirstOrDefaultAsync(t => t.IdTipoHoraExtra == dto.IdTipoHoraExtra)
            ?? throw new NotFoundException("Tipo de hora extra no encontrado.");

        var idRechazado = await SolicitudesWorkflowHelper.ObtenerEstadoRechazadoAsync(_context);
        var existeDuplicado = await _context.Set<SolicitudHorasExtra>()
            .AnyAsync(x =>
                x.IdSolicitudHorasExtra != idSolicitud &&
                x.IdEmpleado == dto.IdEmpleado &&
                x.Fecha == fecha &&
                x.IdTipoHoraExtra == dto.IdTipoHoraExtra &&
                x.IdEstado != idRechazado);
        if (existeDuplicado)
            throw new BusinessException("Ya existe una solicitud de horas extra para ese empleado, fecha y tipo.");

        await SolicitudesConflictosService.ValidarSinConflictoConIncapacidadAsync(
            _context,
            dto.IdEmpleado,
            fecha,
            fecha,
            "La solicitud de horas extra");

        solicitud.Fecha = fecha;
        solicitud.CantidadHoras = dto.CantidadHoras;
        solicitud.IdTipoHoraExtra = dto.IdTipoHoraExtra;
        solicitud.Motivo = dto.Motivo.Trim();
        solicitud.ComentarioAprobacion = null;
        solicitud.IdentityUserIdDecision = null;
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "HORAS_EXTRA_ACTUALIZADAS",
            $"SolicitudHorasExtra #{solicitud.IdSolicitudHorasExtra} actualizada.",
            solicitud.IdEstado,
            actorUserId);

        return await ObtenerConRelacionesAsync(solicitud.IdSolicitudHorasExtra);
    }

    public async Task<SolicitudHorasExtra> EjecutarAccionAsync(int idSolicitud, string accion, string? comentario, string? actorUserId, IEnumerable<string>? roles)
    {
        if (string.IsNullOrWhiteSpace(accion))
            throw new BusinessException("La accion es obligatoria.");

        if (string.Equals(accion.Trim(), WorkflowAcciones.Editar, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("La accion Editar se realiza desde la actualizacion del registro.");

        if (string.Equals(accion.Trim(), WorkflowAcciones.Aprobar, StringComparison.OrdinalIgnoreCase))
            return await AprobarAsync(idSolicitud, actorUserId, comentario);

        if (string.Equals(accion.Trim(), WorkflowAcciones.Rechazar, StringComparison.OrdinalIgnoreCase))
            return await RechazarAsync(idSolicitud, comentario ?? string.Empty, actorUserId);

        var solicitud = await _context.Set<SolicitudHorasExtra>().FirstOrDefaultAsync(x => x.IdSolicitudHorasExtra == idSolicitud)
            ?? throw new NotFoundException("Solicitud de horas extra no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.SolicitudHorasExtra, solicitud.IdEstado, accion, roles);
        var idDestino = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.SolicitudHorasExtra, solicitud.IdEstado, accion, roles);

        solicitud.IdEstado = idDestino;
        solicitud.ComentarioAprobacion = string.IsNullOrWhiteSpace(comentario) ? solicitud.ComentarioAprobacion : comentario.Trim();
        solicitud.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "HORAS_EXTRA_ACCION",
            $"SolicitudHorasExtra #{solicitud.IdSolicitudHorasExtra} ejecuta accion '{accion.Trim()}'.",
            idDestino,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == solicitud.IdEmpleado);
        await NotificarAsync(
            solicitud.IdSolicitudHorasExtra,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Solicitud de horas extra actualizada",
            $"La solicitud de horas extra #{solicitud.IdSolicitudHorasExtra} cambio con la accion '{accion.Trim()}'.");

        return await ObtenerConRelacionesAsync(solicitud.IdSolicitudHorasExtra);
    }

    public async Task<SolicitudHorasExtra> AprobarAsync(int idSolicitud, string? actorUserId, string? comentario)
    {
        var solicitud = await _context.Set<SolicitudHorasExtra>().FirstOrDefaultAsync(x => x.IdSolicitudHorasExtra == idSolicitud)
            ?? throw new NotFoundException("Solicitud de horas extra no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(
            WorkflowEntidades.SolicitudHorasExtra,
            solicitud.IdEstado,
            WorkflowAcciones.Aprobar);

        var idAprobado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.SolicitudHorasExtra,
            solicitud.IdEstado,
            WorkflowAcciones.Aprobar);
        solicitud.IdEstado = idAprobado;
        solicitud.ComentarioAprobacion = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
        solicitud.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "HORAS_EXTRA_APROBADAS",
            $"SolicitudHorasExtra #{solicitud.IdSolicitudHorasExtra} aprobada. {(string.IsNullOrWhiteSpace(comentario) ? string.Empty : $"Comentario: {comentario.Trim()}")}",
            idAprobado,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == solicitud.IdEmpleado);
        await NotificarAsync(
            solicitud.IdSolicitudHorasExtra,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Solicitud de horas extra aprobada",
            $"La solicitud de horas extra #{solicitud.IdSolicitudHorasExtra} fue aprobada.");

        return await ObtenerConRelacionesAsync(solicitud.IdSolicitudHorasExtra);
    }

    public async Task<SolicitudHorasExtra> RechazarAsync(int idSolicitud, string motivoRechazo, string? actorUserId)
    {
        if (string.IsNullOrWhiteSpace(motivoRechazo))
            throw new BusinessException("El motivo de rechazo es obligatorio.");

        var solicitud = await _context.Set<SolicitudHorasExtra>().FirstOrDefaultAsync(x => x.IdSolicitudHorasExtra == idSolicitud)
            ?? throw new NotFoundException("Solicitud de horas extra no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(
            WorkflowEntidades.SolicitudHorasExtra,
            solicitud.IdEstado,
            WorkflowAcciones.Rechazar);

        var idRechazado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.SolicitudHorasExtra,
            solicitud.IdEstado,
            WorkflowAcciones.Rechazar);
        solicitud.IdEstado = idRechazado;
        solicitud.ComentarioAprobacion = motivoRechazo.Trim();
        solicitud.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "HORAS_EXTRA_RECHAZADAS",
            $"SolicitudHorasExtra #{solicitud.IdSolicitudHorasExtra} rechazada. Motivo: {motivoRechazo.Trim()}",
            idRechazado,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == solicitud.IdEmpleado);
        await NotificarAsync(
            solicitud.IdSolicitudHorasExtra,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Solicitud de horas extra rechazada",
            $"La solicitud de horas extra #{solicitud.IdSolicitudHorasExtra} fue rechazada. Motivo: {motivoRechazo.Trim()}");

        return await ObtenerConRelacionesAsync(solicitud.IdSolicitudHorasExtra);
    }

    private async Task NotificarAsync(int idSolicitud, int? idEmpleadoSolicitante, string? solicitanteUserId, string titulo, string mensaje)
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
            $"/operaciones/horas-extra?id={idSolicitud}");
    }

    private async Task<SolicitudHorasExtra> ObtenerConRelacionesAsync(int idSolicitud)
    {
        return await _context.Set<SolicitudHorasExtra>()
            .Include(x => x.Empleado)
            .Include(x => x.TipoHoraExtra)
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.IdSolicitudHorasExtra == idSolicitud)
            ?? throw new NotFoundException("Solicitud de horas extra no encontrada.");
    }
}
