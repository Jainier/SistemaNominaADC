using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs
{
    public class UsuarioPasswordDTO
    {
        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contrasena.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Las contrasenas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
