using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs;

public class NotificacionCreateDTO
{
    [Required]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Mensaje { get; set; } = string.Empty;

    [StringLength(300)]
    public string? UrlDestino { get; set; }

    public List<string> UserIds { get; set; } = new();
}
