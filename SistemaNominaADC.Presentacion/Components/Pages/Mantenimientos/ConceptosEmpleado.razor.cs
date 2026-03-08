using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Helpers;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class ConceptosEmpleado
{
    [Inject] private IEmpleadoConceptoNominaCliente Cliente { get; set; } = null!;
    [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = null!;
    [Inject] private ITipoConceptoNominaCliente TipoConceptoCliente { get; set; } = null!;
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;
    [Inject] private ApiErrorState ApiError { get; set; } = null!;

    private List<EmpleadoConceptoNomina> lista = [];
    private List<Empleado> empleados = [];
    private List<TipoConceptoNomina> conceptos = [];
    private EmpleadoConceptoNomina actual = new();
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Concepto por Empleado";

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        var idsActivos = EstadoActivoFiltro.ObtenerIdsActivos(await EstadoCliente.Lista());
        lista = await Cliente.Lista();
        empleados = (await EmpleadoCliente.Lista())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .ToList();
        conceptos = (await TipoConceptoCliente.Lista())
            .Where(x => EstadoActivoFiltro.EstaActivo(x.IdEstado, idsActivos))
            .Where(x => x.EsDeduccion || x.EsIngreso)
            .OrderBy(x => x.Nombre)
            .ToList();
    }

    private void Crear()
    {
        actual = new EmpleadoConceptoNomina
        {
            Activo = true,
            Prioridad = 0
        };
        tituloFormulario = "Nuevo Concepto por Empleado";
        mostrarFormulario = true;
    }

    private void Editar(EmpleadoConceptoNomina item)
    {
        actual = new EmpleadoConceptoNomina
        {
            IdEmpleadoConceptoNomina = item.IdEmpleadoConceptoNomina,
            IdEmpleado = item.IdEmpleado,
            IdConceptoNomina = item.IdConceptoNomina,
            MontoFijo = item.MontoFijo,
            Porcentaje = item.Porcentaje,
            SaldoPendiente = item.SaldoPendiente,
            Prioridad = item.Prioridad,
            VigenciaDesde = item.VigenciaDesde,
            VigenciaHasta = item.VigenciaHasta,
            Activo = item.Activo
        };
        tituloFormulario = "Editar Concepto por Empleado";
        mostrarFormulario = true;
    }

    private async Task Guardar()
    {
        ApiError.Clear();
        if (await Cliente.Guardar(actual))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private async Task Desactivar()
    {
        ApiError.Clear();
        if (actual.IdEmpleadoConceptoNomina > 0 && await Cliente.Desactivar(actual.IdEmpleadoConceptoNomina))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private void Cancelar() => mostrarFormulario = false;
}
