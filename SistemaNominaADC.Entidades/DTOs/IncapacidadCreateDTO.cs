using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs;

public class IncapacidadCreateDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha fin es obligatoria.")]
    public DateTime FechaFin { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El tipo de incapacidad es obligatorio.")]
    public int IdTipoIncapacidad { get; set; }

    [Range(0, 1000000000, ErrorMessage = "El monto cubierto es invalido.")]
    public decimal? MontoCubierto { get; set; }

    [StringLength(300, ErrorMessage = "El comentario de solicitud no debe exceder 300 caracteres.")]
    public string? ComentarioSolicitud { get; set; }
}
