using System.ComponentModel.DataAnnotations;

namespace SistemaNominaADC.Presentacion.Services.Http;

public static class ApiClientValidationExtensions
{
    public static bool TryValidateModel<T>(this ApiErrorState apiError, T? model, string nullMessage) where T : class
    {
        if (model is null)
        {
            apiError.SetError(nullMessage);
            return false;
        }

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        if (Validator.TryValidateObject(model, context, results, validateAllProperties: true))
        {
            return true;
        }

        var firstMessage = results
            .Select(r => r.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

        apiError.SetError(firstMessage ?? "Datos inválidos.");
        return false;
    }

    public static bool TryValidatePositiveId(this ApiErrorState apiError, int id, string fieldLabel)
    {
        if (id > 0) return true;

        apiError.SetError($"El {fieldLabel} es inválido.");
        return false;
    }

    public static bool TryValidateRequiredText(this ApiErrorState apiError, string? value, string message)
    {
        if (!string.IsNullOrWhiteSpace(value)) return true;

        apiError.SetError(message);
        return false;
    }
}
