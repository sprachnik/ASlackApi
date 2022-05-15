using Microsoft.Extensions.Logging;
using SlackApi.Domain.DTOs;
using System.Text.Json;

namespace SlackApi.App.Services
{
    public class SlackInteractiveEventService : ISlackInteractiveEventService
    {
        private readonly ILogger<ISlackInteractiveEventService> _logger;

        public SlackInteractiveEventService(ILogger<ISlackInteractiveEventService> logger)
        {
            _logger = logger;
        }

        public async Task<SlackResponse> ProcessInteractiveEvent(SlackInteractiveEvent? interactiveEvent)
        {
            var response = new SlackResponse();

            if (interactiveEvent?.BlockActions is null)
                throw new ArgumentNullException(nameof(interactiveEvent));

            
            foreach (var action in interactiveEvent.BlockActions)
            {
                try
                {
                    _logger.LogInformation(JsonSerializer.Serialize(action));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
            }
           
            return response;
        }
    }

    public interface ISlackInteractiveEventService
    {
        Task<SlackResponse> ProcessInteractiveEvent(SlackInteractiveEvent? interactiveEvent);
    }
}
