using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Negocio.Interfaces;

public interface INotificacionService
{
    Task<List<Notificacion>> ListarMisNotificacionesAsync(string identityUserId, bool soloPendientes = false, int max = 50);
    Task MarcarLeidaAsync(int idNotificacion, string identityUserId);
    Task MarcarTodasLeidasAsync(string identityUserId);
    Task EnviarAsync(IEnumerable<string> userIds, string titulo, string mensaje, string? urlDestino = null);
    Task<List<string>> ObtenerUserIdsPorRolesAsync(params string[] roleNames);
    Task<List<string>> ObtenerUserIdsJefaturaDepartamentoAsync(int idDepartamento);
}
