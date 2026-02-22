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
    public Task<List<TipoPermiso>> Lista() => _context.TipoPermisos.ToListAsync();
    public async Task<TipoPermiso> Obtener(int id) => await _context.TipoPermisos.FirstOrDefaultAsync(x => x.IdTipoPermiso == id) ?? throw new NotFoundException("Tipo de permiso no encontrado.");
    public async Task<TipoPermiso> Crear(TipoPermiso modelo) { await Validar(modelo.Nombre, 0); _context.TipoPermisos.Add(modelo); await _context.SaveChangesAsync(); return modelo; }
    public async Task<bool> Actualizar(TipoPermiso modelo) { await Validar(modelo.Nombre, modelo.IdTipoPermiso); var a = await Obtener(modelo.IdTipoPermiso); a.Nombre = modelo.Nombre; a.Estado = modelo.Estado; return await _context.SaveChangesAsync() > 0; }
    public async Task<bool> Desactivar(int id) { var a = await Obtener(id); a.Estado = false; return await _context.SaveChangesAsync() > 0; }
    private async Task Validar(string nombre, int id) { if (string.IsNullOrWhiteSpace(nombre)) throw new BusinessException("El nombre es obligatorio."); if (await _context.TipoPermisos.AnyAsync(x => x.Nombre == nombre && x.IdTipoPermiso != id)) throw new BusinessException("Ya existe un tipo de permiso con ese nombre."); }
}
