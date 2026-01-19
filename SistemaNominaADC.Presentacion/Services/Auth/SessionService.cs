using SistemaNominaADC.Entidades.DTOs;
using System.Net.Http.Json;

namespace SistemaNominaADC.Presentacion.Services.Auth
{
    public class SessionService
    {
        private readonly HttpClient oHttpClient;

        public string? Token { get; private set; }
        public DateTime? Expiration { get; private set; }
        public string? UserName { get; private set; }
        public List<string> Roles { get; private set; } = new();

        public bool IsAuthenticated =>
            !string.IsNullOrEmpty(Token) &&
            Expiration > DateTime.UtcNow;

        public SessionService(HttpClient httpClient)
        {
            oHttpClient = httpClient;
        }

        public async Task<bool> LoginAsync(LoginRequestDTO oLoginRequestDTO)
        {
            HttpResponseMessage oResponse;

            try
            {
                oResponse = await oHttpClient.PostAsJsonAsync(
                    "api/auth/login",
                    oLoginRequestDTO
                );
            }
            catch
            {
                return false;
            }

            if (!oResponse.IsSuccessStatusCode)
                return false;

            LoginResponseDTO? oLoginResponseDto =
                await oResponse.Content.ReadFromJsonAsync<LoginResponseDTO>();

            if (oLoginResponseDto is null)
                return false;

            SetSession(oLoginResponseDto);

            return true;
        }

        public void SetSession(LoginResponseDTO dto)
        {
            Token = dto.Token;
            Expiration = dto.Expiration;
            UserName = dto.UserName;
            Roles = dto.Roles;

            oHttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    Token
                );
        }

        public void Clear()
        {
            Token = null;
            Expiration = null;
            UserName = null;
            Roles.Clear();

            oHttpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
