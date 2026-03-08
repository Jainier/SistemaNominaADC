using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class SolicitudHorasExtra
{
    public int IdSolicitudHorasExtra { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int? IdEmpleado { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime? Fecha { get; set; }

    [Range(typeof(decimal), "0.01", "24", ErrorMessage = "La cantidad de horas debe ser mayor que 0 y menor o igual a 24.")]
    public decimal? CantidadHoras { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El tipo de hora extra es obligatorio.")]
    public int? IdTipoHoraExtra { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int? IdEstado { get; set; }

    [StringLength(200, ErrorMessage = "El motivo no debe exceder 200 caracteres.")]
    public string? Motivo { get; set; }

    [StringLength(300, ErrorMessage = "El comentario de aprobacion no debe exceder 300 caracteres.")]
    public string? ComentarioAprobacion { get; set; }

    [StringLength(450, ErrorMessage = "El identificador del usuario no debe exceder 450 caracteres.")]
    public string? IdentityUserIdDecision { get; set; }

    public Empleado? Empleado { get; set; }
    public TipoHoraExtra? TipoHoraExtra { get; set; }
    public Estado? Estado { get; set; }
}
