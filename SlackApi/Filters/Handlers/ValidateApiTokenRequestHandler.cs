using Microsoft.AspNetCore.Authorization;
using SlackApi.Core.Settings;
using System.Security.Authentication;

namespace SlackApi.Filters.Handlers
{
    public class ValidateApiTokenRequestHandler : AuthorizationHandler<ValidateApiTokenRequestHandlerRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationSettings? _applicationSettings;
        private readonly ILogger<ValidateSlackRequestHandler>? _logger;

        public ValidateApiTokenRequestHandler(IHttpContextAccessor httpContextAccessor,
            ApplicationSettings applicationSettings,
            ILogger<ValidateSlackRequestHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _applicationSettings = applicationSettings;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ValidateApiTokenRequestHandlerRequirement requirement)
        {
            try
            {
                var request = _httpContextAccessor?.HttpContext?.Request;

                if (request is null)
                    throw new AuthenticationException("Unable to authenticate request!");

                var authorizationHeader = request.Headers.Authorization;

                if (authorizationHeader.Any(s => s != null))
                {
                    var token = authorizationHeader.FirstOrDefault(s => s != null);

                    if (_applicationSettings?.ApiToken?.Equals(token?.Replace("Bearer ", string.Empty)) == true)
                    {
                        context.Succeed(requirement);
                    }
                }

                throw new AuthenticationException("Unable to authenticate request!");
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message, e);
            }

            return Task.CompletedTask;
        }
    }

    public class ValidateApiTokenRequestHandlerRequirement : IAuthorizationRequirement
    {
        public ValidateApiTokenRequestHandlerRequirement()
        {
        }
    }
}
