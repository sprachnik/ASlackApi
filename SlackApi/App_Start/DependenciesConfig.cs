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

            return services;
        }
    }
}
