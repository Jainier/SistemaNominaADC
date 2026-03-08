using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class SolicitudVacaciones
{
    public int IdSolicitudVacaciones { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int? IdEmpleado { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad de días debe ser mayor que cero.")]
    public int? CantidadDias { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime? FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime? FechaFin { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int? IdEstado { get; set; }

    [StringLength(300, ErrorMessage = "El comentario de solicitud no debe exceder 300 caracteres.")]
    public string? ComentarioSolicitud { get; set; }

    [StringLength(300, ErrorMessage = "El comentario de aprobacion no debe exceder 300 caracteres.")]
    public string? ComentarioAprobacion { get; set; }

    [StringLength(450, ErrorMessage = "El identificador del usuario no debe exceder 450 caracteres.")]
    public string? IdentityUserIdDecision { get; set; }

    public Empleado? Empleado { get; set; }
    public Estado? Estado { get; set; }
}
