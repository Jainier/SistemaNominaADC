using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class TipoHoraExtraService : ITipoHoraExtraService
{
    private readonly ApplicationDbContext _context;
    public TipoHoraExtraService(ApplicationDbContext context) => _context = context;
    public Task<List<TipoHoraExtra>> Lista() => _context.TipoHoraExtras.ToListAsync();
    public async Task<TipoHoraExtra> Obtener(int id) => await _context.TipoHoraExtras.FirstOrDefaultAsync(x => x.IdTipoHoraExtra == id) ?? throw new NotFoundException("Tipo de hora extra no encontrado.");
    public async Task<TipoHoraExtra> Crear(TipoHoraExtra modelo) { await Validar(modelo.Nombre, 0); _context.TipoHoraExtras.Add(modelo); await _context.SaveChangesAsync(); return modelo; }
    public async Task<bool> Actualizar(TipoHoraExtra modelo) { await Validar(modelo.Nombre, modelo.IdTipoHoraExtra); var a = await Obtener(modelo.IdTipoHoraExtra); a.Nombre = modelo.Nombre; a.Estado = modelo.Estado; return await _context.SaveChangesAsync() > 0; }
    public async Task<bool> Desactivar(int id) { var a = await Obtener(id); a.Estado = false; return await _context.SaveChangesAsync() > 0; }
    private async Task Validar(string nombre, int id) { if (string.IsNullOrWhiteSpace(nombre)) throw new BusinessException("El nombre es obligatorio."); if (await _context.TipoHoraExtras.AnyAsync(x => x.Nombre == nombre && x.IdTipoHoraExtra != id)) throw new BusinessException("Ya existe un tipo de hora extra con ese nombre."); }
}
