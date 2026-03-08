using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IObjetoSistemaAuthorizationService _objetoAuthService;

        public UsuariosController(IUsuarioService usuarioService, IObjetoSistemaAuthorizationService objetoAuthService)
        {
            _usuarioService = usuarioService;
            _objetoAuthService = objetoAuthService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            var usuarios = await _usuarioService.ObtenerTodosAsync();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(string id)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("El id del usuario es invalido.");

            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
            return Ok(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] UsuarioCreateDTO dto)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var id = await _usuarioService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(string id, [FromBody] UsuarioUpdateDTO dto)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("El id del usuario es invalido.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            await _usuarioService.ActualizarAsync(id, dto);
            return NoContent();
        }

        [HttpPut("{id}/password")]
        public async Task<IActionResult> CambiarPassword(string id, [FromBody] UsuarioPasswordDTO dto)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("El id del usuario es invalido.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            await _usuarioService.CambiarPasswordAsync(id, dto);
            return NoContent();
        }

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(string id, [FromBody] UsuarioEstadoDTO dto)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("El id del usuario es invalido.");

            await _usuarioService.CambiarEstadoAsync(id, dto.Activo);
            return NoContent();
        }

        private async Task<IActionResult?> ValidarAccesoModuloAsync()
        {
            var autorizado = await _objetoAuthService.PuedeAccederModuloAsync(User, "Usuario");
            return autorizado ? null : Forbid();
        }
    }
}
