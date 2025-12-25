namespace SistemaNominaADC.Presentacion_Old.Core.Security
{
    using System.ComponentModel.DataAnnotations;
    public class LoginModel
    {
        [Required(ErrorMessage = "El usuario es requerido.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Password { get; set; } = string.Empty;
    }
    
}
