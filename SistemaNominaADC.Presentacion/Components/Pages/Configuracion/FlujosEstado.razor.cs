using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Configuracion;

public partial class FlujosEstado
{
    [Inject] private IFlujoEstadoCliente FlujoEstadoCliente { get; set; } = null!;
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;
    [Inject] private IRolCliente RolCliente { get; set; } = null!;
    [Inject] private IObjetoSistemaCliente ObjetoSistemaCliente { get; set; } = null!;
    [Inject] private ApiErrorState ApiError { get; set; } = null!;

    private List<FlujoEstado> listaFlujos = [];
    private List<Estado> listaEstados = [];
    private List<RolDTO> listaRoles = [];
    private List<ObjetoSistemaDetalleDTO> listaObjetos = [];
    private FlujoEstado flujoActual = new();
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Flujo";

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        listaFlujos = await FlujoEstadoCliente.Lista();
        listaObjetos = (await ObjetoSistemaCliente.Lista())
            .Where(x => x.IdGrupoEstado != 1)
            .OrderBy(x => x.NombreEntidad)
            .ToList();
        listaRoles = (await RolCliente.GetRoles())
            .Where(x => x.Activo)
            .ToList();

        await CargarEstadosPorEntidad(flujoActual.Entidad);
    }

    private void Crear()
    {
        flujoActual = new FlujoEstado
        {
            Entidad = "PlanillaEncabezado",
            Accion = string.Empty,
            Activo = true
        };
        _ = CargarEstadosPorEntidad(flujoActual.Entidad);
        tituloFormulario = "Nuevo Flujo";
        mostrarFormulario = true;
    }

    private void Editar(FlujoEstado item)
    {
        flujoActual = new FlujoEstado
        {
            IdFlujoEstado = item.IdFlujoEstado,
            Entidad = item.Entidad,
            IdEstadoOrigen = item.IdEstadoOrigen,
            IdEstadoDestino = item.IdEstadoDestino,
            Accion = item.Accion,
            RequiereRol = item.RequiereRol,
            Activo = item.Activo
        };
        _ = CargarEstadosPorEntidad(flujoActual.Entidad);
        tituloFormulario = "Editar Flujo";
        mostrarFormulario = true;
    }

    private async Task OnEntidadChanged(ChangeEventArgs e)
    {
        flujoActual.Entidad = e.Value?.ToString() ?? string.Empty;
        await CargarEstadosPorEntidad(flujoActual.Entidad);
        flujoActual.IdEstadoOrigen = null;
        flujoActual.IdEstadoDestino = 0;
    }

    private async Task CargarEstadosPorEntidad(string? entidad)
    {
        if (string.IsNullOrWhiteSpace(entidad))
        {
            listaEstados = [];
            return;
        }

        listaEstados = (await EstadoCliente.ListarEstadosPorEntidad(entidad))
            .Where(x => x.EstadoActivo != false)
            .OrderBy(x => x.Codigo ?? int.MaxValue)
            .ThenBy(x => x.Nombre)
            .ToList();
    }

    private async Task Guardar()
    {
        ApiError.Clear();
        if (await FlujoEstadoCliente.Guardar(flujoActual))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private async Task Desactivar()
    {
        ApiError.Clear();
        if (flujoActual.IdFlujoEstado > 0 && await FlujoEstadoCliente.Desactivar(flujoActual.IdFlujoEstado))
        {
            mostrarFormulario = false;
            await CargarDatos();
        }
    }

    private void Cancelar() => mostrarFormulario = false;
}

