using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs;

public class EjecutarAccionWorkflowDTO
{
    [Required(ErrorMessage = "La accion es obligatoria.")]
    [StringLength(100, ErrorMessage = "La accion no debe exceder 100 caracteres.")]
    public string Accion { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "El comentario no debe exceder 300 caracteres.")]
    public string? Comentario { get; set; }
}
