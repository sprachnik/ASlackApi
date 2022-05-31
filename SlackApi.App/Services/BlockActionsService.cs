using SlackApi.Core.Exceptions;
using SlackApi.Core.Extensions;
using SlackApi.Domain.SlackDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackApi.App.Services
{
    public class BlockActionsService : IBlockActionService
    {
        private readonly IBadgeViewModalService _badgeViewModalService;

        public BlockActionsService(IBadgeViewModalService badgeViewModalService)
        {
            _badgeViewModalService = badgeViewModalService;
        }

        public async Task<SlackResponse> ProcessBlockActions(SlackInteractionPayload? interactiveEvent)
        {
            if (interactiveEvent?.Actions == null)
                return new();

            Parallel.ForEach(interactiveEvent.Actions,
                async action =>
                {
                    _ = action switch
                    {
                        // Give Badge View Modal
                        { ActionId: BadgeViewModalService.BadgeSelectActionId } => await _badgeViewModalService.SelectBadgeAction(interactiveEvent),
                        { ActionId: BadgeViewModalService.UserSelectionActionId } => await _badgeViewModalService.SelectUserAction(interactiveEvent),
                        _ => throw new NotImplementedException($"No action found for {action.ActionId} or {action.BlockId}!")
                    };
                });

            return new();
        }
    }

    public interface IBlockActionService
    {
        Task<SlackResponse> ProcessBlockActions(SlackInteractionPayload? interactiveEvent);
    }
}
