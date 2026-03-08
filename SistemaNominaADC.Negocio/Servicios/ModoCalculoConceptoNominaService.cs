using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class ModoCalculoConceptoNominaService : IModoCalculoConceptoNominaService
{
    private readonly ApplicationDbContext _context;

    public ModoCalculoConceptoNominaService(ApplicationDbContext context) => _context = context;

    public Task<List<ModoCalculoConceptoNomina>> Lista() =>
        _context.ModosCalculoConceptoNomina.Include(x => x.Estado).ToListAsync();

    public async Task<ModoCalculoConceptoNomina> Obtener(int id) =>
        await _context.ModosCalculoConceptoNomina
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.IdModoCalculoConceptoNomina == id)
        ?? throw new NotFoundException("Modo de calculo no encontrado.");

    public async Task<ModoCalculoConceptoNomina> Crear(ModoCalculoConceptoNomina modelo)
    {
        await Validar(modelo, 0);
        _context.ModosCalculoConceptoNomina.Add(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task<bool> Actualizar(ModoCalculoConceptoNomina modelo)
    {
        await Validar(modelo, modelo.IdModoCalculoConceptoNomina);
        var actual = await _context.ModosCalculoConceptoNomina
            .FirstOrDefaultAsync(x => x.IdModoCalculoConceptoNomina == modelo.IdModoCalculoConceptoNomina)
            ?? throw new NotFoundException("Modo de calculo no encontrado.");

        actual.Nombre = modelo.Nombre;
        actual.Descripcion = modelo.Descripcion;
        actual.IdEstado = modelo.IdEstado;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Desactivar(int id)
    {
        var actual = await _context.ModosCalculoConceptoNomina
            .FirstOrDefaultAsync(x => x.IdModoCalculoConceptoNomina == id)
            ?? throw new NotFoundException("Modo de calculo no encontrado.");

        actual.IdEstado = await EstadoSistemaHelper.ObtenerIdEstadoInactivoAsync(_context);
        return await _context.SaveChangesAsync() > 0;
    }

    private async Task Validar(ModoCalculoConceptoNomina modelo, int id)
    {
        if (string.IsNullOrWhiteSpace(modelo.Nombre)) throw new BusinessException("El nombre es obligatorio.");
        if (modelo.IdEstado <= 0) throw new BusinessException("El estado es obligatorio.");

        if (!await _context.Estados.AnyAsync(e => e.IdEstado == modelo.IdEstado))
            throw new NotFoundException("Estado no encontrado.");

        if (await _context.ModosCalculoConceptoNomina.AnyAsync(x =>
            x.Nombre == modelo.Nombre && x.IdModoCalculoConceptoNomina != id))
            throw new BusinessException("Ya existe un modo de calculo con ese nombre.");
    }
}
