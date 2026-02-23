using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs
{
    public class UsuarioCreateDTO
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es valido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public List<string> Roles { get; set; } = new();

        public int? IdEmpleado { get; set; }
    }
}
