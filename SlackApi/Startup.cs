using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using SlackApi.App.JsonConverters;
using SlackApi.App.MemStore;
using SlackApi.App_Start;
using SlackApi.Core.Settings;
using SlackApi.Domain.BadgeDTOs;
using SlackApi.Domain.Constants;
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
                    options.JsonSerializerOptions.Converters.Add(new BlockConverter());
                });

            services
                .Configure<KestrelServerOptions>(options =>
                {
                    options.AllowSynchronousIO = true;
                })
                .AddApplicationInsightsTelemetry()
                .ConfigureIdentityAndSecurity(applicationSettings)
                .InitCache(applicationSettings, connectionStrings)
                .InitDatabases(applicationSettings, connectionStrings)
                .RegisterDependencies(applicationSettings, connectionStrings)
                .ConfigureHttpClient()
                .RegisterAuthenticationFilters()
                .AddAntiforgery(options => options.SuppressXFrameOptionsHeader = true);

            services.ConfigureSwaggerGen(c =>
            {
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }

        public void Init(IServiceProvider services)
        {
            var tableStorageMemStore = services.GetService<ITableStorageMemStore>();

            if (tableStorageMemStore == null)
                throw new Exception($"Unable to init {nameof(ITableStorageMemStore)}!");

            tableStorageMemStore.Init<BadgeTableEntity>(TableStorageTable.Badges).GetAwaiter().GetResult();
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
