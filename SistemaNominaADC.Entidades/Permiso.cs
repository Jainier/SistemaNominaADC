using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class Permiso
{
    public int IdPermiso { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int? IdEmpleado { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El tipo de permiso es obligatorio.")]
    public int? IdTipoPermiso { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime? FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime? FechaFin { get; set; }

    [StringLength(200, ErrorMessage = "El motivo no debe exceder 200 caracteres.")]
    public string? Motivo { get; set; }

    [StringLength(300, ErrorMessage = "El comentario de aprobacion no debe exceder 300 caracteres.")]
    public string? ComentarioAprobacion { get; set; }

    [StringLength(450, ErrorMessage = "El identificador del usuario no debe exceder 450 caracteres.")]
    public string? IdentityUserIdDecision { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int? IdEstado { get; set; }

    public Empleado? Empleado { get; set; }
    public TipoPermiso? TipoPermiso { get; set; }
    public Estado? Estado { get; set; }
}
