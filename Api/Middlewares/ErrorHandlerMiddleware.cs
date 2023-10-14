using Api.Exceptions;
using Application.Contracts;
using System.Net;
using System.Text.Json;

namespace Api.Middlewares
{

    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ErrorHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Path: {context.Request.Path} | Method: {context.Request.Method}");

                var response = context.Response;
                response.ContentType = "application/json";
                response.StatusCode = exception switch
                {
                    ApiException => (int)HttpStatusCode.BadRequest,// custom application error
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,// not found error
                    _ => (int)HttpStatusCode.InternalServerError,// unhandled error
                };

                var errors = new List<string>() { exception.Message };
                if (exception.InnerException != null) errors.Add(exception.InnerException.Message);

                var genericResponse = new GenericResponse(false, errors);
                var result = JsonSerializer.Serialize(genericResponse);
                await response.WriteAsync(result);
            }
        }
    }
}
