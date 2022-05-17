using Microsoft.Extensions.Logging;
using SlackApi.Domain.DTOs;

namespace SlackApi.App.Services
{
    public class SlackInteractiveEventService : ISlackInteractiveEventService
    {
        private readonly ILogger<ISlackInteractiveEventService> _logger;

        public SlackInteractiveEventService(ILogger<ISlackInteractiveEventService> logger)
        {
            _logger = logger;
        }

        public async Task<SlackResponse> ProcessInteractiveEvent(SlackInteractionPayload? interactiveEvent)
        {
            var response = new SlackResponse();

            if (interactiveEvent is null)
                throw new ArgumentNullException(nameof(interactiveEvent));

            Console.WriteLine(interactiveEvent.Type);
           
            return response;
        }
    }

    public interface ISlackInteractiveEventService
    {
        Task<SlackResponse> ProcessInteractiveEvent(SlackInteractionPayload? interactiveEvent);
    }
}
