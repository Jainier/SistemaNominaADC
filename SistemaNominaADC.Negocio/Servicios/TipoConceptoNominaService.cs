using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class TipoConceptoNominaService : ITipoConceptoNominaService
{
    private readonly ApplicationDbContext _context;

    public TipoConceptoNominaService(ApplicationDbContext context) => _context = context;

    public Task<List<TipoConceptoNomina>> Lista() =>
        _context.TiposConceptoNomina
            .Include(x => x.ModoCalculo)
            .Include(x => x.Estado)
            .ToListAsync();

    public async Task<TipoConceptoNomina> Obtener(int id) =>
        await _context.TiposConceptoNomina
            .Include(x => x.ModoCalculo)
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.IdConceptoNomina == id)
        ?? throw new NotFoundException("Tipo de concepto de nomina no encontrado.");

    public async Task<TipoConceptoNomina> Crear(TipoConceptoNomina modelo)
    {
        await Validar(modelo, 0);
        _context.TiposConceptoNomina.Add(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task<bool> Actualizar(TipoConceptoNomina modelo)
    {
        await Validar(modelo, modelo.IdConceptoNomina);

        var actual = await _context.TiposConceptoNomina
            .FirstOrDefaultAsync(x => x.IdConceptoNomina == modelo.IdConceptoNomina)
            ?? throw new NotFoundException("Tipo de concepto de nomina no encontrado.");

        actual.Nombre = modelo.Nombre;
        actual.CodigoConcepto = string.IsNullOrWhiteSpace(modelo.CodigoConcepto) ? null : modelo.CodigoConcepto.Trim();
        actual.IdModoCalculo = modelo.IdModoCalculo;
        actual.FormulaCalculo = modelo.FormulaCalculo;
        actual.CodigoFormula = string.IsNullOrWhiteSpace(modelo.CodigoFormula) ? null : modelo.CodigoFormula.Trim();
        actual.ValorPorcentaje = modelo.ValorPorcentaje;
        actual.ValorFijo = modelo.ValorFijo;
        actual.OrdenCalculo = modelo.OrdenCalculo;
        actual.EsIngreso = modelo.EsIngreso;
        actual.EsDeduccion = modelo.EsDeduccion;
        actual.AfectaCcss = modelo.AfectaCcss;
        actual.AfectaRenta = modelo.AfectaRenta;
        actual.IdEstado = modelo.IdEstado;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Desactivar(int id)
    {
        var actual = await _context.TiposConceptoNomina
            .FirstOrDefaultAsync(x => x.IdConceptoNomina == id)
            ?? throw new NotFoundException("Tipo de concepto de nomina no encontrado.");

        actual.IdEstado = await EstadoSistemaHelper.ObtenerIdEstadoInactivoAsync(_context);
        return await _context.SaveChangesAsync() > 0;
    }

    private async Task Validar(TipoConceptoNomina modelo, int id)
    {
        if (string.IsNullOrWhiteSpace(modelo.Nombre)) throw new BusinessException("El nombre es obligatorio.");
        if (!string.IsNullOrWhiteSpace(modelo.CodigoConcepto) && modelo.CodigoConcepto.Length > 40)
            throw new BusinessException("El codigo de concepto no debe exceder 40 caracteres.");
        if (!modelo.EsIngreso && !modelo.EsDeduccion) throw new BusinessException("Debe marcar ingreso o deduccion.");
        if (modelo.EsIngreso && modelo.EsDeduccion) throw new BusinessException("Un concepto no puede ser ingreso y deduccion al mismo tiempo.");
        if (modelo.IdModoCalculo <= 0) throw new BusinessException("El modo de calculo es obligatorio.");
        if (modelo.IdEstado <= 0) throw new BusinessException("El estado es obligatorio.");

        var modo = await _context.ModosCalculoConceptoNomina
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdModoCalculoConceptoNomina == modelo.IdModoCalculo);
        if (modo is null) throw new NotFoundException("Modo de calculo no encontrado.");

        if (!await _context.Estados.AnyAsync(e => e.IdEstado == modelo.IdEstado))
            throw new NotFoundException("Estado no encontrado.");

        if (await _context.TiposConceptoNomina.AnyAsync(x => x.Nombre == modelo.Nombre && x.IdConceptoNomina != id))
            throw new BusinessException("Ya existe un tipo de concepto con ese nombre.");

        var modoNombre = modo.Nombre?.Trim() ?? string.Empty;
        if (string.Equals(modoNombre, "Porcentaje", StringComparison.OrdinalIgnoreCase))
        {
            if (!modelo.ValorPorcentaje.HasValue)
                throw new BusinessException("Debe indicar valor porcentaje para conceptos de tipo porcentaje.");
            if (modelo.ValorPorcentaje < 0 || modelo.ValorPorcentaje > 1)
                throw new BusinessException("El valor porcentaje debe estar entre 0 y 1.");
        }

        if (string.Equals(modoNombre, "Fijo", StringComparison.OrdinalIgnoreCase))
        {
            if (!modelo.ValorFijo.HasValue)
                throw new BusinessException("Debe indicar valor fijo para conceptos de tipo fijo.");
            if (modelo.ValorFijo < 0)
                throw new BusinessException("El valor fijo no puede ser negativo.");
        }

        var codigoNormalizado = string.IsNullOrWhiteSpace(modelo.CodigoConcepto)
            ? null
            : modelo.CodigoConcepto.Trim();
        if (!string.IsNullOrWhiteSpace(codigoNormalizado) &&
            await _context.TiposConceptoNomina.AnyAsync(x =>
                x.CodigoConcepto == codigoNormalizado && x.IdConceptoNomina != id))
            throw new BusinessException("Ya existe un tipo de concepto con ese codigo.");

        modelo.CodigoConcepto = codigoNormalizado;
        modelo.CodigoFormula = string.IsNullOrWhiteSpace(modelo.CodigoFormula) ? null : modelo.CodigoFormula.Trim();
    }
}
