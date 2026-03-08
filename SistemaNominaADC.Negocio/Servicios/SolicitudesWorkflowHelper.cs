using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;

namespace SistemaNominaADC.Negocio.Servicios;

internal static class SolicitudesWorkflowHelper
{
    public static async Task<int> ObtenerEstadoPendienteAsync(ApplicationDbContext context) =>
        await ObtenerEstadoIdAsync(context, "Pendiente", "En espera");

    public static async Task<int> ObtenerEstadoAprobadoAsync(ApplicationDbContext context) =>
        await ObtenerEstadoIdAsync(context, "Aprobado", "Aprobada");

    public static async Task<int> ObtenerEstadoRechazadoAsync(ApplicationDbContext context) =>
        await ObtenerEstadoIdAsync(context, "Rechazado", "Rechazada");

    public static async Task<int> ObtenerEstadoActivoAsync(ApplicationDbContext context) =>
        await ObtenerEstadoIdAsync(context, "Activo");

    public static async Task<int> ObtenerEstadoIdAsync(ApplicationDbContext context, params string[] nombres)
    {
        var normalizados = nombres
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim().ToUpper())
            .Distinct()
            .ToList();

        var estado = await context.Estados
            .Where(e => e.Nombre != null && normalizados.Contains(e.Nombre.Trim().ToUpper()))
            .OrderBy(e => e.IdEstado)
            .FirstOrDefaultAsync();

        if (estado is null)
            throw new BusinessException($"No se encontró un estado válido para: {string.Join(", ", nombres)}.");

        return estado.IdEstado;
    }

    public static void ValidarRangoFechas(DateTime fechaInicio, DateTime fechaFin, string prefijo)
    {
        if (fechaInicio.Date > fechaFin.Date)
            throw new BusinessException($"{prefijo}: la fecha de inicio no puede ser mayor que la fecha fin.");
    }

    public static int CalcularDiasInclusivos(DateTime fechaInicio, DateTime fechaFin)
    {
        var dias = (fechaFin.Date - fechaInicio.Date).Days + 1;
        if (dias <= 0)
            throw new BusinessException("La cantidad de días calculada es inválida.");
        return dias;
    }

    public static async Task RegistrarBitacoraAsync(
        ApplicationDbContext context,
        string accion,
        string descripcion,
        int? idEstado,
        string? actorUserId)
    {
        context.Bitacoras.Add(new Bitacora
        {
            Fecha = DateTime.Now,
            Accion = accion,
            Descripcion = descripcion,
            IdEstado = idEstado,
            IdentityUserId = actorUserId
        });

        await context.SaveChangesAsync();
    }

    public static async Task<string?> ResolverUsuarioDecisionAsync(ApplicationDbContext context, string? actorUserId)
    {
        if (string.IsNullOrWhiteSpace(actorUserId))
            return null;

        var userName = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == actorUserId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync();

        return string.IsNullOrWhiteSpace(userName)
            ? actorUserId
            : userName.Trim();
    }
}
