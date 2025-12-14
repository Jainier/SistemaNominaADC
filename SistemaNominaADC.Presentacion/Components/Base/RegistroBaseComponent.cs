namespace SistemaNominaADC.Presentacion.Components.Base
{
    using Microsoft.AspNetCore.Components;

    public abstract class RegistroBaseComponent : ComponentBase
    {
        [Inject]
        protected NavigationManager Navigation { get; set; } = null!;

        [Parameter]
        public string Titulo { get; set; } = string.Empty;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public string RutaRetorno { get; set; } = "/";

        protected void Volver()
        {
            Navigation.NavigateTo(RutaRetorno);
        }
    }

}
