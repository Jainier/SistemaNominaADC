using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades
{
    public class Departamento
    {
        public int IdDepartamento { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no debe exceder 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
        public int IdEstado { get; set; }
        public virtual Estado? Estado { get; set; }
    }
}
