using Microsoft.AspNetCore.Server.Kestrel.Core;
using SlackApi.App.Settings;
using SlackApi.App_Start;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlackApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;
        public const string CorsPolicyName = "PermittedOrigins";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var applicationSettings = GetApplicationSettings();
            var connectionStrings = GetConnectionStrings();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services
                .Configure<KestrelServerOptions>(options =>
                {
                    options.AllowSynchronousIO = true;
                })
                .AddApplicationInsightsTelemetry()
                .ConfigureIdentityAndSecurity(applicationSettings)
                .InitCache(applicationSettings, connectionStrings)
                .RegisterDependencies(applicationSettings, connectionStrings);
        }

        #region private methods

        private ApplicationSettings GetApplicationSettings()
        {
            var applicationSettings = new ApplicationSettings();
            Configuration.Bind("AppSettings", applicationSettings);
            return applicationSettings;
        }

        private ConnectionStrings GetConnectionStrings()
        {
            var connectionStrings = new ConnectionStrings();
            Configuration.Bind("ConnectionStrings", connectionStrings);
            return connectionStrings;
        }

        #endregion
    }
}
