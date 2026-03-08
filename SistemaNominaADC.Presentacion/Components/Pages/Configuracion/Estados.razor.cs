using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Configuracion;

public partial class Estados
{
    [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;
    [Inject] private IGrupoEstadoCliente GrupoCliente { get; set; } = null!;

    private List<Estado>? listaEstados;
    private Estado estadoActual = new();
    private bool mostrarFormulario = false;
    private string tituloFormulario = "Nuevo Estado";

    private List<GrupoEstado> listaGrupos = new();
    private List<int> gruposSeleccionados = new();

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        listaEstados = await EstadoCliente.Lista();
        listaGrupos = await GrupoCliente.Lista();
    }

    private void Crear()
    {
        estadoActual = new Estado { EstadoActivo = true };
        gruposSeleccionados.Clear();
        tituloFormulario = "Nuevo Estado";
        mostrarFormulario = true;
    }

    private async Task Editar(Estado item)
    {
        estadoActual = item;
        gruposSeleccionados = await EstadoCliente.ObtenerIdsGruposAsociados(item.IdEstado);
        mostrarFormulario = true;
    }

    private async Task Guardar()
    {
        if (await EstadoCliente.Guardar(estadoActual, gruposSeleccionados))
        {
            mostrarFormulario = false;
            gruposSeleccionados.Clear();
            await CargarDatos();
        }
    }

    private void Cancelar()
    {
        mostrarFormulario = false;
    }

    private async Task Eliminar()
    {
        if (estadoActual.IdEstado <= 0) return;

        if (await EstadoCliente.Eliminar(estadoActual.IdEstado))
        {
            mostrarFormulario = false;
            gruposSeleccionados.Clear();
            await CargarDatos();
        }
    }

    private void AlternarGrupo(int idGrupo, object? valor)
    {
        var seleccionado = valor is bool b && b;
        if (seleccionado)
        {
            if (!gruposSeleccionados.Contains(idGrupo))
                gruposSeleccionados.Add(idGrupo);
            return;
        }

        gruposSeleccionados.Remove(idGrupo);
    }
}
