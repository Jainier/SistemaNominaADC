using System.Security.Claims;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Security;

public interface IObjetoSistemaAuthorizationService
{
    Task<bool> PuedeAccederModuloAsync(ClaimsPrincipal user, string nombreObjeto);
}

public class ObjetoSistemaAuthorizationService : IObjetoSistemaAuthorizationService
{
    private readonly IObjetoSistemaService _objetoSistemaService;

    public ObjetoSistemaAuthorizationService(IObjetoSistemaService objetoSistemaService)
    {
        _objetoSistemaService = objetoSistemaService;
    }

    public async Task<bool> PuedeAccederModuloAsync(ClaimsPrincipal user, string nombreObjeto)
    {
        if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(nombreObjeto))
            return false;

        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (roles.Count == 0)
            return false;

        var objetosPermitidos = await _objetoSistemaService.ListaParaMenu(roles);

        return objetosPermitidos.Any(o =>
            string.Equals(o.NombreEntidad, nombreObjeto.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
