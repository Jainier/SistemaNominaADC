using System.Security.Claims;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Security;

public interface IObjetoSistemaAuthorizationService
{
    Task<bool> PuedeAccederModuloAsync(ClaimsPrincipal user, string nombreObjeto);
    Task<bool> PuedeConsultarCatalogoAsync(ClaimsPrincipal user, string nombreObjeto);
}

public class ObjetoSistemaAuthorizationService : IObjetoSistemaAuthorizationService
{
    private static readonly HashSet<string> CatalogosConsultaLibre = new(StringComparer.OrdinalIgnoreCase)
    {
        "TipoPlanilla",
        "TipoConceptoNomina",
        "ModoCalculoConceptoNomina",
        "TipoHoraExtra",
        "TipoPermiso",
        "TipoIncapacidad",
        "TipoPlanillaConcepto"
    };

    private readonly IObjetoSistemaService _objetoSistemaService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ObjetoSistemaAuthorizationService(
        IObjetoSistemaService objetoSistemaService,
        IHttpContextAccessor httpContextAccessor)
    {
        _objetoSistemaService = objetoSistemaService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> PuedeAccederModuloAsync(ClaimsPrincipal user, string nombreObjeto)
    {
        if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(nombreObjeto))
            return false;

        var metodo = _httpContextAccessor.HttpContext?.Request?.Method;
        if (string.Equals(metodo, "GET", StringComparison.OrdinalIgnoreCase))
            return true;

        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (roles.Count == 0)
            return false;

        if (RolesSistema.EsAdministrador(roles))
            return true;

        var objetosPermitidos = await _objetoSistemaService.ListaParaMenu(roles);
        var nombreSolicitado = ObjetoSistemaCatalogo.Canonicalize(nombreObjeto);

        return objetosPermitidos.Any(o =>
            string.Equals(
                ObjetoSistemaCatalogo.Canonicalize(o.NombreEntidad),
                nombreSolicitado,
                StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> PuedeConsultarCatalogoAsync(ClaimsPrincipal user, string nombreObjeto)
    {
        if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(nombreObjeto))
            return false;

        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (roles.Count == 0)
            return false;

        if (RolesSistema.EsAdministrador(roles))
            return true;

        if (CatalogosConsultaLibre.Contains(nombreObjeto.Trim()))
            return true;

        return await PuedeAccederModuloAsync(user, nombreObjeto);
    }
}
