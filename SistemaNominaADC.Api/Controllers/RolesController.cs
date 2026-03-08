using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRolService _rolService;
        private readonly IObjetoSistemaAuthorizationService _objetoAuthService;

        public RolesController(IRolService rolService, IObjetoSistemaAuthorizationService objetoAuthService)
        {
            _rolService = rolService;
            _objetoAuthService = objetoAuthService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            var roles = await _rolService.ObtenerTodosAsync();
            return Ok(roles);
        }

        [HttpGet("{sRolId}")]
        public async Task<IActionResult> ObtenerPorId(string sRolId)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(sRolId))
                return BadRequest("El id del rol es invalido.");

            var rol = await _rolService.ObtenerPorIdAsync(sRolId);
            return Ok(rol);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] RolCreateUpdateDTO dto)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var rolId = await _rolService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { sRolId = rolId }, null);
        }

        [HttpPut("{sRolId}")]
        public async Task<IActionResult> Actualizar(string sRolId, [FromBody] RolCreateUpdateDTO dto)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(sRolId))
                return BadRequest("El id del rol es invalido.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _rolService.ActualizarAsync(sRolId, dto);
            return NoContent();
        }

        [HttpDelete("{sRolId}")]
        public async Task<IActionResult> Eliminar(string sRolId)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(sRolId))
                return BadRequest("El id del rol es invalido.");

            await _rolService.EliminarAsync(sRolId);
            return NoContent();
        }

        [HttpPatch("InactivarRol/{sRolId}")]
        public async Task<IActionResult> InactivarRol(string sRolId)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(sRolId))
                return BadRequest("El id del rol es invalido.");

            await _rolService.InactivarAsync(sRolId);
            return NoContent();
        }

        [HttpPatch("ActivarRol/{sRolId}")]
        public async Task<IActionResult> ActivarRol(string sRolId)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(sRolId))
                return BadRequest("El id del rol es invalido.");

            await _rolService.ActivarAsync(sRolId);
            return NoContent();
        }

        private async Task<IActionResult?> ValidarAccesoModuloAsync()
        {
            var autorizado = await _objetoAuthService.PuedeAccederModuloAsync(User, "Rol");
            return autorizado ? null : Forbid();
        }
    }
}
