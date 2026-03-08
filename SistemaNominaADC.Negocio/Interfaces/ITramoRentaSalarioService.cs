using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface ITramoRentaSalarioService
{
    Task<List<TramoRentaSalario>> Lista();
    Task<TramoRentaSalario> Obtener(int id);
    Task<TramoRentaSalario> Crear(TramoRentaSalario modelo);
    Task<bool> Actualizar(TramoRentaSalario modelo);
    Task<bool> Desactivar(int id);
}
