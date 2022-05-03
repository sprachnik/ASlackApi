using SlackApi.App.Settings;

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
            var connectionString = GetConnectionStrings();


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
