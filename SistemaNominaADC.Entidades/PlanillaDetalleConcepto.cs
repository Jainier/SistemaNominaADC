using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class PlanillaDetalleConcepto
{
    public int IdDetalleConcepto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El detalle de planilla es obligatorio.")]
    public int IdPlanillaDetalle { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El concepto de nómina es obligatorio.")]
    public int IdConceptoNomina { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
    public decimal Monto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int IdEstado { get; set; }

    public PlanillaDetalle? PlanillaDetalle { get; set; }
    public TipoConceptoNomina? TipoConceptoNomina { get; set; }
    public Estado? Estado { get; set; }
}
