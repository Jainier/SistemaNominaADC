using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class TipoPermiso
{
    public int IdTipoPermiso { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
    [RegularExpression(ValidacionPatrones.NombreGeneral, ErrorMessage = "El nombre solo puede contener letras y separadores válidos.")]
    public string Nombre { get; set; } = string.Empty;
    public bool ConGoceSalarial { get; set; } = true;
    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int IdEstado { get; set; }
    public Estado? Estado { get; set; }
}
