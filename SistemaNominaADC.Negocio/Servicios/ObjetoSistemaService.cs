using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

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
            if (entidad == null)
                throw new BusinessException("La información del objeto es obligatoria.");

            if (string.IsNullOrWhiteSpace(entidad.NombreEntidad))
                throw new BusinessException("El nombre de la entidad es obligatorio.");

            if (entidad.IdGrupoEstado <= 0)
                throw new BusinessException("Debe asignar un grupo de estados válido.");

            var existeGrupo = await _context.GrupoEstados
                .AnyAsync(g => g.IdGrupoEstado == entidad.IdGrupoEstado);
            if (!existeGrupo)
                throw new NotFoundException($"No se encontró el grupo con ID {entidad.IdGrupoEstado}.");

            if (entidad.IdObjeto == 0) _context.ObjetoSistemas.Add(entidad);
            else _context.ObjetoSistemas.Update(entidad);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ObjetoSistema?> ObtenerPorNombre(string nombreEntidad)
        {
            if (string.IsNullOrWhiteSpace(nombreEntidad))
                throw new BusinessException("El nombre de la entidad es requerido.");

            return await _context.ObjetoSistemas
                .Include(o => o.GrupoEstado)
                .FirstOrDefaultAsync(o => o.NombreEntidad == nombreEntidad);
        }
        public async Task<List<ObjetoSistema>> ListaParaMenu()
        {
            return await _context.ObjetoSistemas.ToListAsync();
        }
    }
}
