using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class TipoPlanillaConcepto
{
    [Range(1, int.MaxValue, ErrorMessage = "El tipo de planilla es obligatorio.")]
    public int IdTipoPlanilla { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El concepto de nomina es obligatorio.")]
    public int IdConceptoNomina { get; set; }

    public bool Activo { get; set; } = true;
    public bool Obligatorio { get; set; }
    public bool PermiteMontoManual { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La prioridad no puede ser negativa.")]
    public int Prioridad { get; set; }

    public TipoPlanilla? TipoPlanilla { get; set; }
    public TipoConceptoNomina? TipoConceptoNomina { get; set; }
}
