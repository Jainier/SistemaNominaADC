using SistemaNominaADC.Entidades.DTOs;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http
{
    public interface IRolCliente
    {
        Task<List<RolDTO>> GetRoles();
        Task<bool> CrearRol(string nombre);
        Task<bool> ActualizarRol(RolDTO rol);
        Task<bool> InactivarRol(string id);
    }

    public class RolCliente : IRolCliente
    {
        private readonly HttpClient _http;

        public RolCliente(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<RolDTO>> GetRoles()
        {
            return await _http.GetFromJsonAsync<List<RolDTO>>("api/Roles") ?? new List<RolDTO>();
        }

        public async Task<bool> CrearRol(string nombre)
        {
            var response = await _http.PostAsJsonAsync("api/Roles", nombre);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarRol(RolDTO rol)
        {
            var response = await _http.PutAsJsonAsync($"api/Roles/{rol.Id}", rol);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> InactivarRol(string id)
        {
            var response = await _http.PatchAsync($"api/Roles/InactivarRol/{id}",null);
            return response.IsSuccessStatusCode;
        }
    }
}