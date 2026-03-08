using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Configuracion;

public partial class Organigrama
{
    [Inject] private IEmpleadoJerarquiaCliente OrganigramaCliente { get; set; } = null!;
    [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = null!;
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;

    private List<EmpleadoJerarquia> relaciones = new();
    private List<Empleado> empleados = new();

    private EmpleadoJerarquia relacionActual = new()
    {
        Activo = true,
        VigenciaDesde = DateTime.Today
    };

    private bool mostrarFormulario;
    private string tituloFormulario = "Nueva relación";

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        var idsActivos = EstadoActivoFiltro.ObtenerIdsActivos(await EstadoCliente.Lista());
        relaciones = await OrganigramaCliente.Lista(soloActivos: false);
        empleados = (await EmpleadoCliente.Lista())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .ToList();
    }

    private void Crear()
    {
        relacionActual = new EmpleadoJerarquia
        {
            Activo = true,
            VigenciaDesde = DateTime.Today
        };

        tituloFormulario = "Nueva relación";
        mostrarFormulario = true;
    }

    private void Editar(EmpleadoJerarquia item)
    {
        relacionActual = new EmpleadoJerarquia
        {
            IdEmpleadoJerarquia = item.IdEmpleadoJerarquia,
            IdEmpleado = item.IdEmpleado,
            IdSupervisor = item.IdSupervisor,
            Activo = item.Activo,
            VigenciaDesde = item.VigenciaDesde,
            VigenciaHasta = item.VigenciaHasta,
            Observacion = item.Observacion
        };

        tituloFormulario = "Editar relación";
        mostrarFormulario = true;
    }

    private async Task Guardar()
    {
        if (await OrganigramaCliente.Guardar(relacionActual))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private async Task Desactivar()
    {
        if (relacionActual.IdEmpleadoJerarquia <= 0) return;

        if (await OrganigramaCliente.Desactivar(relacionActual.IdEmpleadoJerarquia))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private void Cancelar()
    {
        mostrarFormulario = false;
    }
}


