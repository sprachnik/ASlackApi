using SlackApi.App.Services;
using SlackApi.App.Settings;

namespace SlackApi.App_Start
{
    public static class DependenciesConfig
    {
        public static IServiceCollection RegisterDependencies(
            this IServiceCollection services,
            ApplicationSettings applicationSettings,
            ConnectionStrings connectionStrings)
        {
            services.AddTransient<ISlackInteractiveEventService, SlackInteractiveEventService>();
            services.AddTransient<ISlackAuthService, SlackAuthService>();

            return services;
        }
    }
}
