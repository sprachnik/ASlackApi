﻿using Dapper.Contrib.Extensions;
using SlackApi.App.Settings;
using SlackApi.Repository.SQL;
using SlackApi.Repository.TableStorage;

namespace SlackApi.App_Start
{
    public static class DatabaseConfig
    {
        public static IServiceCollection InitDatabases(
            this IServiceCollection services,
            ApplicationSettings applicationSettings,
            ConnectionStrings connectionStrings
        )
        {
            if (connectionStrings == null || applicationSettings == null)
                throw new NotImplementedException("Check the ConnectionStrings config is present and " +
                                                  "correct in 'appsettings.json'");

            services.AddSingleton<IDapperRepository, DapperRepository>(
                s => new DapperRepository(connectionStrings.SqlConnection));
            services.AddSingleton(connectionStrings);
            services.AddSingleton<ITableStorageService>(TableStorageFactory
                .Create(connectionStrings.TableStorage));

            SqlMapperExtensions.TableNameMapper = (type) =>
            {
                // Specify specific table names for types here.
                return type.Name switch
                {
                    _ => type.Name
                };
            };

            return services;
        }
    }
}
