using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos;

public partial class TipoHoraExtras
{
    [Inject] private ITipoHoraExtraCliente TipoCliente { get; set; } = null!;

    private List<TipoHoraExtra> listaTipos = new();
    private TipoHoraExtra tipoActual = new();
    private bool mostrarFormulario;
    private string tituloFormulario = "Nuevo Tipo de Hora Extra";

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos() => listaTipos = await TipoCliente.Lista();
    private void Crear() { tipoActual = new TipoHoraExtra(); tituloFormulario = "Nuevo Tipo de Hora Extra"; mostrarFormulario = true; }
    private void Editar(TipoHoraExtra item) { tipoActual = new TipoHoraExtra { IdTipoHoraExtra = item.IdTipoHoraExtra, Nombre = item.Nombre, PorcentajePago = item.PorcentajePago, IdEstado = item.IdEstado }; tituloFormulario = "Editar Tipo de Hora Extra"; mostrarFormulario = true; }
    private async Task Guardar() { if (await TipoCliente.Guardar(tipoActual)) { mostrarFormulario = false; await CargarDatos(); } }
    private async Task Desactivar() { if (tipoActual.IdTipoHoraExtra > 0 && await TipoCliente.Desactivar(tipoActual.IdTipoHoraExtra)) { mostrarFormulario = false; await CargarDatos(); } }
    private void Cancelar() => mostrarFormulario = false;
}
