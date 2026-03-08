using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class NotificacionService : INotificacionService
{
    private readonly ApplicationDbContext _context;

    public NotificacionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notificacion>> ListarMisNotificacionesAsync(string identityUserId, bool soloPendientes = false, int max = 50)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new BusinessException("Usuario inválido para consultar notificaciones.");

        max = Math.Clamp(max, 1, 200);

        var query = _context.Notificaciones
            .Where(x => x.IdentityUserId == identityUserId);

        if (soloPendientes)
            query = query.Where(x => !x.Leida);

        return await query
            .OrderByDescending(x => x.FechaCreacion)
            .Take(max)
            .ToListAsync();
    }

    public async Task MarcarLeidaAsync(int idNotificacion, string identityUserId)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new BusinessException("Usuario inválido para marcar la notificación.");

        var notif = await _context.Notificaciones
            .FirstOrDefaultAsync(x => x.IdNotificacion == idNotificacion && x.IdentityUserId == identityUserId)
            ?? throw new NotFoundException("Notificación no encontrada.");

        if (notif.Leida) return;

        notif.Leida = true;
        notif.FechaLectura = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task MarcarTodasLeidasAsync(string identityUserId)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new BusinessException("Usuario inválido para marcar notificaciones.");

        var items = await _context.Notificaciones
            .Where(x => x.IdentityUserId == identityUserId && !x.Leida)
            .ToListAsync();

        if (items.Count == 0) return;

        var ahora = DateTime.Now;
        foreach (var item in items)
        {
            item.Leida = true;
            item.FechaLectura = ahora;
        }

        await _context.SaveChangesAsync();
    }

    public async Task EnviarAsync(IEnumerable<string> userIds, string titulo, string mensaje, string? urlDestino = null)
    {
        var destinos = userIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (destinos.Count == 0) return;

        var ahora = DateTime.Now;
        var items = destinos.Select(userId => new Notificacion
        {
            IdentityUserId = userId,
            Titulo = string.IsNullOrWhiteSpace(titulo) ? "Notificación" : titulo.Trim(),
            Mensaje = string.IsNullOrWhiteSpace(mensaje) ? "Tiene una actualización." : mensaje.Trim(),
            UrlDestino = string.IsNullOrWhiteSpace(urlDestino) ? null : urlDestino.Trim(),
            Leida = false,
            FechaCreacion = ahora
        });

        await _context.Notificaciones.AddRangeAsync(items);
        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> ObtenerUserIdsPorRolesAsync(params string[] roleNames)
    {
        var normalized = roleNames
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (normalized.Count == 0)
            return new List<string>();

        var roleIds = await _context.Roles
            .Where(r => r.Name != null && normalized.Contains(r.Name.ToUpper()))
            .Select(r => r.Id)
            .ToListAsync();

        if (roleIds.Count == 0)
            return new List<string>();

        return await _context.UserRoles
            .Where(ur => roleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<string>> ObtenerUserIdsJefaturaDepartamentoAsync(int idDepartamento)
    {
        if (idDepartamento <= 0)
            return new List<string>();

        return await _context.DepartamentoJefaturas
            .Where(x => x.Activo && x.IdDepartamento == idDepartamento)
            .Join(
                _context.Empleados,
                j => j.IdEmpleado,
                e => e.IdEmpleado,
                (j, e) => e.IdentityUserId)
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Select(userId => userId!)
            .Distinct()
            .ToListAsync();
    }
}
