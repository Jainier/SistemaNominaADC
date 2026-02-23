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
    public Task<List<TipoHoraExtra>> Lista() => _context.TipoHoraExtras.Include(x => x.Estado).ToListAsync();
    public async Task<TipoHoraExtra> Obtener(int id) => await _context.TipoHoraExtras.Include(x => x.Estado).FirstOrDefaultAsync(x => x.IdTipoHoraExtra == id) ?? throw new NotFoundException("Tipo de hora extra no encontrado.");
    public async Task<TipoHoraExtra> Crear(TipoHoraExtra modelo) { await Validar(modelo, 0); _context.TipoHoraExtras.Add(modelo); await _context.SaveChangesAsync(); return modelo; }
    public async Task<bool> Actualizar(TipoHoraExtra modelo) { await Validar(modelo, modelo.IdTipoHoraExtra); var a = await _context.TipoHoraExtras.FirstOrDefaultAsync(x => x.IdTipoHoraExtra == modelo.IdTipoHoraExtra) ?? throw new NotFoundException("Tipo de hora extra no encontrado."); a.Nombre = modelo.Nombre; a.PorcentajePago = modelo.PorcentajePago; a.IdEstado = modelo.IdEstado; return await _context.SaveChangesAsync() > 0; }
    public async Task<bool> Desactivar(int id) { var a = await _context.TipoHoraExtras.FirstOrDefaultAsync(x => x.IdTipoHoraExtra == id) ?? throw new NotFoundException("Tipo de hora extra no encontrado."); a.IdEstado = await ObtenerIdEstadoPorNombre("Inactivo"); return await _context.SaveChangesAsync() > 0; }
    private async Task Validar(TipoHoraExtra modelo, int id)
    {
        if (string.IsNullOrWhiteSpace(modelo.Nombre)) throw new BusinessException("El nombre es obligatorio.");
        if (!modelo.PorcentajePago.HasValue) throw new BusinessException("El porcentaje de pago es obligatorio.");
        if (modelo.PorcentajePago.Value <= 0 || modelo.PorcentajePago.Value > 9.9999m) throw new BusinessException("El porcentaje de pago debe ser mayor que 0 y menor o igual a 9.9999.");
        if (modelo.IdEstado <= 0) throw new BusinessException("El estado es obligatorio.");
        if (!await _context.Estados.AnyAsync(e => e.IdEstado == modelo.IdEstado)) throw new NotFoundException("Estado no encontrado.");
        if (await _context.TipoHoraExtras.AnyAsync(x => x.Nombre == modelo.Nombre && x.IdTipoHoraExtra != id)) throw new BusinessException("Ya existe un tipo de hora extra con ese nombre.");
    }
    private async Task<int> ObtenerIdEstadoPorNombre(string nombre)
    {
        var estado = await _context.Estados.FirstOrDefaultAsync(e => e.Nombre == nombre);
        if (estado == null) throw new BusinessException($"No se encontrÃ³ el estado '{nombre}'.");
        return estado.IdEstado;
    }
}
