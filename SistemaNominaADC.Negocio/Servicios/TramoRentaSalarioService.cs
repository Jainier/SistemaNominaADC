using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class TramoRentaSalarioService : ITramoRentaSalarioService
{
    private readonly ApplicationDbContext _context;

    public TramoRentaSalarioService(ApplicationDbContext context) => _context = context;

    public Task<List<TramoRentaSalario>> Lista() =>
        _context.TramosRentaSalario
            .OrderBy(x => x.VigenciaDesde)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.DesdeMonto)
            .ToListAsync();

    public async Task<TramoRentaSalario> Obtener(int id) =>
        await _context.TramosRentaSalario
            .FirstOrDefaultAsync(x => x.IdTramoRentaSalario == id)
        ?? throw new NotFoundException("Tramo de renta no encontrado.");

    public async Task<TramoRentaSalario> Crear(TramoRentaSalario modelo)
    {
        await Validar(modelo, 0);
        _context.TramosRentaSalario.Add(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task<bool> Actualizar(TramoRentaSalario modelo)
    {
        await Validar(modelo, modelo.IdTramoRentaSalario);

        var actual = await _context.TramosRentaSalario
            .FirstOrDefaultAsync(x => x.IdTramoRentaSalario == modelo.IdTramoRentaSalario)
            ?? throw new NotFoundException("Tramo de renta no encontrado.");

        actual.DesdeMonto = modelo.DesdeMonto;
        actual.HastaMonto = modelo.HastaMonto;
        actual.Tasa = modelo.Tasa;
        actual.VigenciaDesde = modelo.VigenciaDesde;
        actual.VigenciaHasta = modelo.VigenciaHasta;
        actual.Orden = modelo.Orden;
        actual.Activo = modelo.Activo;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Desactivar(int id)
    {
        var actual = await _context.TramosRentaSalario
            .FirstOrDefaultAsync(x => x.IdTramoRentaSalario == id)
            ?? throw new NotFoundException("Tramo de renta no encontrado.");

        actual.Activo = false;
        return await _context.SaveChangesAsync() > 0;
    }

    private async Task Validar(TramoRentaSalario modelo, int id)
    {
        if (modelo.DesdeMonto < 0m) throw new BusinessException("El monto desde no puede ser negativo.");
        if (modelo.HastaMonto.HasValue && modelo.HastaMonto.Value < 0m) throw new BusinessException("El monto hasta no puede ser negativo.");
        if (modelo.HastaMonto.HasValue && modelo.HastaMonto.Value <= modelo.DesdeMonto)
            throw new BusinessException("El monto hasta debe ser mayor al monto desde.");
        if (modelo.Tasa < 0m || modelo.Tasa > 1m) throw new BusinessException("La tasa debe estar entre 0 y 1.");
        if (modelo.VigenciaHasta.HasValue && modelo.VigenciaHasta.Value.Date < modelo.VigenciaDesde.Date)
            throw new BusinessException("La vigencia hasta no puede ser menor a la vigencia desde.");

        var traslape = await _context.TramosRentaSalario
            .Where(x =>
                x.IdTramoRentaSalario != id &&
                x.Activo &&
                x.VigenciaDesde.Date <= (modelo.VigenciaHasta ?? DateTime.MaxValue).Date &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value.Date >= modelo.VigenciaDesde.Date))
            .AnyAsync(x =>
                RangosSolapan(modelo.DesdeMonto, modelo.HastaMonto, x.DesdeMonto, x.HastaMonto));

        if (traslape)
            throw new BusinessException("Ya existe un tramo de renta que se traslapa con el rango y vigencia indicados.");
    }

    private static bool RangosSolapan(decimal desdeA, decimal? hastaA, decimal desdeB, decimal? hastaB)
    {
        var finA = hastaA ?? decimal.MaxValue;
        var finB = hastaB ?? decimal.MaxValue;
        return desdeA < finB && desdeB < finA;
    }
}
