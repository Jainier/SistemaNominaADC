namespace SistemaNominaADC.Presentacion.Components.Pages
{
    public partial class Home
    {

        private bool bRedireccionado = false;

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !bRedireccionado && !SessionService.IsAuthenticated)
            {
                bRedireccionado = true;
                Navigation.NavigateTo("/login", true);
            }

            return Task.CompletedTask;
        }

    }
}
