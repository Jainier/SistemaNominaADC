using Microsoft.AspNetCore.Components;
using SistemaNominaADC.Entidades.DTOs;

namespace SistemaNominaADC.Presentacion.Components.Pages
{
     public partial class Login
    {
        private LoginRequestDTO loginModel = new();
        private string? error;

        protected override void OnInitialized()
        {
            
            loginModel.Email = "admin@admin.com";
            loginModel.Password = "Admin123*";
        }
        private async Task OnLoginAsync(LoginRequestDTO oLoginRequestDTO)
        {
            var resultado = await AuthService.LoginAsync(oLoginRequestDTO);

            if (resultado is null)
            {
                error = "Credenciales inválidas";
                return;
            }

            await SessionService.SetSessionAsync(
                resultado.Token,
                resultado.UserName,
                resultado.Roles
            );

            AuthStateProvider.NotifyUserAuthentication();

            Navigation.NavigateTo("/");
        }
    }
}
