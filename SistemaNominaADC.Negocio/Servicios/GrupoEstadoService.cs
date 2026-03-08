using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios
{
    public class GrupoEstadoService : IGrupoEstadoService
    {
        private readonly ApplicationDbContext _context;

        public GrupoEstadoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GrupoEstado>> Lista()
        {
            return await _context.GrupoEstados.ToListAsync();
        }

        public async Task<bool> Guardar(GrupoEstado entidad, List<int>? idsEstados = null)
        {
            if (entidad == null)
                throw new BusinessException("La información del grupo es obligatoria.");

            if (string.IsNullOrWhiteSpace(entidad.Nombre))
                throw new BusinessException("El nombre del grupo es requerido.");

            if (entidad.IdGrupoEstado != 0)
            {
                var existe = await _context.GrupoEstados.AnyAsync(g => g.IdGrupoEstado == entidad.IdGrupoEstado);
                if (!existe)
                    throw new NotFoundException($"No se encontró el grupo con ID {entidad.IdGrupoEstado}.");
            }

            idsEstados ??= new List<int>();

            if (idsEstados.Count > 0)
            {
                var idsEstadosValidos = await _context.Estados
                    .Where(e => idsEstados.Contains(e.IdEstado))
                    .Select(e => e.IdEstado)
                    .ToListAsync();

                var idsInvalidos = idsEstados.Except(idsEstadosValidos).ToList();
                if (idsInvalidos.Count > 0)
                    throw new NotFoundException($"No se encontraron los estados: {string.Join(", ", idsInvalidos)}.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (entidad.IdGrupoEstado == 0)
                {
                    entidad.Activo = true;
                    _context.GrupoEstados.Add(entidad);
                }
                else
                {
                    var actual = await _context.GrupoEstados.FirstOrDefaultAsync(g => g.IdGrupoEstado == entidad.IdGrupoEstado)
                        ?? throw new NotFoundException($"No se encontró el grupo con ID {entidad.IdGrupoEstado}.");

                    actual.Nombre = entidad.Nombre;
                    actual.Descripcion = entidad.Descripcion;
                    // El estado Activo se gestiona por desactivación lógica.
                }

                await _context.SaveChangesAsync();

                var actuales = _context.GrupoEstadoDetalles
                    .Where(x => x.IdGrupoEstado == entidad.IdGrupoEstado);
                _context.GrupoEstadoDetalles.RemoveRange(actuales);

                foreach (var idEstado in idsEstados.Distinct())
                {
                    _context.GrupoEstadoDetalles.Add(new GrupoEstadoDetalle
                    {
                        IdGrupoEstado = entidad.IdGrupoEstado,
                        IdEstado = idEstado
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0)
                throw new BusinessException("El id del grupo es inválido.");

            var entidad = await _context.GrupoEstados.FindAsync(id);
            if (entidad == null)
                throw new NotFoundException($"No se encontró el grupo con ID {id}.");

            entidad.Activo = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<GrupoEstado?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new BusinessException("El id del grupo es inválido.");

            var entidad = await _context.GrupoEstados.FindAsync(id);
            if (entidad == null)
                throw new NotFoundException($"No se encontró el grupo con ID {id}.");

            return entidad;
        }

        public async Task<List<int>> ObtenerIdsEstadosAsociados(int idGrupoEstado)
        {
            if (idGrupoEstado <= 0)
                throw new BusinessException("El id del grupo es inválido.");

            return await _context.GrupoEstadoDetalles
                .Where(x => x.IdGrupoEstado == idGrupoEstado)
                .Select(x => x.IdEstado)
                .ToListAsync();
        }
    }
}
