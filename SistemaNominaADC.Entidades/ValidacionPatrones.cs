namespace SistemaNominaADC.Entidades;

public static class ValidacionPatrones
{
    // Solo digitos para documentos tipo cedula (sin espacios ni letras).
    public const string CedulaNumerica = @"^\d{9,20}$";

    // Letras (incluye tildes), espacios y separadores comunes de nombres.
    public const string NombreGeneral = @"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s'\-\.]+$";

    // Identificadores tecnicos: letras, numeros y guion bajo, iniciando con letra.
    public const string IdentificadorTecnico = @"^[A-Za-z][A-Za-z0-9_]*$";

    // Codigo funcional: letras, numeros, guion y guion bajo.
    public const string CodigoGeneral = @"^[A-Za-z0-9_\-]+$";

    // Usuario tecnico para autenticacion.
    public const string NombreUsuario = @"^[A-Za-z0-9._@\-]{3,100}$";
}
