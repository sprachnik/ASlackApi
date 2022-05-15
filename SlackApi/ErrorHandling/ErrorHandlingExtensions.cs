using SlackApi.Core.Exceptions;
using SlackApi.Core.Extensions;
using SlackApi.Extensions;
using System.Net;
using System.Security;
using System.Security.Authentication;

namespace SlackApi.ErrorHandling
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(RequestDelegate next,
            ILogger<ExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex) when (
                ex is UnauthorizedAccessException
                || ex is SecurityException
                || ex is AuthenticationException)
            {
                await HandleExceptionAsync(httpContext, ex, HttpStatusCode.Unauthorized);
            }
            catch (ValidationException ex)
            {
                var modelStateDict = new Dictionary<string, string>();

                if (ex.Errors != null)
                {
                    // Convert the ValidationException into a ModelStateDictionary response
                    foreach (var error in ex.Errors)
                    {
                        if (!modelStateDict.ContainsKey(error.Field.ToCamel()))
                            modelStateDict.Add(error.Field.ToCamel(), error.Messages.ToList().ListToString());
                    }
                }

                await HandleExceptionAsync(
                    httpContext,
                    ex,
                    HttpStatusCode.BadRequest,
                    modelStateDict);
            }
            catch (BusinessException ex)
            {
                await HandleExceptionAsync(httpContext, ex, HttpStatusCode.BadRequest);
            }
            catch (NotFoundException ex)
            {
                await HandleExceptionAsync(httpContext, ex, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex, HttpStatusCode.InternalServerError);
            }
        }

        private Task HandleExceptionAsync(
            HttpContext context,
            Exception ex,
            HttpStatusCode statusCode,
            Dictionary<string, string>? modelState = null)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ApiResponse
            {
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                ModelState = modelState?.Any() != true ? null : modelState
            };

            return context.Response.WriteApiResponseAsync(response);
        }
    }
}
