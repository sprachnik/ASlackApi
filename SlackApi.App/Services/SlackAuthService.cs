using SlackApi.Domain.DTOs;

namespace SlackApi.App.Services
{
    public class SlackAuthService : ISlackAuthService
    {
        public SlackAuthService()
        {

        }

        public SlackResponse ProcessSlackChallenge(SlackEvent incomingEvent)
        {
            if (string.IsNullOrWhiteSpace(incomingEvent.Challenge))
                return new SlackResponse();

            return new SlackResponse { Challenge = incomingEvent.Challenge };
        }
    }

    public interface ISlackAuthService
    {
        SlackResponse ProcessSlackChallenge(SlackEvent incomingEvent);
    }
}
