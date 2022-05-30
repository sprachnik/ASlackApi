using Microsoft.ApplicationInsights;
using SlackApi.App.Telemetry;
using SlackApi.Domain.Constants;
using SlackApi.Domain.SlackDTOs;
using SlackApi.Repository.TableStorage;

namespace SlackApi.App.Services
{
    public class SlackInteractiveEventService : ISlackInteractiveEventService
    {
        private readonly IShortCutService _shortCutService;
        private readonly TelemetryClient _telemetryClient;
        private readonly ITableStorageTelemetry _tableStorageTelemetry;

        public SlackInteractiveEventService(IShortCutService shortCutService,
            TelemetryClient telemetryClient,
            ITableStorageTelemetry tableStorageTelemetry)
        {
            _shortCutService = shortCutService;
            _telemetryClient = telemetryClient;
            _tableStorageTelemetry = tableStorageTelemetry;
        }

        public async Task<SlackResponse> ProcessInteractiveEvent(SlackInteractionPayload? interactiveEvent)
        {
            if (interactiveEvent is null)
                throw new ArgumentNullException(nameof(interactiveEvent));

            var actions = interactiveEvent?.Actions?.Select(a => a.ActionId)
                    .Aggregate((a, b) => $"{a}_{b}") ?? "NoActions";
            var type = $"{interactiveEvent?.Type ?? "NoType"}_{interactiveEvent?.CallbackId ?? "NoCallback"}";
            using var telemetry = new TelemetryTrack(_tableStorageTelemetry,
                _telemetryClient, type, actions, type);

            try
            {
                return await ProcessInteractiveEventAsync(interactiveEvent);
            }
            catch (Exception ex)
            {
                telemetry.Error(ex.Message);
                throw;
            }
        }

        #region private methods

        private async Task<SlackResponse> ProcessInteractiveEventAsync(SlackInteractionPayload? interactiveEvent)
            => interactiveEvent?.Type switch
            {
                SlackInteractionType.ShortCut => await _shortCutService.ProcessShortCut(interactiveEvent),
                _ => new SlackResponse()
            };
        

        #endregion
    }

    public interface ISlackInteractiveEventService
    {
        Task<SlackResponse> ProcessInteractiveEvent(SlackInteractionPayload? interactiveEvent);
    }
}
