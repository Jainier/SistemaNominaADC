using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class AsistenciaService : IAsistenciaService
{
    private readonly ApplicationDbContext _context;

    public AsistenciaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Asistencia>> Historial(int? idEmpleado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        var query = _context.Asistencias
            .Include(a => a.Empleado)
            .Include(a => a.Estado)
            .AsQueryable();

        if (idEmpleado.HasValue && idEmpleado.Value > 0)
            query = query.Where(a => a.IdEmpleado == idEmpleado.Value);

        if (fechaDesde.HasValue)
        {
            var desde = fechaDesde.Value.Date;
            query = query.Where(a => a.Fecha >= desde);
        }

        if (fechaHasta.HasValue)
        {
            var hasta = fechaHasta.Value.Date;
            query = query.Where(a => a.Fecha <= hasta);
        }

        return await query
            .OrderByDescending(a => a.Fecha)
            .ThenByDescending(a => a.HoraEntrada ?? a.HoraSalida)
            .ToListAsync();
    }

    public async Task<Asistencia> RegistrarEntrada(AsistenciaMarcaDTO dto)
    {
        await ValidarMarca(dto);

        var fechaHora = DateTime.Now;
        var fecha = fechaHora.Date;

        var asistenciaDia = await _context.Asistencias
            .FirstOrDefaultAsync(a => a.IdEmpleado == dto.IdEmpleado && a.Fecha == fecha);

        if (asistenciaDia is not null)
        {
            if (asistenciaDia.HoraEntrada.HasValue)
                throw new BusinessException("Ya existe una marca de entrada para este empleado en la fecha indicada.");

            asistenciaDia.HoraEntrada = fechaHora;
            asistenciaDia.Ausencia = false;
            if (!string.IsNullOrWhiteSpace(dto.Justificacion))
                asistenciaDia.Justificacion = dto.Justificacion.Trim();

            await _context.SaveChangesAsync();
            return await ObtenerConRelaciones(asistenciaDia.IdAsistencia);
        }

        var nueva = new Asistencia
        {
            IdEmpleado = dto.IdEmpleado,
            Fecha = fecha,
            HoraEntrada = fechaHora,
            HoraSalida = null,
            Ausencia = false,
            Justificacion = string.IsNullOrWhiteSpace(dto.Justificacion) ? null : dto.Justificacion.Trim(),
            IdEstado = await ObtenerIdEstadoActivo()
        };

        _context.Asistencias.Add(nueva);
        await _context.SaveChangesAsync();

        return await ObtenerConRelaciones(nueva.IdAsistencia);
    }

    public async Task<Asistencia> RegistrarSalida(AsistenciaMarcaDTO dto)
    {
        await ValidarMarca(dto);

        var fechaHora = DateTime.Now;
        var fecha = fechaHora.Date;

        var asistenciaDia = await _context.Asistencias
            .FirstOrDefaultAsync(a => a.IdEmpleado == dto.IdEmpleado && a.Fecha == fecha);

        if (asistenciaDia is null)
            throw new BusinessException("No existe una marca de entrada para este empleado en la fecha indicada.");

        if (!asistenciaDia.HoraEntrada.HasValue)
            throw new BusinessException("No se puede registrar salida sin una marca de entrada previa.");

        if (asistenciaDia.HoraSalida.HasValue)
            throw new BusinessException("Ya existe una marca de salida para este empleado en la fecha indicada.");

        if (fechaHora < asistenciaDia.HoraEntrada.Value)
            throw new BusinessException("La hora de salida no puede ser menor que la hora de entrada.");

        asistenciaDia.HoraSalida = fechaHora;
        if (!string.IsNullOrWhiteSpace(dto.Justificacion))
            asistenciaDia.Justificacion = dto.Justificacion.Trim();

        await _context.SaveChangesAsync();
        return await ObtenerConRelaciones(asistenciaDia.IdAsistencia);
    }

    private async Task ValidarMarca(AsistenciaMarcaDTO dto)
    {
        if (dto is null)
            throw new BusinessException("Los datos de la marca son obligatorios.");

        if (dto.IdEmpleado <= 0)
            throw new BusinessException("El empleado es obligatorio.");

        var empleadoExiste = await _context.Empleados.AnyAsync(e => e.IdEmpleado == dto.IdEmpleado);
        if (!empleadoExiste)
            throw new NotFoundException("Empleado no encontrado.");
    }

    private async Task<int> ObtenerIdEstadoActivo()
    {
        var estado = await _context.Estados
            .Where(e => e.EstadoActivo == true || e.Nombre == "Activo")
            .OrderByDescending(e => e.Nombre == "Activo")
            .FirstOrDefaultAsync();

        if (estado is null)
            throw new BusinessException("No se encontró un estado activo para registrar asistencia.");

        return estado.IdEstado;
    }

    private async Task<Asistencia> ObtenerConRelaciones(int id)
    {
        return await _context.Asistencias
            .Include(a => a.Empleado)
            .Include(a => a.Estado)
            .FirstOrDefaultAsync(a => a.IdAsistencia == id)
            ?? throw new NotFoundException("Asistencia no encontrada.");
    }
}
