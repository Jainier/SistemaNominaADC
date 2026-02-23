using SistemaNominaADC.Entidades.DTOs;
using SistemaNominaADC.Presentacion.Services.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http
{
    public interface IObjetoSistemaCliente
    {
        Task<List<ObjetoSistemaDetalleDTO>> Lista();
        Task<bool> Guardar(ObjetoSistemaCreateUpdateDTO entidad);
        Task<bool> Inactivar(int idObjeto);
        Task<ObjetoSistemaDetalleDTO?> ObtenerPorNombre(string nombreEntidad);
        Task<List<ObjetoSistemaDetalleDTO>> ListaParaMenu();
    }

    public class ObjetoSistemaCliente : IObjetoSistemaCliente
    {
        private readonly HttpClient _http;
        private readonly ApiErrorState _apiError;
        private readonly SessionService _sessionService;

        public ObjetoSistemaCliente(HttpClient http, ApiErrorState apiError, SessionService sessionService)
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

        public async Task<List<ObjetoSistemaDetalleDTO>> Lista()
        {
            _apiError.Clear();
            EnsureAuthHeader();
            try
            {
                var response = await _http.GetAsync("api/ObjetosSistema/Lista");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar objetos del sistema.");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<ObjetoSistemaDetalleDTO>>() ?? new();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar objetos del sistema: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> Guardar(ObjetoSistemaCreateUpdateDTO entidad)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateModel(entidad, "Los datos del objeto del sistema son obligatorios.")) return false;
            EnsureAuthHeader();
            try
            {
                var response = await _http.PostAsJsonAsync("api/ObjetosSistema/Guardar", entidad);
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para guardar objetos del sistema.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al guardar el objeto del sistema: {ex.Message}");
                return false;
            }
        }

        public async Task<ObjetoSistemaDetalleDTO?> ObtenerPorNombre(string nombreEntidad)
        {
            _apiError.Clear();
            if (!_apiError.TryValidateRequiredText(nombreEntidad, "El nombre de la entidad es obligatorio.")) return null;
            EnsureAuthHeader();
            try
            {
                var response = await _http.GetAsync($"api/ObjetosSistema/Obtener/{nombreEntidad}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para consultar el objeto del sistema.");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<ObjetoSistemaDetalleDTO>();
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar el objeto del sistema: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Inactivar(int idObjeto)
        {
            _apiError.Clear();
            if (!_apiError.TryValidatePositiveId(idObjeto, "id del objeto")) return false;
            EnsureAuthHeader();
            try
            {
                var response = await _http.DeleteAsync($"api/ObjetosSistema/Inactivar/{idObjeto}");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para inactivar el objeto del sistema.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al inactivar el objeto del sistema: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ObjetoSistemaDetalleDTO>> ListaParaMenu()
        {
            _apiError.Clear();
            await _sessionService.WaitForInitialRestoreAsync();

            if (!_sessionService.IsAuthenticated || string.IsNullOrWhiteSpace(_sessionService.Token))
            {
                return new();
            }

            EnsureAuthHeader();
            try
            {
                var response = await _http.GetAsync("api/ObjetosSistema/ListaParaMenu");
                if (!response.IsSuccessStatusCode)
                {
                    await response.SetApiErrorAsync(_apiError, "No autorizado para cargar el menú.");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<ObjetoSistemaDetalleDTO>>() ?? new();
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _apiError.SetError($"Error al cargar el menu: {ex.Message}");
                return new();
            }
        }
    }
}
