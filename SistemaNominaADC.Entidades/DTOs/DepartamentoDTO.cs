using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades.DTO
{
    public class DepartamentoDTO
    {
        public int IdDepartamento { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "El estado es obligatorio.")]
        public int IdEstado { get; set; }
        public string? EstadoNombre { get; set; }
    }
}
