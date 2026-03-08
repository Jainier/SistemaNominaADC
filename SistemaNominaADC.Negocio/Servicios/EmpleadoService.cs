using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class EmpleadoService : IEmpleadoService
{
    private readonly ApplicationDbContext _context;
    public EmpleadoService(ApplicationDbContext context) => _context = context;

    public Task<List<Empleado>> Lista() => _context.Empleados
        .Include(e => e.Puesto)
        .Include(e => e.Estado)
        .ToListAsync();

    public async Task<Empleado> Obtener(int id) => await _context.Empleados
        .Include(e => e.Puesto)
        .Include(e => e.Estado)
        .FirstOrDefaultAsync(x => x.IdEmpleado == id) ?? throw new NotFoundException("Empleado no encontrado.");

    public async Task<Empleado> Crear(Empleado modelo)
    {
        await Validar(modelo);
        _context.Empleados.Add(modelo);
        await _context.SaveChangesAsync();

        await CrearSaldoVacacionesInicialAsync(modelo);
        return modelo;
    }

    public async Task<bool> Actualizar(Empleado modelo)
    {
        await Validar(modelo, modelo.IdEmpleado);
        var actual = await _context.Empleados.FirstOrDefaultAsync(x => x.IdEmpleado == modelo.IdEmpleado)
            ?? throw new NotFoundException("Empleado no encontrado.");
        actual.Cedula = modelo.Cedula;
        actual.NombreCompleto = modelo.NombreCompleto;
        actual.IdPuesto = modelo.IdPuesto;
        actual.SalarioBase = modelo.SalarioBase;
        actual.FechaIngreso = modelo.FechaIngreso;
        actual.FechaSalida = modelo.FechaSalida;
        actual.IdEstado = modelo.IdEstado;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Desactivar(int id)
    {
        var actual = await _context.Empleados.FirstOrDefaultAsync(x => x.IdEmpleado == id)
            ?? throw new NotFoundException("Empleado no encontrado.");
        actual.IdEstado = await EstadoSistemaHelper.ObtenerIdEstadoInactivoAsync(_context);
        actual.FechaSalida ??= DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    private async Task Validar(Empleado modelo, int idActual = 0)
    {
        if (string.IsNullOrWhiteSpace(modelo.Cedula)) throw new BusinessException("La cedula es obligatoria.");
        if (string.IsNullOrWhiteSpace(modelo.NombreCompleto)) throw new BusinessException("El nombre es obligatorio.");
        if (modelo.IdPuesto <= 0) throw new BusinessException("El puesto es obligatorio.");
        var puestoExiste = await _context.Puestos.AnyAsync(p => p.IdPuesto == modelo.IdPuesto);
        if (!puestoExiste) throw new NotFoundException("Puesto no encontrado.");
        if (modelo.IdEstado <= 0) throw new BusinessException("El estado es obligatorio.");
        var estadoExiste = await _context.Estados.AnyAsync(e => e.IdEstado == modelo.IdEstado);
        if (!estadoExiste) throw new NotFoundException("Estado no encontrado.");
        var duplicado = await _context.Empleados.AnyAsync(e => e.Cedula == modelo.Cedula && e.IdEmpleado != idActual);
        if (duplicado) throw new BusinessException("Ya existe un empleado con la misma cedula.");
    }

    private async Task CrearSaldoVacacionesInicialAsync(Empleado empleado)
    {
        if (empleado.IdEmpleado <= 0)
            return;

        var existeSaldo = await _context.Vacaciones.AnyAsync(v => v.IdEmpleado == empleado.IdEmpleado);
        if (existeSaldo)
            return;

        var diasIniciales = CalcularDiasVacacionesPorMesesCumplidos(empleado.FechaIngreso, DateTime.Today);
        var idEstadoActivo = await EstadoSistemaHelper.ObtenerIdEstadoActivoAsync(_context);

        _context.Vacaciones.Add(new Vacaciones
        {
            IdEmpleado = empleado.IdEmpleado,
            DiasRestantes = diasIniciales,
            IdEstado = idEstadoActivo
        });

        await _context.SaveChangesAsync();
    }
    private static int CalcularDiasVacacionesPorMesesCumplidos(DateTime fechaIngreso, DateTime fechaCorte)
    {
        var ingreso = fechaIngreso.Date;
        var corte = fechaCorte.Date;
        if (ingreso > corte)
            return 0;

        var mesesCompletos = (corte.Year - ingreso.Year) * 12 + (corte.Month - ingreso.Month);
        if (corte.Day < ingreso.Day)
            mesesCompletos--;

        return Math.Max(0, mesesCompletos);
    }
}
