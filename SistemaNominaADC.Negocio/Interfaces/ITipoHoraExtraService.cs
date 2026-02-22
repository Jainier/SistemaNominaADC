using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface ITipoHoraExtraService
{
    Task<List<TipoHoraExtra>> Lista();
    Task<TipoHoraExtra> Obtener(int id);
    Task<TipoHoraExtra> Crear(TipoHoraExtra modelo);
    Task<bool> Actualizar(TipoHoraExtra modelo);
    Task<bool> Desactivar(int id);
}
