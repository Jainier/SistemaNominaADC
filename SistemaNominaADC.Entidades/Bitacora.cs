using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades
{
    public class Bitacora
    {
        public int IdBitacora { get; set; }
        public DateTime? Fecha { get; set; }
        [StringLength(150, ErrorMessage = "La acción no debe exceder 150 caracteres.")]
        public string? Accion { get; set; }
        public string? Descripcion { get; set; }
        public int? IdEstado { get; set; }
        [StringLength(450)]
        public string? IdentityUserId { get; set; }
    }
}
