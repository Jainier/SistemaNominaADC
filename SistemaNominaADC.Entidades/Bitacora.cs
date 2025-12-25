using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades
{
    public class Bitacora
    {
        public int IdBitacora { get; set; }
        public string? IdEmpleado { get; set; } 
        public DateTime Fecha { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
    }
}
