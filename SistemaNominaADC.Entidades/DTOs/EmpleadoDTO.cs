using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades.DTO
{
    public class EmpleadoDTO
    {
        public int IdEmpleado { get; set; }
        public string? IdentityUserId { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public decimal SalarioBase { get; set; }
        public int IdPuesto { get; set; }
        public string? NombrePuesto { get; set; }
        public int IdEstado { get; set; }
        public string? NombreEstado { get; set; }
    }
}
