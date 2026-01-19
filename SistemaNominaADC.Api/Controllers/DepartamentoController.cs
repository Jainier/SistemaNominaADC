using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTO;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class DepartamentoController : ControllerBase
    {
        private readonly IDepartamentoService _departamentoService;

        public DepartamentoController(IDepartamentoService departamentoService)
        {
            _departamentoService = departamentoService;
        }

        //[Authorize]
        [HttpGet()]
        public async Task<IActionResult> Lista()
        {

            var lista = await _departamentoService.Lista();

            if (lista == null || !lista.Any())
                return NoContent();

            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            if (id <= 0)
                return BadRequest("El id es inválido.");

            var departamento = await _departamentoService.Obtener(id);

            if (departamento == null)
                return NotFound($"No se encontró el departamento con id {id}.");

            return Ok(departamento);
        }

        [HttpPost()]
        public async Task<IActionResult> Crear([FromBody] DepartamentoDTO departamentoDTO)
        {
            try
            {

                Console.WriteLine("Llegó2");
                if (departamentoDTO == null)
                    return BadRequest("Datos incorrectos o insuficientes.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var guardado = await _departamentoService.Crear(departamentoDTO);

                if (!guardado)
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "No fue posible guardar el departamento.");

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GUARDAR:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException?.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Ocurrió un error al procesar la solicitud: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Departamento departamento)
        {
            try
            {

                if (id <= 0)
                    return BadRequest("El id es inválido.");

                if (departamento == null)
                    return BadRequest("Los datos del departamento son obligatorios.");

                if (id != departamento.IdDepartamento)
                    return BadRequest("El id del departamento no coincide.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                //var existente = await _departamentoService.Obtener(id);
                //if (existente == null)
                    //return NotFound($"No se encontró el departamento con id {id}.");

                var actualizado = await _departamentoService.Actualizar(departamento);

                if (!actualizado)
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        "No fue posible actualizar el departamento.");
                //Console.WriteLine("Llegó5" + existente);
                return NoContent();

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GUARDAR:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException?.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Ocurrió un error al procesar la solicitud: {ex.Message}");
            }
        }

        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (id <= 0)
                return BadRequest("El id es inválido.");

            var eliminado = await _departamentoService.Eliminar(id);

            if (!eliminado)
                return NotFound($"No se encontró el departamento con id {id}.");

            return NoContent();
        }
    }
}