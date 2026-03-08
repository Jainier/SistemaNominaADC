using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Operaciones;

public partial class Asistencia
{
    [Inject] private IAsistenciaCliente AsistenciaCliente { get; set; } = null!;
    [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = null!;
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;
    [Inject] private SessionService SessionService { get; set; } = null!;
    [Inject] protected ApiErrorState ApiError { get; set; } = null!;

    private List<Empleado> empleados = new();
    private List<SistemaNominaADC.Entidades.Asistencia> asistencias = new();

    private int idEmpleadoSeleccionado;
    private string? justificacionMarca;
    private bool esAdministrador;
    private Empleado? empleadoActual;

    private int? filtroIdEmpleado;
    private DateTime? filtroDesde = FiltroFechasHelper.PrimerDiaMesActual();
    private DateTime? filtroHasta = FiltroFechasHelper.UltimoDiaMesActual();
    private bool procesandoMarca;
    private bool mostrarRegistro = true;

    protected override async Task OnInitializedAsync()
    {
        esAdministrador = RolesSistema.EsAdministrador(SessionService.Roles);

        await CargarEmpleados();
        await CargarAsistencias();
    }

    private async Task CargarEmpleados()
    {
        var idsActivos = EstadoActivoFiltro.ObtenerIdsActivos(await EstadoCliente.Lista());

        if (esAdministrador)
        {
            empleados = (await EmpleadoCliente.Lista())
                .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
                .ToList();
            return;
        }

        empleadoActual = await AsistenciaCliente.ObtenerMiEmpleado();
        if (empleadoActual is null)
        {
            empleados = new();
            return;
        }

        empleados = new List<Empleado> { empleadoActual };
        idEmpleadoSeleccionado = empleadoActual.IdEmpleado;
        filtroIdEmpleado = empleadoActual.IdEmpleado;
    }

    private async Task CargarAsistencias()
    {
        if (filtroDesde.HasValue && filtroHasta.HasValue && filtroDesde.Value.Date > filtroHasta.Value.Date)
        {
            ApiError.SetError("La fecha inicial no puede ser mayor que la fecha final.");
            return;
        }

        asistencias = await AsistenciaCliente.Historial(filtroIdEmpleado, filtroDesde, filtroHasta);
    }

    private async Task MarcarEntrada()
    {
        if (procesandoMarca)
            return;

        if (idEmpleadoSeleccionado <= 0)
        {
            ApiError.SetError("Debe seleccionar un empleado.");
            return;
        }

        procesandoMarca = true;
        try
        {
            var dto = new AsistenciaMarcaDTO
            {
                IdEmpleado = idEmpleadoSeleccionado,
                Justificacion = justificacionMarca
            };

            if (await AsistenciaCliente.RegistrarEntrada(dto))
            {
                justificacionMarca = null;
                filtroIdEmpleado ??= idEmpleadoSeleccionado;
                await CargarAsistencias();
                mostrarRegistro = false;
            }
        }
        finally
        {
            procesandoMarca = false;
        }
    }

    private async Task MarcarSalida()
    {
        if (procesandoMarca)
            return;

        if (idEmpleadoSeleccionado <= 0)
        {
            ApiError.SetError("Debe seleccionar un empleado.");
            return;
        }

        procesandoMarca = true;
        try
        {
            var dto = new AsistenciaMarcaDTO
            {
                IdEmpleado = idEmpleadoSeleccionado,
                Justificacion = justificacionMarca
            };

            if (await AsistenciaCliente.RegistrarSalida(dto))
            {
                justificacionMarca = null;
                filtroIdEmpleado ??= idEmpleadoSeleccionado;
                await CargarAsistencias();
                mostrarRegistro = false;
            }
        }
        finally
        {
            procesandoMarca = false;
        }
    }

    private static string ObtenerEstadoJornada(SistemaNominaADC.Entidades.Asistencia item)
    {
        if (item.HoraEntrada.HasValue && item.HoraSalida.HasValue)
            return "Completa";

        if (item.HoraEntrada.HasValue && !item.HoraSalida.HasValue)
            return "Pendiente de salida";

        if (!item.HoraEntrada.HasValue && item.HoraSalida.HasValue)
            return "Inconsistente";

        return "Sin marcas";
    }

    private void MostrarLista()
    {
        mostrarRegistro = false;
    }

    private void MostrarRegistro()
    {
        mostrarRegistro = true;
    }
}

