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

        public async Task<bool> Guardar(Estado modelo)
        {
            if (modelo.IdEstado == 0)
                _context.Estados.Add(modelo);
            else
                _context.Estados.Update(modelo);

            return await _context.SaveChangesAsync() > 0;
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
    }
}
