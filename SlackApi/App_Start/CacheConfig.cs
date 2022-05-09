using SlackApi.App.Settings;

namespace SlackApi.App_Start
{
    public static class CacheConfig
    {
        public static IServiceCollection InitCache(this IServiceCollection services,
           ApplicationSettings applicationSettings, ConnectionStrings connectionStrings)
        {
            services.AddDistributedMemoryCache();
            return services;
        }
    }
}
