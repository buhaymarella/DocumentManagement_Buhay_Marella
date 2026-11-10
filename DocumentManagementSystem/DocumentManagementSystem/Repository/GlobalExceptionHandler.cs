using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagementSystem.Repository
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken ct)
        {
            var (status, title, detail) = exception switch
            {
                InvalidUploadException e => (StatusCodes.Status400BadRequest, "Invalid upload", e.Message),
                FileTooLargeException e => (StatusCodes.Status413PayloadTooLarge, "File too large", e.Message),
                UnsupportedFileTypeException e => (StatusCodes.Status415UnsupportedMediaType, "Unsupported file type", e.Message),
                BadHttpRequestException e => (e.StatusCode,
                    e.StatusCode == StatusCodes.Status413PayloadTooLarge ? "File too large" : "Bad request",
                    "The request could not be processed. The upload may exceed the size limit."),
                InvalidDataException => (StatusCodes.Status400BadRequest, "Invalid upload",
                    "The multipart request is malformed or too large."),
                _ => (StatusCodes.Status500InternalServerError, "Server error",
                    "An unexpected error occurred.")
            };

            if (status >= 500)
                _logger.LogError(exception, "Unhandled exception");

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            }, ct);

            return true;
        }
    }
}
