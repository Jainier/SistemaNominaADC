using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class PlanillaEncabezado
{
    public int IdPlanilla { get; set; }

    [Required(ErrorMessage = "La fecha de inicio del período es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime PeriodoInicio { get; set; }

    [Required(ErrorMessage = "La fecha fin del período es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime PeriodoFin { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El período de aguinaldo no puede ser negativo.")]
    public int? PeriodoAguinaldo { get; set; }

    [Required(ErrorMessage = "La fecha de pago es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime FechaPago { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El tipo de planilla es obligatorio.")]
    public int IdTipoPlanilla { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El estado es invalido.")]
    public int IdEstado { get; set; }

    [StringLength(450, ErrorMessage = "El identificador del usuario no debe exceder 450 caracteres.")]
    public string? IdentityUserIdDecision { get; set; }

    public TipoPlanilla? TipoPlanilla { get; set; }
    public Estado? Estado { get; set; }
}
