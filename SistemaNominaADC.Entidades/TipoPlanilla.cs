using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class TipoPlanilla
{
    public int IdTipoPlanilla { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
    [RegularExpression(ValidacionPatrones.NombreGeneral, ErrorMessage = "El nombre solo puede contener letras y separadores válidos.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "La descripcion no debe exceder 100 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El modo de calculo es obligatorio.")]
    [StringLength(30, ErrorMessage = "El modo de calculo no debe exceder 30 caracteres.")]
    public string ModoCalculo { get; set; } = "Regular";

    public bool AportaBaseCcss { get; set; } = true;
    public bool AportaBaseRentaMensual { get; set; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int IdEstado { get; set; }

    public Estado? Estado { get; set; }
}
