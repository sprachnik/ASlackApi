using SlackApi.App.HttpClients;
using SlackApi.Domain.SlackDTOs;

namespace SlackApi.App.Services
{
    public class SendMessageService : ISendMessageService
    {
        private readonly SlackApiClient _slackApiClient;

        public SendMessageService(SlackApiClient slackApiClient)
        {
            _slackApiClient = slackApiClient;
        }

        public async Task<SlackApiResponse?> PostMessage(SlackMessageRequest message)
            => await _slackApiClient.SlackPostRequest(SlackEndpoints.PostMessageUrl, message);
    }

    public interface ISendMessageService
    {
        Task<SlackApiResponse?> PostMessage(SlackMessageRequest message);
    }
}
