using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class TipoPermisos
{
    [Inject] private ITipoPermisoCliente TipoCliente { get; set; } = null!;

    private List<TipoPermiso> listaTipos = new();
    private TipoPermiso tipoActual = new();
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Tipo de Permiso";

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos() => listaTipos = await TipoCliente.Lista();
    private void Crear() { tipoActual = new TipoPermiso(); tituloFormulario = "Nuevo Tipo de Permiso"; mostrarFormulario = true; }
    private void Editar(TipoPermiso item) { tipoActual = new TipoPermiso { IdTipoPermiso = item.IdTipoPermiso, Nombre = item.Nombre, IdEstado = item.IdEstado }; tituloFormulario = "Editar Tipo de Permiso"; mostrarFormulario = true; }
    private async Task Guardar() { if (await TipoCliente.Guardar(tipoActual)) { mostrarFormulario = false; await CargarDatos(); } }
    private async Task Desactivar() { if (tipoActual.IdTipoPermiso > 0 && await TipoCliente.Desactivar(tipoActual.IdTipoPermiso)) { mostrarFormulario = false; await CargarDatos(); } }
    private void Cancelar() => mostrarFormulario = false;
}
