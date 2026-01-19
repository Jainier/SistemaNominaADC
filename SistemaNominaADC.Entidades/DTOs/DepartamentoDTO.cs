using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades.DTO
{
    public class DepartamentoDTO
    {
        public int IdDepartamento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdEstado { get; set; }
        public string? EstadoNombre { get; set; }
    }
}
