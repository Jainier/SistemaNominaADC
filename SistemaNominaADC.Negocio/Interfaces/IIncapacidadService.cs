using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IIncapacidadService
{
    Task<List<Incapacidad>> HistorialAsync(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idEstado = null);
    Task<List<string>> ObtenerAccionesDisponibles(int idIncapacidad, IEnumerable<string>? roles);
    Task<Incapacidad> CrearAsync(IncapacidadCreateDTO dto, string? actorUserId, byte[]? archivoBytes, string? nombreArchivo);
    Task<Incapacidad> ActualizarRegistradaAsync(int idIncapacidad, IncapacidadCreateDTO dto, string? actorUserId, byte[]? archivoBytes, string? nombreArchivo);
    Task<Incapacidad> EjecutarAccionAsync(int idIncapacidad, string accion, string? comentario, string? actorUserId, IEnumerable<string>? roles);
    Task<Incapacidad> ValidarAsync(int idIncapacidad, string? comentario, string? actorUserId);
    Task<Incapacidad> RechazarAsync(int idIncapacidad, string motivoRechazo, string? actorUserId);
    Task<(byte[] contenido, string nombreArchivo, string contentType)?> ObtenerAdjuntoAsync(int idIncapacidad);
}
