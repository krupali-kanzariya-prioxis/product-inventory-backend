using System.Diagnostics;
using System.Text.Json;

namespace ProductInventoryTrackerAPI.Helper
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomMiddleware> _logger;

        public CustomMiddleware(RequestDelegate next, ILogger<CustomMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Request: {Method} {Path} at {Time}", context.Request.Method, context.Request.Path, DateTime.UtcNow);
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request: {Method} {Path} at {Time}", context.Request.Method, context.Request.Path, DateTime.UtcNow);
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "An unexpected error occurred.",
                };

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);


            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("Finished: {Method} {Path} in {ElapsedMilliseconds}ms", context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
            }
        }

    }
}