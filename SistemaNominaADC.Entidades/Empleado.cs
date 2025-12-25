using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades
{
    public class Empleado
    {
        public int IdEmpleado { get; set; }
        public string? IdentityUserId { get; set; } 
        public string Cedula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaSalida { get; set; }
        public int IdPuesto { get; set; }
        public decimal SalarioBase { get; set; }
        public bool Estado { get; set; }

        // Propiedad de navegación
        public Puesto? Puesto { get; set; }
    }
}