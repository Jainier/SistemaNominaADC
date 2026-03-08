using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using System.Security.Claims;

namespace SistemaNominaADC.Api.Security;

public class SolicitudesAuthorizationService : ISolicitudesAuthorizationService
{
    private readonly ApplicationDbContext _context;

    public SolicitudesAuthorizationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EsAprobadorGlobalAsync(ClaimsPrincipal user)
    {
        if (RolesSistema.EsAprobadorGlobal(user))
            return true;

        var userId = await ObtenerIdentityUserIdAsync(user);
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var rolesUsuario = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(
                _context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!)
            .ToListAsync();

        return rolesUsuario.Any(r =>
            string.Equals(r, RolesSistema.Administrador, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, RolesSistema.RRHH, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> EsJefaturaDepartamentoAsync(ClaimsPrincipal user)
    {
        var departamentos = await ObtenerDepartamentosGestionadosAsync(user);
        return departamentos.Count > 0;
    }

    public async Task<bool> PuedeAprobarEmpleadoAsync(ClaimsPrincipal user, int idEmpleado)
    {
        if (idEmpleado <= 0) return false;

        if (await EsAprobadorGlobalAsync(user))
            return true;

        var idEmpleadoActual = await ObtenerIdEmpleadoActualAsync(user);
        if (idEmpleadoActual.HasValue && idEmpleadoActual.Value == idEmpleado)
            return false;

        var empleadosGestionables = await ObtenerEmpleadosGestionablesAsync(user);
        return empleadosGestionables.Contains(idEmpleado);
    }

    public async Task<List<int>> ObtenerDepartamentosGestionadosAsync(ClaimsPrincipal user)
    {
        var idEmpleado = await ObtenerIdEmpleadoActualAsync(user);
        if (!idEmpleado.HasValue) return new List<int>();

        return await _context.DepartamentoJefaturas
            .Where(x => x.Activo && x.IdEmpleado == idEmpleado.Value)
            .Select(x => x.IdDepartamento)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<int>> ObtenerEmpleadosGestionablesAsync(ClaimsPrincipal user)
    {
        if (await EsAprobadorGlobalAsync(user))
        {
            return await _context.Empleados
                .Select(e => e.IdEmpleado)
                .Distinct()
                .ToListAsync();
        }

        var idEmpleadoActual = await ObtenerIdEmpleadoActualAsync(user);
        if (!idEmpleadoActual.HasValue)
            return new List<int>();

        var idsGestionables = new HashSet<int> { idEmpleadoActual.Value };

        // 1) Organigrama explícito: supervisor -> subordinados (transitivo).
        var subordinadosOrganigrama = await ObtenerSubordinadosEnOrganigramaAsync(idEmpleadoActual.Value);
        foreach (var id in subordinadosOrganigrama)
            idsGestionables.Add(id);

        // 2) Fallback de compatibilidad con jefaturas por departamento.
        var departamentos = await ObtenerDepartamentosGestionadosAsync(user);
        if (departamentos.Count > 0)
        {
            var idsEquipoDepartamento = await _context.Empleados
                .Where(e => e.Puesto != null && departamentos.Contains(e.Puesto.IdDepartamento))
                .Select(e => e.IdEmpleado)
                .Distinct()
                .ToListAsync();

            foreach (var id in idsEquipoDepartamento)
                idsGestionables.Add(id);
        }

        return idsGestionables.ToList();
    }

    public async Task<int?> ObtenerIdEmpleadoActualAsync(ClaimsPrincipal user)
    {
        var userId = await ObtenerIdentityUserIdAsync(user);
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var idEmpleado = await _context.Empleados
            .Where(e => e.IdentityUserId == userId)
            .Select(e => (int?)e.IdEmpleado)
            .FirstOrDefaultAsync();

        return idEmpleado;
    }

    private async Task<string?> ObtenerIdentityUserIdAsync(ClaimsPrincipal user)
    {
        var userId =
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue("nameid") ??
            user.FindFirstValue("sub");

        if (!string.IsNullOrWhiteSpace(userId))
            return userId;

        var email = user.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.Trim().ToUpperInvariant();
            var userByEmail = await _context.Users
                .Where(u => u.NormalizedEmail == normalizedEmail)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(userByEmail))
                return userByEmail;
        }

        var userName = user.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrWhiteSpace(userName))
        {
            var normalizedUserName = userName.Trim().ToUpperInvariant();
            var userByName = await _context.Users
                .Where(u => u.NormalizedUserName == normalizedUserName || u.UserName == userName)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(userByName))
                return userByName;
        }

        return null;
    }

    private async Task<List<int>> ObtenerSubordinadosEnOrganigramaAsync(int idSupervisor)
    {
        var hoy = DateTime.UtcNow.Date;

        var relaciones = await _context.EmpleadoJerarquias
            .AsNoTracking()
            .Where(x =>
                x.Activo &&
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde <= hoy) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta >= hoy))
            .Select(x => new { x.IdEmpleado, x.IdSupervisor })
            .ToListAsync();

        var porSupervisor = relaciones
            .GroupBy(x => x.IdSupervisor)
            .ToDictionary(g => g.Key, g => g.Select(x => x.IdEmpleado).ToList());

        var resultado = new HashSet<int>();
        var visitadosSupervisores = new HashSet<int>();
        var cola = new Queue<int>();
        cola.Enqueue(idSupervisor);

        while (cola.Count > 0)
        {
            var supervisorActual = cola.Dequeue();
            if (!visitadosSupervisores.Add(supervisorActual))
                continue;

            if (!porSupervisor.TryGetValue(supervisorActual, out var directos))
                continue;

            foreach (var sub in directos)
            {
                if (resultado.Add(sub))
                    cola.Enqueue(sub);
            }
        }

        return resultado.ToList();
    }
}
