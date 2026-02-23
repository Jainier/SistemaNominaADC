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
    public Task<List<TipoIncapacidad>> Lista() => _context.TipoIncapacidades.Include(x => x.Estado).ToListAsync();
    public async Task<TipoIncapacidad> Obtener(int id) => await _context.TipoIncapacidades.Include(x => x.Estado).FirstOrDefaultAsync(x => x.IdTipoIncapacidad == id) ?? throw new NotFoundException("Tipo de incapacidad no encontrado.");
    public async Task<TipoIncapacidad> Crear(TipoIncapacidad modelo) { await Validar(modelo, 0); _context.TipoIncapacidades.Add(modelo); await _context.SaveChangesAsync(); return modelo; }
    public async Task<bool> Actualizar(TipoIncapacidad modelo) { await Validar(modelo, modelo.IdTipoIncapacidad); var a = await _context.TipoIncapacidades.FirstOrDefaultAsync(x => x.IdTipoIncapacidad == modelo.IdTipoIncapacidad) ?? throw new NotFoundException("Tipo de incapacidad no encontrado."); a.Nombre = modelo.Nombre; a.IdEstado = modelo.IdEstado; return await _context.SaveChangesAsync() > 0; }
    public async Task<bool> Desactivar(int id) { var a = await _context.TipoIncapacidades.FirstOrDefaultAsync(x => x.IdTipoIncapacidad == id) ?? throw new NotFoundException("Tipo de incapacidad no encontrado."); a.IdEstado = await ObtenerIdEstadoPorNombre("Inactivo"); return await _context.SaveChangesAsync() > 0; }
    private async Task Validar(TipoIncapacidad modelo, int id)
    {
        if (string.IsNullOrWhiteSpace(modelo.Nombre)) throw new BusinessException("El nombre es obligatorio.");
        if (modelo.IdEstado <= 0) throw new BusinessException("El estado es obligatorio.");
        if (!await _context.Estados.AnyAsync(e => e.IdEstado == modelo.IdEstado)) throw new NotFoundException("Estado no encontrado.");
        if (await _context.TipoIncapacidades.AnyAsync(x => x.Nombre == modelo.Nombre && x.IdTipoIncapacidad != id)) throw new BusinessException("Ya existe un tipo de incapacidad con ese nombre.");
    }
    private async Task<int> ObtenerIdEstadoPorNombre(string nombre)
    {
        var estado = await _context.Estados.FirstOrDefaultAsync(e => e.Nombre == nombre);
        if (estado == null) throw new BusinessException($"No se encontró el estado '{nombre}'.");
        return estado.IdEstado;
    }
}
