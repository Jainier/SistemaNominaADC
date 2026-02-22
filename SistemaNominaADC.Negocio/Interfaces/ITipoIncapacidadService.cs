using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface ITipoIncapacidadService
{
    Task<List<TipoIncapacidad>> Lista();
    Task<TipoIncapacidad> Obtener(int id);
    Task<TipoIncapacidad> Crear(TipoIncapacidad modelo);
    Task<bool> Actualizar(TipoIncapacidad modelo);
    Task<bool> Desactivar(int id);
}
