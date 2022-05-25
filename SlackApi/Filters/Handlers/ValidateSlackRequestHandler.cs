using Microsoft.AspNetCore.Authorization;
using SlackApi.App.Auth;
using SlackApi.Core.Extensions;
using SlackApi.Core.Settings;
using System.Security.Authentication;
using System.Text;

namespace SlackApi.Filters.Handlers
{
    public class ValidateSlackRequestHandler : AuthorizationHandler<ValidateSlackRequestHandlerRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationSettings? _applicationSettings;
        private readonly ILogger<ValidateSlackRequestHandler>? _logger;

        public ValidateSlackRequestHandler(IHttpContextAccessor httpContextAccessor,
            ApplicationSettings applicationSettings,
            ILogger<ValidateSlackRequestHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _applicationSettings = applicationSettings;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ValidateSlackRequestHandlerRequirement requirement)
        {
            try
            {
                AuthenticateSlackRequest();
                context.Succeed(requirement);
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message, e);
            }

            return Task.CompletedTask;
        }

        public void AuthenticateSlackRequest()
        {
            var request = _httpContextAccessor?.HttpContext?.Request;

            if (request is null)
                throw new AuthenticationException("Unable to authenticate request!");

            request.EnableBuffering();

            var isSignatureFound = request.Headers.TryGetValue("X-Slack-Signature", out var signatureHeaders);

            if (!isSignatureFound || signatureHeaders.Any() == false)
                throw new AuthenticationException("Unable to verify request signature header!");

            var isRequestTimeStampFound = request.Headers.TryGetValue("X-Slack-Request-Timestamp", out var timestampHeaders);

            if (!isRequestTimeStampFound || signatureHeaders.Any() == false)
                throw new AuthenticationException("Unable to verify request timestamp header!");

            var slackSignature = signatureHeaders.FirstOrDefault();
            var slackTimestamp = timestampHeaders.FirstOrDefault();

            var timeStampDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(slackTimestamp ?? ""));
            var now = DateTimeOffset.UtcNow.AddMinutes(1);

            if (now.Subtract(timeStampDateTime).TotalMinutes > 5)
                throw new AuthenticationException("Unable to verify request timestamp!");

            var version = _applicationSettings?.SlackSettings?.DefaultVersionNo;
            var byteString = Encoding.UTF8.GetString(request.GetRawBodyBytes());

            var baseString = $"{version}:{slackTimestamp}:{byteString}";
            var calculatedSignature = $"{version}=" + HashHelper
                .GetSHA256HashFromKey(_applicationSettings?.SlackSettings?.SigningSecret ?? "", baseString);

            if (!calculatedSignature.Equals(slackSignature))
                throw new AuthenticationException("Unable to verify request signature!");
        }
    }

    public class ValidateSlackRequestHandlerRequirement : IAuthorizationRequirement
    {
        public ValidateSlackRequestHandlerRequirement()
        {
        }
    }
}
