using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface INominaCalculator
{
    Task<NominaCalculoEmpleadoDTO> CalcularEmpleado(
        int idPlanilla,
        int idEmpleado,
        DateTime periodoInicio,
        DateTime periodoFin,
        int idEstadoActivo,
        int idEstadoAprobado,
        bool trazaDetallada = false);
}
