using Microsoft.AspNetCore.Identity;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios
{
    public class RolService : IRolService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RolService(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<RolDTO>> ObtenerTodosAsync()
        {
            var roles = await _roleManager.Roles.Select(r => new RolDTO
            {
                Id = r.Id,
                Nombre = r.Name!,
                Activo = r.Activo,
                EsSistema = r.EsSistema
            }).ToListAsync();

            return roles;
        }

        public async Task<RolDTO> ObtenerPorIdAsync(string sRolId)
        {
            if (string.IsNullOrWhiteSpace(sRolId))
                throw new BusinessException("El id del rol es obligatorio.");

            var rol = await _roleManager.FindByIdAsync(sRolId);
            if (rol == null)
                throw new NotFoundException($"No se encontró el rol con ID {sRolId}.");

            return new RolDTO
            {
                Id = rol.Id,
                Nombre = rol.Name!,
                Activo = rol.Activo,
                EsSistema = rol.EsSistema
            };
        }

        public async Task<string> CrearAsync(RolCreateUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new BusinessException("El nombre del rol es obligatorio.");

            if (await _roleManager.RoleExistsAsync(dto.Nombre))
                throw new BusinessException("Ya existe un rol con ese nombre.");

            var rol = new ApplicationRole
            {
                Name = dto.Nombre.Trim(),
                NormalizedName = dto.Nombre.Trim().ToUpper(),
                Activo = true,
                EsSistema = false
            };

            var resultado = await _roleManager.CreateAsync(rol);

            if (!resultado.Succeeded)
                throw new BusinessException(string.Join(" | ",
                    resultado.Errors.Select(e => e.Description)));

            return rol.Id;
        }

        public async Task ActualizarAsync(string sRolId, RolCreateUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(sRolId))
                throw new BusinessException("El id del rol es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new BusinessException("El nombre del rol es obligatorio.");

            var rol = await _roleManager.FindByIdAsync(sRolId);
            if (rol == null)
                throw new NotFoundException($"No se encontró el rol con ID {sRolId}.");

            if (!string.Equals(rol.Name, dto.Nombre, StringComparison.OrdinalIgnoreCase)
                && await _roleManager.RoleExistsAsync(dto.Nombre))
            {
                throw new BusinessException("Ya existe un rol con ese nombre.");
            }

            rol.Name = dto.Nombre.Trim();
            rol.NormalizedName = dto.Nombre.Trim().ToUpperInvariant();

            var resultado = await _roleManager.UpdateAsync(rol);
            if (!resultado.Succeeded)
                throw new BusinessException(string.Join(" | ",
                    resultado.Errors.Select(e => e.Description)));
        }

        public async Task EliminarAsync(string sRolId)
        {
            // Seguridad: mantener borrado lógico incluso si se llama al endpoint DELETE legacy.
            await InactivarAsync(sRolId);
        }

        public async Task InactivarAsync(string sRolId)
        {
            if (string.IsNullOrWhiteSpace(sRolId))
                throw new BusinessException("El id del rol es obligatorio.");

            var rol = await _roleManager.FindByIdAsync(sRolId);
            if (rol == null)
                throw new NotFoundException($"No se encontró el rol con ID {sRolId}.");

            if (rol.EsSistema)
                throw new BusinessException("No se puede inactivar un rol del sistema.");

            rol.Activo = false;
            var resultado = await _roleManager.UpdateAsync(rol);
            if (!resultado.Succeeded)
                throw new BusinessException(string.Join(" | ", resultado.Errors.Select(e => e.Description)));
        }

        public async Task ActivarAsync(string sRolId)
        {
            if (string.IsNullOrWhiteSpace(sRolId))
                throw new BusinessException("El id del rol es obligatorio.");

            var rol = await _roleManager.FindByIdAsync(sRolId);
            if (rol == null)
                throw new NotFoundException($"No se encontró el rol con ID {sRolId}.");

            rol.Activo = true;
            var resultado = await _roleManager.UpdateAsync(rol);
            if (!resultado.Succeeded)
                throw new BusinessException(string.Join(" | ", resultado.Errors.Select(e => e.Description)));
        }
    }
}
