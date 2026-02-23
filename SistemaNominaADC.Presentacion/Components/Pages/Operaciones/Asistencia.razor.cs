using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Auth;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Operaciones;

public partial class Asistencia
{
    [Inject] private IAsistenciaCliente AsistenciaCliente { get; set; } = null!;
    [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = null!;
    [Inject] private SessionService SessionService { get; set; } = null!;
    [Inject] protected ApiErrorState ApiError { get; set; } = null!;

    private List<Empleado> empleados = new();
    private List<SistemaNominaADC.Entidades.Asistencia> asistencias = new();

    private int idEmpleadoSeleccionado;
    private string? justificacionMarca;
    private bool esAdministrador;
    private Empleado? empleadoActual;

    private int? filtroIdEmpleado;
    private DateTime? filtroDesde = DateTime.Today.AddDays(-7);
    private DateTime? filtroHasta = DateTime.Today;

    protected override async Task OnInitializedAsync()
    {
        esAdministrador = SessionService.Roles.Any(r =>
            string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "Administrador", StringComparison.OrdinalIgnoreCase));

        await CargarEmpleados();
        await CargarAsistencias();
    }

    private async Task CargarEmpleados()
    {
        if (esAdministrador)
        {
            empleados = await EmpleadoCliente.Lista();
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
        asistencias = await AsistenciaCliente.Historial(filtroIdEmpleado, filtroDesde, filtroHasta);
    }

    private async Task MarcarEntrada()
    {
        if (idEmpleadoSeleccionado <= 0)
        {
            ApiError.SetError("Debe seleccionar un empleado.");
            return;
        }

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
        }
    }

    private async Task MarcarSalida()
    {
        if (idEmpleadoSeleccionado <= 0)
        {
            ApiError.SetError("Debe seleccionar un empleado.");
            return;
        }

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
        }
    }
}
