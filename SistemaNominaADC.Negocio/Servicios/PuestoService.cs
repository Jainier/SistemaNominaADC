using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class PuestoService : IPuestoService
{
    private readonly ApplicationDbContext _context;
    public PuestoService(ApplicationDbContext context) => _context = context;

    public Task<List<Puesto>> Lista() => _context.Puestos.Include(p => p.Departamento).ToListAsync();

    public async Task<Puesto> Obtener(int id) => await _context.Puestos.Include(p => p.Departamento)
        .FirstOrDefaultAsync(x => x.IdPuesto == id) ?? throw new NotFoundException("Puesto no encontrado.");

    public async Task<Puesto> Crear(Puesto modelo)
    {
        await Validar(modelo);
        _context.Puestos.Add(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task<bool> Actualizar(Puesto modelo)
    {
        await Validar(modelo, modelo.IdPuesto);
        var actual = await _context.Puestos.FirstOrDefaultAsync(x => x.IdPuesto == modelo.IdPuesto)
            ?? throw new NotFoundException("Puesto no encontrado.");
        actual.Nombre = modelo.Nombre;
        actual.IdDepartamento = modelo.IdDepartamento;
        actual.SalarioBase = modelo.SalarioBase;
        actual.Estado = modelo.Estado;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Desactivar(int id)
    {
        var actual = await _context.Puestos.FirstOrDefaultAsync(x => x.IdPuesto == id)
            ?? throw new NotFoundException("Puesto no encontrado.");

        var empleadosActivos = await _context.Empleados.AnyAsync(e => e.IdPuesto == id && e.Estado);
        if (empleadosActivos)
            throw new BusinessException("No se puede desactivar el puesto porque tiene empleados activos asociados.");

        actual.Estado = false;
        return await _context.SaveChangesAsync() > 0;
    }

    private async Task Validar(Puesto modelo, int idActual = 0)
    {
        if (string.IsNullOrWhiteSpace(modelo.Nombre)) throw new BusinessException("El nombre es obligatorio.");
        if (modelo.IdDepartamento <= 0) throw new BusinessException("El departamento es obligatorio.");
        var deptoExiste = await _context.Departamentos.AnyAsync(d => d.IdDepartamento == modelo.IdDepartamento);
        if (!deptoExiste) throw new NotFoundException("Departamento no encontrado.");
        var duplicado = await _context.Puestos.AnyAsync(p => p.IdDepartamento == modelo.IdDepartamento && p.Nombre == modelo.Nombre && p.IdPuesto != idActual);
        if (duplicado) throw new BusinessException("Ya existe un puesto con el mismo nombre en el departamento indicado.");
    }
}
