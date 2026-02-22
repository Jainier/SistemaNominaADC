using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IEmpleadoService
{
    Task<List<Empleado>> Lista();
    Task<Empleado> Obtener(int id);
    Task<Empleado> Crear(Empleado modelo);
    Task<bool> Actualizar(Empleado modelo);
    Task<bool> Desactivar(int id);
}
