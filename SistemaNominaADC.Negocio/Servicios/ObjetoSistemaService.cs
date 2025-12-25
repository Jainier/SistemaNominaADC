using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Negocio.Servicios
{
    public class ObjetoSistemaService : IObjetoSistemaService
    {
        private readonly ApplicationDbContext _context;
        public ObjetoSistemaService(ApplicationDbContext context) => _context = context;

        public async Task<List<ObjetoSistema>> Lista()
        {
            return await _context.ObjetoSistemas
                .Include(o => o.GrupoEstado)
                .ToListAsync();
        }

        public async Task<bool> Guardar(ObjetoSistema entidad)
        {
            if (entidad.IdObjeto == 0) _context.ObjetoSistemas.Add(entidad);
            else _context.ObjetoSistemas.Update(entidad);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ObjetoSistema?> ObtenerPorNombre(string nombreEntidad)
        {
            return await _context.ObjetoSistemas
                .Include(o => o.GrupoEstado)
                .FirstOrDefaultAsync(o => o.NombreEntidad == nombreEntidad);
        }
    }
}
