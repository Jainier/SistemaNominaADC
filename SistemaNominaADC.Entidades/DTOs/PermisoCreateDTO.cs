using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs;

public class PermisoCreateDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El tipo de permiso es obligatorio.")]
    public int IdTipoPermiso { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    public DateTime FechaFin { get; set; }

    [Required(ErrorMessage = "El motivo es obligatorio.")]
    [StringLength(200, ErrorMessage = "El motivo no debe exceder 200 caracteres.")]
    public string Motivo { get; set; } = string.Empty;

}
