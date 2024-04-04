
using System.Net;

/**
 * Middleware 
 * Checks if valid ApiKey specified in request header and 
 * Checks if supported ApiVersion specified in request header
 **/
namespace BookShop.API.Controllers.Services
{
    public class ApiMiddleware(RequestDelegate request, IApiKeyValidator validator, ILogger<ApiMiddleware> logger)
    {
        private readonly RequestDelegate _request = request;
        private readonly IApiKeyValidator _validator = validator;
        private readonly ILogger<ApiMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(context.Request.Headers[ApiConstants.ApiKeyHeader]))
                {
                    LogError((int)HttpStatusCode.BadRequest, context);
                    return;
                }

                if (!decimal.TryParse(context.Request.Headers[ApiConstants.ApiVersionHeader], out decimal version) ||
                    version != 1 && version != 2)
                {
                    LogError((int)HttpStatusCode.ExpectationFailed, context);
                    return;
                }

                string? userKey = context.Request.Headers[ApiConstants.ApiKeyHeader];
                if (!_validator.isValid(userKey!))
                {
                    LogError((int)HttpStatusCode.Unauthorized, context);
                    return;
                }

                await _request(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message, args: ex.StackTrace);
                LogError((int)HttpStatusCode.InternalServerError, context);
                return;
            }
        }
        private void LogError(int statusCode, HttpContext context)
        {
            context.Response.StatusCode = statusCode;
            _logger.LogError(message: @"Access declined at {@DateTime}, entered API Version is not supported, response status code: {@statusCode}", DateTime.Now, statusCode);
        }
    }
}
