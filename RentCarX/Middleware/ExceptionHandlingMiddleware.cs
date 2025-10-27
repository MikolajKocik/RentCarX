using RentCarX.Domain.ExceptionModels;
using RentCarX.Domain.Exceptions; 
using System.Net;
using System.Text.Json; 
using System.Text.Json.Serialization; 

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
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An unhandled exception occurred: {ex.Message}", ex.Message);

                await HandleExceptionAsync(context, ex); 
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode = HttpStatusCode.InternalServerError; 
            string title = "An unexpected internal server error occurred.";
            string detail = "An unexpected error occurred. Please try again later."; 
            string errorCode = "InternalServerError"; 

            switch (exception)
            {
                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest; 
                    title = "Bad Request";
                    detail = badRequestException.Message; 
                    errorCode = null; 
                    break;
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound; 
                    title = "Not Found";
                    detail = notFoundException.Message; 
                    errorCode = null; 
                    break;
                case ConflictException conflictException:
                    statusCode = HttpStatusCode.Conflict; 
                    title = "Conflict";
                    detail = conflictException.Message; 
                    errorCode = null; 
                    break;

                case EmailNotConfirmedException emailNotConfirmedException:
                    statusCode = HttpStatusCode.Unauthorized;
                    title = "Email Not Confirmed"; 
                    detail = emailNotConfirmedException.Message; 
                    errorCode = emailNotConfirmedException.ErrorCode; 
                    break;

                case AlreadyDeletedException alreadyDeletedException:
                    statusCode = HttpStatusCode.Forbidden;
                    title = "Forbidden";
                    detail = alreadyDeletedException.Message;
                    errorCode = null;
                    break;

                case UnauthorizedException unauthorizedException: 
                    statusCode = HttpStatusCode.Unauthorized; 
                    title = "Unauthorized";
                    detail = unauthorizedException.Message;
                    errorCode = null; 
                    break;

                case UnauthorizedAccessException unauthorizedAccessException: 
                    statusCode = HttpStatusCode.Forbidden; 
                    title = "Forbidden";
                    detail = "Access denied due to insufficient permissions."; 
                    errorCode = null; 
                    break;

                default:
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var errorResponse = new ErrorDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = detail,
                ErrorCode = errorCode 
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
            };

            await context.Response.WriteAsJsonAsync(errorResponse, options);
        }

        private class ErrorDetails
        {
            public int Status { get; set; } 
            public string Title { get; set; } 
            public string Detail { get; set; } 

            public string? ErrorCode { get; set; }

            public ErrorDetails() { }

            public ErrorDetails(int status, string title, string detail, string? errorCode = null)
            {
                Status = status;
                Title = title;
                Detail = detail;
                ErrorCode = errorCode;
            }
        }
    }
}