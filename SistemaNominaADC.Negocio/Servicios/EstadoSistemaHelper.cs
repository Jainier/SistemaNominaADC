using Microsoft.EntityFrameworkCore;
using SistemaNominaADC.Datos;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Negocio.Excepciones;

namespace SistemaNominaADC.Negocio.Servicios;

public static class EstadoSistemaHelper
{
    public static async Task<int> ObtenerIdEstadoActivoAsync(ApplicationDbContext context)
    {
        var estado = await context.Estados
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Codigo == EstadoCodigosSistema.Activo);

        if (estado is null)
            throw new BusinessException($"No se encontro el estado con codigo {EstadoCodigosSistema.Activo}.");

        return estado.IdEstado;
    }

    public static async Task<int> ObtenerIdEstadoPorCodigoAsync(ApplicationDbContext context, int codigo)
    {
        var estado = await context.Estados
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Codigo == codigo);

        if (estado is null)
            throw new BusinessException($"No se encontro el estado con codigo {codigo}.");

        return estado.IdEstado;
    }

    public static async Task<int> ObtenerIdEstadoInactivoAsync(ApplicationDbContext context)
        => await ObtenerIdEstadoPorCodigoAsync(context, EstadoCodigosSistema.Inactivo);
}
