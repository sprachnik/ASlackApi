using SlackApi.App.HttpClients;

namespace SlackApi.App_Start
{
    public static class HttpClientConfig
    {
        public static IServiceCollection ConfigureHttpClient(
            this IServiceCollection services)
        {
            services.AddHttpClient<SlackApiClient>();

            return services;
        }
    }
}
