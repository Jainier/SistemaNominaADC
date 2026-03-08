namespace SistemaNominaADC.Entidades;

public sealed record ObjetoSistemaDef(
    string NombreEntidad,
    string Seccion,
    string Ruta,
    string Icono,
    string Etiqueta);

public static class ObjetoSistemaCatalogo
{
    public static readonly IReadOnlyList<ObjetoSistemaDef> Items = new List<ObjetoSistemaDef>
    {
        new("Asistencia", "Operaciones", "/operaciones/asistencia", "bi-clock-history", "Asistencia"),
        new("Permiso", "Operaciones", "/operaciones/permisos", "bi-calendar2-check", "Permisos"),
        new("SolicitudVacaciones", "Operaciones", "/operaciones/vacaciones", "bi-umbrella", "Vacaciones"),
        new("SolicitudHorasExtra", "Operaciones", "/operaciones/horas-extra", "bi-clock", "Horas Extra"),
        new("Incapacidad", "Operaciones", "/operaciones/incapacidades", "bi-heart-pulse", "Incapacidades"),
        new("PlanillaEncabezado", "Operaciones", "/operaciones/planillas", "bi-receipt-cutoff", "Planillas"),
        new("MiPlanilla", "Operaciones", "/operaciones/mis-planillas", "bi-file-earmark-pdf", "Mis Planillas"),

        new("Departamento", "Mantenimientos", "/mantenimientos/departamentos", "bi-building", "Departamentos"),
        new("Puesto", "Mantenimientos", "/mantenimientos/puestos", "bi-briefcase", "Puestos"),
        new("Empleado", "Mantenimientos", "/mantenimientos/empleados", "bi-people", "Empleados"),
        new("TipoPermiso", "Mantenimientos", "/mantenimientos/tipopermisos", "bi-calendar2-check", "Tipos de Permiso"),
        new("TipoIncapacidad", "Mantenimientos", "/mantenimientos/tipoincapacidad", "bi-heart-pulse", "Tipos de Incapacidad"),
        new("TipoHoraExtra", "Mantenimientos", "/mantenimientos/tipohoraextras", "bi-clock-history", "Tipos de Hora Extra"),
        new("ModoCalculoConceptoNomina", "Mantenimientos", "/mantenimientos/modos-calculo-concepto-nomina", "bi-calculator", "Modos de Cálculo de Nómina"),
        new("TipoConceptoNomina", "Mantenimientos", "/mantenimientos/tipos-concepto-nomina", "bi-list-check", "Conceptos de Nómina"),
        new("TipoPlanilla", "Mantenimientos", "/mantenimientos/tipos-planilla", "bi-card-checklist", "Tipos de Planilla"),
        new("TipoPlanillaConcepto", "Mantenimientos", "/mantenimientos/tipos-planilla-concepto", "bi-diagram-3", "Conceptos por Tipo de Planilla"),
        new("TramoRentaSalario", "Mantenimientos", "/mantenimientos/tramos-renta-salario", "bi-graph-up", "Tramos de Renta"),

        new("Estado", "Configuracion", "/configuracion/estados", "bi-gear-wide-connected", "Estados"),
        new("GrupoEstado", "Configuracion", "/configuracion/grupos", "bi-collection-fill", "Grupos de Estados"),
        new("ObjetoSistema", "Configuracion", "/configuracion/objetos", "bi-box-seam-fill", "Objetos del Sistema"),
        new("Rol", "Configuracion", "/configuracion/roles", "bi-shield-lock-fill", "Roles"),
        new("Usuario", "Configuracion", "/configuracion/usuarios", "bi-person-gear", "Usuarios"),
        new("DepartamentoJefatura", "Configuracion", "/configuracion/jefaturas-departamento", "bi-diagram-3", "Jefaturas por Departamento"),
        new("EmpleadoJerarquia", "Configuracion", "/configuracion/organigrama", "bi-diagram-2", "Organigrama"),
        new("FlujoEstado", "Configuracion", "/configuracion/flujos-estado", "bi-bezier2", "Flujos de Estado"),
        new("EmpleadoConceptoNomina", "Mantenimientos", "/mantenimientos/conceptos-empleado", "bi-cash-coin", "Conceptos por Empleado")
    };

    public static readonly IReadOnlyDictionary<string, ObjetoSistemaDef> ByNombre =
        Items.GroupBy(i => i.NombreEntidad, StringComparer.OrdinalIgnoreCase)
             .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, string> AliasToCanonical =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Estados"] = "Estado",
            ["Grupos"] = "GrupoEstado",
            ["ObjetosSistema"] = "ObjetoSistema",
            ["Roles"] = "Rol",
            ["Usuarios"] = "Usuario",
            ["JefaturasDepartamento"] = "DepartamentoJefatura",
            ["Organigrama"] = "EmpleadoJerarquia",
            ["FlujosEstado"] = "FlujoEstado",
            ["TramosRentaSalario"] = "TramoRentaSalario",
            ["ConceptosEmpleado"] = "EmpleadoConceptoNomina",
            ["TipoPlanillaConceptos"] = "TipoPlanillaConcepto",
            ["Permisos"] = "Permiso",
            ["Vacaciones"] = "SolicitudVacaciones",
            ["HorasExtra"] = "SolicitudHorasExtra",
            ["Incapacidades"] = "Incapacidad",
            ["Planillas"] = "PlanillaEncabezado",
            ["MisPlanillas"] = "MiPlanilla"
        };

    public static string Canonicalize(string? nombreEntidad)
    {
        if (string.IsNullOrWhiteSpace(nombreEntidad))
            return string.Empty;

        var nombre = nombreEntidad.Trim();
        if (ByNombre.ContainsKey(nombre))
            return nombre;

        return AliasToCanonical.TryGetValue(nombre, out var canonical)
            ? canonical
            : nombre;
    }
}

