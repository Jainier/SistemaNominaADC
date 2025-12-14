using System.Collections.Generic;
using System.Linq;

namespace SistemaNominaADC.Presentacion.Core
{
    public static class ModuloProvider
    {
        public static List<ModuloSistema> ObtenerModulos()
        {
            return new List<ModuloSistema>
            {
                new ModuloSistema
                {
                    Nombre = "Vacaciones",
                    RutaBase = "/vacaciones",
                    Orden = 1
                },
                new ModuloSistema
                {
                    Nombre = "Horas Extra",
                    RutaBase = "/horas-extra",
                    Orden = 2
                },
                new ModuloSistema
                {
                    Nombre = "Permisos",
                    RutaBase = "/permisos",
                    Orden = 3
                },
                new ModuloSistema
                {
                    Nombre = "Nómina",
                    RutaBase = "/nomina",
                    Orden = 4
                },
                new ModuloSistema
                {
                    Nombre = "Aguinaldo",
                    RutaBase = "/aguinaldo",
                    Orden = 5
                },
                new ModuloSistema
                {
                    Nombre = "Asistencia",
                    RutaBase = "/asistencia",
                    Orden = 6
                },
                new ModuloSistema
                {
                    Nombre = "Incapacidades",
                    RutaBase = "/incapacidades",
                    Orden = 7
                },
                new ModuloSistema
                {
                    Nombre = "Evaluaciones",
                    RutaBase = "/evaluaciones",
                    Orden = 8
                },
                new ModuloSistema
                {
                    Nombre = "Liquidaciones",
                    RutaBase = "/liquidaciones",
                    Orden = 9
                },
                new ModuloSistema
                {
                    Nombre = "Consultas",
                    RutaBase = "/consultas",
                    Orden = 10
                },
                new ModuloSistema
                {
                    Nombre = "Reportes",
                    RutaBase = "/reportes",
                    Orden = 11
                },
                new ModuloSistema
                {
                    Nombre = "Catalogos",
                    RutaBase = "/catalogos",
                    Orden = 12
                },
                new ModuloSistema
                {
                    Nombre = "Seguridad",
                    RutaBase = "/seguridad",
                    Orden = 13
                },
            }
            .OrderBy(m => m.Orden)
            .ToList();
        }
    }
}
