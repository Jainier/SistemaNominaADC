using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Api.Security;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ObjetosSistemaController : ControllerBase
    {
        private readonly IObjetoSistemaService _objetoService;
        private readonly IObjetoSistemaAuthorizationService _objetoAuthService;

        public ObjetosSistemaController(
            IObjetoSistemaService objetoService,
            IObjetoSistemaAuthorizationService objetoAuthService)
        {
            _objetoService = objetoService;
            _objetoAuthService = objetoAuthService;
        }

        [HttpGet("Lista")]
        public async Task<IActionResult> Lista()
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            return Ok(await _objetoService.Lista());
        }

        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] ObjetoSistemaCreateUpdateDTO entidad)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (entidad == null)
                return BadRequest("La informacion del objeto es obligatoria.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(await _objetoService.Guardar(entidad));
        }

        [HttpGet("Obtener/{nombre}")]
        public async Task<IActionResult> Obtener(string nombre)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("El nombre de la entidad es requerido.");

            return Ok(await _objetoService.ObtenerPorNombre(nombre));
        }

        [HttpDelete("Inactivar/{id:int}")]
        public async Task<IActionResult> Inactivar(int id)
        {
            var acceso = await ValidarAccesoModuloAsync();
            if (acceso != null) return acceso;

            if (id <= 0)
                return BadRequest("El id es invalido.");

            await _objetoService.Inactivar(id);
            return NoContent();
        }

        [HttpGet("ListaParaMenu")]
        public async Task<IActionResult> ListaParaMenu()
        {
            var roles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value);

            return Ok(await _objetoService.ListaParaMenu(roles));
        }

        private async Task<IActionResult?> ValidarAccesoModuloAsync()
        {
            var autorizado = await _objetoAuthService.PuedeAccederModuloAsync(User, "ObjetoSistema");
            return autorizado ? null : Forbid();
        }
    }
}
