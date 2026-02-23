using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades.DTOs;

public class AsistenciaMarcaDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    public DateTime? FechaHoraMarca { get; set; }

    [StringLength(1000, ErrorMessage = "La justificación no debe exceder 1000 caracteres.")]
    public string? Justificacion { get; set; }
}
