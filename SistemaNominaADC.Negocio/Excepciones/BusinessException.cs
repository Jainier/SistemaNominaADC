using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaNominaADC.Negocio.Excepciones
{
    public class BusinessException : Exception
    {
        public BusinessException(string sMensaje) : base(sMensaje) { }
    }
}