using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http
{
    public interface IUsuarioCliente
    {
        Task<List<UsuarioDTO>> GetUsuarios();
        Task<UsuarioDTO?> GetUsuario(string id);
        Task<bool> Crear(UsuarioCreateDTO dto);
        Task<bool> Actualizar(string id, UsuarioUpdateDTO dto);
        Task<bool> CambiarPassword(string id, UsuarioPasswordDTO dto);
        Task<bool> CambiarEstado(string id, bool activo);
    }

    public class UsuarioCliente : IUsuarioCliente
    {
        private readonly HttpClient _http;
        private readonly ApiErrorState _apiError;
        private readonly SessionService _sessionService;

        public UsuarioCliente(HttpClient http, ApiErrorState apiError, SessionService sessionService)
        {
            _http = http;
            _apiError = apiError;
            _sessionService = sessionService;
        }

        private void EnsureAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization = null;
            if (!string.IsNullOrWhiteSpace(_sessionService.Token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _sessionService.Token);
            }
        }

        public async Task<List<UsuarioDTO>> GetUsuarios()
        {
            _apiError.Clear();
            EnsureAuthHeader();
            try
            {
                var response = await _http.GetAsync("api/Usuarios");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar usuarios.");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<UsuarioDTO>>() ?? new();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar usuarios: {ex.Message}");
                return new();
            }
        }

        public async Task<UsuarioDTO?> GetUsuario(string id)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateRequiredText(id, "El id del usuario es obligatorio.")) return null;
            EnsureAuthHeader();
            try
            {
                var response = await _http.GetAsync($"api/Usuarios/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar el usuario.");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<UsuarioDTO>();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar usuario: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Crear(UsuarioCreateDTO dto)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateModel(dto, "Los datos del usuario son obligatorios.")) return false;
            EnsureAuthHeader();
            try
            {
                var response = await _http.PostAsJsonAsync("api/Usuarios", dto);
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para crear usuarios.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al crear usuario: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Actualizar(string id, UsuarioUpdateDTO dto)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateRequiredText(id, "El id del usuario es obligatorio.")) return false;
            if (!_apiError.TryValidateModel(dto, "Los datos del usuario son obligatorios.")) return false;
            EnsureAuthHeader();
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Usuarios/{id}", dto);
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para actualizar usuarios.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al actualizar usuario: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CambiarPassword(string id, UsuarioPasswordDTO dto)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateRequiredText(id, "El id del usuario es obligatorio.")) return false;
            if (!_apiError.TryValidateModel(dto, "Los datos de password son obligatorios.")) return false;
            EnsureAuthHeader();
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Usuarios/{id}/password", dto);
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para cambiar password.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cambiar password: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CambiarEstado(string id, bool activo)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateRequiredText(id, "El id del usuario es obligatorio.")) return false;
            EnsureAuthHeader();
            try
            {
                var response = await _http.PatchAsJsonAsync($"api/Usuarios/{id}/estado", new UsuarioEstadoDTO { Activo = activo });
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para cambiar estado de usuarios.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cambiar estado: {ex.Message}");
                return false;
            }
        }
    }
}
