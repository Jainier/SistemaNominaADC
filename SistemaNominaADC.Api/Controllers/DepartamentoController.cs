using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTO;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartamentoController : ControllerBase
    {
        private readonly IDepartamentoService _departamentoService;
        private readonly IObjetoSistemaAuthorizationService _objetoAuthService;

        public DepartamentoController(
            IDepartamentoService departamentoService,
            IObjetoSistemaAuthorizationService objetoAuthService)
        {
            _departamentoService = departamentoService;
            _objetoAuthService = objetoAuthService;
        }

        [HttpGet()]
        public async Task<IActionResult> Lista()
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            var lista = await _departamentoService.Lista();

            if (lista == null || !lista.Any())
                return NoContent();

            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (id <= 0)
                return BadRequest("El id es inválido.");

            var departamento = await _departamentoService.Obtener(id);

            if (departamento == null)
                return NotFound($"No se encontró el departamento con id {id}.");

            return Ok(departamento);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] DepartamentoDTO departamentoDTO)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (departamentoDTO == null) return BadRequest("Datos insuficientes.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var guardado = await _departamentoService.Crear(departamentoDTO);
            if (!guardado) return StatusCode(500, "No se pudo guardar el departamento.");

            return CreatedAtAction(nameof(Obtener), new { id = 0 }, departamentoDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Departamento departamento)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (id <= 0)
                return BadRequest("El id es inválido.");

            if (departamento == null)
                return BadRequest("Los datos del departamento son obligatorios.");

            if (id != departamento.IdDepartamento)
                return BadRequest("El id del departamento no coincide.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var actualizado = await _departamentoService.Actualizar(departamento);

            return actualizado ? NoContent() : NotFound("No se encontró el departamento para actualizar.");
        }

        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (id <= 0)
                return BadRequest("El id es inválido.");

            var eliminado = await _departamentoService.Eliminar(id);

            if (!eliminado)
                return NotFound($"No se encontró el departamento con id {id}.");

            return NoContent();
        }

        private async Task<IActionResult?> ValidarAccesoModuloAsync()
        {
            var autorizado = await _objetoAuthService.PuedeAccederModuloAsync(User, "Departamento");
            return autorizado ? null : Forbid();
        }
    }
}
