using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface IMiPlanillaService
{
    Task<List<MiPlanillaHistorialItemDTO>> HistorialPorEmpleadoAsync(int idEmpleado);
    Task<MiPlanillaDetalleDTO> ObtenerDetallePorEmpleadoAsync(int idEmpleado, int idPlanilla);
}

