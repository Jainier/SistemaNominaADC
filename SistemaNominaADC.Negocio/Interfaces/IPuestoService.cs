using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IPuestoService
{
    Task<List<Puesto>> Lista();
    Task<Puesto> Obtener(int id);
    Task<Puesto> Crear(Puesto modelo);
    Task<bool> Actualizar(Puesto modelo);
    Task<bool> Desactivar(int id);
}
