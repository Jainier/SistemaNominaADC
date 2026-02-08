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
        private const string sRuta = "api/Roles";

        public RolCliente(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<RolDTO>> GetRoles()
        {
            var sUrlFinal = new Uri(_http.BaseAddress!, "api/Roles").ToString();
            Console.WriteLine($"[RolCliente] BaseAddress = {_http.BaseAddress}");
            Console.WriteLine($"[RolCliente] GET => {sUrlFinal}");

            var response = await _http.GetAsync("api/Roles");
            Console.WriteLine($"[RolCliente] StatusCode = {(int)response.StatusCode} {response.StatusCode}");

            var sBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[RolCliente] Body = {sBody}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<RolDTO>>() ?? new();
        }

        public async Task<bool> CrearRol(string nombre)
        {
            var response = await _http.PostAsJsonAsync(sRuta, nombre);
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