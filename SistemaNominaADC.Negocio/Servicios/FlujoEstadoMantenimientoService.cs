using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class FlujoEstadoMantenimientoService : IFlujoEstadoMantenimientoService
{
    private readonly ApplicationDbContext _context;

    public FlujoEstadoMantenimientoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<FlujoEstado>> Lista() =>
        _context.FlujosEstado
            .Include(x => x.EstadoOrigen)
            .Include(x => x.EstadoDestino)
            .OrderBy(x => x.Entidad)
            .ThenBy(x => x.Accion)
            .ThenBy(x => x.IdEstadoOrigen)
            .ToListAsync();

    public async Task<FlujoEstado> Obtener(int id) =>
        await _context.FlujosEstado
            .Include(x => x.EstadoOrigen)
            .Include(x => x.EstadoDestino)
            .FirstOrDefaultAsync(x => x.IdFlujoEstado == id)
        ?? throw new NotFoundException("Flujo de estado no encontrado.");

    public async Task<FlujoEstado> Crear(FlujoEstado modelo)
    {
        await Validar(modelo, 0);
        _context.FlujosEstado.Add(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task<bool> Actualizar(FlujoEstado modelo)
    {
        await Validar(modelo, modelo.IdFlujoEstado);

        var actual = await _context.FlujosEstado
            .FirstOrDefaultAsync(x => x.IdFlujoEstado == modelo.IdFlujoEstado)
            ?? throw new NotFoundException("Flujo de estado no encontrado.");

        actual.Entidad = modelo.Entidad.Trim();
        actual.IdEstadoOrigen = modelo.IdEstadoOrigen;
        actual.IdEstadoDestino = modelo.IdEstadoDestino;
        actual.Accion = modelo.Accion.Trim();
        actual.RequiereRol = string.IsNullOrWhiteSpace(modelo.RequiereRol) ? null : modelo.RequiereRol.Trim();
        actual.Activo = modelo.Activo;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Desactivar(int id)
    {
        var actual = await _context.FlujosEstado
            .FirstOrDefaultAsync(x => x.IdFlujoEstado == id)
            ?? throw new NotFoundException("Flujo de estado no encontrado.");

        actual.Activo = false;
        return await _context.SaveChangesAsync() > 0;
    }

    private async Task Validar(FlujoEstado modelo, int id)
    {
        if (string.IsNullOrWhiteSpace(modelo.Entidad))
            throw new BusinessException("La entidad es obligatoria.");

        if (string.IsNullOrWhiteSpace(modelo.Accion))
            throw new BusinessException("La accion es obligatoria.");

        if (modelo.IdEstadoDestino <= 0)
            throw new BusinessException("El estado destino es obligatorio.");

        if (modelo.Entidad.Trim().Length > 100)
            throw new BusinessException("La entidad no debe exceder 100 caracteres.");

        if (modelo.Accion.Trim().Length > 50)
            throw new BusinessException("La accion no debe exceder 50 caracteres.");

        if (!await _context.Estados.AnyAsync(x => x.IdEstado == modelo.IdEstadoDestino))
            throw new NotFoundException("Estado destino no encontrado.");

        if (modelo.IdEstadoOrigen.HasValue && !await _context.Estados.AnyAsync(x => x.IdEstado == modelo.IdEstadoOrigen.Value))
            throw new NotFoundException("Estado origen no encontrado.");

        if (!string.IsNullOrWhiteSpace(modelo.RequiereRol))
        {
            var rolExiste = await _context.Roles
                .AnyAsync(x => x.Name != null && x.Name == modelo.RequiereRol.Trim());

            if (!rolExiste)
                throw new BusinessException("El rol requerido no existe.");
        }

        var entidadNorm = modelo.Entidad.Trim().ToUpperInvariant();
        var accionNorm = modelo.Accion.Trim().ToUpperInvariant();

        var existe = await _context.FlujosEstado.AnyAsync(x =>
            x.IdFlujoEstado != id &&
            x.IdEstadoOrigen == modelo.IdEstadoOrigen &&
            x.IdEstadoDestino == modelo.IdEstadoDestino &&
            x.Entidad.ToUpper() == entidadNorm &&
            x.Accion.ToUpper() == accionNorm);

        if (existe)
            throw new BusinessException("Ya existe una transicion con la misma entidad, accion, estado origen y estado destino.");
    }
}
