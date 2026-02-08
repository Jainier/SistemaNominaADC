using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjetosSistemaController : ControllerBase
    {
        private readonly IObjetoSistemaService _objetoService;
        public ObjetosSistemaController(IObjetoSistemaService objetoService) => _objetoService = objetoService;

        [HttpGet("Lista")]
        public async Task<IActionResult> Lista() => Ok(await _objetoService.Lista());

        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] ObjetoSistema entidad)
        {
            if (entidad == null)
                return BadRequest("La información del objeto es obligatoria.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(await _objetoService.Guardar(entidad));
        }

        [HttpGet("Obtener/{nombre}")]
        public async Task<IActionResult> Obtener(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("El nombre de la entidad es requerido.");

            return Ok(await _objetoService.ObtenerPorNombre(nombre));
        }

        [HttpGet("ListaParaMenu")]
        public async Task<IActionResult> ListaParaMenu() => Ok(await _objetoService.ListaParaMenu());
    }
}
