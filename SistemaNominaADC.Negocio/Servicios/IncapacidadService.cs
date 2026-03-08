using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class IncapacidadService : IIncapacidadService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificacionService _notificacionService;
    private readonly IFlujoEstadoService _flujoEstadoService;
    private readonly string _rutaAdjuntosEscritura;
    private readonly List<string> _rutasAdjuntosLectura;

    public IncapacidadService(
        ApplicationDbContext context,
        INotificacionService notificacionService,
        IFlujoEstadoService flujoEstadoService)
    {
        _context = context;
        _notificacionService = notificacionService;
        _flujoEstadoService = flujoEstadoService;
        _rutaAdjuntosEscritura = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Incapacidades");

        var rutaBin = Path.Combine(AppContext.BaseDirectory, "Uploads", "Incapacidades");
        _rutasAdjuntosLectura = new List<string> { _rutaAdjuntosEscritura };
        if (!string.Equals(rutaBin, _rutaAdjuntosEscritura, StringComparison.OrdinalIgnoreCase))
            _rutasAdjuntosLectura.Add(rutaBin);
    }

    public async Task<List<Incapacidad>> HistorialAsync(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null)
    {
        var query = _context.Incapacidades
            .Include(x => x.Empleado)
            .ThenInclude(e => e!.Puesto)
            .Include(x => x.TipoIncapacidad)
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

        return await query.OrderByDescending(x => x.IdIncapacidad).ToListAsync();
    }

    public async Task<List<string>> ObtenerAccionesDisponibles(int idIncapacidad, IEnumerable<string>? roles)
    {
        var incapacidad = await _context.Incapacidades
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdIncapacidad == idIncapacidad)
            ?? throw new NotFoundException("Incapacidad no encontrada.");

        if (!incapacidad.IdEstado.HasValue || incapacidad.IdEstado.Value <= 0)
            return new List<string>();

        return await _flujoEstadoService.ObtenerAccionesDisponiblesAsync(
            WorkflowEntidades.Incapacidad,
            incapacidad.IdEstado.Value,
            roles);
    }

    public async Task<Incapacidad> CrearAsync(IncapacidadCreateDTO dto, string? actorUserId, byte[]? archivoBytes, string? nombreArchivo)
    {

        if (dto.IdEmpleado <= 0)
            throw new BusinessException("El empleado es obligatorio.");

        if (dto.IdTipoIncapacidad <= 0)
            throw new BusinessException("El tipo de incapacidad es obligatorio.");

        SolicitudesWorkflowHelper.ValidarRangoFechas(dto.FechaInicio, dto.FechaFin, "Incapacidad");

        var empleado = await _context.Empleados
            .Include(e => e.Puesto)
            .FirstOrDefaultAsync(e => e.IdEmpleado == dto.IdEmpleado)
            ?? throw new NotFoundException("Empleado no encontrado.");

        var tipoExiste = await _context.TipoIncapacidades.AnyAsync(x => x.IdTipoIncapacidad == dto.IdTipoIncapacidad);
        if (!tipoExiste)
            throw new NotFoundException("Tipo de incapacidad no encontrado.");

        await ValidarSolapamientoConIncapacidadesAsync(dto.IdEmpleado, dto.FechaInicio, dto.FechaFin);
        await SolicitudesConflictosService.ValidarIncapacidadSinConflictosOperativosAsync(_context, dto.IdEmpleado, dto.FechaInicio, dto.FechaFin);

        var idRegistrada = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.Incapacidad,
            null,
            WorkflowAcciones.Crear);

        var entidad = new Incapacidad
        {
            IdEmpleado = dto.IdEmpleado,
            FechaInicio = dto.FechaInicio.Date,
            FechaFin = dto.FechaFin.Date,
            IdTipoIncapacidad = dto.IdTipoIncapacidad,
            MontoCubierto = dto.MontoCubierto,
            ComentarioSolicitud = string.IsNullOrWhiteSpace(dto.ComentarioSolicitud) ? null : dto.ComentarioSolicitud.Trim(),
            IdEstado = idRegistrada,
            FechaRegistro = DateTime.Now
        };

        if (archivoBytes is { Length: > 0 } && !string.IsNullOrWhiteSpace(nombreArchivo))
        {
            var (nombreGuardado, rutaRelativa) = await GuardarAdjuntoAsync(archivoBytes, nombreArchivo);
            entidad.NombreDocumento = nombreGuardado;
            entidad.RutaDocumento = rutaRelativa;
        }

        _context.Incapacidades.Add(entidad);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "INCAPACIDAD_REGISTRADA",
            $"Incapacidad #{entidad.IdIncapacidad} registrada para empleado #{empleado.IdEmpleado}.",
            idRegistrada,
            actorUserId);

        await NotificarAsync(
            entidad.IdIncapacidad,
            empleado.IdEmpleado,
            empleado.IdentityUserId,
            "Nueva incapacidad registrada",
            $"Se registró la incapacidad #{entidad.IdIncapacidad} en estado registrada.");

        return await ObtenerConRelacionesAsync(entidad.IdIncapacidad);
    }

    public async Task<Incapacidad> ActualizarRegistradaAsync(int idIncapacidad, IncapacidadCreateDTO dto, string? actorUserId, byte[]? archivoBytes, string? nombreArchivo)
    {
        if (dto.IdEmpleado <= 0)
            throw new BusinessException("El empleado es obligatorio.");
        if (dto.IdTipoIncapacidad <= 0)
            throw new BusinessException("El tipo de incapacidad es obligatorio.");

        var incapacidad = await _context.Incapacidades.FirstOrDefaultAsync(x => x.IdIncapacidad == idIncapacidad)
            ?? throw new NotFoundException("Incapacidad no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(
            WorkflowEntidades.Incapacidad,
            incapacidad.IdEstado,
            WorkflowAcciones.Editar);
        if (!incapacidad.IdEmpleado.HasValue || incapacidad.IdEmpleado.Value <= 0)
            throw new BusinessException("La incapacidad no tiene empleado asociado.");
        if (dto.IdEmpleado != incapacidad.IdEmpleado.Value)
            throw new BusinessException("No se permite cambiar el empleado en una incapacidad existente.");

        SolicitudesWorkflowHelper.ValidarRangoFechas(dto.FechaInicio, dto.FechaFin, "Incapacidad");

        var tipoExiste = await _context.TipoIncapacidades.AnyAsync(x => x.IdTipoIncapacidad == dto.IdTipoIncapacidad);
        if (!tipoExiste)
            throw new NotFoundException("Tipo de incapacidad no encontrado.");

        await ValidarSolapamientoConIncapacidadesAsync(dto.IdEmpleado, dto.FechaInicio, dto.FechaFin, idIncapacidad);
        await SolicitudesConflictosService.ValidarIncapacidadSinConflictosOperativosAsync(_context, dto.IdEmpleado, dto.FechaInicio, dto.FechaFin);

        incapacidad.FechaInicio = dto.FechaInicio.Date;
        incapacidad.FechaFin = dto.FechaFin.Date;
        incapacidad.IdTipoIncapacidad = dto.IdTipoIncapacidad;
        incapacidad.MontoCubierto = dto.MontoCubierto;
        incapacidad.ComentarioSolicitud = string.IsNullOrWhiteSpace(dto.ComentarioSolicitud) ? null : dto.ComentarioSolicitud.Trim();
        incapacidad.ComentarioAprobacion = null;
        incapacidad.ComentarioRevision = null;
        incapacidad.IdentityUserIdDecision = null;

        if (archivoBytes is { Length: > 0 } && !string.IsNullOrWhiteSpace(nombreArchivo))
        {
            var (nombreGuardado, rutaRelativa) = await GuardarAdjuntoAsync(archivoBytes, nombreArchivo);
            incapacidad.NombreDocumento = nombreGuardado;
            incapacidad.RutaDocumento = rutaRelativa;
        }

        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "INCAPACIDAD_ACTUALIZADA",
            $"Incapacidad #{incapacidad.IdIncapacidad} actualizada.",
            incapacidad.IdEstado,
            actorUserId);

        return await ObtenerConRelacionesAsync(incapacidad.IdIncapacidad);
    }

    public async Task<Incapacidad> EjecutarAccionAsync(int idIncapacidad, string accion, string? comentario, string? actorUserId, IEnumerable<string>? roles)
    {
        if (string.IsNullOrWhiteSpace(accion))
            throw new BusinessException("La accion es obligatoria.");

        if (string.Equals(accion.Trim(), WorkflowAcciones.Editar, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("La accion Editar se realiza desde la actualizacion del registro.");

        if (string.Equals(accion.Trim(), WorkflowAcciones.Aprobar, StringComparison.OrdinalIgnoreCase))
            return await ValidarAsync(idIncapacidad, comentario, actorUserId);

        if (string.Equals(accion.Trim(), WorkflowAcciones.Rechazar, StringComparison.OrdinalIgnoreCase))
            return await RechazarAsync(idIncapacidad, comentario ?? string.Empty, actorUserId);

        var incapacidad = await _context.Incapacidades.FirstOrDefaultAsync(x => x.IdIncapacidad == idIncapacidad)
            ?? throw new NotFoundException("Incapacidad no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(WorkflowEntidades.Incapacidad, incapacidad.IdEstado, accion, roles);
        var idDestino = await _flujoEstadoService.ObtenerEstadoDestinoAsync(WorkflowEntidades.Incapacidad, incapacidad.IdEstado, accion, roles);

        incapacidad.IdEstado = idDestino;
        incapacidad.ComentarioAprobacion = string.IsNullOrWhiteSpace(comentario) ? incapacidad.ComentarioAprobacion : comentario.Trim();
        incapacidad.ComentarioRevision = string.IsNullOrWhiteSpace(comentario) ? incapacidad.ComentarioRevision : comentario.Trim();
        incapacidad.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "INCAPACIDAD_ACCION",
            $"Incapacidad #{incapacidad.IdIncapacidad} ejecuta accion '{accion.Trim()}'.",
            idDestino,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == incapacidad.IdEmpleado);
        await NotificarAsync(
            incapacidad.IdIncapacidad,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Incapacidad actualizada",
            $"La incapacidad #{incapacidad.IdIncapacidad} cambio con la accion '{accion.Trim()}'.");

        return await ObtenerConRelacionesAsync(incapacidad.IdIncapacidad);
    }

    public async Task<Incapacidad> ValidarAsync(int idIncapacidad, string? comentario, string? actorUserId)
    {
        var incapacidad = await _context.Incapacidades.FirstOrDefaultAsync(x => x.IdIncapacidad == idIncapacidad)
            ?? throw new NotFoundException("Incapacidad no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(
            WorkflowEntidades.Incapacidad,
            incapacidad.IdEstado,
            WorkflowAcciones.Aprobar);

        var idValidada = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.Incapacidad,
            incapacidad.IdEstado,
            WorkflowAcciones.Aprobar);
        incapacidad.IdEstado = idValidada;
        incapacidad.ComentarioAprobacion = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
        incapacidad.ComentarioRevision = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
        incapacidad.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "INCAPACIDAD_VALIDADA",
            $"Incapacidad #{incapacidad.IdIncapacidad} validada. {(string.IsNullOrWhiteSpace(comentario) ? string.Empty : $"Comentario: {comentario.Trim()}")}",
            idValidada,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == incapacidad.IdEmpleado);
        await NotificarAsync(
            incapacidad.IdIncapacidad,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Incapacidad validada",
            $"La incapacidad #{incapacidad.IdIncapacidad} fue validada.");

        return await ObtenerConRelacionesAsync(incapacidad.IdIncapacidad);
    }

    public async Task<Incapacidad> RechazarAsync(int idIncapacidad, string motivoRechazo, string? actorUserId)
    {
        if (string.IsNullOrWhiteSpace(motivoRechazo))
            throw new BusinessException("El motivo de rechazo es obligatorio.");

        var incapacidad = await _context.Incapacidades.FirstOrDefaultAsync(x => x.IdIncapacidad == idIncapacidad)
            ?? throw new NotFoundException("Incapacidad no encontrada.");

        await _flujoEstadoService.ValidarTransicionAsync(
            WorkflowEntidades.Incapacidad,
            incapacidad.IdEstado,
            WorkflowAcciones.Rechazar);

        var idRechazada = await _flujoEstadoService.ObtenerEstadoDestinoAsync(
            WorkflowEntidades.Incapacidad,
            incapacidad.IdEstado,
            WorkflowAcciones.Rechazar);
        incapacidad.IdEstado = idRechazada;
        incapacidad.ComentarioAprobacion = motivoRechazo.Trim();
        incapacidad.ComentarioRevision = motivoRechazo.Trim();
        incapacidad.IdentityUserIdDecision = await SolicitudesWorkflowHelper.ResolverUsuarioDecisionAsync(_context, actorUserId);
        await _context.SaveChangesAsync();

        await SolicitudesWorkflowHelper.RegistrarBitacoraAsync(
            _context,
            "INCAPACIDAD_RECHAZADA",
            $"Incapacidad #{incapacidad.IdIncapacidad} rechazada. Motivo: {motivoRechazo.Trim()}",
            idRechazada,
            actorUserId);

        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == incapacidad.IdEmpleado);
        await NotificarAsync(
            incapacidad.IdIncapacidad,
            empleado?.IdEmpleado,
            empleado?.IdentityUserId,
            "Incapacidad rechazada",
            $"La incapacidad #{incapacidad.IdIncapacidad} fue rechazada. Motivo: {motivoRechazo.Trim()}");

        return await ObtenerConRelacionesAsync(incapacidad.IdIncapacidad);
    }

    public async Task<(byte[] contenido, string nombreArchivo, string contentType)?> ObtenerAdjuntoAsync(int idIncapacidad)
    {
        var incapacidad = await _context.Incapacidades
            .FirstOrDefaultAsync(x => x.IdIncapacidad == idIncapacidad);

        if (incapacidad is null || string.IsNullOrWhiteSpace(incapacidad.RutaDocumento) || string.IsNullOrWhiteSpace(incapacidad.NombreDocumento))
            return null;

        var rutaAbsoluta = _rutasAdjuntosLectura
            .Select(rutaBase => Path.Combine(rutaBase, incapacidad.RutaDocumento))
            .FirstOrDefault(File.Exists);
        if (string.IsNullOrWhiteSpace(rutaAbsoluta))
            return null;

        var bytes = await File.ReadAllBytesAsync(rutaAbsoluta);
        var ext = Path.GetExtension(incapacidad.NombreDocumento).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        return (bytes, incapacidad.NombreDocumento, contentType);
    }

    private async Task<(string nombreGuardado, string rutaRelativa)> GuardarAdjuntoAsync(byte[] archivoBytes, string nombreArchivoOriginal)
    {
        Directory.CreateDirectory(_rutaAdjuntosEscritura);

        var nombreLimpio = Path.GetFileName(nombreArchivoOriginal.Trim());
        var nombreGuardado = $"{Guid.NewGuid():N}_{nombreLimpio}";
        var rutaRelativa = nombreGuardado;
        var rutaAbsoluta = Path.Combine(_rutaAdjuntosEscritura, rutaRelativa);
        await File.WriteAllBytesAsync(rutaAbsoluta, archivoBytes);

        return (nombreLimpio, rutaRelativa);
    }

    private async Task<Incapacidad> ObtenerConRelacionesAsync(int idIncapacidad)
    {
        return await _context.Incapacidades
            .Include(x => x.Empleado)
            .Include(x => x.TipoIncapacidad)
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.IdIncapacidad == idIncapacidad)
            ?? throw new NotFoundException("Incapacidad no encontrada.");
    }

    private async Task ValidarSolapamientoConIncapacidadesAsync(int idEmpleado, DateTime fechaInicio, DateTime fechaFin, int? idIncapacidadExcluir = null)
    {
        var idRechazado = await SolicitudesWorkflowHelper.ObtenerEstadoRechazadoAsync(_context);

        var existeSolapamiento = await _context.Incapacidades
            .AnyAsync(x =>
                x.IdEmpleado == idEmpleado &&
                (!idIncapacidadExcluir.HasValue || x.IdIncapacidad != idIncapacidadExcluir.Value) &&
                x.IdEstado != idRechazado &&
                x.FechaInicio != null &&
                x.FechaFin != null &&
                x.FechaInicio.Value.Date <= fechaFin.Date &&
                x.FechaFin.Value.Date >= fechaInicio.Date);

        if (existeSolapamiento)
            throw new BusinessException("Ya existe una incapacidad en ese rango.");
    }

    private async Task NotificarAsync(int idIncapacidad, int? idEmpleadoSolicitante, string? solicitanteUserId, string titulo, string mensaje)
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
            $"/operaciones/incapacidades?id={idIncapacidad}");
    }
}
