using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTO;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios
{
    public class DepartamentoService : IDepartamentoService
    {
        private readonly ApplicationDbContext _context;

        public DepartamentoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Departamento>> Lista()
        {
            var filas = await _context.Departamentos
                .AsNoTracking()
                .Select(d => new
                {
                    d.IdDepartamento,
                    Nombre = EF.Property<string?>(d, nameof(Departamento.Nombre)),
                    IdEstado = EF.Property<int?>(d, nameof(Departamento.IdEstado)),
                    EstadoNombre = d.Estado != null ? d.Estado.Nombre : null
                })
                .ToListAsync();

            return filas
                .Where(f => f.IdEstado.HasValue && f.IdEstado.Value > 0 && !string.IsNullOrWhiteSpace(f.Nombre))
                .Select(f => new Departamento
                {
                    IdDepartamento = f.IdDepartamento,
                    Nombre = f.Nombre!,
                    IdEstado = f.IdEstado!.Value,
                    Estado = new Estado
                    {
                        IdEstado = f.IdEstado.Value,
                        Nombre = f.EstadoNombre
                    }
                })
                .ToList();
        }

        public async Task<Departamento?> Obtener(int id)
        {
            if (id <= 0)
                throw new BusinessException("El id del departamento es invalido.");

            var fila = await _context.Departamentos
                .AsNoTracking()
                .Where(d => d.IdDepartamento == id)
                .Select(d => new
                {
                    d.IdDepartamento,
                    Nombre = EF.Property<string?>(d, nameof(Departamento.Nombre)),
                    IdEstado = EF.Property<int?>(d, nameof(Departamento.IdEstado)),
                    EstadoNombre = d.Estado != null ? d.Estado.Nombre : null
                })
                .FirstOrDefaultAsync();

            if (fila == null)
                throw new NotFoundException($"No se encontro el departamento con id {id}.");

            if (!fila.IdEstado.HasValue || fila.IdEstado.Value <= 0)
                throw new BusinessException("El departamento tiene un estado invalido. Corrija el dato en base de datos.");

            if (string.IsNullOrWhiteSpace(fila.Nombre))
                throw new BusinessException("El departamento tiene nombre invalido. Corrija el dato en base de datos.");

            return new Departamento
            {
                IdDepartamento = fila.IdDepartamento,
                Nombre = fila.Nombre,
                IdEstado = fila.IdEstado.Value,
                Estado = new Estado
                {
                    IdEstado = fila.IdEstado.Value,
                    Nombre = fila.EstadoNombre
                }
            };
        }

        public async Task<bool> Crear(DepartamentoDTO modelo)
        {
            if (modelo == null)
                throw new BusinessException("Los datos del departamento son obligatorios.");

            await ValidarDepartamentoAsync(modelo.Nombre, modelo.IdEstado);

            var departamento = new Departamento
            {
                Nombre = modelo.Nombre.Trim(),
                IdEstado = modelo.IdEstado
            };
            _context.Departamentos.Add(departamento);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Actualizar(Departamento departamento)
        {
            if (departamento == null)
                throw new BusinessException("Los datos del departamento son obligatorios.");

            if (departamento.IdDepartamento <= 0)
                throw new BusinessException("El id del departamento es invalido.");

            await ValidarDepartamentoAsync(departamento.Nombre, departamento.IdEstado, departamento.IdDepartamento);

            var existente = await _context.Departamentos
                .FirstOrDefaultAsync(d => d.IdDepartamento == departamento.IdDepartamento);

            if (existente == null)
                throw new NotFoundException($"No se encontro el departamento con id {departamento.IdDepartamento}.");

            existente.Nombre = departamento.Nombre;
            existente.IdEstado = departamento.IdEstado;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0)
                throw new BusinessException("El id del departamento es invalido.");

            var modelo = await _context.Departamentos.FirstOrDefaultAsync(d => d.IdDepartamento == id);
            if (modelo == null)
                throw new NotFoundException($"No se encontro el departamento con id {id}.");

            var idEstadoActivo = await EstadoSistemaHelper.ObtenerIdEstadoActivoAsync(_context);
            var tienePuestosActivos = await _context.Puestos.AnyAsync(p => p.IdDepartamento == id && p.IdEstado == idEstadoActivo);
            if (tienePuestosActivos)
                throw new BusinessException("No se puede eliminar el departamento porque tiene puestos activos asociados.");

            modelo.IdEstado = await EstadoSistemaHelper.ObtenerIdEstadoInactivoAsync(_context);
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task ValidarDepartamentoAsync(string nombre, int idEstado, int idDepartamentoActual = 0)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BusinessException("El nombre del departamento es obligatorio.");

            if (idEstado <= 0)
                throw new BusinessException("El estado del departamento es invalido.");

            var duplicado = await _context.Departamentos.AnyAsync(d => d.Nombre == nombre && d.IdDepartamento != idDepartamentoActual);
            if (duplicado)
                throw new BusinessException("Ya existe un departamento con ese nombre.");

            var existeEstado = await _context.Estados.AnyAsync(e => e.IdEstado == idEstado);
            if (!existeEstado)
                throw new NotFoundException($"No se encontro el estado con id {idEstado}.");
        }
    }
}
