using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios
{
    public class ObjetoSistemaService : IObjetoSistemaService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ObjetoSistemaService(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<List<ObjetoSistemaDetalleDTO>> Lista()
        {
            var objetos = await _context.ObjetoSistemas
                .Include(o => o.GrupoEstado)
                .ToListAsync();

            var roles = await _context.ObjetoSistemaRoles
                .AsNoTracking()
                .ToListAsync();

            var rolesPorObjeto = roles
                .GroupBy(r => r.IdObjeto)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

            return objetos.Select(o => new ObjetoSistemaDetalleDTO
            {
                IdObjeto = o.IdObjeto,
                NombreEntidad = o.NombreEntidad,
                IdGrupoEstado = o.IdGrupoEstado,
                NombreGrupoEstado = o.GrupoEstado?.Nombre,
                Roles = rolesPorObjeto.TryGetValue(o.IdObjeto, out var lst) ? lst : new List<string>()
            }).ToList();
        }

        public async Task<bool> Guardar(ObjetoSistemaCreateUpdateDTO entidad)
        {
            if (entidad == null)
                throw new BusinessException("La información del objeto es obligatoria.");

            if (string.IsNullOrWhiteSpace(entidad.NombreEntidad))
                throw new BusinessException("El nombre de la entidad es obligatorio.");

            if (entidad.IdGrupoEstado <= 0)
                throw new BusinessException("Debe asignar un grupo de estados válido.");

            if (entidad.Roles == null || entidad.Roles.Count == 0)
                throw new BusinessException("Debe asignar al menos un rol para visualizar el mantenimiento.");

            var existeGrupo = await _context.GrupoEstados
                .AnyAsync(g => g.IdGrupoEstado == entidad.IdGrupoEstado);
            if (!existeGrupo)
                throw new NotFoundException($"No se encontró el grupo con ID {entidad.IdGrupoEstado}.");

            await ValidarRolesAsync(entidad.Roles);

            ObjetoSistema objeto;
            if (entidad.IdObjeto == 0)
            {
                objeto = new ObjetoSistema();
                _context.ObjetoSistemas.Add(objeto);
            }
            else
            {
                objeto = await _context.ObjetoSistemas.FirstOrDefaultAsync(o => o.IdObjeto == entidad.IdObjeto)
                         ?? throw new NotFoundException($"No se encontró el objeto con ID {entidad.IdObjeto}.");
                _context.ObjetoSistemas.Update(objeto);
            }

            objeto.NombreEntidad = entidad.NombreEntidad.Trim();
            objeto.IdGrupoEstado = entidad.IdGrupoEstado;

            await _context.SaveChangesAsync();

            await GuardarRolesObjetoAsync(objeto.IdObjeto, entidad.Roles);

            return true;
        }

        public async Task<ObjetoSistemaDetalleDTO?> ObtenerPorNombre(string nombreEntidad)
        {
            if (string.IsNullOrWhiteSpace(nombreEntidad))
                throw new BusinessException("El nombre de la entidad es requerido.");

            var objeto = await _context.ObjetoSistemas
                .Include(o => o.GrupoEstado)
                .FirstOrDefaultAsync(o => o.NombreEntidad == nombreEntidad);
            if (objeto == null)
                return null;

            var roles = await _context.ObjetoSistemaRoles
                .Where(r => r.IdObjeto == objeto.IdObjeto)
                .Select(r => r.RoleName)
                .ToListAsync();

            return new ObjetoSistemaDetalleDTO
            {
                IdObjeto = objeto.IdObjeto,
                NombreEntidad = objeto.NombreEntidad,
                IdGrupoEstado = objeto.IdGrupoEstado,
                NombreGrupoEstado = objeto.GrupoEstado?.Nombre,
                Roles = roles
            };
        }

        public async Task<bool> Inactivar(int idObjeto)
        {
            if (idObjeto <= 0)
                throw new BusinessException("El id del objeto es inválido.");

            var existe = await _context.ObjetoSistemas.AnyAsync(o => o.IdObjeto == idObjeto);
            if (!existe)
                throw new NotFoundException($"No se encontró el objeto con ID {idObjeto}.");

            var rolesActuales = await _context.ObjetoSistemaRoles
                .Where(r => r.IdObjeto == idObjeto)
                .ToListAsync();

            if (rolesActuales.Count == 0)
                return true;

            _context.ObjetoSistemaRoles.RemoveRange(rolesActuales);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<ObjetoSistemaDetalleDTO>> ListaParaMenu(IEnumerable<string> rolesUsuario)
        {
            var roles = rolesUsuario?.ToList() ?? new List<string>();
            if (roles.Count == 0)
                return new List<ObjetoSistemaDetalleDTO>();

            var objetos = await _context.ObjetoSistemas.ToListAsync();
            var rolesObjetos = await _context.ObjetoSistemaRoles.ToListAsync();

            var rolesPorObjeto = rolesObjetos
                .GroupBy(r => r.IdObjeto)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

            var visibles = new List<ObjetoSistemaDetalleDTO>();

            foreach (var obj in objetos)
            {
                if (!rolesPorObjeto.TryGetValue(obj.IdObjeto, out var rolesPermitidos) || rolesPermitidos.Count == 0)
                    continue;

                if (!rolesPermitidos.Intersect(roles, StringComparer.OrdinalIgnoreCase).Any())
                    continue;

                visibles.Add(new ObjetoSistemaDetalleDTO
                {
                    IdObjeto = obj.IdObjeto,
                    NombreEntidad = obj.NombreEntidad,
                    IdGrupoEstado = obj.IdGrupoEstado,
                    Roles = rolesPermitidos
                });
            }

            return visibles;
        }

        private async Task ValidarRolesAsync(IEnumerable<string> roles)
        {
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    throw new BusinessException($"El rol '{role}' no existe.");
            }
        }

        private async Task GuardarRolesObjetoAsync(int idObjeto, IEnumerable<string> roles)
        {
            var actuales = await _context.ObjetoSistemaRoles
                .Where(r => r.IdObjeto == idObjeto)
                .ToListAsync();

            if (actuales.Count > 0)
            {
                _context.ObjetoSistemaRoles.RemoveRange(actuales);
            }

            var nuevos = roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => new ObjetoSistemaRol
                {
                    IdObjeto = idObjeto,
                    RoleName = r.Trim()
                });

            await _context.ObjetoSistemaRoles.AddRangeAsync(nuevos);
            await _context.SaveChangesAsync();
        }
    }
}
