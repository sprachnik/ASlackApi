using SlackApi.Domain.TableStorage;
using System.Runtime.Serialization;
using System.Text.Json;

namespace SlackApi.App.Telemetry
{
    public class TelemetryTableEntity : TableEntityBase
    {
        public string? MetricsString { get; set; }

        [IgnoreDataMember]
        public Dictionary<string, double>? Metrics 
            => string.IsNullOrWhiteSpace(MetricsString) 
                ? new Dictionary<string, double>() 
                : JsonSerializer.Deserialize<Dictionary<string, double>>(MetricsString);

        public string? PropertiesString { get; set; }

        [IgnoreDataMember]
        public Dictionary<string, double>? Properties
            => string.IsNullOrWhiteSpace(PropertiesString)
                ? new Dictionary<string, double>()
                : JsonSerializer.Deserialize<Dictionary<string, double>>(PropertiesString);

        public bool Success { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateFinished { get; set; }
        public string? Duration { get; set; } 
        public string? EventName { get; set; }
    }
}
