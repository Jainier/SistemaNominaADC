using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IPlanillaEncabezadoService
{
    Task<List<PlanillaEncabezado>> Lista();
    Task<PlanillaEncabezado> Obtener(int id);
    Task<PlanillaEncabezado> Crear(PlanillaEncabezado modelo);
    Task<bool> Actualizar(PlanillaEncabezado modelo);
    Task<bool> Desactivar(int id);
    Task<bool> EjecutarAccionAsync(int idPlanilla, string accion, IEnumerable<string>? roles);
    Task<List<string>> ObtenerAccionesDisponibles(int idPlanilla, IEnumerable<string>? roles);
}
