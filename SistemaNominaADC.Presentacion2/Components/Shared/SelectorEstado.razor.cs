using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Interfaces;

namespace SistemaNominaADC.Presentacion.Components.Shared
{
    public partial class SelectorEstado<TEntidad> : ComponentBase
    {
        [Inject] private IEstadoService EstadoService { get; set; } = null!;
        [Parameter] public int IdEstadoSeleccionado { get; set; }
        [Parameter] public EventCallback<int> IdEstadoSeleccionadoChanged { get; set; }

        private List<Estado>? estados;

        protected override async Task OnInitializedAsync()
        {
            string nombreEntidad = typeof(TEntidad).Name;
            estados = await EstadoService.ListarEstadosPorEntidad(nombreEntidad);
        }

        private async Task OnIdEstadoChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int id))
            {
                IdEstadoSeleccionado = id;
                await IdEstadoSeleccionadoChanged.InvokeAsync(id);
            }
        }
    }
}