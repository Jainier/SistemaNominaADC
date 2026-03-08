using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Negocio.Excepciones;

namespace SistemaNominaADC.Negocio.Servicios;

public static class SolicitudesConflictosService
{
    public static async Task ValidarSinConflictoConIncapacidadAsync(
        ApplicationDbContext context,
        int idEmpleado,
        DateTime fechaInicio,
        DateTime fechaFin,
        string nombreSolicitud,
        int? idIncapacidadExcluir = null)
    {
        var idRechazado = await SolicitudesWorkflowHelper.ObtenerEstadoRechazadoAsync(context);

        var existeIncapacidad = await context.Incapacidades
            .AnyAsync(x =>
                x.IdEmpleado == idEmpleado &&
                (!idIncapacidadExcluir.HasValue || x.IdIncapacidad != idIncapacidadExcluir.Value) &&
                x.IdEstado != idRechazado &&
                x.FechaInicio != null &&
                x.FechaFin != null &&
                x.FechaInicio.Value.Date <= fechaFin.Date &&
                x.FechaFin.Value.Date >= fechaInicio.Date);

        if (existeIncapacidad)
            throw new BusinessException($"{nombreSolicitud} no se puede registrar porque existe una incapacidad en ese rango.");
    }

    public static async Task ValidarIncapacidadSinConflictosOperativosAsync(
        ApplicationDbContext context,
        int idEmpleado,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        var idRechazado = await SolicitudesWorkflowHelper.ObtenerEstadoRechazadoAsync(context);

        var conflictoHorasExtra = await context.SolicitudesHorasExtra
            .AnyAsync(x =>
                x.IdEmpleado == idEmpleado &&
                x.IdEstado != idRechazado &&
                x.Fecha != null &&
                x.Fecha.Value.Date >= fechaInicio.Date &&
                x.Fecha.Value.Date <= fechaFin.Date);

        if (conflictoHorasExtra)
            throw new BusinessException("No se puede registrar incapacidad: existe una solicitud de horas extra en esas fechas.");

        var conflictoPermisos = await context.Permisos
            .AnyAsync(x =>
                x.IdEmpleado == idEmpleado &&
                x.IdEstado != idRechazado &&
                x.FechaInicio != null &&
                x.FechaFin != null &&
                x.FechaInicio.Value.Date <= fechaFin.Date &&
                x.FechaFin.Value.Date >= fechaInicio.Date);

        if (conflictoPermisos)
            throw new BusinessException("No se puede registrar incapacidad: existe un permiso en esas fechas.");

        var conflictoVacaciones = await context.SolicitudesVacaciones
            .AnyAsync(x =>
                x.IdEmpleado == idEmpleado &&
                x.IdEstado != idRechazado &&
                x.FechaInicio != null &&
                x.FechaFin != null &&
                x.FechaInicio.Value.Date <= fechaFin.Date &&
                x.FechaFin.Value.Date >= fechaInicio.Date);

        if (conflictoVacaciones)
            throw new BusinessException("No se puede registrar incapacidad: existe una solicitud de vacaciones en esas fechas.");

        var conflictoAsistencia = await context.Asistencias
            .AnyAsync(x =>
                x.IdEmpleado == idEmpleado &&
                x.Fecha.Date >= fechaInicio.Date &&
                x.Fecha.Date <= fechaFin.Date);

        if (conflictoAsistencia)
            throw new BusinessException("No se puede registrar incapacidad: existe asistencia registrada en esas fechas.");
    }
}
