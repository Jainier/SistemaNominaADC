using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades
{
    public class Estado
    {
        public int IdEstado { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "El código debe ser mayor a 0.")]
        public int? Codigo { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        public string? Nombre { get; set; }
        [StringLength(250, ErrorMessage = "La descripción no debe exceder 250 caracteres.")]
        public string? Descripcion { get; set; }
        public bool? EstadoActivo { get; set; }
    }
}
