using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SistemaNominaADC.Presentacion.Services.Http;

public static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<string> ReadErrorMessageAsync(this HttpResponseMessage response)
    {
        if (response is null)
            return "Error desconocido.";

        if (response.Content is null)
            return BuildFallbackMessage(response.StatusCode, response.ReasonPhrase);

        var raw = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(raw))
            return BuildFallbackMessage(response.StatusCode, response.ReasonPhrase);

        var trimmed = raw.Trim();

        // Prefer ProblemDetails/ValidationProblemDetails when the API returns problem+json.
        if (LooksLikeProblemDetails(response))
        {
            var validation = TryDeserialize<ValidationProblemDetails>(trimmed);
            if (validation?.Errors is { Count: > 0 })
            {
                var messages = validation.Errors
                    .SelectMany(kvp => kvp.Value.Select(msg => $"{kvp.Key}: {msg}"));
                return string.Join(" | ", messages);
            }

            var problem = TryDeserialize<ProblemDetails>(trimmed);
            if (problem is not null)
            {
                if (!string.IsNullOrWhiteSpace(problem.Detail))
                    return problem.Detail!;
                if (!string.IsNullOrWhiteSpace(problem.Title))
                    return problem.Title!;
            }
        }

        return trimmed;
    }

    public static async Task SetApiErrorAsync(
        this HttpResponseMessage response,
        ApiErrorState apiError,
        string unauthorizedMessage,
        string? forbiddenMessage = null)
    {
        var error = await response.ReadErrorMessageAsync();

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            apiError.SetError(string.IsNullOrWhiteSpace(error) ? unauthorizedMessage : error);
            return;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            apiError.SetError(string.IsNullOrWhiteSpace(error)
                ? (forbiddenMessage ?? "No tienes permisos para esta acción.")
                : error);
            return;
        }

        apiError.SetError(error);
    }

    private static bool LooksLikeProblemDetails(HttpResponseMessage response)
    {
        var mediaType = response.Content?.Headers.ContentType?.MediaType;
        return string.Equals(mediaType, "application/problem+json", StringComparison.OrdinalIgnoreCase);
    }

    private static T? TryDeserialize<T>(string raw) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(raw, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string BuildFallbackMessage(System.Net.HttpStatusCode statusCode, string? reasonPhrase)
    {
        var status = (int)statusCode;
        return string.IsNullOrWhiteSpace(reasonPhrase)
            ? $"Error HTTP {status}."
            : $"Error HTTP {status}: {reasonPhrase}.";
    }
}
