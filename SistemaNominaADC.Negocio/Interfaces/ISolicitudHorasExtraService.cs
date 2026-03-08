using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface ISolicitudHorasExtraService
{
    Task<List<SolicitudHorasExtra>> HistorialAsync(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null);
    Task<List<string>> ObtenerAccionesDisponibles(int idSolicitud, IEnumerable<string>? roles);
    Task<SolicitudHorasExtra> CrearAsync(SolicitudHorasExtraCreateDTO dto, string? actorUserId);
    Task<SolicitudHorasExtra> ActualizarPendienteAsync(int idSolicitud, SolicitudHorasExtraCreateDTO dto, string? actorUserId);
    Task<SolicitudHorasExtra> EjecutarAccionAsync(int idSolicitud, string accion, string? comentario, string? actorUserId, IEnumerable<string>? roles);
    Task<SolicitudHorasExtra> AprobarAsync(int idSolicitud, string? actorUserId, string? comentario);
    Task<SolicitudHorasExtra> RechazarAsync(int idSolicitud, string motivoRechazo, string? actorUserId);
}
