using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades.DTO
{
    public class EstadoDTO
    {
        public int IdEstado { get; set; }
        public int? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool EstadoActivo { get; set; }
    }
}
