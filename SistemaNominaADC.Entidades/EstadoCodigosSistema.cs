namespace SistemaNominaADC.Entidades;

public static class EstadoCodigosSistema
{
    public const int Nulo = 0;
    public const int Activo = 10;
    public const int Inactivo = 20;
    public const int Pendiente = 30;
    public const int Aprobado = 40;
    public const int Rechazado = 50;
    public const int PendienteCalculo = 60;
    public const int Calculado = 70;

    public static readonly HashSet<int> CodigosSistema =
    [
        Nulo,
        Activo,
        Inactivo,
        Pendiente,
        Aprobado,
        Rechazado,
        PendienteCalculo,
        Calculado
    ];
}
