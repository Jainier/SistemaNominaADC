using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTO;
using SistemaNominaADC.Negocio.Excepciones;
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
            if (id <= 0)
                throw new BusinessException("El id del departamento es inválido.");

            var departamento = await _context.Departamentos
                .Include(d => d.Estado)
                .FirstOrDefaultAsync(d => d.IdDepartamento == id);

            if (departamento == null)
                throw new NotFoundException($"No se encontró el departamento con id {id}.");

            return departamento;
        }

        public async Task<bool> Crear(DepartamentoDTO modelo)
        {
            if (modelo == null)
                throw new BusinessException("Los datos del departamento son obligatorios.");

            await ValidarDepartamentoAsync(modelo.Nombre, modelo.IdEstado);

            var departamento = _mapper.Map<Departamento>(modelo);
            _context.Departamentos.Add(departamento);

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> Actualizar(Departamento departamento)
        {
            if (departamento == null)
                throw new BusinessException("Los datos del departamento son obligatorios.");

            if (departamento.IdDepartamento <= 0)
                throw new BusinessException("El id del departamento es inválido.");

            await ValidarDepartamentoAsync(departamento.Nombre, departamento.IdEstado);

            var existente = await _context.Departamentos
                    .FirstOrDefaultAsync(d => d.IdDepartamento == departamento.IdDepartamento);

            if (existente == null)
                throw new NotFoundException($"No se encontró el departamento con id {departamento.IdDepartamento}.");

            existente.Nombre = departamento.Nombre;
            existente.IdEstado = departamento.IdEstado;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0)
                throw new BusinessException("El id del departamento es inválido.");

            var modelo = await _context.Departamentos.FirstOrDefaultAsync(d => d.IdDepartamento == id);
            if (modelo == null)
                throw new NotFoundException($"No se encontró el departamento con id {id}.");

            _context.Departamentos.Remove(modelo);
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task ValidarDepartamentoAsync(string nombre, int idEstado)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BusinessException("El nombre del departamento es obligatorio.");

            if (idEstado <= 0)
                throw new BusinessException("El estado del departamento es inválido.");

            var existeEstado = await _context.Estados.AnyAsync(e => e.IdEstado == idEstado);
            if (!existeEstado)
                throw new NotFoundException($"No se encontró el estado con id {idEstado}.");
        }
    }
}
