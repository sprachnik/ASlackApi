using SlackApi.Domain.Constants;
using SlackApi.Domain.SlackDTOs;

namespace SlackApi.App.Services
{
    public class ShortcutService : IShortCutService
    {
        private readonly IBadgeService _badgeService;

        public ShortcutService(IBadgeService badgeService)
        {
            _badgeService = badgeService;
        }

        public async Task<SlackResponse> ProcessShortCut(SlackInteractionPayload payload)
            => payload.CallbackId switch
            {
                ShortCutCallback.SendABadge => await _badgeService.OpenSendBadgeView(payload),
                _ => throw new NotImplementedException($"{payload.CallbackId} is not supported!")
            };
        

        #region private methods


        #endregion
    }

    public interface IShortCutService
    {
        Task<SlackResponse> ProcessShortCut(SlackInteractionPayload payload);
    }
}
