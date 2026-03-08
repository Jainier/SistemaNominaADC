using System.ComponentModel.DataAnnotations;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Entidades.DTO
{
    public class PuestoDTO
    {
        public int IdPuesto { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no debe exceder 150 caracteres.")]
        [RegularExpression(ValidacionPatrones.NombreGeneral, ErrorMessage = "El nombre solo puede contener letras y separadores válidos.")]
        public string Nombre { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El salario base debe ser mayor o igual a 0.")]
        public decimal SalarioBase { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El departamento es obligatorio.")]
        public int IdDepartamento { get; set; }

        public string? NombreDepartamento { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
        public int IdEstado { get; set; }

        public string? NombreEstado { get; set; }
    }
}
