using Microsoft.AspNetCore.Authorization;
using SlackApi.Filters.Handlers;

namespace SlackApi.App_Start
{
    public static class AuthenticationFiltersConfig
    {
        public static IServiceCollection RegisterAuthenticationFilters(
            this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ValidateSlackRequest", policy =>
                    policy.Requirements.Add(new ValidateSlackRequestHandlerRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, ValidateSlackRequestHandler>();

            return services;
        }
    }
}
