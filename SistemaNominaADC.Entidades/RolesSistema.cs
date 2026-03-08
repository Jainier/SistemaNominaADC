using System.Security.Claims;

namespace SistemaNominaADC.Entidades;

public static class RolesSistema
{
    public const string Administrador = "Administrador";
    public const string AdminLegacy = "Admin";
    public const string RRHH = "RRHH";
    public const string EncargadoDatosMaestros = "Encargado Datos Maestros";

    public static readonly string[] RolesAdministrativos = { Administrador };
    public static readonly string[] RolesAprobadorGlobal = { Administrador, RRHH };

    public static bool EsAdministrador(ClaimsPrincipal user) =>
        user.Claims.Any(c =>
            (c.Type == ClaimTypes.Role ||
             c.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
             c.Type.Equals("roles", StringComparison.OrdinalIgnoreCase)) &&
            EsRolAdministrador(c.Value));

    public static bool EsAprobadorGlobal(ClaimsPrincipal user) =>
        TieneRol(user, Administrador) || TieneRol(user, RRHH);

    public static bool EsAdministrador(IEnumerable<string> roles) =>
        roles.Any(EsRolAdministrador);

    public static bool EsRolAdministrador(string? rol) =>
        !string.IsNullOrWhiteSpace(rol) &&
        (string.Equals(rol.Trim(), Administrador, StringComparison.OrdinalIgnoreCase) ||
         string.Equals(rol.Trim(), AdminLegacy, StringComparison.OrdinalIgnoreCase));

    private static bool TieneRol(ClaimsPrincipal user, string rol) =>
        user.Claims.Any(c =>
            (c.Type == ClaimTypes.Role ||
             c.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
             c.Type.Equals("roles", StringComparison.OrdinalIgnoreCase)) &&
            string.Equals(c.Value, rol, StringComparison.OrdinalIgnoreCase));
}
