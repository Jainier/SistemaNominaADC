using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Negocio.Interfaces;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoController : ControllerBase
    {
        private readonly IEstadoService _estadoService;
        public EstadoController(IEstadoService estadoService) => _estadoService = estadoService;

        [HttpGet("Lista")]
        public async Task<IActionResult> Lista()
            => Ok(await _estadoService.Lista());

        [HttpGet("Obtener/{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            if (id <= 0) return BadRequest("El ID debe ser mayor a cero.");

            var resultado = await _estadoService.Obtener(id);

            return resultado == null
                ? NotFound($"No se encontró el estado con ID {id}.")
                : Ok(resultado);
        }

        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] EstadoRequest request)
        {
            if (request?.Entidad == null)
                return BadRequest("La información del estado es obligatoria.");

            if (string.IsNullOrWhiteSpace(request.Entidad.Nombre))
                return BadRequest("El nombre del estado es requerido.");

            var resultado = await _estadoService.Guardar(request.Entidad, request.IdsGrupos);
            return Ok(resultado);
        }

        [HttpGet("GruposAsociados/{id}")]
        public async Task<IActionResult> ObtenerIdsGruposAsociados(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            var grupos = await _estadoService.ObtenerIdsGruposAsociados(id);
            return Ok(grupos);
        }

        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (id <= 0) return BadRequest("ID inválido para eliminación.");

            var exito = await _estadoService.Eliminar(id);

            return exito ? NoContent() : NotFound("No se pudo eliminar: el registro no existe.");
        }

        [HttpGet("PorEntidad/{nombre}")]
        public async Task<IActionResult> ListarEstadosPorEntidad(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("El nombre de la entidad es requerido.");

            return Ok(await _estadoService.ListarEstadosPorEntidad(nombre));
        }
    }

    public class EstadoRequest
    {
        public Estado Entidad { get; set; } = null!;
        public List<int> IdsGrupos { get; set; } = new();
    }
}