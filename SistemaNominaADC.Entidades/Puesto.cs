using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades
{
    public class Puesto
    {
        public int IdPuesto { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no debe exceder 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
        [Range(0, double.MaxValue, ErrorMessage = "El salario base debe ser mayor o igual a 0.")]
        public decimal SalarioBase { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "El departamento es obligatorio.")]
        public int IdDepartamento { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
        public int IdEstado { get; set; }

        public Departamento? Departamento { get; set; }
        public Estado? Estado { get; set; }
    }
}
