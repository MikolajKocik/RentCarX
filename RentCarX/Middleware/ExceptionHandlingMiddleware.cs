using RentCarX.Domain.Exceptions;

namespace RentCarX.Presentation.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("ExceptionHandlingMiddleware activated");

            try
            {
                await _next(context);
            }
            catch (BadRequestException ex)
            {
                _logger.LogError(ex, "BadRequestException caught: {ErrorMessage}", ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                var errorResponse = new
                {
                    status = StatusCodes.Status400BadRequest,
                    title = "Bad Request",
                    message = "Validation failed for the request."
                };
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "NotFoundException caught: {ErrorMessage}", ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                var errorResponse = new
                {
                    status = StatusCodes.Status404NotFound,
                    title = "Not Found",
                    message = ex.Message
                };
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
            catch (ConflictException ex)
            {
                _logger.LogError(ex, "ConflictException caught: {ErrorMessage}", ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                var errorResponse = new
                {
                    status = StatusCodes.Status409Conflict,
                    title = "Conflict",
                    message = ex.Message
                };
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogError(ex, "UnauthorizedException caught: {ErrorMessage}", ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var errorResponse = new
                {
                    status = StatusCodes.Status401Unauthorized,
                    title = "Unauthorized",
                    message = ex.Message
                };
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "UnauthorizedAccessException caught: {ErrorMessage}", ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                var errorResponse = new
                {
                    status = StatusCodes.Status403Forbidden,
                    title = "Forbidden",
                    message = "Unauthorized access due to insufficient permissions."
                };
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var errorResponse = new
                {
                    status = StatusCodes.Status500InternalServerError,
                    title = "Internal Server Error",
                    message = "An internal server error occurred."
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}