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
            if (entidad.IdGrupoEstado == 0)
                _context.GrupoEstados.Add(entidad);
            else
                _context.GrupoEstados.Update(entidad);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Eliminar(int id)
        {
            var entidad = await _context.GrupoEstados.FindAsync(id);
            if (entidad == null) return false;

            _context.GrupoEstados.Remove(entidad);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<GrupoEstado?> ObtenerPorId(int id)
        {
            return await _context.GrupoEstados.FindAsync(id);
        }
    }
}
