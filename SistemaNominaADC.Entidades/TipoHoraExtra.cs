using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class TipoHoraExtra
{
    public int IdTipoHoraExtra { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "El porcentaje de pago es obligatorio.")]
    [Range(typeof(decimal), "0.0001", "9.9999", ErrorMessage = "El porcentaje de pago debe ser mayor que 0 y menor o igual a 9.9999.")]
    public decimal? PorcentajePago { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
    public int IdEstado { get; set; }
    public Estado? Estado { get; set; }
}
