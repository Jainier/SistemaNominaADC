using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public RolesController(IRolService rolService)
        {
            _rolService = rolService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var roles = await _rolService.ObtenerTodosAsync();
            return Ok(roles);
        }

        [HttpGet("{sRolId}")]
        public async Task<IActionResult> ObtenerPorId(string sRolId)
        {
            if (string.IsNullOrWhiteSpace(sRolId))
                return BadRequest("El id del rol es inválido.");

            var rol = await _rolService.ObtenerPorIdAsync(sRolId);
            return Ok(rol);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Crear([FromBody] RolCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var iRolId = await _rolService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { iRolId }, null);
        }

        [HttpPut("{sRolId}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Actualizar(string sRolId, [FromBody] RolCreateUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(sRolId))
                return BadRequest("El id del rol es inválido.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _rolService.ActualizarAsync(sRolId, dto);
            return NoContent();
        }

        [HttpDelete("{sRolId}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(string sRolId)
        {
            if (string.IsNullOrWhiteSpace(sRolId))
                return BadRequest("El id del rol es inválido.");

            await _rolService.EliminarAsync(sRolId);
            return NoContent();
        }
    }
}
