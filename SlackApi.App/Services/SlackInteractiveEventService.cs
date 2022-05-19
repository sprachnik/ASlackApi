using Microsoft.Extensions.Logging;
using SlackApi.Domain.Constants;
using SlackApi.Domain.DTOs;

namespace SlackApi.App.Services
{
    public class SlackInteractiveEventService : ISlackInteractiveEventService
    {
        private readonly ILogger<ISlackInteractiveEventService> _logger;
        private readonly IShortCutService _shortCutService;

        public SlackInteractiveEventService(ILogger<ISlackInteractiveEventService> logger,
            IShortCutService shortCutService)
        {
            _logger = logger;
            _shortCutService = shortCutService;
        }

        public async Task<SlackResponse> ProcessInteractiveEvent(SlackInteractionPayload? interactiveEvent)
        {
            if (interactiveEvent is null)
                throw new ArgumentNullException(nameof(interactiveEvent));
           
            return await ProcessInteractiveEventAsync(interactiveEvent);
        }

        #region private methods

        private async Task<SlackResponse> ProcessInteractiveEventAsync(SlackInteractionPayload interactiveEvent)
            => interactiveEvent.Type switch
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
