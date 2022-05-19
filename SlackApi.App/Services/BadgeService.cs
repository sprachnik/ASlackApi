using SlackApi.Domain.DTOs;

namespace SlackApi.App.Services
{
    public class BadgeService : IBadgeService
    {
        public BadgeService()
        {

        }

        public async Task<SlackResponse> SendABadge()
        {

            return new SlackResponse();
        }
    }

    public interface IBadgeService
    {
        Task<SlackResponse> SendABadge();
    }
}
