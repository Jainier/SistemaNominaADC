using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs;

public class SolicitudVacacionesCreateDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    public DateTime FechaFin { get; set; }

    [StringLength(300, ErrorMessage = "El comentario de solicitud no debe exceder 300 caracteres.")]
    public string? ComentarioSolicitud { get; set; }
}
