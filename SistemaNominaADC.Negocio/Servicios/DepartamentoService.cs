using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTO;
using SistemaNominaADC.Negocio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Negocio.Servicios
{
    public class DepartamentoService : IDepartamentoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DepartamentoService(ApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<Departamento>> Lista()
        {
            return await _context.Departamentos
                    .Include(d => d.Estado) 
                    .ToListAsync();
        }

        public async Task<Departamento?> Obtener(int id)
        {
            return await _context.Departamentos.FirstOrDefaultAsync(d => d.IdDepartamento == id);
        }

        public async Task<bool> Crear(DepartamentoDTO modelo)
        {
            var departamento = _mapper.Map<Departamento>(modelo);
            _context.Departamentos.Add(departamento);

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> Actualizar(Departamento departamento)
        {
            var existente = await _context.Departamentos
                    .FirstOrDefaultAsync(d => d.IdDepartamento == departamento.IdDepartamento);

            if (existente == null)
                return false;

            existente.Nombre = departamento.Nombre;
            existente.IdEstado = departamento.IdEstado;

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
