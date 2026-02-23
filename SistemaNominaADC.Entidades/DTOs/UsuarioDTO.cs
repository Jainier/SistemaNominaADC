using System.Collections.Generic;

namespace SistemaNominaADC.Entidades.DTOs
{
    public class UsuarioDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public List<string> Roles { get; set; } = new();
        public int? IdEmpleado { get; set; }
        public string? NombreEmpleado { get; set; }
    }
}
