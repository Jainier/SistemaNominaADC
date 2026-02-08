using Microsoft.AspNetCore.Identity;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
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
            return _roleManager.Roles.Select(r => new RolDTO
            {
                Id = r.Id,
                Nombre = r.Name!,
                Activo = r.Activo,
                EsSistema = r.EsSistema
            }).ToList();
        }

        public async Task<RolDTO> ObtenerPorIdAsync(string sRolId)
        {
            // Identity usa string, no int → error de diseño del contrato
            throw new BusinessException("La obtención por Id entero no es compatible con Identity.");
        }

        public async Task<int> CrearAsync(RolCreateUpdateDTO dto)
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

            return 1;
        }

        public async Task ActualizarAsync(string sRolId, RolCreateUpdateDTO dto)
        {
            throw new BusinessException("La actualización por Id entero no es compatible con Identity.");
        }

        public async Task EliminarAsync(string sRolId)
        {
            throw new BusinessException("La eliminación por Id entero no es compatible con Identity.");
        }
    }
}
