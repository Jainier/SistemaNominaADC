using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class TipoConceptoNomina
{
    public int IdConceptoNomina { get; set; }

    [RegularExpression(ValidacionPatrones.CodigoGeneral, ErrorMessage = "El código de concepto solo permite letras, números, guion y guion bajo.")]
    [StringLength(40, ErrorMessage = "El codigo no debe exceder 40 caracteres.")]
    public string? CodigoConcepto { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no debe exceder 150 caracteres.")]
    [RegularExpression(ValidacionPatrones.NombreGeneral, ErrorMessage = "El nombre solo puede contener letras y separadores válidos.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El modo de calculo es obligatorio.")]
    public int IdModoCalculo { get; set; }

    [StringLength(1000, ErrorMessage = "La formula no debe exceder 1000 caracteres.")]
    public string? FormulaCalculo { get; set; }

    [StringLength(60, ErrorMessage = "El codigo de formula no debe exceder 60 caracteres.")]
    [RegularExpression(ValidacionPatrones.CodigoGeneral, ErrorMessage = "El código de fórmula solo permite letras, números, guion y guion bajo.")]
    public string? CodigoFormula { get; set; }

    [Range(0, 1, ErrorMessage = "El porcentaje debe estar entre 0 y 1.")]
    public decimal? ValorPorcentaje { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto fijo no puede ser negativo.")]
    public decimal? ValorFijo { get; set; }

    public int OrdenCalculo { get; set; }

    public bool EsIngreso { get; set; }
    public bool EsDeduccion { get; set; }
    public bool AfectaCcss { get; set; } = true;
    public bool AfectaRenta { get; set; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int IdEstado { get; set; }

    public ModoCalculoConceptoNomina? ModoCalculo { get; set; }
    public Estado? Estado { get; set; }
}
