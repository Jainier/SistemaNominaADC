using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using SistemaNominaADC.Entidades.DTOs;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;

namespace SistemaNominaADC.Presentacion.Services.Auth
{

    public class SessionService
    {
        private readonly ProtectedLocalStorage _storage;
        private readonly SemaphoreSlim _restoreLock = new(1, 1);
        private bool _browserStorageReady;
        private TaskCompletionSource<bool> _initialRestoreCompletedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsAuthenticated { get; private set; }
        public bool IsRestoring { get; private set; }
        public bool RestoreAttempted { get; private set; }
        public bool InitialRestoreCompleted { get; private set; }
        public string? Token { get; private set; }
        public string? UserName { get; private set; }
        public string? Email { get; private set; }
        public List<string> Roles { get; private set; } = new();

        public SessionService(ProtectedLocalStorage storage)
        {
            _storage = storage;
        }

        public async Task SetSessionAsync(string sToken, string sUserName, string? sEmail, List<string> lstRoles)
        {
            Token = sToken;
            UserName = sUserName;
            Email = sEmail;
            Roles = lstRoles;
            IsAuthenticated = true;
            IsRestoring = false;
            RestoreAttempted = true;
            MarkInitialRestoreCompleted();

            await _storage.SetAsync("auth_token", sToken);
            await _storage.SetAsync("auth_user", sUserName);
            await _storage.SetAsync("auth_email", sEmail);
            await _storage.SetAsync("auth_roles", lstRoles);
        }

        public void EnableBrowserStorageInterop()
        {
            _browserStorageReady = true;
        }

        public void MarkInitialRestoreCompleted()
        {
            InitialRestoreCompleted = true;
            _initialRestoreCompletedTcs.TrySetResult(true);
        }

        public async Task WaitForInitialRestoreAsync(CancellationToken cancellationToken = default)
        {
            if (InitialRestoreCompleted)
                return;

            try
            {
                await _initialRestoreCompletedTcs.Task.WaitAsync(cancellationToken);
            }
            catch (Exception)
            {
                // Método defensivo: no bloquear ni romper llamadas HTTP por sincronización de sesión.
            }
        }

        public async Task<bool> RestoreSessionAsync()
        {
            Console.WriteLine("[SessionService] RestoreSessionAsync() llamado");

            if (!_browserStorageReady)
            {
                Console.WriteLine("[SessionService] RestoreSessionAsync() omitido: JSInterop/BrowserStorage aun no disponible.");
                return false;
            }

            await _restoreLock.WaitAsync();
            try
            {
                if (RestoreAttempted && !IsRestoring)
                {
                    return IsAuthenticated;
                }

                RestoreAttempted = true;
                IsRestoring = true;
                try
                {
                    var tokenResult = await _storage.GetAsync<string>("auth_token");
                    Console.WriteLine($"[SessionService] auth_token success={tokenResult.Success}, length={(tokenResult.Value?.Length ?? 0)}");

                    if (!tokenResult.Success || string.IsNullOrWhiteSpace(tokenResult.Value))
                        return false;

                    Token = tokenResult.Value;
                    UserName = (await _storage.GetAsync<string>("auth_user")).Value;
                    Email = (await _storage.GetAsync<string>("auth_email")).Value;
                    Roles = (await _storage.GetAsync<List<string>>("auth_roles")).Value ?? new();

                    IsAuthenticated = true;
                    return true;
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("[SessionService] RestoreSessionAsync() bloqueado por prerender (JSInterop no disponible)");

                    // JSInterop no disponible (prerender). Aún no se puede restaurar.
                    // Permite reintento luego del primer render.
                    RestoreAttempted = false;
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SessionService] RestoreSessionAsync() falló: {ex.GetType().Name} - {ex.Message}");
                    return false;
                }
                finally
                {
                    IsRestoring = false;
                }
            }
            finally
            {
                _restoreLock.Release();
            }
        }
        public async Task ClearAsync()
        {
            await _storage.DeleteAsync("auth_token");
            await _storage.DeleteAsync("auth_user");
            await _storage.DeleteAsync("auth_roles");

            IsAuthenticated = false;
            IsRestoring = false;
            RestoreAttempted = false;
            InitialRestoreCompleted = false;
            Token = null;
            UserName = null;
            Email = null;
            Roles.Clear();
            _initialRestoreCompletedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

}
