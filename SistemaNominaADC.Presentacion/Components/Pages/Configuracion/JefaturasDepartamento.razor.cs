using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Configuracion;

public partial class JefaturasDepartamento
{
    [Inject] private IDepartamentoJefaturaCliente JefaturaCliente { get; set; } = null!;
    [Inject] private IDepartamentoCliente DepartamentoCliente { get; set; } = null!;
    [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = null!;
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;

    private List<DepartamentoJefatura> listaJefaturas = new();
    private List<Departamento> departamentos = new();
    private List<Empleado> empleados = new();

    private DepartamentoJefatura jefaturaActual = new()
    {
        Activo = true,
        TipoJefatura = "Lider"
    };

    private bool mostrarFormulario;
    private string tituloFormulario = "Nueva Jefatura";

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        var idsActivos = EstadoActivoFiltro.ObtenerIdsActivos(await EstadoCliente.Lista());
        listaJefaturas = await JefaturaCliente.Lista(soloActivos: false);
        departamentos = (await DepartamentoCliente.Lista())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .ToList();
        empleados = (await EmpleadoCliente.Lista())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .ToList();
    }

    private void Crear()
    {
        jefaturaActual = new DepartamentoJefatura
        {
            Activo = true,
            TipoJefatura = "Lider"
        };

        tituloFormulario = "Nueva Jefatura";
        mostrarFormulario = true;
    }

    private void Editar(DepartamentoJefatura item)
    {
        jefaturaActual = new DepartamentoJefatura
        {
            IdDepartamentoJefatura = item.IdDepartamentoJefatura,
            IdDepartamento = item.IdDepartamento,
            IdEmpleado = item.IdEmpleado,
            TipoJefatura = item.TipoJefatura,
            Activo = item.Activo,
            VigenciaDesde = item.VigenciaDesde,
            VigenciaHasta = item.VigenciaHasta
        };

        tituloFormulario = "Editar Jefatura";
        mostrarFormulario = true;
    }

    private async Task Guardar()
    {
        if (await JefaturaCliente.Guardar(jefaturaActual))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private async Task Desactivar()
    {
        if (jefaturaActual.IdDepartamentoJefatura <= 0) return;

        if (await JefaturaCliente.Desactivar(jefaturaActual.IdDepartamentoJefatura))
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


