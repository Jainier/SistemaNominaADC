using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Presentacion.Services.Http;
using System.Linq;


namespace SistemaNominaADC.Presentacion.Components.Shared
{
    public partial class SelectorEstado<TEntidad> : ComponentBase
    {
        [Inject] private IEstadoCliente EstadoCliente { get; set; } = null!;
        [Parameter] public int IdEstadoSeleccionado { get; set; }
        [Parameter] public EventCallback<int> IdEstadoSeleccionadoChanged { get; set; }
        [Parameter] public string? NombreEntidad { get; set; }

        private List<Estado> estados = new();

        protected override async Task OnInitializedAsync()
        {
            string nombreEntidad = string.IsNullOrWhiteSpace(NombreEntidad)
                ? typeof(TEntidad).Name
                : NombreEntidad.Trim();

            estados = await EstadoCliente.ListarEstadosPorEntidad(nombreEntidad) ?? new List<Estado>();

            var selectedId = IdEstadoSeleccionado;
            estados = estados
                .Where(e => e.EstadoActivo != false || (selectedId > 0 && e.IdEstado == selectedId))
                .ToList();

            if (selectedId > 0 && estados.All(e => e.IdEstado != selectedId))
            {
                var estadoSeleccionado = await EstadoCliente.Obtener(selectedId);
                if (estadoSeleccionado is not null)
                    estados.Add(estadoSeleccionado);
            }
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
