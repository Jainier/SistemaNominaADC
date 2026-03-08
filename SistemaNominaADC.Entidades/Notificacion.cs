using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class Notificacion
{
    public int IdNotificacion { get; set; }

    [Required]
    [StringLength(450)]
    public string IdentityUserId { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Mensaje { get; set; } = string.Empty;

    [StringLength(300)]
    public string? UrlDestino { get; set; }

    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLectura { get; set; }
}
