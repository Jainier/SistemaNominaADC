using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IPermisoService
{
    Task<List<Permiso>> HistorialAsync(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null);
    Task<List<Permiso>> HistorialPorEmpleadosAsync(IReadOnlyCollection<int> idsEmpleados, int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null);
    Task<List<string>> ObtenerAccionesDisponibles(int idPermiso, IEnumerable<string>? roles);
    Task<Permiso> CrearAsync(PermisoCreateDTO dto, string? actorUserId);
    Task<Permiso> ActualizarPendienteAsync(int idPermiso, PermisoCreateDTO dto, string? actorUserId);
    Task<Permiso> EjecutarAccionAsync(int idPermiso, string accion, string? comentario, string? actorUserId, IEnumerable<string>? roles);
    Task<Permiso> AprobarAsync(int idPermiso, string? actorUserId, string? comentario);
    Task<Permiso> RechazarAsync(int idPermiso, string motivoRechazo, string? actorUserId);
}
