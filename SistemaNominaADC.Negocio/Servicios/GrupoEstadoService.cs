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

        public async Task<bool> Guardar(GrupoEstado entidad)
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

            if (entidad.IdGrupoEstado == 0)
                _context.GrupoEstados.Add(entidad);
            else
                _context.GrupoEstados.Update(entidad);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0)
                throw new BusinessException("El id del grupo es inválido.");

            var entidad = await _context.GrupoEstados.FindAsync(id);
            if (entidad == null)
                throw new NotFoundException($"No se encontró el grupo con ID {id}.");

            _context.GrupoEstados.Remove(entidad);
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
    }
}
