using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs;

public class SolicitudDecisionDTO
{
    [StringLength(500, ErrorMessage = "El comentario no debe exceder 500 caracteres.")]
    public string? Comentario { get; set; }
}
