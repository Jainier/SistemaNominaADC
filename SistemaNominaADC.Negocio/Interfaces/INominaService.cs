using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface INominaService
{
    Task<NominaResumenPlanillaDTO> CalcularPlanilla(int idPlanilla, string? actorUserId, IEnumerable<string>? roles = null);
    Task<NominaResumenPlanillaDTO> RecalcularPlanilla(int idPlanilla, string? actorUserId, IEnumerable<string>? roles = null);
    Task<bool> AprobarPlanilla(int idPlanilla, string? actorUserId, IEnumerable<string>? roles = null);
    Task<bool> RechazarPlanilla(int idPlanilla, string? actorUserId, IEnumerable<string>? roles = null);
    Task<NominaResumenPlanillaDTO> ObtenerResumenPlanilla(int idPlanilla);
}
