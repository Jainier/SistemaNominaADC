using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class Incapacidad
{
    public int IdIncapacidad { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int? IdEmpleado { get; set; }
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime? FechaInicio { get; set; }
    [Required(ErrorMessage = "La fecha fin es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime? FechaFin { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "El tipo de incapacidad es obligatorio.")]
    public int? IdTipoIncapacidad { get; set; }
    [Range(0, 1000000000, ErrorMessage = "El monto cubierto es invalido.")]
    public decimal? MontoCubierto { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int? IdEstado { get; set; }

    [MaxLength(255)]
    public string? NombreDocumento { get; set; }

    [MaxLength(500)]
    public string? RutaDocumento { get; set; }

    [MaxLength(300)]
    public string? ComentarioRevision { get; set; }

    [MaxLength(300)]
    public string? ComentarioSolicitud { get; set; }

    [MaxLength(300)]
    public string? ComentarioAprobacion { get; set; }

    [MaxLength(450)]
    public string? IdentityUserIdDecision { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public Empleado? Empleado { get; set; }
    public TipoIncapacidad? TipoIncapacidad { get; set; }
    public Estado? Estado { get; set; }
}
