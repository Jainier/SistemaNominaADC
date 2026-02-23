using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades.DTO
{
    public class PuestoDTO
    {
        public int IdPuesto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal SalarioBase { get; set; }
        public int IdDepartamento { get; set; }
        public string? NombreDepartamento { get; set; }
        public int IdEstado { get; set; }
        public string? NombreEstado { get; set; }
    }
}
