namespace SistemaNominaADC.Negocio.Interfaces;

public interface IFlujoEstadoService
{
    Task<int> ObtenerEstadoDestinoAsync(string entidad, int? idEstadoActual, string accion, IEnumerable<string>? roles = null);
    Task ValidarTransicionAsync(string entidad, int? idEstadoActual, string accion, IEnumerable<string>? roles = null);
    Task<List<string>> ObtenerAccionesDisponiblesAsync(string entidad, int idEstadoActual, IEnumerable<string>? roles = null);
}
