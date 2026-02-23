using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class TipoIncapacidad
{
    public int IdTipoIncapacidad { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int IdEstado { get; set; }
    public Estado? Estado { get; set; }
}
