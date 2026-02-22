using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface ITipoPermisoService
{
    Task<List<TipoPermiso>> Lista();
    Task<TipoPermiso> Obtener(int id);
    Task<TipoPermiso> Crear(TipoPermiso modelo);
    Task<bool> Actualizar(TipoPermiso modelo);
    Task<bool> Desactivar(int id);
}
