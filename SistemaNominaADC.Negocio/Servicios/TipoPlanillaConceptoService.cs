using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class TipoPlanillaConceptoService : ITipoPlanillaConceptoService
{
    private readonly ApplicationDbContext _context;

    public TipoPlanillaConceptoService(ApplicationDbContext context) => _context = context;

    public Task<List<TipoPlanillaConcepto>> Lista(int? idTipoPlanilla = null)
    {
        var query = _context.TiposPlanillaConcepto
            .Include(x => x.TipoPlanilla)
            .Include(x => x.TipoConceptoNomina)
            .AsQueryable();

        if (idTipoPlanilla.HasValue && idTipoPlanilla.Value > 0)
            query = query.Where(x => x.IdTipoPlanilla == idTipoPlanilla.Value);

        return query
            .OrderBy(x => x.IdTipoPlanilla)
            .ThenBy(x => x.Prioridad)
            .ThenBy(x => x.IdConceptoNomina)
            .ToListAsync();
    }

    public async Task<TipoPlanillaConcepto> Obtener(int idTipoPlanilla, int idConceptoNomina) =>
        await _context.TiposPlanillaConcepto
            .Include(x => x.TipoPlanilla)
            .Include(x => x.TipoConceptoNomina)
            .FirstOrDefaultAsync(x => x.IdTipoPlanilla == idTipoPlanilla && x.IdConceptoNomina == idConceptoNomina)
        ?? throw new NotFoundException("Configuracion de concepto por tipo de planilla no encontrada.");

    public async Task<TipoPlanillaConcepto> Crear(TipoPlanillaConcepto modelo)
    {
        await Validar(modelo, esNuevo: true);
        _context.TiposPlanillaConcepto.Add(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task<bool> Actualizar(TipoPlanillaConcepto modelo)
    {
        await Validar(modelo, esNuevo: false);

        var actual = await _context.TiposPlanillaConcepto
            .FirstOrDefaultAsync(x => x.IdTipoPlanilla == modelo.IdTipoPlanilla && x.IdConceptoNomina == modelo.IdConceptoNomina)
            ?? throw new NotFoundException("Configuracion de concepto por tipo de planilla no encontrada.");

        actual.Activo = modelo.Activo;
        actual.Obligatorio = modelo.Obligatorio;
        actual.PermiteMontoManual = modelo.PermiteMontoManual;
        actual.Prioridad = modelo.Prioridad;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Desactivar(int idTipoPlanilla, int idConceptoNomina)
    {
        var actual = await _context.TiposPlanillaConcepto
            .FirstOrDefaultAsync(x => x.IdTipoPlanilla == idTipoPlanilla && x.IdConceptoNomina == idConceptoNomina)
            ?? throw new NotFoundException("Configuracion de concepto por tipo de planilla no encontrada.");

        actual.Activo = false;
        return await _context.SaveChangesAsync() > 0;
    }

    private async Task Validar(TipoPlanillaConcepto modelo, bool esNuevo)
    {
        if (modelo.IdTipoPlanilla <= 0)
            throw new BusinessException("El tipo de planilla es obligatorio.");

        if (modelo.IdConceptoNomina <= 0)
            throw new BusinessException("El concepto de nomina es obligatorio.");

        if (modelo.Prioridad < 0)
            throw new BusinessException("La prioridad no puede ser negativa.");

        if (!await _context.TiposPlanilla.AnyAsync(x => x.IdTipoPlanilla == modelo.IdTipoPlanilla))
            throw new NotFoundException("Tipo de planilla no encontrado.");

        if (!await _context.TiposConceptoNomina.AnyAsync(x => x.IdConceptoNomina == modelo.IdConceptoNomina))
            throw new NotFoundException("Concepto de nomina no encontrado.");

        if (esNuevo)
        {
            var existe = await _context.TiposPlanillaConcepto
                .AnyAsync(x => x.IdTipoPlanilla == modelo.IdTipoPlanilla && x.IdConceptoNomina == modelo.IdConceptoNomina);

            if (existe)
                throw new BusinessException("Ese concepto ya esta configurado para el tipo de planilla.");
        }
    }
}
