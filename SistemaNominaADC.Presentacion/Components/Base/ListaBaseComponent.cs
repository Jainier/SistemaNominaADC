namespace SistemaNominaADC.Presentacion_Old.Components.Base
{
    using Microsoft.AspNetCore.Components;

    public abstract class ListaBaseComponent : ComponentBase
    {
        [Parameter]
        public string Titulo { get; set; } = string.Empty;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public EventCallback OnNuevo { get; set; }

        protected async Task OnNuevoClick()
        {
            if (OnNuevo.HasDelegate)
                await OnNuevo.InvokeAsync();
        }
    }
}
