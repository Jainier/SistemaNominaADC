using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IEmpleadoJerarquiaService
{
    Task<List<EmpleadoJerarquia>> ListaAsync(int? idSupervisor = null, int? idEmpleado = null, bool soloActivos = true);
    Task<EmpleadoJerarquia> ObtenerAsync(int idEmpleadoJerarquia);
    Task<EmpleadoJerarquia> CrearAsync(EmpleadoJerarquia entidad);
    Task<EmpleadoJerarquia> ActualizarAsync(EmpleadoJerarquia entidad);
    Task DesactivarAsync(int idEmpleadoJerarquia);
}
