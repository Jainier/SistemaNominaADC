using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class TipoPlanillaService : ITipoPlanillaService
{
    private readonly ApplicationDbContext _context;

    public TipoPlanillaService(ApplicationDbContext context) => _context = context;

    public Task<List<TipoPlanilla>> Lista(bool soloActivos = false)
    {
        var query = _context.TiposPlanilla.Include(x => x.Estado).AsQueryable();
        if (soloActivos)
        {
            query = query.Where(x => x.Estado != null && x.Estado.Codigo == EstadoCodigosSistema.Activo);
        }

        return query.ToListAsync();
    }

    public async Task<TipoPlanilla> Obtener(int id) =>
        await _context.TiposPlanilla
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.IdTipoPlanilla == id)
        ?? throw new NotFoundException("Tipo de planilla no encontrado.");

    public async Task<TipoPlanilla> Crear(TipoPlanilla modelo)
    {
        await Validar(modelo, 0);
        _context.TiposPlanilla.Add(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task<bool> Actualizar(TipoPlanilla modelo)
    {
        await Validar(modelo, modelo.IdTipoPlanilla);

        var actual = await _context.TiposPlanilla
            .FirstOrDefaultAsync(x => x.IdTipoPlanilla == modelo.IdTipoPlanilla)
            ?? throw new NotFoundException("Tipo de planilla no encontrado.");

        actual.Nombre = modelo.Nombre;
        actual.Descripcion = modelo.Descripcion;
        actual.ModoCalculo = modelo.ModoCalculo;
        actual.AportaBaseCcss = modelo.AportaBaseCcss;
        actual.AportaBaseRentaMensual = modelo.AportaBaseRentaMensual;
        actual.IdEstado = modelo.IdEstado;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Desactivar(int id)
    {
        var actual = await _context.TiposPlanilla
            .FirstOrDefaultAsync(x => x.IdTipoPlanilla == id)
            ?? throw new NotFoundException("Tipo de planilla no encontrado.");

        actual.IdEstado = await EstadoSistemaHelper.ObtenerIdEstadoInactivoAsync(_context);
        return await _context.SaveChangesAsync() > 0;
    }

    private async Task Validar(TipoPlanilla modelo, int id)
    {
        if (string.IsNullOrWhiteSpace(modelo.Nombre)) throw new BusinessException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(modelo.ModoCalculo)) throw new BusinessException("El modo de calculo es obligatorio.");
        if (modelo.IdEstado <= 0) throw new BusinessException("El estado es obligatorio.");

        var modo = modelo.ModoCalculo.Trim();
        if (!string.Equals(modo, "Regular", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(modo, "Extraordinaria", StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("El modo de calculo debe ser 'Regular' o 'Extraordinaria'.");

        modelo.ModoCalculo = string.Equals(modo, "Regular", StringComparison.OrdinalIgnoreCase)
            ? "Regular"
            : "Extraordinaria";

        if (!await _context.Estados.AnyAsync(e => e.IdEstado == modelo.IdEstado))
            throw new NotFoundException("Estado no encontrado.");

        if (await _context.TiposPlanilla.AnyAsync(x => x.Nombre == modelo.Nombre && x.IdTipoPlanilla != id))
            throw new BusinessException("Ya existe un tipo de planilla con ese nombre.");
    }
}
