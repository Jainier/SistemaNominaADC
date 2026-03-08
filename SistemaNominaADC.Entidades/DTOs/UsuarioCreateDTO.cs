using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Entidades.DTOs
{
    public class UsuarioCreateDTO
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(100, ErrorMessage = "El usuario no debe exceder 100 caracteres.")]
        [RegularExpression(ValidacionPatrones.NombreUsuario, ErrorMessage = "El usuario tiene un formato inválido.")]
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
