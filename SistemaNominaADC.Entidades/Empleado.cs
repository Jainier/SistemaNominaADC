using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades
{
    public class Empleado
    {
        public int IdEmpleado { get; set; }
        public string? IdentityUserId { get; set; } 
        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20, ErrorMessage = "La cédula no debe exceder 20 caracteres.")]
        public string Cedula { get; set; } = string.Empty;
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no debe exceder 200 caracteres.")]
        public string NombreCompleto { get; set; } = string.Empty;
        [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaSalida { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "El puesto es obligatorio.")]
        public int IdPuesto { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "El salario base debe ser mayor o igual a 0.")]
        public decimal SalarioBase { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
        public int IdEstado { get; set; }

        // Propiedad de navegación
        public Puesto? Puesto { get; set; }
        public Estado? Estado { get; set; }
    }
}
