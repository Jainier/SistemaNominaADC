using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class FlujoEstadoService : IFlujoEstadoService
{
    private readonly ApplicationDbContext _context;

    public FlujoEstadoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> ObtenerEstadoDestinoAsync(string entidad, int? idEstadoActual, string accion, IEnumerable<string>? roles = null)
    {
        var transicion = await ResolverTransicionAsync(entidad, idEstadoActual, accion, roles)
            ?? throw new BusinessException($"No existe transicion configurada para {entidad} con accion {accion}.");

        return transicion.IdEstadoDestino;
    }

    public async Task ValidarTransicionAsync(string entidad, int? idEstadoActual, string accion, IEnumerable<string>? roles = null)
    {
        _ = await ResolverTransicionAsync(entidad, idEstadoActual, accion, roles)
            ?? throw new BusinessException($"Accion no permitida: {accion} para {entidad}.");
    }

    public async Task<List<string>> ObtenerAccionesDisponiblesAsync(string entidad, int idEstadoActual, IEnumerable<string>? roles = null)
    {
        if (string.IsNullOrWhiteSpace(entidad))
            throw new BusinessException("La entidad del flujo es obligatoria.");

        var entidadNorm = Normalizar(entidad);
        var rolesLista = roles?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? [];
        var esAdmin = RolesSistema.EsAdministrador(rolesLista);

        var candidatas = await _context.FlujosEstado
            .AsNoTracking()
            .Where(x => x.Activo && x.IdEstadoOrigen == idEstadoActual)
            .ToListAsync();

        return candidatas
            .Where(x => Normalizar(x.Entidad) == entidadNorm)
            .Where(x => esAdmin || string.IsNullOrWhiteSpace(x.RequiereRol) || rolesLista.Any(r => string.Equals(r, x.RequiereRol, StringComparison.OrdinalIgnoreCase)))
            .Select(x => x.Accion?.Trim() ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<FlujoEstado?> ResolverTransicionAsync(string entidad, int? idEstadoActual, string accion, IEnumerable<string>? roles)
    {
        if (string.IsNullOrWhiteSpace(entidad))
            throw new BusinessException("La entidad del flujo es obligatoria.");

        var entidadNorm = Normalizar(entidad);
        var accionNorm = string.IsNullOrWhiteSpace(accion) ? null : Normalizar(accion);
        var rolesLista = roles?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? [];
        var esAdmin = RolesSistema.EsAdministrador(rolesLista);

        var candidatas = await _context.FlujosEstado
            .AsNoTracking()
            .Where(x => x.Activo)
            .ToListAsync();

        var transicionPorAccion = candidatas
            .Where(x => Normalizar(x.Entidad) == entidadNorm)
            .Where(x => !idEstadoActual.HasValue || x.IdEstadoOrigen == idEstadoActual.Value)
            .Where(x => accionNorm is not null && Normalizar(x.Accion) == accionNorm)
            .Where(x => esAdmin || string.IsNullOrWhiteSpace(x.RequiereRol) || rolesLista.Any(r => string.Equals(r, x.RequiereRol, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.IdFlujoEstado)
            .FirstOrDefault();

        if (transicionPorAccion is not null)
            return transicionPorAccion;

        // Para estado inicial (codigo 0), no forzar accion "Crear": tomar cualquier transicion valida.
        if (idEstadoActual.HasValue)
        {
            var codigoEstadoActual = await _context.Estados
                .Where(e => e.IdEstado == idEstadoActual.Value)
                .Select(e => e.Codigo)
                .FirstOrDefaultAsync();

            if (codigoEstadoActual == EstadoCodigosSistema.Nulo)
            {
                return candidatas
                    .Where(x => Normalizar(x.Entidad) == entidadNorm)
                    .Where(x => x.IdEstadoOrigen == idEstadoActual.Value)
                    .Where(x => esAdmin || string.IsNullOrWhiteSpace(x.RequiereRol) || rolesLista.Any(r => string.Equals(r, x.RequiereRol, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(x => x.IdFlujoEstado)
                    .FirstOrDefault();
            }
        }

        return null;
    }

    private static string Normalizar(string valor) => valor.Trim().ToUpperInvariant();
}
