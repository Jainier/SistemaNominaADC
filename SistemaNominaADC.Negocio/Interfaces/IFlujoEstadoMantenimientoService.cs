using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IFlujoEstadoMantenimientoService
{
    Task<List<FlujoEstado>> Lista();
    Task<FlujoEstado> Obtener(int id);
    Task<FlujoEstado> Crear(FlujoEstado modelo);
    Task<bool> Actualizar(FlujoEstado modelo);
    Task<bool> Desactivar(int id);
}
