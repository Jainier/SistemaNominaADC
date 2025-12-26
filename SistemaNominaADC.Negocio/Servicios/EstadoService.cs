using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
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

        public async Task<Estado> Obtener(int id)
        {
            return await _context.Estados.FirstOrDefaultAsync(d => d.IdEstado == id);
        }

        public async Task<bool> Guardar(Estado entidad, List<int> idsGrupos)
        {
            // Iniciamos una transacción
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Guardar o Actualizar los datos del Estado
                if (entidad.IdEstado == 0)
                    _context.Estados.Add(entidad);
                else
                    _context.Estados.Update(entidad);

                await _context.SaveChangesAsync();

                // 2. Borrar asociaciones viejas en la tabla puente
                var actuales = _context.GrupoEstadoDetalles
                    .Where(x => x.IdEstado == entidad.IdEstado);
                _context.GrupoEstadoDetalles.RemoveRange(actuales);

                // 3. Insertar las nuevas asociaciones
                foreach (var idGrupo in idsGrupos)
                {
                    _context.GrupoEstadoDetalles.Add(new GrupoEstadoDetalle
                    {
                        IdEstado = entidad.IdEstado,
                        IdGrupoEstado = idGrupo
                    });
                }

                await _context.SaveChangesAsync();

                // Si todo salió bien, confirmamos los cambios en la BD
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                // Si hubo un error, deshacemos todo para no dejar basura
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<int>> ObtenerIdsGruposAsociados(int idEstado)
        {
            return await _context.GrupoEstadoDetalles
                .Where(x => x.IdEstado == idEstado)
                .Select(x => x.IdGrupoEstado)
                .ToListAsync();
        }
        public async Task<bool> Eliminar(int id)
        {
            var modelo = await _context.Estados.FirstOrDefaultAsync(d => d.IdEstado == id);
            if (modelo != null)
            {
                _context.Estados.Remove(modelo);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<Estado?>> ListarEstadosPorEntidad(string nombreEntidad)
        {
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
