using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Excepciones;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Negocio.Servicios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsuarioService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IEnumerable<UsuarioDTO>> ObtenerTodosAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var empleados = await _context.Empleados.ToListAsync();

            var empleadosPorUser = empleados
                .Where(e => !string.IsNullOrWhiteSpace(e.IdentityUserId))
                .ToDictionary(e => e.IdentityUserId!, e => e);

            var resultado = new List<UsuarioDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                empleadosPorUser.TryGetValue(user.Id, out var empleado);

                resultado.Add(new UsuarioDTO
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Activo = EsActivo(user),
                    Roles = roles.ToList(),
                    IdEmpleado = empleado?.IdEmpleado,
                    NombreEmpleado = empleado?.NombreCompleto
                });
            }

            return resultado;
        }

        public async Task<UsuarioDTO> ObtenerPorIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new BusinessException("El id del usuario es obligatorio.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new NotFoundException($"No se encontro el usuario con ID {id}.");

            var roles = await _userManager.GetRolesAsync(user);
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdentityUserId == user.Id);

            return new UsuarioDTO
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Activo = EsActivo(user),
                Roles = roles.ToList(),
                IdEmpleado = empleado?.IdEmpleado,
                NombreEmpleado = empleado?.NombreCompleto
            };
        }

        public async Task<string> CrearAsync(UsuarioCreateDTO dto)
        {
            if (dto == null)
                throw new BusinessException("La informacion del usuario es obligatoria.");

            var userName = dto.UserName.Trim();
            var email = dto.Email.Trim();

            if (string.IsNullOrWhiteSpace(userName))
                throw new BusinessException("El usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(email))
                throw new BusinessException("El correo es obligatorio.");

            if (await _userManager.FindByNameAsync(userName) != null)
                throw new BusinessException("Ya existe un usuario con ese nombre.");

            if (await _userManager.FindByEmailAsync(email) != null)
                throw new BusinessException("Ya existe un usuario con ese correo.");

            await ValidarRolesAsync(dto.Roles);
            ValidarReglaEmpleadoPorRoles(dto.Roles, dto.IdEmpleado);

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                LockoutEnabled = true
            };

            AplicarEstado(user, dto.Activo);

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                throw new BusinessException(string.Join(" | ", createResult.Errors.Select(e => e.Description)));

            if (dto.Roles.Count > 0)
            {
                var addRoles = await _userManager.AddToRolesAsync(user, dto.Roles);
                if (!addRoles.Succeeded)
                    throw new BusinessException(string.Join(" | ", addRoles.Errors.Select(e => e.Description)));
            }

            if (dto.IdEmpleado.HasValue)
                await AsignarEmpleadoAsync(user.Id, dto.IdEmpleado.Value);

            return user.Id;
        }

        public async Task ActualizarAsync(string id, UsuarioUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new BusinessException("El id del usuario es obligatorio.");

            if (dto == null)
                throw new BusinessException("La informacion del usuario es obligatoria.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new NotFoundException($"No se encontro el usuario con ID {id}.");

            var userName = dto.UserName.Trim();
            var email = dto.Email.Trim();

            if (string.IsNullOrWhiteSpace(userName))
                throw new BusinessException("El usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(email))
                throw new BusinessException("El correo es obligatorio.");

            var existeUser = await _userManager.Users.AnyAsync(u => u.Id != id && u.UserName == userName);
            if (existeUser)
                throw new BusinessException("Ya existe un usuario con ese nombre.");

            var existeEmail = await _userManager.Users.AnyAsync(u => u.Id != id && u.Email == email);
            if (existeEmail)
                throw new BusinessException("Ya existe un usuario con ese correo.");

            await ValidarRolesAsync(dto.Roles);
            ValidarReglaEmpleadoPorRoles(dto.Roles, dto.IdEmpleado);

            user.UserName = userName;
            user.NormalizedUserName = userName.ToUpperInvariant();
            user.Email = email;
            user.NormalizedEmail = email.ToUpperInvariant();

            AplicarEstado(user, dto.Activo);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new BusinessException(string.Join(" | ", updateResult.Errors.Select(e => e.Description)));

            await SincronizarRolesAsync(user, dto.Roles);

            await AsignarEmpleadoAsync(user.Id, dto.IdEmpleado);
        }

        public async Task CambiarPasswordAsync(string id, UsuarioPasswordDTO dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new BusinessException("El id del usuario es obligatorio.");

            if (dto == null)
                throw new BusinessException("La informacion del password es obligatoria.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new NotFoundException($"No se encontro el usuario con ID {id}.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                throw new BusinessException(string.Join(" | ", result.Errors.Select(e => e.Description)));
        }

        public async Task CambiarEstadoAsync(string id, bool activo)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new BusinessException("El id del usuario es obligatorio.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new NotFoundException($"No se encontro el usuario con ID {id}.");

            AplicarEstado(user, activo);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BusinessException(string.Join(" | ", result.Errors.Select(e => e.Description)));
        }

        private static bool EsActivo(ApplicationUser user)
        {
            return !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;
        }

        private static void AplicarEstado(ApplicationUser user, bool activo)
        {
            user.LockoutEnabled = true;
            user.LockoutEnd = activo ? null : DateTimeOffset.MaxValue;
        }

        private async Task ValidarRolesAsync(IEnumerable<string> roles)
        {
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    throw new BusinessException($"El rol '{role}' no existe.");
            }
        }

        private static void ValidarReglaEmpleadoPorRoles(IEnumerable<string> roles, int? idEmpleado)
        {
            var rolesList = roles?.Where(r => !string.IsNullOrWhiteSpace(r)).ToList() ?? new List<string>();
            var tieneRolAdmin = rolesList.Any(EsRolAdministrador);

            // Regla: si no es admin, debe estar asociado a un empleado.
            if (!tieneRolAdmin && !idEmpleado.HasValue)
                throw new BusinessException("Los usuarios que no son administradores deben tener un empleado asociado.");
        }

        private static bool EsRolAdministrador(string rol)
        {
            return string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rol, "Administrador", StringComparison.OrdinalIgnoreCase);
        }

        private async Task SincronizarRolesAsync(ApplicationUser user, List<string> nuevosRoles)
        {
            var actuales = await _userManager.GetRolesAsync(user);

            var rolesEliminar = actuales.Except(nuevosRoles).ToList();
            var rolesAgregar = nuevosRoles.Except(actuales).ToList();

            if (rolesEliminar.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesEliminar);
                if (!removeResult.Succeeded)
                    throw new BusinessException(string.Join(" | ", removeResult.Errors.Select(e => e.Description)));
            }

            if (rolesAgregar.Count > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesAgregar);
                if (!addResult.Succeeded)
                    throw new BusinessException(string.Join(" | ", addResult.Errors.Select(e => e.Description)));
            }
        }

        private async Task AsignarEmpleadoAsync(string userId, int? idEmpleado)
        {
            var empleadoActual = await _context.Empleados.FirstOrDefaultAsync(e => e.IdentityUserId == userId);

            if (!idEmpleado.HasValue)
            {
                if (empleadoActual != null)
                {
                    empleadoActual.IdentityUserId = null;
                    await _context.SaveChangesAsync();
                }
                return;
            }

            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado.Value);
            if (empleado == null)
                throw new NotFoundException($"No se encontro el empleado con ID {idEmpleado.Value}.");

            if (!string.IsNullOrWhiteSpace(empleado.IdentityUserId) && empleado.IdentityUserId != userId)
                throw new BusinessException("El empleado ya esta asociado a otro usuario.");

            if (empleadoActual != null && empleadoActual.IdEmpleado != empleado.IdEmpleado)
            {
                empleadoActual.IdentityUserId = null;
            }

            empleado.IdentityUserId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
