using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios;

public class DepartamentoJefaturaService : IDepartamentoJefaturaService
{
    private readonly ApplicationDbContext _context;

    public DepartamentoJefaturaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartamentoJefatura>> ListaAsync(int? idDepartamento = null, bool soloActivos = true)
    {
        var query = _context.DepartamentoJefaturas
            .Include(x => x.Departamento)
            .Include(x => x.Empleado)
            .ThenInclude(e => e!.Puesto)
            .AsQueryable();

        if (idDepartamento.HasValue && idDepartamento.Value > 0)
            query = query.Where(x => x.IdDepartamento == idDepartamento.Value);

        if (soloActivos)
            query = query.Where(x => x.Activo);

        return await query
            .OrderBy(x => x.IdDepartamento)
            .ThenBy(x => x.TipoJefatura)
            .ThenBy(x => x.IdDepartamentoJefatura)
            .ToListAsync();
    }

    public async Task<DepartamentoJefatura> ObtenerAsync(int idDepartamentoJefatura)
    {
        if (idDepartamentoJefatura <= 0)
            throw new BusinessException("El id de jefatura es inválido.");

        return await _context.DepartamentoJefaturas
            .Include(x => x.Departamento)
            .Include(x => x.Empleado)
            .FirstOrDefaultAsync(x => x.IdDepartamentoJefatura == idDepartamentoJefatura)
            ?? throw new NotFoundException("Jefatura no encontrada.");
    }

    public async Task<DepartamentoJefatura> CrearAsync(DepartamentoJefatura entidad)
    {
        await ValidarAsync(entidad, 0);
        _context.DepartamentoJefaturas.Add(entidad);
        await _context.SaveChangesAsync();
        return await ObtenerAsync(entidad.IdDepartamentoJefatura);
    }

    public async Task<DepartamentoJefatura> ActualizarAsync(DepartamentoJefatura entidad)
    {
        if (entidad.IdDepartamentoJefatura <= 0)
            throw new BusinessException("El id de jefatura es inválido.");

        await ValidarAsync(entidad, entidad.IdDepartamentoJefatura);

        var actual = await _context.DepartamentoJefaturas
            .FirstOrDefaultAsync(x => x.IdDepartamentoJefatura == entidad.IdDepartamentoJefatura)
            ?? throw new NotFoundException("Jefatura no encontrada.");

        actual.IdDepartamento = entidad.IdDepartamento;
        actual.IdEmpleado = entidad.IdEmpleado;
        actual.TipoJefatura = NormalizarTipo(entidad.TipoJefatura);
        actual.Activo = entidad.Activo;
        actual.VigenciaDesde = entidad.VigenciaDesde?.Date;
        actual.VigenciaHasta = entidad.VigenciaHasta?.Date;

        await _context.SaveChangesAsync();
        return await ObtenerAsync(actual.IdDepartamentoJefatura);
    }

    public async Task DesactivarAsync(int idDepartamentoJefatura)
    {
        var actual = await _context.DepartamentoJefaturas
            .FirstOrDefaultAsync(x => x.IdDepartamentoJefatura == idDepartamentoJefatura)
            ?? throw new NotFoundException("Jefatura no encontrada.");

        if (!actual.Activo) return;

        actual.Activo = false;
        await _context.SaveChangesAsync();
    }

    public async Task<List<int>> ObtenerDepartamentosGestionadosPorUserIdAsync(string identityUserId)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            return new List<int>();

        var idEmpleado = await _context.Empleados
            .Where(e => e.IdentityUserId == identityUserId)
            .Select(e => e.IdEmpleado)
            .FirstOrDefaultAsync();

        if (idEmpleado <= 0)
            return new List<int>();

        return await _context.DepartamentoJefaturas
            .Where(x => x.Activo && x.IdEmpleado == idEmpleado)
            .Select(x => x.IdDepartamento)
            .Distinct()
            .ToListAsync();
    }

    private async Task ValidarAsync(DepartamentoJefatura entidad, int idActual)
    {
        if (entidad is null)
            throw new BusinessException("La jefatura es obligatoria.");

        if (entidad.IdDepartamento <= 0)
            throw new BusinessException("El departamento es obligatorio.");

        if (entidad.IdEmpleado <= 0)
            throw new BusinessException("El empleado es obligatorio.");

        entidad.TipoJefatura = NormalizarTipo(entidad.TipoJefatura);

        if (entidad.VigenciaDesde.HasValue && entidad.VigenciaHasta.HasValue
            && entidad.VigenciaDesde.Value.Date > entidad.VigenciaHasta.Value.Date)
        {
            throw new BusinessException("La vigencia desde no puede ser mayor que la vigencia hasta.");
        }

        var departamentoExiste = await _context.Departamentos.AnyAsync(x => x.IdDepartamento == entidad.IdDepartamento);
        if (!departamentoExiste)
            throw new NotFoundException("Departamento no encontrado.");

        var empleado = await _context.Empleados
            .Include(x => x.Puesto)
            .FirstOrDefaultAsync(x => x.IdEmpleado == entidad.IdEmpleado)
            ?? throw new NotFoundException("Empleado no encontrado.");

        if (empleado.Puesto is null || empleado.Puesto.IdDepartamento != entidad.IdDepartamento)
            throw new BusinessException("El empleado debe pertenecer al mismo departamento que gestiona.");

        if (entidad.Activo)
        {
            var activosDepartamento = await _context.DepartamentoJefaturas
                .Where(x => x.Activo && x.IdDepartamento == entidad.IdDepartamento && x.IdDepartamentoJefatura != idActual)
                .CountAsync();

            if (activosDepartamento >= 2)
                throw new BusinessException("Solo se permiten dos jefaturas activas por departamento (líder y sublíder).");

            var existeEmpleadoActivo = await _context.DepartamentoJefaturas
                .AnyAsync(x =>
                    x.Activo &&
                    x.IdDepartamento == entidad.IdDepartamento &&
                    x.IdEmpleado == entidad.IdEmpleado &&
                    x.IdDepartamentoJefatura != idActual);

            if (existeEmpleadoActivo)
                throw new BusinessException("El empleado ya está asignado como jefatura activa en este departamento.");

            var existeTipoActivo = await _context.DepartamentoJefaturas
                .AnyAsync(x =>
                    x.Activo &&
                    x.IdDepartamento == entidad.IdDepartamento &&
                    x.TipoJefatura == entidad.TipoJefatura &&
                    x.IdDepartamentoJefatura != idActual);

            if (existeTipoActivo)
                throw new BusinessException($"Ya existe una jefatura activa con tipo {entidad.TipoJefatura} en este departamento.");
        }
    }

    private static string NormalizarTipo(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new BusinessException("El tipo de jefatura es obligatorio.");

        var t = tipo.Trim().ToUpperInvariant();
        return t switch
        {
            "LIDER" => "Lider",
            "SUBLIDER" => "SubLider",
            _ => throw new BusinessException("Tipo de jefatura inválido. Debe ser Lider o SubLider.")
        };
    }
}
