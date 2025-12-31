using SistemaNominaADC.Entidades;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Http
{
    public interface IDepartamentoCliente
    {
        Task<List<Departamento>> Lista();
        Task<Departamento?> Obtener(int id);
        Task<bool> Guardar(Departamento modelo);
        Task<bool> Eliminar(int id);
    }

    public class DepartamentoCliente : IDepartamentoCliente
    {
        private readonly HttpClient _http;
        public DepartamentoCliente(HttpClient http) => _http = http;

        public async Task<List<Departamento>> Lista() =>
            await _http.GetFromJsonAsync<List<Departamento>>("api/Departamento/Lista") ?? new();

        public async Task<Departamento?> Obtener(int id) =>
            await _http.GetFromJsonAsync<Departamento>($"api/Departamento/Obtener/{id}");

        public async Task<bool> Guardar(Departamento modelo) =>
            (await _http.PostAsJsonAsync("api/Departamento/Guardar", modelo)).IsSuccessStatusCode;

        public async Task<bool> Eliminar(int id) =>
            (await _http.DeleteAsync($"api/Departamento/Eliminar/{id}")).IsSuccessStatusCode;
    }
}