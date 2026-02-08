using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using SistemaNominaADC.Entidades.DTOs;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SistemaNominaADC.Presentacion.Services.Auth
{

    public class SessionService
    {
        private readonly ProtectedLocalStorage _storage;

        public bool IsAuthenticated { get; private set; }
        public string? Token { get; private set; }
        public string? UserName { get; private set; }
        public List<string> Roles { get; private set; } = new();

        public SessionService(ProtectedLocalStorage storage)
        {
            _storage = storage;
        }

        public async Task SetSessionAsync(string sToken, string sUserName, List<string> lstRoles)
        {
            Token = sToken;
            UserName = sUserName;
            Roles = lstRoles;
            IsAuthenticated = true;

            await _storage.SetAsync("auth_token", sToken);
            await _storage.SetAsync("auth_user", sUserName);
            await _storage.SetAsync("auth_roles", lstRoles);
        }

        public async Task<bool> RestoreSessionAsync()
        {
            var tokenResult = await _storage.GetAsync<string>("auth_token");

            if (!tokenResult.Success)
                return false;

            Token = tokenResult.Value;
            UserName = (await _storage.GetAsync<string>("auth_user")).Value;
            Roles = (await _storage.GetAsync<List<string>>("auth_roles")).Value ?? new();

            IsAuthenticated = true;
            return true;
        }

        public async Task ClearAsync()
        {
            await _storage.DeleteAsync("auth_token");
            await _storage.DeleteAsync("auth_user");
            await _storage.DeleteAsync("auth_roles");

            IsAuthenticated = false;
            Token = null;
            UserName = null;
            Roles.Clear();
        }
    }

}