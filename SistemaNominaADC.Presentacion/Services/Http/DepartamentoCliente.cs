using Azure;
using SistemaNominaADC.Entidades;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

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
            await _http.GetFromJsonAsync<List<Departamento>>("api/Departamento/") ?? new();

        public async Task<Departamento?> Obtener(int id) =>
            await _http.GetFromJsonAsync<Departamento>($"api/Departamento/{id}");

        public async Task<bool> Guardar(Departamento depto)
        {
            try
            {
                HttpResponseMessage response;

                if (depto.IdDepartamento == 0)
                {
                    Console.WriteLine("Llegó 1");
                    response = await _http.PostAsJsonAsync("api/Departamento", depto);
                    Console.WriteLine(response.RequestMessage);

                }
                else
                {
                    response = await _http.PutAsJsonAsync(
                        $"api/departamento/{depto.IdDepartamento}", depto);
                }

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"ERROR1: {response.StatusCode}");
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
             catch(Exception ex)
            {
                Console.WriteLine($"ERROR2: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> Eliminar(int id) =>
            (await _http.DeleteAsync($"api/Departamento/Eliminar/{id}")).IsSuccessStatusCode;
    }
}