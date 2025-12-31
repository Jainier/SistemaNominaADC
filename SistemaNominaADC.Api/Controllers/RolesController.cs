    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Authorization;
    using SistemaNominaADC.Entidades;
    using global::SistemaNominaADC.Entidades.DTOs;

    namespace SistemaNominaADC.API.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class RolesController : ControllerBase
        {
            private readonly RoleManager<ApplicationRole> _roleManager;

            public RolesController(RoleManager<ApplicationRole> roleManager)
            {
                _roleManager = roleManager;
            }

            [HttpGet]
            public async Task<IActionResult> GetRoles()
            {
                var roles = await _roleManager.Roles.ToListAsync();
                return Ok(roles);
            }

            [HttpPost]
            public async Task<IActionResult> GuardarRol([FromBody] string nombreRol)
            {
                var result = await _roleManager.CreateAsync(new ApplicationRole(nombreRol));
                return result.Succeeded ? Ok() : BadRequest(result.Errors);
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> ActualizarRol(string id, [FromBody] RolDTO rolDto)
            {
                var rol = await _roleManager.FindByIdAsync(id);
                if (rol == null) return NotFound();

                rol.Name = rolDto.Nombre;
                var result = await _roleManager.UpdateAsync(rol);
                return result.Succeeded ? Ok() : BadRequest(result.Errors);
            }

            [HttpPatch("Inactivar/{id}")]
            public async Task<IActionResult> InactivarRol(string id)
            {
                var rol = await _roleManager.FindByIdAsync(id);
                if (rol == null) return NotFound();

                rol.Activo = false;

                var result = await _roleManager.UpdateAsync(rol);
                if (result.Succeeded)
                {
                    return Ok(new { mensaje = "El rol ha sido inactivado correctamente." });
                }

                return BadRequest(result.Errors);
            }
        }
    }

