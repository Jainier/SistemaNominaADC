using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class TipoIncapacidadService : ITipoIncapacidadService
{
    private readonly ApplicationDbContext _context;
    public TipoIncapacidadService(ApplicationDbContext context) => _context = context;
    public Task<List<TipoIncapacidad>> Lista() => _context.TipoIncapacidades.ToListAsync();
    public async Task<TipoIncapacidad> Obtener(int id) => await _context.TipoIncapacidades.FirstOrDefaultAsync(x => x.IdTipoIncapacidad == id) ?? throw new NotFoundException("Tipo de incapacidad no encontrado.");
    public async Task<TipoIncapacidad> Crear(TipoIncapacidad modelo) { await Validar(modelo.Nombre, 0); _context.TipoIncapacidades.Add(modelo); await _context.SaveChangesAsync(); return modelo; }
    public async Task<bool> Actualizar(TipoIncapacidad modelo) { await Validar(modelo.Nombre, modelo.IdTipoIncapacidad); var a = await Obtener(modelo.IdTipoIncapacidad); a.Nombre = modelo.Nombre; a.Estado = modelo.Estado; return await _context.SaveChangesAsync() > 0; }
    public async Task<bool> Desactivar(int id) { var a = await Obtener(id); a.Estado = false; return await _context.SaveChangesAsync() > 0; }
    private async Task Validar(string nombre, int id) { if (string.IsNullOrWhiteSpace(nombre)) throw new BusinessException("El nombre es obligatorio."); if (await _context.TipoIncapacidades.AnyAsync(x => x.Nombre == nombre && x.IdTipoIncapacidad != id)) throw new BusinessException("Ya existe un tipo de incapacidad con ese nombre."); }
}
