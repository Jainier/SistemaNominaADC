using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface ISolicitudVacacionesService
{
    Task<List<SolicitudVacaciones>> HistorialAsync(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null);
    Task<List<string>> ObtenerAccionesDisponibles(int idSolicitud, IEnumerable<string>? roles);
    Task<SolicitudVacaciones> CrearAsync(SolicitudVacacionesCreateDTO dto, string? actorUserId);
    Task<SolicitudVacaciones> ActualizarPendienteAsync(int idSolicitud, SolicitudVacacionesCreateDTO dto, string? actorUserId);
    Task<SolicitudVacaciones> EjecutarAccionAsync(int idSolicitud, string accion, string? comentario, string? actorUserId, IEnumerable<string>? roles);
    Task<SolicitudVacaciones> AprobarAsync(int idSolicitud, string? actorUserId, string? comentario);
    Task<SolicitudVacaciones> RechazarAsync(int idSolicitud, string motivoRechazo, string? actorUserId);
}
