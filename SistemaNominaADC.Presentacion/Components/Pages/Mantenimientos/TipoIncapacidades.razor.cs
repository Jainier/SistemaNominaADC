using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class TipoIncapacidades
{
    [Inject] private ITipoIncapacidadCliente TipoCliente { get; set; } = null!;

    private List<TipoIncapacidad> listaTipos = new();
    private TipoIncapacidad tipoActual = new();
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Tipo de Incapacidad";

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos() => listaTipos = await TipoCliente.Lista();
    private void Crear() { tipoActual = new TipoIncapacidad(); tituloFormulario = "Nuevo Tipo de Incapacidad"; mostrarFormulario = true; }
    private void Editar(TipoIncapacidad item) { tipoActual = new TipoIncapacidad { IdTipoIncapacidad = item.IdTipoIncapacidad, Nombre = item.Nombre, IdEstado = item.IdEstado }; tituloFormulario = "Editar Tipo de Incapacidad"; mostrarFormulario = true; }
    private async Task Guardar() { if (await TipoCliente.Guardar(tipoActual)) { mostrarFormulario = false; await CargarDatos(); } }
    private async Task Desactivar() { if (tipoActual.IdTipoIncapacidad > 0 && await TipoCliente.Desactivar(tipoActual.IdTipoIncapacidad)) { mostrarFormulario = false; await CargarDatos(); } }
    private void Cancelar() => mostrarFormulario = false;
}
