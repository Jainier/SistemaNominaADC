using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Entidades;

public class DepartamentoJefatura
{
    public int IdDepartamentoJefatura { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El departamento es obligatorio.")]
    public int IdDepartamento { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "El tipo de jefatura es obligatorio.")]
    [StringLength(20, ErrorMessage = "El tipo de jefatura no debe exceder 20 caracteres.")]
    [RegularExpression(ValidacionPatrones.NombreGeneral, ErrorMessage = "El tipo de jefatura solo puede contener letras y separadores válidos.")]
    public string TipoJefatura { get; set; } = "Lider";

    public bool Activo { get; set; } = true;
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }

    public Departamento? Departamento { get; set; }
    public Empleado? Empleado { get; set; }
}
