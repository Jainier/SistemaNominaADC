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
    public class EstadoService : IEstadoService
    {
        private readonly ApplicationDbContext _context;

        public EstadoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Estado>> Lista()
        {
            return await _context.Estados.ToListAsync();
        }

        public async Task<Estado?> Obtener(int id)
        {
            if (id <= 0)
                throw new BusinessException("El id del estado es inválido.");

            var estado = await _context.Estados.FirstOrDefaultAsync(d => d.IdEstado == id);
            if (estado == null)
                throw new NotFoundException($"No se encontró el estado con ID {id}.");

            return estado;
        }

        public async Task<bool> Guardar(Estado entidad, List<int> idsGrupos)
        {
            if (entidad == null)
                throw new BusinessException("La información del estado es obligatoria.");

            if (string.IsNullOrWhiteSpace(entidad.Nombre))
                throw new BusinessException("El nombre del estado es requerido.");

            idsGrupos ??= new List<int>();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (entidad.IdEstado != 0)
                {
                    var existe = await _context.Estados.AnyAsync(e => e.IdEstado == entidad.IdEstado);
                    if (!existe)
                        throw new NotFoundException($"No se encontró el estado con ID {entidad.IdEstado}.");
                }

                if (idsGrupos.Any())
                {
                    var idsValidos = await _context.GrupoEstados
                        .Where(g => idsGrupos.Contains(g.IdGrupoEstado))
                        .Select(g => g.IdGrupoEstado)
                        .ToListAsync();

                    var idsInvalidos = idsGrupos.Except(idsValidos).ToList();
                    if (idsInvalidos.Any())
                        throw new NotFoundException($"No se encontraron los grupos: {string.Join(", ", idsInvalidos)}.");
                }

                if (entidad.IdEstado == 0)
                    _context.Estados.Add(entidad);
                else
                    _context.Estados.Update(entidad);

                await _context.SaveChangesAsync();

                var actuales = _context.GrupoEstadoDetalles
                    .Where(x => x.IdEstado == entidad.IdEstado);
                _context.GrupoEstadoDetalles.RemoveRange(actuales);

                foreach (var idGrupo in idsGrupos)
                {
                    _context.GrupoEstadoDetalles.Add(new GrupoEstadoDetalle
                    {
                        IdEstado = entidad.IdEstado,
                        IdGrupoEstado = idGrupo
                    });
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<int>> ObtenerIdsGruposAsociados(int idEstado)
        {
            if (idEstado <= 0)
                throw new BusinessException("El id del estado es inválido.");

            return await _context.GrupoEstadoDetalles
                .Where(x => x.IdEstado == idEstado)
                .Select(x => x.IdGrupoEstado)
                .ToListAsync();
        }
        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0)
                throw new BusinessException("El id del estado es inválido.");

            var modelo = await _context.Estados.FirstOrDefaultAsync(d => d.IdEstado == id);
            if (modelo == null)
                throw new NotFoundException($"No se encontró el estado con ID {id}.");

            _context.Estados.Remove(modelo);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Estado?>> ListarEstadosPorEntidad(string nombreEntidad)
        {
            if (string.IsNullOrWhiteSpace(nombreEntidad))
                throw new BusinessException("El nombre de la entidad es requerido.");

            var objeto = await _context.ObjetoSistemas
                .FirstOrDefaultAsync(o => o.NombreEntidad == nombreEntidad);

            if (objeto == null) return new List<Estado?>();

            return await _context.GrupoEstadoDetalles
                .Where(gd => gd.IdGrupoEstado == objeto.IdGrupoEstado)
                .Select(gd => gd.Estado)
                .ToListAsync();
        }
    }
}
