using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http
{
    public interface IRolCliente
    {
        Task<List<RolDTO>> GetRoles();
        Task<bool> CrearRol(string nombre);
        Task<bool> ActualizarRol(RolDTO rol);
        Task<bool> InactivarRol(string id);
        Task<bool> ActivarRol(string id);
    }

    public class RolCliente : IRolCliente
    {
        private readonly HttpClient _http;
        private readonly ApiErrorState _apiError;
        private readonly SessionService _sessionService;
        private const string sRuta = "api/Roles";

        public RolCliente(HttpClient http, ApiErrorState apiError, SessionService sessionService)
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

        public async Task<List<RolDTO>> GetRoles()
        {
            _apiError.Clear();
            EnsureAuthHeader();
            try
            {
                var response = await _http.GetAsync("api/Roles");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar roles.");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<RolDTO>>() ?? new();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar roles: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> CrearRol(string nombre)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateRequiredText(nombre, "El nombre del rol es obligatorio.")) return false;
            EnsureAuthHeader();
            try
            {
                var response = await _http.PostAsJsonAsync(sRuta, nombre);
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para crear roles.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al crear el rol: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarRol(RolDTO rol)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateModel(rol, "Los datos del rol son obligatorios.")) return false;
            EnsureAuthHeader();
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Roles/{rol.Id}", rol);
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para actualizar roles.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al actualizar el rol: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> InactivarRol(string id)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateRequiredText(id, "El id del rol es obligatorio.")) return false;
            EnsureAuthHeader();
            try
            {
                var response = await _http.PatchAsync($"api/Roles/InactivarRol/{id}", null);
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para inactivar roles.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al inactivar el rol: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActivarRol(string id)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateRequiredText(id, "El id del rol es obligatorio.")) return false;
            EnsureAuthHeader();
            try
            {
                var response = await _http.PatchAsync($"api/Roles/ActivarRol/{id}", null);
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para activar roles.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al activar el rol: {ex.Message}");
                return false;
            }
        }
    }
}
