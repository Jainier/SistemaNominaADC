using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SistemaNominaADC.Entidades
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base()
        {
            Activo = true;
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
            Activo = true;
        }

        public bool Activo { get; set; } = true;
    }
}