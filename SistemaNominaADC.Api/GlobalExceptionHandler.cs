using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SistemaNominaADC.Negocio.Excepciones;

namespace SistemaNominaADC.Api
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
     HttpContext httpContext,
     Exception exception,
     CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Error detectado: {Message}", exception.Message);

            var (statusCode, title) = exception switch
            {
                BusinessException => (StatusCodes.Status400BadRequest, "Regla de negocio"),
                NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autorizado"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                _ => (StatusCodes.Status500InternalServerError, "Error del Servidor")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message, 
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
