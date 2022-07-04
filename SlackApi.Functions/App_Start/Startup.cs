using Microsoft.Extensions.DependencyInjection;
using SlackApi.Core.Settings;
using SlackApi.Repository.StorageQueue;
using System.Text.Json;

namespace App
{
    public static class Startup
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddLogging();

            services.AddSingleton(new ApplicationSettings
            {
                MaxTransactionSize = GetEnvironmentVariable<double>("MaxTransactionSize"),
                DefaultWeeklyBalance = GetEnvironmentVariable<double>("DefaultWeeklyBalance"),
                ApiToken = GetEnvironmentVariable<string>("ApiToken"),
                SlackSettings = new SlackSettings
                {
                    DefaultSlackWebookUri = GetEnvironmentVariable<string>("DefaultSlackWebookUri"),
                    OutgoingOAuthToken = GetEnvironmentVariable<string>("OutgoingOAuthToken"),
                    IncomingToken = GetEnvironmentVariable<string>("IncomingToken"),
                    SigningSecret = GetEnvironmentVariable<string>("SigningSecret"),
                    DefaultVersionNo = GetEnvironmentVariable<string?>("DefaultVersionNo") ?? "v0"
                }
            });

            services.AddSingleton(new ConnectionStrings
            {
                QueueStorage = GetEnvironmentVariable<string>("QueueStorage")
            });

            services.AddTransient<IQueueService, QueueService>();
        }

        public static T? GetEnvironmentVariable<T>(string name)
        {
            var variableString = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

            if (string.IsNullOrWhiteSpace(variableString))
                return default;

            if (typeof(T) == typeof(int))
            {
                return (T)(object)Convert.ToInt32(variableString);
            }

            if (typeof(T) == typeof(bool))
            {
                return (T)(object)Convert.ToBoolean(variableString);
            }

            if (typeof(T) == typeof(double))
            {
                return (T)(object)Convert.ToDouble(variableString);
            }

            if (typeof(T) == typeof(DateTime))
            {
                return (T)(object)Convert.ToDateTime(variableString);
            }

            if (typeof(T) == typeof(string[]))
            {
                return JsonSerializer.Deserialize<T>(variableString);
            }

            return (T)(object)variableString;
        }
    }
}