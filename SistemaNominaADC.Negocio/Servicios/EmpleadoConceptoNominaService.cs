using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class EmpleadoConceptoNominaService : IEmpleadoConceptoNominaService
{
    private readonly ApplicationDbContext _context;

    public EmpleadoConceptoNominaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<EmpleadoConceptoNomina>> Lista() =>
        _context.EmpleadosConceptoNomina
            .Include(x => x.Empleado)
            .Include(x => x.TipoConceptoNomina)
            .OrderBy(x => x.IdEmpleado)
            .ThenBy(x => x.Prioridad)
            .ToListAsync();

    public async Task<EmpleadoConceptoNomina> Obtener(int id) =>
        await _context.EmpleadosConceptoNomina
            .Include(x => x.Empleado)
            .Include(x => x.TipoConceptoNomina)
            .FirstOrDefaultAsync(x => x.IdEmpleadoConceptoNomina == id)
        ?? throw new NotFoundException("Configuracion de concepto por empleado no encontrada.");

    public async Task<EmpleadoConceptoNomina> Crear(EmpleadoConceptoNomina modelo)
    {
        await Validar(modelo, 0);
        _context.EmpleadosConceptoNomina.Add(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task<bool> Actualizar(EmpleadoConceptoNomina modelo)
    {
        await Validar(modelo, modelo.IdEmpleadoConceptoNomina);

        var actual = await _context.EmpleadosConceptoNomina
            .FirstOrDefaultAsync(x => x.IdEmpleadoConceptoNomina == modelo.IdEmpleadoConceptoNomina)
            ?? throw new NotFoundException("Configuracion de concepto por empleado no encontrada.");

        actual.IdEmpleado = modelo.IdEmpleado;
        actual.IdConceptoNomina = modelo.IdConceptoNomina;
        actual.MontoFijo = modelo.MontoFijo;
        actual.Porcentaje = modelo.Porcentaje;
        actual.SaldoPendiente = modelo.SaldoPendiente;
        actual.Prioridad = modelo.Prioridad;
        actual.VigenciaDesde = modelo.VigenciaDesde?.Date;
        actual.VigenciaHasta = modelo.VigenciaHasta?.Date;
        actual.Activo = modelo.Activo;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Desactivar(int id)
    {
        var actual = await _context.EmpleadosConceptoNomina
            .FirstOrDefaultAsync(x => x.IdEmpleadoConceptoNomina == id)
            ?? throw new NotFoundException("Configuracion de concepto por empleado no encontrada.");

        actual.Activo = false;
        return await _context.SaveChangesAsync() > 0;
    }

    private async Task Validar(EmpleadoConceptoNomina modelo, int id)
    {
        if (modelo.IdEmpleado <= 0) throw new BusinessException("El empleado es obligatorio.");
        if (modelo.IdConceptoNomina <= 0) throw new BusinessException("El concepto de nomina es obligatorio.");
        if (!modelo.MontoFijo.HasValue && !modelo.Porcentaje.HasValue)
            throw new BusinessException("Debe indicar monto fijo o porcentaje.");
        if (modelo.MontoFijo.HasValue && modelo.MontoFijo.Value < 0)
            throw new BusinessException("El monto fijo no puede ser negativo.");
        if (modelo.Porcentaje.HasValue && (modelo.Porcentaje.Value < 0 || modelo.Porcentaje.Value > 1))
            throw new BusinessException("El porcentaje debe estar entre 0 y 1.");
        if (modelo.SaldoPendiente.HasValue && modelo.SaldoPendiente.Value < 0)
            throw new BusinessException("El saldo pendiente no puede ser negativo.");
        if (modelo.VigenciaDesde.HasValue && modelo.VigenciaHasta.HasValue && modelo.VigenciaHasta.Value.Date < modelo.VigenciaDesde.Value.Date)
            throw new BusinessException("La vigencia hasta no puede ser menor que vigencia desde.");

        if (!await _context.Empleados.AnyAsync(x => x.IdEmpleado == modelo.IdEmpleado))
            throw new NotFoundException("Empleado no encontrado.");

        var concepto = await _context.TiposConceptoNomina
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdConceptoNomina == modelo.IdConceptoNomina)
            ?? throw new NotFoundException("Concepto de nomina no encontrado.");

        if (!concepto.EsDeduccion && !concepto.EsIngreso)
            throw new BusinessException("El concepto seleccionado no es aplicable para configuracion por empleado.");

        var fechaDesde = modelo.VigenciaDesde?.Date ?? DateTime.MinValue.Date;
        var fechaHasta = modelo.VigenciaHasta?.Date ?? DateTime.MaxValue.Date;

        var existeCruce = await _context.EmpleadosConceptoNomina
            .Where(x =>
                x.IdEmpleadoConceptoNomina != id &&
                x.Activo &&
                x.IdEmpleado == modelo.IdEmpleado &&
                x.IdConceptoNomina == modelo.IdConceptoNomina)
            .AnyAsync(x =>
                (x.VigenciaDesde ?? DateTime.MinValue).Date <= fechaHasta &&
                (x.VigenciaHasta ?? DateTime.MaxValue).Date >= fechaDesde);

        if (existeCruce)
            throw new BusinessException("Ya existe una configuracion activa de ese concepto para el empleado en un rango de vigencia que se cruza.");
    }
}
