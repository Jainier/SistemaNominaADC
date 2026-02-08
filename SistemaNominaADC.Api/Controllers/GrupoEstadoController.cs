using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrupoEstadoController : ControllerBase
    {
        private readonly IGrupoEstadoService _grupoService;
        public GrupoEstadoController(IGrupoEstadoService grupoService) => _grupoService = grupoService;

        [HttpGet("Lista")]
        public async Task<IActionResult> Lista()
        {
            var lista = await _grupoService.Lista();
            return (lista == null || !lista.Any()) ? NoContent() : Ok(lista);
        }

        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            var resultado = await _grupoService.ObtenerPorId(id);
            return resultado == null ? NotFound($"No se encontró el grupo con ID {id}.") : Ok(resultado);
        }

        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] GrupoEstado entidad)
        {
            if (entidad == null || string.IsNullOrWhiteSpace(entidad.Nombre)) // Validar campos requeridos
                return BadRequest("La información del grupo es incompleta.");

            var resultado = await _grupoService.Guardar(entidad);
            return Ok(resultado);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] GrupoEstado entidad)
        {
            if (id <= 0 || entidad == null || id != entidad.IdGrupoEstado)
                return BadRequest("Los datos de la solicitud son inconsistentes.");

            var actualizado = await _grupoService.Guardar(entidad);

            return actualizado != null ? NoContent() : NotFound("No se pudo actualizar el grupo.");
        }

        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            var exito = await _grupoService.Eliminar(id);
            return exito ? NoContent() : NotFound("No se encontró el registro para eliminar.");
        }
    }
}