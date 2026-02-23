using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades.DTOs
{
    public class RolCreateUpdateDTO
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
    }
}
