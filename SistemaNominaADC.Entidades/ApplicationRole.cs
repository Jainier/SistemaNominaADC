using Microsoft.AspNetCore.Identity;

namespace SistemaNominaADC.Entidades
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base()
        {
            Activo = true;
            EsSistema = false;
        }

        public ApplicationRole(string sRoleName, bool bEsSistema = false) : base(sRoleName)
        {
            Activo = true;
            EsSistema = bEsSistema;
        }

        public bool Activo { get; set; }
        public bool EsSistema { get; set; }
    }
}
