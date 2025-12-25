using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Entidades
{
    public class Puesto
    {
        public int IdPuesto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal SalarioBase { get; set; }
        public int IdDepartamento { get; set; }
        public bool Estado { get; set; }

        public Departamento? Departamento { get; set; }
    }
}
