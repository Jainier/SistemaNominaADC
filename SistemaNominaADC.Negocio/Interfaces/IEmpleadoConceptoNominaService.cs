using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IEmpleadoConceptoNominaService
{
    Task<List<EmpleadoConceptoNomina>> Lista();
    Task<EmpleadoConceptoNomina> Obtener(int id);
    Task<EmpleadoConceptoNomina> Crear(EmpleadoConceptoNomina modelo);
    Task<bool> Actualizar(EmpleadoConceptoNomina modelo);
    Task<bool> Desactivar(int id);
}
