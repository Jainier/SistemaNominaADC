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
    public class DepartamentoService : IDepartamentoService
    {
        private readonly ApplicationDbContext _context;

        public DepartamentoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Departamento>> Lista()
        {
            return await _context.Departamentos
                    .Include(d => d.Estado) 
                    .ToListAsync();
        }

        public async Task<Departamento> Obtener(int id)
        {
            return await _context.Departamentos.FirstOrDefaultAsync(d => d.IdDepartamento == id);
        }

        public async Task<bool> Guardar(Departamento modelo)
        {
            if (modelo.IdDepartamento == 0)
                _context.Departamentos.Add(modelo);
            else
                _context.Departamentos.Update(modelo);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Eliminar(int id)
        {
            var modelo = await _context.Departamentos.FirstOrDefaultAsync(d => d.IdDepartamento == id);
            if (modelo != null)
            {
                _context.Departamentos.Remove(modelo);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
