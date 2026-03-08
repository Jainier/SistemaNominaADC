using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class EmpleadoJerarquiaService : IEmpleadoJerarquiaService
{
    private readonly ApplicationDbContext _context;

    public EmpleadoJerarquiaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmpleadoJerarquia>> ListaAsync(int? idSupervisor = null, int? idEmpleado = null, bool soloActivos = true)
    {
        var hoy = DateTime.UtcNow.Date;

        var query = _context.EmpleadoJerarquias
            .Include(x => x.Empleado)
            .Include(x => x.Supervisor)
            .AsQueryable();

        if (idSupervisor.HasValue && idSupervisor.Value > 0)
            query = query.Where(x => x.IdSupervisor == idSupervisor.Value);

        if (idEmpleado.HasValue && idEmpleado.Value > 0)
            query = query.Where(x => x.IdEmpleado == idEmpleado.Value);

        if (soloActivos)
        {
            query = query.Where(x =>
                x.Activo &&
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde <= hoy) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta >= hoy));
        }

        return await query
            .OrderBy(x => x.IdSupervisor)
            .ThenBy(x => x.IdEmpleado)
            .ThenBy(x => x.IdEmpleadoJerarquia)
            .ToListAsync();
    }

    public async Task<EmpleadoJerarquia> ObtenerAsync(int idEmpleadoJerarquia)
    {
        if (idEmpleadoJerarquia <= 0)
            throw new BusinessException("El id de la relación es inválido.");

        return await _context.EmpleadoJerarquias
            .Include(x => x.Empleado)
            .Include(x => x.Supervisor)
            .FirstOrDefaultAsync(x => x.IdEmpleadoJerarquia == idEmpleadoJerarquia)
            ?? throw new NotFoundException("Relación de jerarquía no encontrada.");
    }

    public async Task<EmpleadoJerarquia> CrearAsync(EmpleadoJerarquia entidad)
    {
        await ValidarAsync(entidad, 0);
        _context.EmpleadoJerarquias.Add(entidad);
        await _context.SaveChangesAsync();
        return await ObtenerAsync(entidad.IdEmpleadoJerarquia);
    }

    public async Task<EmpleadoJerarquia> ActualizarAsync(EmpleadoJerarquia entidad)
    {
        if (entidad.IdEmpleadoJerarquia <= 0)
            throw new BusinessException("El id de la relación es inválido.");

        await ValidarAsync(entidad, entidad.IdEmpleadoJerarquia);

        var actual = await _context.EmpleadoJerarquias
            .FirstOrDefaultAsync(x => x.IdEmpleadoJerarquia == entidad.IdEmpleadoJerarquia)
            ?? throw new NotFoundException("Relación de jerarquía no encontrada.");

        actual.IdEmpleado = entidad.IdEmpleado;
        actual.IdSupervisor = entidad.IdSupervisor;
        actual.Activo = entidad.Activo;
        actual.VigenciaDesde = entidad.VigenciaDesde?.Date;
        actual.VigenciaHasta = entidad.VigenciaHasta?.Date;
        actual.Observacion = string.IsNullOrWhiteSpace(entidad.Observacion) ? null : entidad.Observacion.Trim();

        await _context.SaveChangesAsync();
        return await ObtenerAsync(actual.IdEmpleadoJerarquia);
    }

    public async Task DesactivarAsync(int idEmpleadoJerarquia)
    {
        var actual = await _context.EmpleadoJerarquias
            .FirstOrDefaultAsync(x => x.IdEmpleadoJerarquia == idEmpleadoJerarquia)
            ?? throw new NotFoundException("Relación de jerarquía no encontrada.");

        if (!actual.Activo) return;

        actual.Activo = false;
        await _context.SaveChangesAsync();
    }

    private async Task ValidarAsync(EmpleadoJerarquia entidad, int idActual)
    {
        if (entidad is null)
            throw new BusinessException("La relación de jerarquía es obligatoria.");

        if (entidad.IdEmpleado <= 0)
            throw new BusinessException("El empleado es obligatorio.");

        if (entidad.IdSupervisor <= 0)
            throw new BusinessException("El supervisor es obligatorio.");

        if (entidad.IdEmpleado == entidad.IdSupervisor)
            throw new BusinessException("Un empleado no puede ser su propio supervisor.");

        if (entidad.VigenciaDesde.HasValue && entidad.VigenciaHasta.HasValue
            && entidad.VigenciaDesde.Value.Date > entidad.VigenciaHasta.Value.Date)
        {
            throw new BusinessException("La vigencia desde no puede ser mayor que la vigencia hasta.");
        }

        var empleadoExiste = await _context.Empleados.AnyAsync(x => x.IdEmpleado == entidad.IdEmpleado);
        if (!empleadoExiste)
            throw new NotFoundException("Empleado no encontrado.");

        var supervisorExiste = await _context.Empleados.AnyAsync(x => x.IdEmpleado == entidad.IdSupervisor);
        if (!supervisorExiste)
            throw new NotFoundException("Supervisor no encontrado.");

        var duplicada = await _context.EmpleadoJerarquias.AnyAsync(x =>
            x.IdEmpleado == entidad.IdEmpleado &&
            x.IdSupervisor == entidad.IdSupervisor &&
            x.IdEmpleadoJerarquia != idActual);
        if (duplicada)
            throw new BusinessException("Ya existe una relación igual en el organigrama.");

        if (entidad.Activo)
        {
            var tieneOtraActiva = await _context.EmpleadoJerarquias.AnyAsync(x =>
                x.IdEmpleado == entidad.IdEmpleado &&
                x.Activo &&
                x.IdEmpleadoJerarquia != idActual);
            if (tieneOtraActiva)
                throw new BusinessException("El empleado ya tiene un supervisor activo en el organigrama.");
        }

        if (await ProvocaCicloAsync(entidad.IdEmpleado, entidad.IdSupervisor, idActual))
            throw new BusinessException("La relación genera un ciclo en el organigrama.");
    }

    private async Task<bool> ProvocaCicloAsync(int idEmpleado, int idSupervisor, int idActual)
    {
        var relaciones = await _context.EmpleadoJerarquias
            .Where(x => x.Activo && x.IdEmpleadoJerarquia != idActual)
            .Select(x => new { x.IdEmpleado, x.IdSupervisor })
            .ToListAsync();

        relaciones.Add(new { IdEmpleado = idEmpleado, IdSupervisor = idSupervisor });

        var supervisorPorEmpleado = relaciones
            .GroupBy(x => x.IdEmpleado)
            .ToDictionary(g => g.Key, g => g.Select(x => x.IdSupervisor).ToList());

        var visitados = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(idSupervisor);

        while (stack.Count > 0)
        {
            var actual = stack.Pop();
            if (!visitados.Add(actual))
                continue;

            if (actual == idEmpleado)
                return true;

            if (supervisorPorEmpleado.TryGetValue(actual, out var supervisores))
            {
                foreach (var sup in supervisores)
                    stack.Push(sup);
            }
        }

        return false;
    }
}
