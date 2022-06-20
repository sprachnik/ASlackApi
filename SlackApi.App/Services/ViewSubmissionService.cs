using SlackApi.Domain.Constants;
using SlackApi.Domain.SlackDTOs;

namespace SlackApi.App.Services
{
    public class ViewSubmissionService : IViewSubmissionService
    {
        private readonly IBadgeViewModalService _badgeViewModalService;

        public ViewSubmissionService(IBadgeViewModalService badgeViewModalService)
        {
            _badgeViewModalService = badgeViewModalService;
        }

        public async Task<SlackResponse> ProcessViewSubmission(SlackInteractionPayload payload)
        {
            if (payload?.Type != SlackInteractionType.ViewSubmission)
                throw new Exception($"Cannot process event type {payload?.Type}!");

            return payload.Type switch
            {
                SlackInteractionType.ViewSubmission => await _badgeViewModalService.SubmitBadgeView(payload),
                _ => throw new NotImplementedException($"Cannot process event type {payload?.Type}!")
            };
        }
    }

    public interface IViewSubmissionService
    {
        Task<SlackResponse> ProcessViewSubmission(SlackInteractionPayload payload);
    }
}
