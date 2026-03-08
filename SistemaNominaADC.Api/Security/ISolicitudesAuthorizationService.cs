using System.Security.Claims;

namespace SistemaNominaADC.Api.Security;

public interface ISolicitudesAuthorizationService
{
    Task<bool> EsAprobadorGlobalAsync(ClaimsPrincipal user);
    Task<bool> EsJefaturaDepartamentoAsync(ClaimsPrincipal user);
    Task<bool> PuedeAprobarEmpleadoAsync(ClaimsPrincipal user, int idEmpleado);
    Task<List<int>> ObtenerDepartamentosGestionadosAsync(ClaimsPrincipal user);
    Task<List<int>> ObtenerEmpleadosGestionablesAsync(ClaimsPrincipal user);
    Task<int?> ObtenerIdEmpleadoActualAsync(ClaimsPrincipal user);
}
