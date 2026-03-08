using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class SolicitudVacacionesService : ISolicitudVacacionesService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificacionService _notificacionService;
    private readonly IFlujoEstadoService _flujoEstadoService;

    public SolicitudVacacionesService(
        ApplicationDbContext context,
        INotificacionService notificacionService,
        IFlujoEstadoService flujoEstadoService)
    {
        _context = context;
        _notificacionService = notificacionService;
        _flujoEstadoService = flujoEstadoService;
    }

    public async Task<List<SolicitudVacaciones>> HistorialAsync(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null)
    {
        var query = _context.Set<SolicitudVacaciones>()
            .Include(x => x.Empleado)
            .ThenInclude(e => e!.Puesto)
            .Include(x => x.Estado)
            .AsQueryable();

        if (idEmpleado.HasValue && idEmpleado.Value > 0)
            query = query.Where(x => x.IdEmpleado == idEmpleado.Value);

        if (fechaDesde.HasValue)
            query = query.Where(x => x.FechaInicio >= fechaDesde.Value.Date);

        if (fechaHasta.HasValue)
            query = query.Where(x => x.FechaFin <= fechaHasta.Value.Date);

        if (idEstado.HasValue && idEstado.Value > 0)
            query = query.Where(x => x.IdEstado == idEstado.Value);

        return await query.OrderByDescending(x => x.IdSolicitudVacaciones).ToListAsync();
    }

    public async Task<List<string>> ObtenerAccionesDisponibles(int idSolicitud, IEnumerable<string>? roles)
    {
        var solicitud = await _context.Set<SolicitudVacaciones>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdSolicitudVacaciones == idSolicitud)
            ?? throw new NotFoundException("Solicitud de vacaciones no encontrada.");

        if (!solicitud.IdEstado.HasValue || solicitud.IdEstado.Value <= 0)
            return new List<string>();

        return await _flujoEstadoService.ObtenerAccionesDisponiblesAsync(
            WorkflowEntidades.SolicitudVacaciones,
            solicitud.IdEstado.Value,
            roles);
    }

    public async Task<SolicitudVacaciones> CrearAsync(SolicitudVacacionesCreateDTO dto, string? actorUserId)
    {
        if (dto.IdEmpleado <= 0)
            throw new BusinessException("El empleado es obligatorio.");

        SolicitudesWorkflowHelper.ValidarRangoFechas(dto.FechaInicio, dto.FechaFin, "Solicitud de vacaciones");
        var diasSolicitados = SolicitudesWorkflowHelper.CalcularDiasInclusivos(dto.FechaInicio, dto.FechaFin);

        var empleado = await _context.Empleados
            .Include(e => e.Puesto)
            .FirstOrDefaultAsync(e => e.IdEmpleado == dto.IdEmpleado)
            ?? throw new NotFoundException("Empleado no encontrado.");

        var saldo = await ObtenerOCrearSaldoVacacionesAsync(empleado);
        if ((saldo.DiasRestantes ?? 0) < diasSolicitados)
            throw new BusinessException("El saldo de vacaciones es insuficiente para esta solicitud.");

        await ValidarSolapamientoConPermisosAsync(dto.IdEmpleado, dto.FechaInicio, dto.FechaFin);
        await ValidarSolapamientoConVacacionesAsync(dto.IdEmpleado, dto.FechaInicio, dto.FechaFin);
        await SolicitudesConflictosService.ValidarSinConflictoConIncapacidadAsync(
            _context,
            dto.IdEmpleado,
            dto.FechaInicio,
            dto.FechaFin,
            "La solicitud de vacaciones");

        var idEstadoPendiente = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.SolicitudVacaciones,
            null,
            WorkflowAcciones.Crear);

        var entidad = new SolicitudVacaciones
        {
            IdEmpleado = dto.IdEmpleado,
            CantidadDias = diasSolicitados,
            FechaInicio = dto.FechaInicio.Date,
            FechaFin = dto.FechaFin.Date,
            IdEstado = idEstadoPendiente,
            ComentarioSolicitud = string.IsNullOrWhiteSpace(dto.ComentarioSolicitud) ? null : dto.ComentarioSolicitud.Trim()
        };

        saldo.DiasRestantes = (saldo.DiasRestantes ?? 0) - diasSolicitados;
        _context.Set<SolicitudVacaciones>().Add(entidad);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "VACACIONES_SOLICITADAS",
            $"SolicitudVacaciones #{entidad.IdSolicitudVacaciones} creada para empleado #{empleado.IdEmpleado}. Dias: {diasSolicitados}.",
            idEstadoPendiente,
            actorUserId);

        await NotificarAsync(
            entidad.IdSolicitudVacaciones,
            empleado.IdEmpleado,
            empleado.IdentityUserId,
            "Nueva solicitud de vacaciones",
            $"Se registro la solicitud de vacaciones #{entidad.IdSolicitudVacaciones} en estado pendiente.");

        return await ObtenerConRelacionesAsync(entidad.IdSolicitudVacaciones);
    }

    public async Task<SolicitudVacaciones> ActualizarPendienteAsync(int idSolicitud, SolicitudVacacionesCreateDTO dto, string? actorUserId)
    {
        if (dto.IdEmpleado <= 0)
            throw new BusinessException("El empleado es obligatorio.");

        var solicitud = await _context.Set<SolicitudVacaciones>()
            .FirstOrDefaultAsync(x => x.IdSolicitudVacaciones == idSolicitud)
            ?? throw new NotFoundException("Solicitud de vacaciones no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(
            WorkflowEntidades.SolicitudVacaciones,
            solicitud.IdEstado,
            WorkflowAcciones.Editar);

        if (!solicitud.IdEmpleado.HasValue || solicitud.IdEmpleado.Value <= 0)
            throw new BusinessException("La solicitud no tiene empleado asociado.");
        if (dto.IdEmpleado != solicitud.IdEmpleado.Value)
            throw new BusinessException("No se permite cambiar el empleado en una solicitud existente.");

        SolicitudesWorkflowHelper.ValidarRangoFechas(dto.FechaInicio, dto.FechaFin, "Solicitud de vacaciones");
        var diasSolicitadosNuevos = SolicitudesWorkflowHelper.CalcularDiasInclusivos(dto.FechaInicio, dto.FechaFin);

        var idEmpleadoAnterior = solicitud.IdEmpleado.Value;
        var diasAnteriores = solicitud.CantidadDias ?? 0;

        var empleadoDestino = await _context.Empleados
            .Include(e => e.Puesto)
            .FirstOrDefaultAsync(e => e.IdEmpleado == dto.IdEmpleado)
            ?? throw new NotFoundException("Empleado no encontrado.");

        await ValidarSolapamientoConPermisosAsync(dto.IdEmpleado, dto.FechaInicio, dto.FechaFin);
        await ValidarSolapamientoConVacacionesAsync(dto.IdEmpleado, dto.FechaInicio, dto.FechaFin, idSolicitud);
        await SolicitudesConflictosService.ValidarSinConflictoConIncapacidadAsync(
            _context,
            dto.IdEmpleado,
            dto.FechaInicio,
            dto.FechaFin,
            "La solicitud de vacaciones");

        if (idEmpleadoAnterior == dto.IdEmpleado)
        {
            var saldo = await ObtenerOCrearSaldoVacacionesAsync(empleadoDestino);
            saldo.DiasRestantes = (saldo.DiasRestantes ?? 0) + diasAnteriores;

            if ((saldo.DiasRestantes ?? 0) < diasSolicitadosNuevos)
                throw new BusinessException("El saldo de vacaciones es insuficiente para la actualizacion.");

            saldo.DiasRestantes = (saldo.DiasRestantes ?? 0) - diasSolicitadosNuevos;
        }
        else
        {
            var empleadoAnterior = await _context.Empleados
                .Include(e => e.Puesto)
                .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleadoAnterior)
                ?? throw new NotFoundException("Empleado original no encontrado.");

            var saldoAnterior = await ObtenerOCrearSaldoVacacionesAsync(empleadoAnterior);
            saldoAnterior.DiasRestantes = (saldoAnterior.DiasRestantes ?? 0) + diasAnteriores;

            var saldoNuevo = await ObtenerOCrearSaldoVacacionesAsync(empleadoDestino);
            if ((saldoNuevo.DiasRestantes ?? 0) < diasSolicitadosNuevos)
                throw new BusinessException("El saldo de vacaciones del empleado destino es insuficiente.");

            saldoNuevo.DiasRestantes = (saldoNuevo.DiasRestantes ?? 0) - diasSolicitadosNuevos;
        }

        solicitud.IdEmpleado = dto.IdEmpleado;
        solicitud.CantidadDias = diasSolicitadosNuevos;
        solicitud.FechaInicio = dto.FechaInicio.Date;
        solicitud.FechaFin = dto.FechaFin.Date;
        solicitud.ComentarioSolicitud = string.IsNullOrWhiteSpace(dto.ComentarioSolicitud) ? null : dto.ComentarioSolicitud.Trim();
        solicitud.ComentarioAprobacion = null;
        solicitud.IdentityUserIdDecision = null;

        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "VACACIONES_ACTUALIZADAS",
            $"SolicitudVacaciones #{solicitud.IdSolicitudVacaciones} actualizada. Empleado: #{dto.IdEmpleado}. Dias: {diasSolicitadosNuevos}.",
            solicitud.IdEstado,
            actorUserId);

        return await ObtenerConRelacionesAsync(solicitud.IdSolicitudVacaciones);
    }

    public async Task<SolicitudVacaciones> EjecutarAccionAsync(int idSolicitud, string accion, string? comentario, string? actorUserId, IEnumerable<string>? roles)
    {
        if (string.IsNullOrWhiteSpace(accion))
            throw new BusinessException("La accion es obligatoria.");

        if (string.Equals(accion.Trim(), WorkflowAcciones.Editar, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("La accion Editar se realiza desde la actualizacion del registro.");

        if (string.Equals(accion.Trim(), WorkflowAcciones.Aprobar, StringComparison.OrdinalIgnoreCase))
            return await AprobarAsync(idSolicitud, actorUserId, comentario);

        if (string.Equals(accion.Trim(), WorkflowAcciones.Rechazar, StringComparison.OrdinalIgnoreCase))
            return await RechazarAsync(idSolicitud, comentario ?? string.Empty, actorUserId);

        var solicitud = await _context.Set<SolicitudVacaciones>().FirstOrDefaultAsync(x => x.IdSolicitudVacaciones == idSolicitud)
            ?? throw new NotFoundException("Solicitud de vacaciones no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.SolicitudVacaciones, solicitud.IdEstado, accion, roles);
        var idDestino = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.SolicitudVacaciones, solicitud.IdEstado, accion, roles);

        solicitud.IdEstado = idDestino;
        solicitud.ComentarioAprobacion = string.IsNullOrWhiteSpace(comentario) ? solicitud.ComentarioAprobacion : comentario.Trim();
        solicitud.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "VACACIONES_ACCION",
            $"SolicitudVacaciones #{solicitud.IdSolicitudVacaciones} ejecuta accion '{accion.Trim()}'.",
            idDestino,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == solicitud.IdEmpleado);
        await NotificarAsync(
            solicitud.IdSolicitudVacaciones,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Solicitud de vacaciones actualizada",
            $"La solicitud de vacaciones #{solicitud.IdSolicitudVacaciones} cambio con la accion '{accion.Trim()}'.");

        return await ObtenerConRelacionesAsync(solicitud.IdSolicitudVacaciones);
    }

    public async Task<SolicitudVacaciones> AprobarAsync(int idSolicitud, string? actorUserId, string? comentario)
    {
        var solicitud = await _context.Set<SolicitudVacaciones>().FirstOrDefaultAsync(x => x.IdSolicitudVacaciones == idSolicitud)
            ?? throw new NotFoundException("Solicitud de vacaciones no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(
            WorkflowEntidades.SolicitudVacaciones,
            solicitud.IdEstado,
            WorkflowAcciones.Aprobar);

        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == solicitud.IdEmpleado)
            ?? throw new NotFoundException("Empleado no encontrado.");

        var idAprobado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.SolicitudVacaciones,
            solicitud.IdEstado,
            WorkflowAcciones.Aprobar);
        solicitud.IdEstado = idAprobado;
        solicitud.ComentarioAprobacion = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
        solicitud.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "VACACIONES_APROBADAS",
            $"SolicitudVacaciones #{solicitud.IdSolicitudVacaciones} aprobada. {(string.IsNullOrWhiteSpace(comentario) ? string.Empty : $"Comentario: {comentario.Trim()}")}",
            idAprobado,
            actorUserId);

        await NotificarAsync(
            solicitud.IdSolicitudVacaciones,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Solicitud de vacaciones aprobada",
            $"La solicitud de vacaciones #{solicitud.IdSolicitudVacaciones} fue aprobada.");

        return await ObtenerConRelacionesAsync(solicitud.IdSolicitudVacaciones);
    }

    public async Task<SolicitudVacaciones> RechazarAsync(int idSolicitud, string motivoRechazo, string? actorUserId)
    {
        if (string.IsNullOrWhiteSpace(motivoRechazo))
            throw new BusinessException("El motivo de rechazo es obligatorio.");

        var solicitud = await _context.Set<SolicitudVacaciones>().FirstOrDefaultAsync(x => x.IdSolicitudVacaciones == idSolicitud)
            ?? throw new NotFoundException("Solicitud de vacaciones no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(
            WorkflowEntidades.SolicitudVacaciones,
            solicitud.IdEstado,
            WorkflowAcciones.Rechazar);

        var idRechazado = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.SolicitudVacaciones,
            solicitud.IdEstado,
            WorkflowAcciones.Rechazar);
        solicitud.IdEstado = idRechazado;
        solicitud.ComentarioAprobacion = motivoRechazo.Trim();
        solicitud.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);

        if (solicitud.IdEmpleado.HasValue && solicitud.IdEmpleado.Value > 0)
        {
            var empleadoSolicitud = await _context.Empleados
                .FirstOrDefaultAsync(e => e.IdEmpleado == solicitud.IdEmpleado.Value)
                ?? throw new NotFoundException("Empleado no encontrado.");
            var saldo = await ObtenerOCrearSaldoVacacionesAsync(empleadoSolicitud);
            saldo.DiasRestantes = (saldo.DiasRestantes ?? 0) + (solicitud.CantidadDias ?? 0);
        }

        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "VACACIONES_RECHAZADAS",
            $"SolicitudVacaciones #{solicitud.IdSolicitudVacaciones} rechazada. Motivo: {motivoRechazo.Trim()}",
            idRechazado,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == solicitud.IdEmpleado);
        await NotificarAsync(
            solicitud.IdSolicitudVacaciones,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Solicitud de vacaciones rechazada",
            $"La solicitud de vacaciones #{solicitud.IdSolicitudVacaciones} fue rechazada. Motivo: {motivoRechazo.Trim()}");

        return await ObtenerConRelacionesAsync(solicitud.IdSolicitudVacaciones);
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
            $"/operaciones/vacaciones?id={idSolicitud}");
    }

    private async Task<SolicitudVacaciones> ObtenerConRelacionesAsync(int idSolicitud)
    {
        return await _context.Set<SolicitudVacaciones>()
            .Include(x => x.Empleado)
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.IdSolicitudVacaciones == idSolicitud)
            ?? throw new NotFoundException("Solicitud de vacaciones no encontrada.");
    }

    private async Task ValidarSolapamientoConPermisosAsync(int idEmpleado, DateTime fechaInicio, DateTime fechaFin)
    {
        var idRechazado = await SolicitudesWorkflowHelper.ObtenerEstadoRechazadoAsync(_context);

        var existeSolapamiento = await _context.Set<Permiso>()
            .AnyAsync(x =>
                x.IdEmpleado == idEmpleado &&
                x.IdEstado != idRechazado &&
                x.FechaInicio != null &&
                x.FechaFin != null &&
                x.FechaInicio.Value.Date <= fechaFin.Date &&
                x.FechaFin.Value.Date >= fechaInicio.Date);

        if (existeSolapamiento)
            throw new BusinessException("La solicitud de vacaciones se solapa con un permiso existente.");
    }

    private async Task ValidarSolapamientoConVacacionesAsync(int idEmpleado, DateTime fechaInicio, DateTime fechaFin, int? idSolicitudExcluir = null)
    {
        var idRechazado = await SolicitudesWorkflowHelper.ObtenerEstadoRechazadoAsync(_context);

        var existeSolapamiento = await _context.Set<SolicitudVacaciones>()
            .AnyAsync(x =>
                x.IdEmpleado == idEmpleado &&
                (!idSolicitudExcluir.HasValue || x.IdSolicitudVacaciones != idSolicitudExcluir.Value) &&
                x.IdEstado != idRechazado &&
                x.FechaInicio != null &&
                x.FechaFin != null &&
                x.FechaInicio.Value.Date <= fechaFin.Date &&
                x.FechaFin.Value.Date >= fechaInicio.Date);

        if (existeSolapamiento)
            throw new BusinessException("Ya existe una solicitud de vacaciones en ese rango.");
    }

    private async Task<Vacaciones> ObtenerOCrearSaldoVacacionesAsync(Empleado empleado)
    {
        var saldo = await _context.Set<Vacaciones>()
            .FirstOrDefaultAsync(x => x.IdEmpleado == empleado.IdEmpleado);
        if (saldo is not null)
            return saldo;

        var idEstadoActivo = await SolicitudesWorkflowHelper.ObtenerEstadoActivoAsync(_context);
        saldo = new Vacaciones
        {
            IdEmpleado = empleado.IdEmpleado,
            DiasRestantes = CalcularDiasAcumuladosPorAntiguedad(empleado.FechaIngreso, DateTime.Today),
            IdEstado = idEstadoActivo
        };

        _context.Set<Vacaciones>().Add(saldo);
        await _context.SaveChangesAsync();
        return saldo;
    }

    private static int CalcularDiasAcumuladosPorAntiguedad(DateTime fechaIngreso, DateTime fechaCorte)
    {
        var ingreso = fechaIngreso.Date;
        var corte = fechaCorte.Date;
        if (ingreso > corte)
            return 0;

        var mesesCompletos = (corte.Year - ingreso.Year) * 12 + (corte.Month - ingreso.Month);
        if (corte.Day < ingreso.Day)
            mesesCompletos--;

        return Math.Max(0, mesesCompletos);
    }
}
