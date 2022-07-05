using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using SlackApi.Core.Extensions;
using SlackApi.Repository.TableStorage;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SlackApi.App.Telemetry
{
    public class TelemetryTrack : IDisposable
    {
        private readonly ITableStorageTelemetry? _tableStorageTelemetry;
        private readonly TelemetryClient _telemetryClient;
        private bool _isSuccess = true;
        private TelemetryTableEntity? _telemetry = null;
        private DateTime _dateStarted;
        private string? _tableName = null;
        private EventTelemetry _event;

        public TelemetryTrack(TelemetryClient telemetryClient, string eventName)
        {
            _dateStarted = DateTime.UtcNow;
            _telemetryClient = telemetryClient;
            _event = new EventTelemetry(eventName);
            AddMetric(eventName, 1);
        }

        public TelemetryTrack(ITableStorageTelemetry tableStorageTelemetry,
            TelemetryClient telemetryClient,
            string eventName,
            string key,
            string tableName) : this(telemetryClient, eventName)
        {
            _tableStorageTelemetry = tableStorageTelemetry;
            _tableName = Regex.Replace(tableName, "[^A-Za-z0-9 -]", "")
                .ToLower().Truncate(63);
            _telemetry = new TelemetryTableEntity
            {
                PartitionKey = key,
                RowKey = _dateStarted.ToReverseTicksMillis().ToString("D16"),
                EventName = eventName,
                DateStarted = _dateStarted,
            };
        }

        public void AddProperty(string property, string value)
        {
            if (_event.Properties.ContainsKey(property))
                _event.Properties[property] = value;

            _event.Properties.Add(property, value);
        }

        public void AddMetric(string metric, double value)
        {
            if (_event.Metrics.ContainsKey(metric))
                _event.Metrics[metric] = value;

            _event.Metrics.Add(metric, value);
        }

        public void Error(string error)
        {
            _isSuccess = false;
            AddMetric("Error", 1);
            AddProperty("Error", error);
        }

        public void Dispose()
        {
            var dateFinished = DateTime.UtcNow;
            var duration = dateFinished - _dateStarted;
            AddMetric("Duration", duration.TotalMilliseconds);
            CompleteTelemetry(dateFinished, duration);
            _telemetryClient.TrackEvent(_event);
        }

        private void CompleteTelemetry(DateTime dateFinished, TimeSpan duration)
        {
            if (_telemetry == null || _tableStorageTelemetry == null || _tableName == null)
                return;
            try
            {
                _telemetry.DateFinished = dateFinished;
                _telemetry.Duration = duration.TotalMilliseconds.ToString();
                _telemetry.MetricsString = JsonSerializer.Serialize(_event.Metrics);
                _telemetry.PropertiesString = JsonSerializer.Serialize(_event.Properties);
                _telemetry.Success = _isSuccess;
                _telemetry.EventName = _event.Name;
                _tableStorageTelemetry.Upsert(_telemetry, _tableName);
            }
            catch (Exception ex)
            {
                AddProperty("TelemetryTableStorageSaveError", ex.Message);
            }
        }
    }
}
