using Microsoft.AspNetCore.Authentication.JwtBearer;
using SlackApi.Core.Settings;
using System.IdentityModel.Tokens.Jwt;

namespace SlackApi.App_Start
{
    public static class IdentityConfig
    {
        public static IServiceCollection ConfigureIdentityAndSecurity(this IServiceCollection services,
            ApplicationSettings applicationSettings)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(options =>
                {

                });

            return services;
        }
    }
}
