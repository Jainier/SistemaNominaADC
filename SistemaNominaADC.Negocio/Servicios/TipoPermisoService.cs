using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class TipoPermisoService : ITipoPermisoService
{
    private readonly ApplicationDbContext _context;
    public TipoPermisoService(ApplicationDbContext context) => _context = context;
    public Task<List<TipoPermiso>> Lista() => _context.TipoPermisos.Include(x => x.Estado).ToListAsync();
    public async Task<TipoPermiso> Obtener(int id) => await _context.TipoPermisos.Include(x => x.Estado).FirstOrDefaultAsync(x => x.IdTipoPermiso == id) ?? throw new NotFoundException("Tipo de permiso no encontrado.");
    public async Task<TipoPermiso> Crear(TipoPermiso modelo) { await Validar(modelo, 0); _context.TipoPermisos.Add(modelo); await _context.SaveChangesAsync(); return modelo; }
    public async Task<bool> Actualizar(TipoPermiso modelo) { await Validar(modelo, modelo.IdTipoPermiso); var a = await _context.TipoPermisos.FirstOrDefaultAsync(x => x.IdTipoPermiso == modelo.IdTipoPermiso) ?? throw new NotFoundException("Tipo de permiso no encontrado."); a.Nombre = modelo.Nombre; a.IdEstado = modelo.IdEstado; return await _context.SaveChangesAsync() > 0; }
    public async Task<bool> Desactivar(int id) { var a = await _context.TipoPermisos.FirstOrDefaultAsync(x => x.IdTipoPermiso == id) ?? throw new NotFoundException("Tipo de permiso no encontrado."); a.IdEstado = await ObtenerIdEstadoPorNombre("Inactivo"); return await _context.SaveChangesAsync() > 0; }
    private async Task Validar(TipoPermiso modelo, int id)
    {
        if (string.IsNullOrWhiteSpace(modelo.Nombre)) throw new BusinessException("El nombre es obligatorio.");
        if (modelo.IdEstado <= 0) throw new BusinessException("El estado es obligatorio.");
        if (!await _context.Estados.AnyAsync(e => e.IdEstado == modelo.IdEstado)) throw new NotFoundException("Estado no encontrado.");
        if (await _context.TipoPermisos.AnyAsync(x => x.Nombre == modelo.Nombre && x.IdTipoPermiso != id)) throw new BusinessException("Ya existe un tipo de permiso con ese nombre.");
    }
    private async Task<int> ObtenerIdEstadoPorNombre(string nombre)
    {
        var estado = await _context.Estados.FirstOrDefaultAsync(e => e.Nombre == nombre);
        if (estado == null) throw new BusinessException($"No se encontró el estado '{nombre}'.");
        return estado.IdEstado;
    }
}
