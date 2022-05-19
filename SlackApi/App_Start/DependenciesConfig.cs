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
            // Transient
            services.AddTransient<ISlackInteractiveEventService, SlackInteractiveEventService>();
            services.AddTransient<ISlackAuthService, SlackAuthService>();
            services.AddTransient<IShortCutService, ShortcutService>();
            services.AddTransient<IBadgeService, BadgeService>();

            // Scopes

            // Singleton
            services.AddSingleton(applicationSettings);
            services.AddSingleton(connectionStrings);

            return services;
        }
    }
}
