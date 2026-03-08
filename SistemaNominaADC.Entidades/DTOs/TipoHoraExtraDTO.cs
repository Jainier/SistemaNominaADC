using System.ComponentModel.DataAnnotations;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Entidades.DTO;

public class TipoHoraExtraDTO
{
    public int IdTipoHoraExtra { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
    [RegularExpression(ValidacionPatrones.NombreGeneral, ErrorMessage = "El nombre solo puede contener letras y separadores válidos.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El porcentaje de pago es obligatorio.")]
    [Range(typeof(decimal), "1", "9.9999", ErrorMessage = "El porcentaje de pago debe ser mayor o igual a 1 y menor o igual a 9.9999.")]
    public decimal? PorcentajePago { get; set; }
    public bool Estado { get; set; } = true;
}
