using SlackApi.Core.Enums;

namespace SlackApi.Core.Extensions
{
    public static class ConfigExtensions
    {
        public static string? GetEnvironmentVariable(string environmentVariable) =>
            Environment.GetEnvironmentVariable(environmentVariable);

        public static EnvironmentType GetCurrentEnvironment()
        {
            var success = Enum.TryParse(GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), out EnvironmentType environment);
            return success ? environment : EnvironmentType.Development;
        }
    }
}
