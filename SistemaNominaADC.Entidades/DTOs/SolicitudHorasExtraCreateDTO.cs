using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs;

public class SolicitudHorasExtraCreateDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateTime Fecha { get; set; }

    [Range(typeof(decimal), "0.01", "24", ErrorMessage = "La cantidad de horas debe ser mayor que 0 y menor o igual a 24.")]
    public decimal CantidadHoras { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El tipo de hora extra es obligatorio.")]
    public int IdTipoHoraExtra { get; set; }

    [Required(ErrorMessage = "El motivo es obligatorio.")]
    [StringLength(200, ErrorMessage = "El motivo no debe exceder 200 caracteres.")]
    public string Motivo { get; set; } = string.Empty;

}
