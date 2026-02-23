using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Http;

namespace SistemaNominaADC.Presentacion.Components.Pages.Mantenimientos
{
    public partial class Usuarios
    {
        [Inject] private IUsuarioCliente UsuarioCliente { get; set; } = default!;
        [Inject] private IRolCliente RolCliente { get; set; } = default!;
        [Inject] private IEmpleadoCliente EmpleadoCliente { get; set; } = default!;

        private List<UsuarioDTO> listaUsuarios = new();
        private List<RolDTO> listaRoles = new();
        private List<Empleado> listaEmpleados = new();

        private UsuarioFormModel usuarioFormulario = new();
        private string? usuarioIdActual;
        private bool mostrarFormulario;
        private bool esNuevo;
        private string tituloFormulario = "Nuevo Usuario";
        private List<string> rolesSeleccionados = new();
        private string? errorPassword;

        protected override async Task OnInitializedAsync()
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            listaUsuarios = await UsuarioCliente.GetUsuarios();
            listaRoles = await RolCliente.GetRoles();
            listaEmpleados = await EmpleadoCliente.Lista();
        }

        private void Crear()
        {
            usuarioFormulario = new UsuarioFormModel();
            usuarioIdActual = null;
            rolesSeleccionados = new List<string>();
            esNuevo = true;
            tituloFormulario = "Nuevo Usuario";
            mostrarFormulario = true;
            errorPassword = null;
        }

        private void Editar(UsuarioDTO usuario)
        {
            usuarioIdActual = usuario.Id;
            usuarioFormulario = new UsuarioFormModel
            {
                UserName = usuario.UserName,
                Email = usuario.Email,
                Activo = usuario.Activo,
                IdEmpleado = usuario.IdEmpleado,
                Password = string.Empty,
                ConfirmPassword = string.Empty
            };
            rolesSeleccionados = usuario.Roles.ToList();
            esNuevo = false;
            tituloFormulario = "Editar Usuario";
            mostrarFormulario = true;
            errorPassword = null;
        }

        private async Task Guardar()
        {
            errorPassword = null;

            if (!PuedeGuardarSegunRolYEmpleado())
                return;

            if (esNuevo)
            {
                if (string.IsNullOrWhiteSpace(usuarioFormulario.Password))
                {
                    errorPassword = "La contrasena es obligatoria.";
                    return;
                }

                if (!string.Equals(usuarioFormulario.Password, usuarioFormulario.ConfirmPassword, StringComparison.Ordinal))
                {
                    errorPassword = "Las contrasenas no coinciden.";
                    return;
                }

                var dto = new UsuarioCreateDTO
                {
                    UserName = usuarioFormulario.UserName,
                    Email = usuarioFormulario.Email,
                    Password = usuarioFormulario.Password,
                    Activo = usuarioFormulario.Activo,
                    Roles = rolesSeleccionados.ToList(),
                    IdEmpleado = usuarioFormulario.IdEmpleado
                };

                var resultado = await UsuarioCliente.Crear(dto);
                if (resultado)
                {
                    mostrarFormulario = false;
                    await CargarDatos();
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(usuarioIdActual))
                return;

            var updateDto = new UsuarioUpdateDTO
            {
                UserName = usuarioFormulario.UserName,
                Email = usuarioFormulario.Email,
                Activo = usuarioFormulario.Activo,
                Roles = rolesSeleccionados.ToList(),
                IdEmpleado = usuarioFormulario.IdEmpleado
            };

            var actualizado = await UsuarioCliente.Actualizar(usuarioIdActual, updateDto);
            if (!actualizado)
                return;

            if (!string.IsNullOrWhiteSpace(usuarioFormulario.Password) ||
                !string.IsNullOrWhiteSpace(usuarioFormulario.ConfirmPassword))
            {
                if (!string.Equals(usuarioFormulario.Password, usuarioFormulario.ConfirmPassword, StringComparison.Ordinal))
                {
                    errorPassword = "Las contrasenas no coinciden.";
                    return;
                }

                var pwdDto = new UsuarioPasswordDTO
                {
                    NewPassword = usuarioFormulario.Password,
                    ConfirmPassword = usuarioFormulario.ConfirmPassword
                };

                var pwdResult = await UsuarioCliente.CambiarPassword(usuarioIdActual, pwdDto);
                if (!pwdResult)
                    return;
            }

            mostrarFormulario = false;
            await CargarDatos();
        }

        private void Cancelar()
        {
            mostrarFormulario = false;
            _ = CargarDatos();
        }

        private async Task Inactivar()
        {
            if (string.IsNullOrWhiteSpace(usuarioIdActual))
                return;

            if (await UsuarioCliente.CambiarEstado(usuarioIdActual, false))
            {
                mostrarFormulario = false;
                await CargarDatos();
            }
        }

        private void AlternarRol(string rol, object? value)
        {
            var isChecked = value is bool b && b;
            if (isChecked)
            {
                if (!rolesSeleccionados.Contains(rol))
                    rolesSeleccionados.Add(rol);
            }
            else
            {
                rolesSeleccionados.Remove(rol);
            }
        }

        private bool PuedeGuardarSegunRolYEmpleado()
        {
            var esAdmin = rolesSeleccionados.Any(EsRolAdministrador);
            if (!esAdmin && !usuarioFormulario.IdEmpleado.HasValue)
            {
                errorPassword = "Si el usuario no es administrador, debe asociarse a un empleado.";
                return false;
            }

            return true;
        }

        private static bool EsRolAdministrador(string rol)
        {
            return string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rol, "Administrador", StringComparison.OrdinalIgnoreCase);
        }

        private class UsuarioFormModel
        {
            [Required(ErrorMessage = "El usuario es obligatorio.")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo es obligatorio.")]
            [EmailAddress(ErrorMessage = "El formato del correo no es valido.")]
            public string Email { get; set; } = string.Empty;

            public bool Activo { get; set; } = true;

            public int? IdEmpleado { get; set; }

            public string Password { get; set; } = string.Empty;

            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
