using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Presentacion.Components.Pages
{
     public partial class Login
    {
        private LoginRequestDTO loginModel = new();
        private string? error;

        private async Task OnLoginAsync(LoginRequestDTO oLoginRequestDTO)
        {
            var ok = await SessionService.LoginAsync(oLoginRequestDTO);
            //await SessionService.SetTokenAsync(oResultado.Token);
            if (ok)
            {
                AuthStateProvider.NotifyUserAuthentication();
                SessionService.NotifySessionChanged();
                Navigation.NavigateTo("/");
            }
            else
            {
                error = "Credenciales inválidas";
                return;
            }


        }
    }
}
