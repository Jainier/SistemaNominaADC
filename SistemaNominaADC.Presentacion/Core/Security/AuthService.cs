using SistemaNominaADC.Presentacion_Old.Core.Security;

public class AuthService
{
    private UsuarioSesion? oUsuarioSesion;

    public event Action? OnAuthStateChanged;


    public async Task<UsuarioSesion?> LoginAsync(string username, string password)
    { 
        if (username == "admin" && password == "123")
        {
            oUsuarioSesion = new UsuarioSesion
            {
                IdUsuario = 1,
                sUsuario = username,
                sRol = "Administrador",
            };

            OnAuthStateChanged?.Invoke();
            return oUsuarioSesion;
        }

        return null;
    }

    public void Logout()
    {
        oUsuarioSesion = null;
        OnAuthStateChanged?.Invoke();
    }

    public UsuarioSesion? ObtenerUsuarioSesion()
    {
        return oUsuarioSesion;
    }
}
